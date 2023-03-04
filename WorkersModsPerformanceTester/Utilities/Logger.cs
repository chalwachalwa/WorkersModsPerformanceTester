using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkersModsPerformanceTester.Utilities
{
    internal class Logger : ILogger
    {
        private int warningCounter;
        private int errorCounter;

        public Logger()
        {
            Console.WriteLine("Logger initialized"); 
        }

        public void Write(string write)
        {
            Console.WriteLine(DateTime.Now.ToLongTimeString() +"|"+ write);
            try
            {
                using(var writer = new StreamWriter("log.txt", append: true))
                {
                    writer.WriteLine(write);
                }
            }
            catch(Exception e)
            {
                Console.WriteLine(DateTime.Now.ToLongTimeString() + "|" + "Unable to write log to file.");
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

            var log = DateTime.Now.ToLongTimeString() + "|" + levelText + "|" + write;

            Console.WriteLine(log);
            try
            {
                using (var writer = new StreamWriter("log.txt", append: true))
                {
                    writer.WriteLine(log);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(DateTime.Now.ToLongTimeString() + "|WARN |" + "Unable to write log to file.");
            }
        }
    }
}
