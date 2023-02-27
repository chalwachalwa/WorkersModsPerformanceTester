using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkersModsPerformanceTester
{
    internal class Mod
    {
        private string _workshopconfigPath; 
        private Dictionary<string, string> _modProperties;

        public Mod(string path)
        {
            Models = new List<Model>();
            Folder = path;
            Id = Path.GetFileName(Path.GetDirectoryName(Folder));
            _workshopconfigPath = Path.Combine(path, "workshopconfig.ini");
            Subfolders = Directory.GetDirectories(Folder, "*", SearchOption.AllDirectories);        
        }

        public string Id { get; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string Folder { get; }
        public string[] Subfolders { get; }
        public string WorkshopconfigPath { get; }
        public string[] Warnings { get; set; }
        public List<Model> Models { get; }


        // example of logistics curve
        // https://www.wolframalpha.com/input?i=F%28x%29%3D0.5-0.5*tanh%28%28x-0.985297-3.5%29%2F%282*1.1122%29%29
        private double CalculateScore(Model model)
        {
            const double lodCoeficient = 40;
            const double facesCoeficient = 40;
            const double texturesCoeficient = 20;

            const double vehiclesMeanFaces = 6242.7128;
            const double vehiclesStandardDeviationFaces = 10107.5839;
            const double vehiclesXOffsetFaces = 34188.0;

            const double vehiclesMeanTextures = 0.9852;
            const double vehiclesStandardDeviationTextures = 1.1122;
            const double vehiclesXOffsetTextures = 3.5;

            const double buildingsMeanFaces = 2964.829;
            const double buildingsStandardDeviationFaces = 2594.9566;
            const double buildingsXOffsetFaces = 7415;
                         
            const double buildingsMeanTextures = 0.865;
            const double buildingsStandardDeviationTextures = 0.641;
            const double buildingsXOffsetTextures = 1.7;

            double lodValue = (double)int.Parse(model.LODsCount) / 2;
            double facesValue = 0.0;
            double texturesValue = 0.0;
            if (Type == "WORKSHOP_ITEMTYPE_VEHICLE")
            {
                facesValue = 0.5 - 0.5 * Math.Tanh(((double)int.Parse(model.Faces) - vehiclesXOffsetFaces - vehiclesMeanFaces) / (2 * vehiclesStandardDeviationFaces));
                texturesValue = 0.5 - 0.5 * Math.Tanh(((double)double.Parse(model.TexturesSize) - vehiclesXOffsetTextures - vehiclesStandardDeviationTextures) / (2 * vehiclesStandardDeviationTextures));
            }
            else if(Type == "WORKSHOP_ITEMTYPE_BUILDING")
            {
                facesValue = 0.5 - 0.5 * Math.Tanh(((double)int.Parse(model.Faces) - buildingsXOffsetFaces - buildingsMeanFaces) / (2 * buildingsStandardDeviationFaces));
                texturesValue = 0.5 - 0.5 * Math.Tanh(((double)double.Parse(model.Faces) - buildingsXOffsetTextures - buildingsMeanTextures) / (2 * buildingsStandardDeviationTextures));
            }
            
            return lodCoeficient * lodValue + facesCoeficient * facesValue + texturesCoeficient * texturesValue;
        }
    }
}
