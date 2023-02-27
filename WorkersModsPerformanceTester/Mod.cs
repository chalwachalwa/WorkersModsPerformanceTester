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
        public string AuthorId { get; set; }
        public string Type { get; set; }
        public string Folder { get; }
        public string[] Subfolders { get; }
        public string WorkshopconfigPath { get; }
        public string[] Warnings { get; set; }
        public List<Model> Models { get; }


        // example of logistics curve
        // https://www.wolframalpha.com/input?i=F%28x%29%3D0.5-0.5*tanh%28%28x-0.985297-3.5%29%2F%282*1.1122%29%29
        
    }
}
