using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkersModsPerformanceTester
{
    internal class Mod
    {
        private string _workshopconfigPath; 
        private Dictionary<string, string> _modProperties;


        public string Name { get; }
        public string Id { get; }
        public string Directory { get; }
        public string[] Warnings { get; }

    }
}
