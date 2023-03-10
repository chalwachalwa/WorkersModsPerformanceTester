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

            var selectedMaterialFiles = materialFiles.Where(x => !x.Contains("_") && !x.Contains("LOD", StringComparison.InvariantCultureIgnoreCase)).ToArray();  //exclude skins and lods
            string selectedMaterialFile = "";

            if (!selectedMaterialFiles.Any())
            {
                var exception = new ApplicationException("No .mtl file for model {NmfPath}");
                exception.Data.Add("folder", FolderPath);
                exception.Data.Add("name", Name);
                throw exception;
            }
            else if(selectedMaterialFiles.Length > 1)
            {
                foreach(var file in selectedMaterialFiles)
                {
                    var firstLine = "";
                    using(var sr = new StreamReader(file))
                    {
                        firstLine = sr.ReadLine();
                    }
                    if(!firstLine.Contains("blender", StringComparison.InvariantCultureIgnoreCase))
                    {
                        selectedMaterialFile = file;
                        break;
                    }
                }
                if(selectedMaterialFile == "")
                {
                    var exception =  new ApplicationException("Cannot find material file");
                    exception.Data.Add("folder", FolderPath);
                    exception.Data.Add("name", Name);
                    throw exception;
                }


                // case if somebody pasted blender main.mtl
                //selectedMaterialFiles = selectedMaterialFiles.Where(x => x != "main.mtl").ToArray(); 
                //if (selectedMaterialFiles.Length > 1)
                //{
                //    Console.WriteLine($"Can't match .mtl file for model {NmfPath} from selected:");
                //    foreach (var name in selectedMaterialFiles) Console.WriteLine(name);
                //    TexturesSize = 0;
                //    LODsCount = -1;
                //    Faces = -1;
                //    return;
                //}
            }
            selectedMaterialFile = selectedMaterialFiles.First();

            MaterialPath = Path.Combine(FolderPath, selectedMaterialFile);

            var texturesPaths = GetTexturesRelativePaths(MaterialPath);
            TexturesSize = CountTexturesSize(texturesPaths);

            LODsCount = nmfFiles.Count(x => x.Contains("LOD", StringComparison.InvariantCultureIgnoreCase));
            Faces = ReadFacets(NmfPath);
            if (jointExist)
            {
                Faces += ReadFacets(Path.Combine(FolderPath, jointPath));
            }

        }
    }
}
