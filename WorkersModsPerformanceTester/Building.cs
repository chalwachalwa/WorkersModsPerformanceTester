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
        private readonly string[] _propertiesToRead = new[] { "MODEL", "MATERIAL", "MODEL_LOD", "MODEL_LOD2" };
        private string _renderconfigPath;

        public Building(string path, string type)
        {
            PropertiesToRead = _propertiesToRead;

            _renderconfigPath = path;
            
            var folder = Directory.GetParent(path);
            FolderPath = folder.FullName;
            Name = folder.Name;
            Type = type;
            
            var renderProperties = ReadRelatedConfig(_renderconfigPath);
            NmfPath = Path.Combine(FolderPath, renderProperties["MODEL"]);
            MaterialPath = Path.Combine(FolderPath, renderProperties["MATERIAL"]);
            
            var texturesPaths = GetTexturesRelativePaths(MaterialPath);
            TexturesSize = CountTexturesSize(texturesPaths);

            LODsCount = renderProperties.Count(x => x.Key.Contains("LOD", StringComparison.InvariantCultureIgnoreCase));
            Faces = ReadFacets(NmfPath);
        } 
    }
}
