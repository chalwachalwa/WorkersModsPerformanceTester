using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkersModsPerformanceTester
{
    internal class Model
    {
        virtual public string FolderPath { get; protected set; }
        virtual public string Name { get; protected set; }
        virtual public string NmfPath { get; protected set; }
        virtual public string MaterialPath { get; protected set; }
        virtual public string LODsCount { get; protected set; }
        virtual public string TexturesSize { get; protected set; }
        virtual public string Vertices { get; protected set; }

        virtual public string[] PropertiesToRead { get; protected set; }

        protected virtual Dictionary<string, string> ReadRelatedConfig(string path)
        {
            var result = new Dictionary<string, string>();
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
                            if (PropertiesToRead.Contains(propertyName))
                            {
                                result.Add(propertyName, words.First(x => x.Contains(".")));
                            }
                        }
                    }
                }
            }
            catch (FileNotFoundException e)
            {
                throw new ApplicationException("Invalid mod - no renderconfig.ini", e);
            }
            return result;
        }

        protected virtual string CountTexturesSize(Dictionary<string, string> texturesPaths)
        {
            var filteredTextures = texturesPaths.Where(x => x.Key.Contains("$TEXTURE"))
                .Where(x => !x.Value.Contains("blankspecular.dds") && !x.Value.Contains("blankbump.dds"))
                .Select(x => x.Value)
                .Distinct()
                .ToArray();

            var pathsToTextures = filteredTextures.Select(x => Path.Combine(Directory.GetParent(MaterialPath).FullName, x));
            long sum = 0;
            foreach (var path in pathsToTextures)
            {
                try
                {
                    sum += new FileInfo(path).Length;
                }
                catch (FileNotFoundException e)
                {
                    ; // handle case that someone reference implicitly \media_soviet\buildings
                      // program treats all paths as relative to .mtl path
                      // todo: should I count it?
                }
            }
            return sum.GetHumanReadableFileSize();
        }

        protected readonly string[] texturesPropertiesToRead = new[] { "$TEXTURE_MTL", "$TEXTURE" };
        protected virtual Dictionary<string, string> GetTexturesRelativePaths(string path)
        {
            var results = new Dictionary<string, string>();

            try
            {
                using (var fileReader = new StreamReader(path))
                {
                    var currentSubmaterial = "";
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
                        if (propertyName == "$SUBMATERIAL")
                        {
                            currentSubmaterial = words.Last();
                            continue;
                        }
                        if (texturesPropertiesToRead.Contains(propertyName))
                        {
                            var textureNumber = "";
                            if (words.Length == 3)
                            {
                                textureNumber = words[1];
                            }
                            results.TryAdd($"{currentSubmaterial}_{propertyName}{textureNumber}", words.Last());
                        }
                    }
                }
            }
            catch (FileNotFoundException e)
            {
                throw new ApplicationException("Invalid mod - no workshopconfig.ini", e);
            }
            return results;
        }

        protected static int ReadVertices(string path)
        {
            int  debugMax = 0;
            using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                using (var br = new BinaryReader(fs))
                {
                    // header
                    var buffer = br.ReadBytes(20);
                    var numMaterials = BitConverter.ToInt32(buffer, 8);
                    var numNodes = BitConverter.ToInt32(buffer, 12);

                    if (numNodes > debugMax) debugMax = numNodes;
                    Console.WriteLine(debugMax);

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
    }
}
