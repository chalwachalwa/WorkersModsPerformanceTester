﻿using System;
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

        protected static int ReadFacets(string path)
        {
            int  debugMax = 0;
            var faces = 0;
            using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                using (var br = new BinaryReader(fs))
                {
                    // header
                    var header = Encoding.ASCII.GetString(br.ReadBytes(8));
                    var numMaterials = BitConverter.ToInt32(br.ReadBytes(4), 0);
                    var numNodes = BitConverter.ToInt32(br.ReadBytes(4), 0);
                    var sizeTotal =  BitConverter.ToInt32(br.ReadBytes(4), 0);

                    // materials
                    for (int i = 0; i < numMaterials; i++)
                    {
                        var materialName = Encoding.UTF8.GetString(br.ReadBytes(64));
                    }

                    for (int i = 0; i < numNodes; i++)
                    {
                        // node type
                        var nodeType = BitConverter.ToInt32(br.ReadBytes(4), 0);

                        if (nodeType == 0)
                        {
                            var modelSize = BitConverter.ToInt32(br.ReadBytes(4), 0);
                            var modelName = Encoding.UTF8.GetString(br.ReadBytes(64));
                            var modelParentId = BitConverter.ToInt16(br.ReadBytes(2), 0);
                            var modelNumChilds = BitConverter.ToInt16(br.ReadBytes(2), 0);
                            br.ReadBytes(152);

                            //buffer = br.ReadBytes(228);
                            var modelNumLODs = BitConverter.ToInt32(br.ReadBytes(4), 0);
                            //if (numLods != 1) throw new ApplicationException();
                            for (int j = 0; j < modelNumLODs; j++)
                            {
                                var modelLODSize = BitConverter.ToInt32(br.ReadBytes(4), 0);
                                var numVertices = BitConverter.ToInt32(br.ReadBytes(4), 0);
                                var numIndices = BitConverter.ToInt32(br.ReadBytes(4), 0);
                                faces += numIndices / 3;
                                var numSubsets = BitConverter.ToInt32(br.ReadBytes(4), 0);
                                var numMorphTargets = BitConverter.ToInt32(br.ReadBytes(4), 0);
                                var vertexAttributes = BitConverter.ToInt32(br.ReadBytes(4), 0);
                                var morphMask = BitConverter.ToInt32(br.ReadBytes(4), 0);
                                br.ReadBytes(2 * numIndices);
                                br.ReadBytes(12 * numVertices);
                                
                                if ((vertexAttributes & (1 << 3)) == (1 << 3))
                                {
                                    br.ReadBytes(12 * numVertices);
                                }
                                if ((vertexAttributes & (1 << 4)) == (1 << 4))
                                {
                                    br.ReadBytes(12 * numVertices);
                                }
                                if ((vertexAttributes & (1 << 5)) == (1 << 5))
                                {
                                    br.ReadBytes(12 * numVertices);
                                }
                                if ((vertexAttributes & (1 << 8)) == (1 << 8))
                                {
                                    br.ReadBytes(8 * numVertices);
                                }
                                if ((vertexAttributes & (1 << 16)) == (1 << 16))
                                {
                                    br.ReadBytes(12 * numVertices);
                                }
                                if ((vertexAttributes & (1 << 18)) == (1 << 18))
                                {
                                    br.ReadBytes(10 * 4 * numIndices / 3);
                                }



                                br.ReadBytes(12 * numSubsets);
                            }
                            

                        }
                        else if (nodeType == 1)
                        {
                            br.ReadBytes(288);
                        }
                        else if(nodeType == 2)
                        {
                            debugMax++;
                        }else
                        {
                            throw new ApplicationException("Node type 2 unhandled");
                        }
                    }
                }
            }
            return vertices;
        }
    }
}
