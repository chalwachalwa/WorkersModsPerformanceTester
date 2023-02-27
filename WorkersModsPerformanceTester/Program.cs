using System;
using System.Diagnostics;
using System.IO;
using System.Collections.Generic;

namespace WorkersModsPerformanceTester
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Processing...");

            var csvBuilder = new CsvBuilder();
            csvBuilder.SetUpColumns("Mod number", "Mod name\\submod", "Lod files", "Textures size[MB]", "Vertices", "Path", "Warnings");

            var workshopPath = "C:\\Program Files (x86)\\Steam\\steamapps\\workshop\\content\\784150";
            var modFolders = Directory.GetDirectories(workshopPath);

            int i = 0;

            // map      - WORKSHOP_ITEMTYPE_LANDSCAPE
            // vehicle  - WORKSHOP_ITEMTYPE_VEHICLE
            // building - WORKSHOP_ITEMTYPE_BUILDING
            var modTypes = new[] { "WORKSHOP_ITEMTYPE_BUILDING", "WORKSHOP_ITEMTYPE_VEHICLE" };

            using (var progress = new ProgressBar())
            {
                foreach (var modFolder in modFolders)
                {
                    progress.Report((double)i++ / modFolders.Length);

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
                        csvBuilder.AddRow(modNumber, "", "", "", "", modFolder, "Mod invalid - missing property in workshopconfig.ini");
                        continue;
                    }
                    catch (ApplicationException e)
                    {
                        csvBuilder.AddRow(modNumber, "", "", "", "", modFolder, e.Message);
                        continue;
                    }

                    if (!modTypes.Contains(modType)) continue;

                    var subfolders = Directory.GetDirectories(modFolder, "*", SearchOption.AllDirectories);
                    int x = 0;
                    foreach (var subfolder in subfolders)
                    {
                        var files = Directory.GetFiles(subfolder);
                        if (!files.Any()) continue; // empty subfolder or models are nested

                        //if (Directory.GetParent(subfolder).Name == "784150")
                        //    continue;

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
                            catch(ApplicationException e)
                            {
                                csvBuilder.AddRow(modNumber, "", "", "", "", modFolder, e.Message);
                                continue;
                            }
                            
                            if(modelPathRelative.Contains("../"))
                            {
                                modelPath = Path.GetFullPath(Path.Combine(subfolder, modelPathRelative));
                                // todo: maybe select that as variant to avoid duplicates?
                            }
                            else
                            {
                                csvBuilder.AddRow(modNumber, "", "", "", "", modFolder, "Mod invalid - No model");
                                continue;
                            }
                        }
                        var vertices = ReadVertices(modelPath).ToString();

                        var appendSubmod = subfolders.Length > 1;
                        var name = appendSubmod ? modName + "\\" + new DirectoryInfo(Path.GetDirectoryName(subfolder + "\\")).Name : modName;
                        csvBuilder.AddRow(modNumber, name, filesWithLod.Count().ToString(), texturesSize, vertices, subfolder);
                    };
                }
            }
            //Console.Write(csvBuilder.ToString());
            try
            {
                File.WriteAllText("result.csv", csvBuilder.ToString());
            }
            catch (IOException e)
            {
                Console.WriteLine(e);
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
    
    public static class Format
    {
        public static string GetHumanReadableFileSize(this long size)
        {  
            var normSize = size / Math.Pow(1024, 2);
            return String.Format("{0:0.000}", normSize); 
        }
    }
}