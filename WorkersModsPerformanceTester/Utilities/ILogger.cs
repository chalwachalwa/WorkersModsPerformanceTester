using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkersModsPerformanceTester.Utilities
{
    public enum Level
    {
        Error,
        Warning,
        Information
    }

    public interface ILogger
    {
        void Write(string write);
        void Write(Level level, string message);
        //void Write(Level level, Exception e, string message);
    }
}
