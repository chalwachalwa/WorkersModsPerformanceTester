using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkersModsPerformanceTester
{
    internal class Vehicle : Model
    {
        private readonly string[] _propertiesToRead = new[] { "$JOINT" };
        private string _sciptIniPath;

        public Vehicle(string path, string type)
        {
            PropertiesToRead = _propertiesToRead;
            _sciptIniPath = path;

            var folder = Directory.GetParent(path);
            FolderPath = folder.FullName;
            Name = folder.Name;
            Type = type;

            var scriptProperties = ReadRelatedConfig(_sciptIniPath); // script ini!
            var jointExist = scriptProperties.TryGetValue("$JOINT", out var jointPath);

            var nmfFiles = Directory.GetFiles(FolderPath, "*.nmf");
            NmfPath = nmfFiles.First(x => !x.Contains("joint", StringComparison.InvariantCultureIgnoreCase));

            var materialFiles = Directory.GetFiles(FolderPath, "*.mtl")
                .Select(x => Path.GetFileName(x));

            var selectedMaterialFile = materialFiles.Where(x => !x.Contains("_") && !x.Contains("LOD", StringComparison.InvariantCultureIgnoreCase))
                .Single(x => x != "main.mtl"); //exclude skins and lods

            MaterialPath = Path.Combine(FolderPath, selectedMaterialFile);

            var texturesPaths = GetTexturesRelativePaths(MaterialPath);
            TexturesSize = CountTexturesSize(texturesPaths);

            LODsCount = nmfFiles.Count(x => x.Contains("LOD", StringComparison.InvariantCultureIgnoreCase)).ToString();
            Faces = ReadFacets(NmfPath);
            if (jointExist)
            {
                Faces += ReadFacets(Path.Combine(FolderPath, jointPath));
            }

        }
    }
}
