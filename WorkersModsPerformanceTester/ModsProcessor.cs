using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkersModsPerformanceTester
{
    internal class ModsProcessor
    {
        private readonly string[] modTypes = new[] { "WORKSHOP_ITEMTYPE_BUILDING", "WORKSHOP_ITEMTYPE_VEHICLE" };

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

                var modNumber = Path.GetFileName(Path.GetDirectoryName(modFolder));

                string modType;
                string modName;
                try
                {
                    Dictionary<string, string> modProperties;
                    modProperties = GetScriptProperties(Path.Combine(modFolder, "workshopconfig.ini"));
                    modType = modProperties["$ITEM_TYPE"];
                    modName = modProperties["$ITEM_NAME"];
                }
                catch (KeyNotFoundException e)
                {
                    _csvBuilder.AddRow(modNumber, "", "", "", "", modFolder, "Mod invalid - missing property in workshopconfig.ini");
                    continue;
                }
                catch (ApplicationException e)
                {
                    _csvBuilder.AddRow(modNumber, "", "", "", "", modFolder, e.Message);
                    continue;
                }
                
                // Exclude mods that are not vechicles or buildings
                if (!modTypes.Contains(modType)) continue; 

                var subfolders = Directory.GetDirectories(modFolder, "*", SearchOption.AllDirectories);

                foreach (var subfolder in subfolders)
                {
                    // ignore empty subfolder or when models are nested
                    var files = Directory.GetFiles(subfolder);
                    if (!files.Any()) continue; 

                    if (subfolder.Contains("textur", StringComparison.InvariantCultureIgnoreCase)
                        || subfolder.Contains("sound", StringComparison.InvariantCultureIgnoreCase)
                        || subfolder.Contains("joint", StringComparison.InvariantCultureIgnoreCase)
                        || subfolder.Contains("resource", StringComparison.InvariantCultureIgnoreCase)) continue;
                    //todo: handle textures folders

                    var nmfFiles = Directory.GetFiles(subfolder, "*.nmf");
                    var filesWithLod = nmfFiles.Where(x =>
                    {
                        var fileName = Path.GetFileName(x);
                        var fileNameLongerThanTwo = fileName.Remove(fileName.Length - 4, 4).Length > 2;
                        return fileNameLongerThanTwo && fileName.Contains("LOD", StringComparison.InvariantCultureIgnoreCase);
                    }).ToArray();

                    var texturesSize = Directory.GetFiles(subfolder, "*.dds")
                        .Sum(x => new FileInfo(x).Length)
                        .GetHumanReadableFileSize();

                    var modelPath = nmfFiles.Except(filesWithLod)
                        .FirstOrDefault(x => !x.EndsWith("joint.nmf") && !x.EndsWith("anim.nmf"));

                    if (modelPath == null)
                    {
                        //check for variants
                        string modelPathRelative = null;
                        try
                        {
                            modelPathRelative = ReadRelatedModel(Path.Combine(subfolder, "renderconfig.ini"));
                        }
                        catch (ApplicationException e)
                        {
                            _csvBuilder.AddRow(modNumber, "", "", "", "", modFolder, e.Message);
                            continue;
                        }

                        if (modelPathRelative.Contains("../"))
                        {
                            modelPath = Path.GetFullPath(Path.Combine(subfolder, modelPathRelative));
                            // todo: maybe select that as variant to avoid duplicates?
                        }
                        else
                        {
                            _csvBuilder.AddRow(modNumber, "", "", "", "", modFolder, "Mod invalid - No model");
                            continue;
                        }
                    }
                    var vertices = ReadVertices(modelPath).ToString();

                    var appendSubmod = subfolders.Length > 1;
                    var name = appendSubmod ? modName + "\\" + new DirectoryInfo(Path.GetDirectoryName(subfolder + "\\")).Name : modName;
                    _csvBuilder.AddRow(modNumber, name, filesWithLod.Count().ToString(), texturesSize, vertices, subfolder);
                };
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

        //private static readonly string[] ScriptProperties = { "$ITEM_TYPE", "" };

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

        private static string ReadRelatedModel(string path)
        {
            try
            {
                using (var fileReader = new StreamReader(path))
                {
                    while (!fileReader.EndOfStream)
                    {
                        string line = fileReader.ReadLine().Trim();
                        var words = line.Split(" ");
                        string propertyName;
                        if (words.Length > 1)
                        {
                            propertyName = words.First();
                            if (propertyName == "MODEL")
                            {
                                return words.Last();
                            }
                            else
                            {
                                continue;
                            }
                        }
                    }
                }
            }
            catch (FileNotFoundException e)
            {
                throw new ApplicationException("Invalid mod - no renderconfig.ini", e);
            }
            throw new ApplicationException("Invalid mod - no model info in renderconfig.ini");
        }
    }
}
