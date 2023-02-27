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

    }
}
