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
        virtual public int LODsCount { get; protected set; }
        virtual public string TexturesSize { get; protected set; }
        virtual public int Vertices { get; protected set; }

    }
}
