using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkersModsPerformanceTester
{
    internal class Building : Model
    {
        private string _renderconfigPath;

        public Building(string path)
        {
            _renderconfigPath = path;
            
            var folder = Directory.GetParent(path);
            FolderPath = folder.FullName;
            Name = folder.Name;
            
            var renderProperties = ReadRelatedConfig(_renderconfigPath);
            NmfPath = Path.Combine(FolderPath, renderProperties["MODEL"]);
            MaterialPath = Path.Combine(FolderPath, renderProperties["MATERIAL"]);
            
            var texturesPaths = GetTexturesRelativePaths(MaterialPath);
            TexturesSize = CountTexturesSize(texturesPaths);

            LODsCount = renderProperties.Count(x => x.Key.Contains("LOD", StringComparison.InvariantCultureIgnoreCase)).ToString();
            Vertices = ReadVertices(NmfPath).ToString();
        }

        private readonly string[] propertiesToRead = new[] { "MODEL", "MATERIAL", "MODEL_LOD", "MODEL_LOD2" };

        private Dictionary<string,string> ReadRelatedConfig(string path)
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
                            if (propertiesToRead.Contains(propertyName))
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

        private string CountTexturesSize(Dictionary<string, string> texturesPaths)
        {
            var filteredTextures = texturesPaths.Where(x => x.Key.Contains("$TEXTURE"))
                .Where(x => !x.Value.Contains("blankspecular.dds") && !x.Value.Contains("blankbump.dds"))
                .Select(x => x.Value)
                .Distinct()
                .ToArray();
            string a;
            
            var pathsToTextures = filteredTextures.Select(x => Path.Combine(Directory.GetParent(MaterialPath).FullName, x));
            long sum = 0;
            foreach (var path in pathsToTextures)
            {
                try
                {
                    sum += new FileInfo(path).Length;
                }
                catch(FileNotFoundException e)
                {
                    ; // handle case that someone reference implicitly \media_soviet\buildings
                      // program treats all paths as relative to .mtl path
                      // todo: should I count it?
                }
            }
            return sum.GetHumanReadableFileSize();
        }

        private readonly string[] texturesPropertiesToRead = new[] { "$TEXTURE_MTL", "$TEXTURE" };
        private Dictionary<string, string> GetTexturesRelativePaths(string path)
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
                        if(propertyName == "$SUBMATERIAL")
                        {
                            currentSubmaterial = words.Last();
                            continue;
                        }
                        if (texturesPropertiesToRead.Contains(propertyName)) 
                        {
                            var textureNumber = "";
                            if(words.Length == 3)
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
    }
}
