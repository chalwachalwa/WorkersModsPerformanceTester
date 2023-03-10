using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using WorkersModsPerformanceTester.Utilities;

namespace WorkersModsPerformanceTester
{
    internal class ModsProcessor
    {
        // others not handled
        // WORKSHOP_ITEMTYPE_BUILDINGEDITOR_ELEMENT
        // WORKSHOP_ITEMTYPE_VEHICLESKIN
        // WORKSHOP_ITEMTYPE_BUILDINGSKIN
        private readonly string[] _modTypesAllowedToProcess = new[] { "WORKSHOP_ITEMTYPE_BUILDING", "WORKSHOP_ITEMTYPE_VEHICLE" };
        private Dictionary<string, string> _usersMemoryCache;

        private readonly CsvBuilder _csvBuilder;
        private readonly ProgressBar _progressBar;
        private readonly string _workshopPath;
        private readonly bool _scrapUsers;
        private readonly Logger _logger;

        public ModsProcessor(CsvBuilder csvBuilder, ProgressBar progressBar)
        {
            _csvBuilder = csvBuilder;
            _progressBar = progressBar;
            _workshopPath = Program.Settings.WorkshopPath;
            _scrapUsers = Program.Settings.ScrapUsers;
            _logger = Program.Logger;
        }

        public void Process()
        {
            string[] modFolders = { };
            try
            {
                modFolders = Directory.GetDirectories(_workshopPath);
            }
            catch(DirectoryNotFoundException e)
            {
                _logger.Write(Level.Error, $"{modFolders} not found:", e);
            }

            int progressBarIterator = 0;

            foreach (var modFolder in modFolders)
            {
                _progressBar.Report((double)progressBarIterator++ / modFolders.Length);
                var mod = new Mod(modFolder + "//");

                try
                {
                    GetModProperties(mod);
                }
                catch (Exception e)
                {
                    continue;
                    _logger.Write(Level.Warning, "Get modProperties unhandled exception",e);
                }
                
                // Exclude mods that are not vechicles or buildings
                if (!_modTypesAllowedToProcess.Contains(mod.Type)) continue;
                
                ProcessModModels(mod);

                foreach (var model in mod.Models)
                {
                    _csvBuilder.AddRow(mod.Id, mod.AuthorId, mod.AuthorName, mod.Type,$"{mod.Name}/{model.Name}", model.LODsCount.ToString(), model.TexturesSize.GetHumanReadableFileSize() , model.Faces.ToString(), model.Score, model.FolderPath, $"https://steamcommunity.com/sharedfiles/filedetails/?id={mod.Id}");
                }
            }
        }

        private void ProcessModModels(Mod mod)
        {
            try
            {
                if (mod.Type == "WORKSHOP_ITEMTYPE_BUILDING")
                {
                    var buildings = mod.Subfolders.SelectMany(x => Directory.GetFiles(x, "renderconfig.ini")).Select(x => new Building(x, mod.Type)).ToArray();
                    mod.Models.AddRange(buildings);
                }
                else if (mod.Type == "WORKSHOP_ITEMTYPE_VEHICLE")
                {
                    var buildings = mod.Subfolders.SelectMany(x => Directory.GetFiles(x, "script.ini"))
                        .Select(x => new Vehicle(x, mod.Type)).ToArray();
                    mod.Models.AddRange(buildings);
                }
            }
            catch(Exception e)
            {
                _logger.Write(Level.Error, "Unhandled mod processing error: ",e);
            }
        }

        private void GetModProperties(Mod mod)
        {
            try
            {
                Dictionary<string, string> modProperties;
                modProperties = GetScriptProperties(Path.Combine(mod.Folder, "workshopconfig.ini"));
                mod.Type = modProperties["$ITEM_TYPE"];
                mod.Name = modProperties["$ITEM_NAME"];
                mod.AuthorId = modProperties["$OWNER_ID"];

                if (_scrapUsers)
                {
                    Task<string> task = Task.Run<string>(async () => await GetAuthorName(mod.AuthorId));
                    mod.AuthorName = task.Result;
                }
            }
            catch (KeyNotFoundException e)
            {
                _csvBuilder.AddRow(mod.Id, "", "", "", "","", "", "", "", mod.Folder,$"https://steamcommunity.com/sharedfiles/filedetails/?id={mod.Id}", "Mod invalid - missing property in workshopconfig.ini");
                throw e;
            }
            catch (ApplicationException e)
            {
                _csvBuilder.AddRow(mod.Id, "","", "", "","", "", "", "", mod.Folder, $"https://steamcommunity.com/sharedfiles/filedetails/?id={mod.Id}", e.Message);
                throw e;
            }
        }

        private static Dictionary<string, string> GetScriptProperties(string path)
        {
            var results = new Dictionary<string, string>();

            try
            {
                using (var fileReader = new StreamReader(path))
                {
                    while (!fileReader.EndOfStream)
                    {
                        var line = fileReader.ReadLine();
                        if (string.IsNullOrWhiteSpace(line)) continue;
                        var words = line.Split(" ");

                        string propertyName;
                        if (words.Length > 1)
                        {
                            propertyName = words[0];
                            if (!propertyName.StartsWith('$')) continue;
                        }
                        else
                        {
                            continue;
                        }

                        results.TryAdd(propertyName, line.Replace("\"", "").Replace(propertyName, "").Trim());
                    }
                }
            }
            catch (FileNotFoundException e)
            {
                throw new ApplicationException("Invalid mod - no workshopconfig.ini", e);
            }
            return results;
        }

        private async Task<string> GetAuthorName(string id)
        {
            string text, html, json, result = "-";
            try
            {
                if (!File.Exists("cache.json"))
                {
                    File.Create("cache.json");
                    Task.Delay(500).RunSynchronously();
                }
                Dictionary<string, string> users = null;
                if (_usersMemoryCache == null)
                {
                    using (var sr = new StreamReader("cache.json"))
                    {
                        text = sr.ReadToEnd();
                        if (!string.IsNullOrEmpty(text))
                        {
                            users = JsonConvert.DeserializeObject<Dictionary<string, string>>(text);
                            _usersMemoryCache = users;
                        }
                        else
                        {
                            users = new Dictionary<string, string>();
                        }
                    }
                }
                else
                {
                    users = _usersMemoryCache;
                }

                var sucess = users.TryGetValue(id, out var name);
                if (sucess)
                {
                    return name;
                }

                using (var httpClient = new HttpClient())
                {
                    using (HttpResponseMessage response = httpClient.GetAsync($"https://www.steamidfinder.com/lookup/{id}/").Result)
                    {
                        using (HttpContent content = response.Content)
                        {
                            html = content.ReadAsStringAsync().Result;
                        }
                    }

                    var pattern = @"(?<=>name<\/th>\n\s*<td><code>)(.*)(?=<\/code><\/td)";
                    var match = Regex.Match(html, pattern);
                    result = match.Value;
                    if (string.IsNullOrEmpty(result)) return "-";

                    users.TryAdd(id, result);
                    json = JsonConvert.SerializeObject(users);
                }
                using (var sr = new StreamWriter("cache.json"))
                {
                    sr.Write(json);
                }
            }
            catch(Exception e)
            {
                ;
            }
            return result;
        }
    }
}
