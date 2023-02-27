using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkersModsPerformanceTester
{
    public static class Format
    {
        public static string GetHumanReadableFileSize(this long size)
        {
            var normSize = size / Math.Pow(1024, 2);
            return String.Format("{0:0.000}", normSize);
        }
    }
}
