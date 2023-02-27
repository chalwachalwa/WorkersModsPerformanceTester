using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace WorkersModsPerformanceTester
{
    internal class ModsProcessor
    {
        // others not handled
        // WORKSHOP_ITEMTYPE_BUILDINGEDITOR_ELEMENT
        // WORKSHOP_ITEMTYPE_VEHICLESKIN
        // WORKSHOP_ITEMTYPE_BUILDINGSKIN
        private readonly string[] _modTypesAllowedToProcess = new[] { "WORKSHOP_ITEMTYPE_BUILDING", "WORKSHOP_ITEMTYPE_VEHICLE" };

        private readonly CsvBuilder _csvBuilder;
        private readonly ProgressBar _progressBar;

        public ModsProcessor(CsvBuilder csvBuilder, ProgressBar progressBar)
        {
            _csvBuilder = csvBuilder;
            _progressBar = progressBar;
        }

        public void Process()
        {
            var workshopPath = "C:\\Program Files (x86)\\Steam\\steamapps\\workshop\\content\\784150";
            var modFolders = Directory.GetDirectories(workshopPath);

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
                }
                
                // Exclude mods that are not vechicles or buildings
                if (!_modTypesAllowedToProcess.Contains(mod.Type)) continue;

                if (mod.Type == "WORKSHOP_ITEMTYPE_BUILDING")
                {
                    var buildings = mod.Subfolders.SelectMany(x => Directory.GetFiles(x, "renderconfig.ini")).Select(x => new Building(x)).ToArray();
                    mod.Models.AddRange(buildings);
                }
                else if (mod.Type == "WORKSHOP_ITEMTYPE_VEHICLE")
                {
                    var buildings = mod.Subfolders.SelectMany(x => Directory.GetFiles(x, "script.ini"))
                        .Select(x => new Vehicle(x)).ToArray();
                    mod.Models.AddRange(buildings);
                }
                foreach (var model in mod.Models)
                {
                    _csvBuilder.AddRow(mod.Id, mod.Type,model.Name, model.LODsCount, model.TexturesSize, model.Faces, model.FolderPath);
                }
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
            }
            catch (KeyNotFoundException e)
            {
                _csvBuilder.AddRow(mod.Id, "", "","", "", "", mod.Folder, "Mod invalid - missing property in workshopconfig.ini");
                throw e;
            }
            catch (ApplicationException e)
            {
                _csvBuilder.AddRow(mod.Id, "", "","", "", "", mod.Folder, e.Message);
                throw e;
            }
        }

        private static int ReadVertices(string path)
        {
            using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                using (var br = new BinaryReader(fs))
                {
                    // header
                    var buffer = br.ReadBytes(20);
                    var numMaterials = BitConverter.ToInt32(buffer, 8);
                    var numNodes = BitConverter.ToInt32(buffer, 12);

                    // materials
                    var offset = numMaterials * 64;
                    buffer = br.ReadBytes(offset);
                    for (int i = 0; i < numNodes; i++)
                    {
                        // node type
                        buffer = br.ReadBytes(4);
                        var nodeType = BitConverter.ToInt32(buffer, 0);

                        if (nodeType == 0)
                        {
                            buffer = br.ReadBytes(228);
                            var numLods = BitConverter.ToInt32(buffer, 224);
                            if (numLods != 1) throw new ApplicationException();
                            buffer = br.ReadBytes(8);
                            return BitConverter.ToInt32(buffer, 4);
                        }
                        else if (nodeType == 1)
                        {
                            buffer = br.ReadBytes(288);
                        }
                        else
                        {
                            throw new ApplicationException("Node type 2 unhandled");
                        }
                    }

                }
            }
            throw new ApplicationException("Cannot read vertices");
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
    }
}
