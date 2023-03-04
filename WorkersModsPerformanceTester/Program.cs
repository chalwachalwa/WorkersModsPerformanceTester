using System;
using System.Diagnostics;
using System.IO;
using System.Collections.Generic;
using WorkersModsPerformanceTester.Utilities;
using Unity;
using System.ComponentModel;

namespace WorkersModsPerformanceTester
{
    internal class Program
    {
        public static Settings Settings = new Settings()
        {
            WorkshopPath = @"C:\Program Files (x86)\Steam\steamapps\workshop\content\784150",
            OutputPath = "mods.csv",
            ScrapUsers = true
        };
        public static Logger Logger = new Logger();

        static void Main(string[] args)
        {
            ParseArguments(args);

            Console.WriteLine("Processing...");

            var csvBuilder = new CsvBuilder();
            csvBuilder.SetUpColumns("Mod number", "AuthorId", "AuthorName", "Mod type","Mod name\\submod", "Lod files", "Textures size[MB]", "Faces", "Score", "Path", "Workshop URL", "Warnings");

            using (var progress = new ProgressBar())
            {
                var modProcessor = new ModsProcessor(csvBuilder, progress);
                modProcessor.Process();
            }
            try
            {
                File.WriteAllText(Settings.OutputPath, csvBuilder.ToString());
            }
            catch (IOException e)
            {
                Console.WriteLine(e);
            }
        }

        private static void ParseArguments(string[] args)
        {
            try
            {
                for (int i = 0; i < args.Length; i++)
                {
                    if (args[i] == "--path")
                    {
                        i++;
                        Settings.WorkshopPath = args[i];
                    }
                    else if (args[i] == "--output")
                    {
                        i++;
                        Settings.OutputPath = args[i];
                    }
                    else if (args[i] == "--nousers")
                    {
                        Settings.ScrapUsers = false;
                    }
                }
            }
            catch (IndexOutOfRangeException e)
            {
                Console.WriteLine("Invalid arguments");
            }
        }
    }
}