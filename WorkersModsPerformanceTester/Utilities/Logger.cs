using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkersModsPerformanceTester.Utilities
{
    internal class Logger : ILogger, IDisposable
    {
        private StreamWriter _writer;
        private int warningCounter;
        private int errorCounter;

        public Logger()
        {
            _writer = new StreamWriter("log.txt");
            Console.WriteLine("Logger initialized"); 
        }

        public void Dispose()
        {
            _writer.Dispose();
        }

        public void Write(string write)
        {
            Console.WriteLine(DateTime.Now.ToShortTimeString() +"|"+ write);
            try
            {
                _writer.WriteLine(write);
            }
            catch(Exception e)
            {
                Console.WriteLine(DateTime.Now.ToShortTimeString() + "|" + "Unable to write log to file.");
            }
        }

        public void Write(Level level,string write)
        {
            string levelText = "";
            switch (level)
            {
                case Level.Warning: levelText       = "WARN "; warningCounter++; break;
                case Level.Error: levelText         = "ERROR"; errorCounter++;   break;
                case Level.Information: levelText   = "INFO "; break;
            }

            Console.WriteLine(DateTime.Now.ToShortTimeString() + "|" + levelText + "|" + write);
            try
            {
                _writer.WriteLine(write);
            }
            catch (Exception e)
            {
                Console.WriteLine(DateTime.Now.ToShortTimeString() + "|WARN |" + "Unable to write log to file.");
            }
        }
    }
}
