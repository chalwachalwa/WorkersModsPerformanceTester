using System;
using System.Diagnostics;
using System.IO;
using System.Collections.Generic;

namespace WorkersModsPerformanceTester
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string workshopPath = @"C:\Program Files (x86)\Steam\steamapps\workshop\content\784150";
            string outputPath = "mods.csv";
            bool scrapUsers = true;
            try
            {
                for (int i = 0; i < args.Length; i++)
                {
                    if (args[i] == "--path")
                    {
                        i++;
                        workshopPath = args[i];
                    }
                    else if (args[i] == "--output")
                    {
                        i++;
                        outputPath = args[i];
                    }
                    else if (args[i] == "--nousers")
                    {
                        scrapUsers = false;
                    }
                }
            }catch(IndexOutOfRangeException e)
            {
                Console.WriteLine("Invalid arguments");
            }
            

            Console.WriteLine("Processing...");

            var csvBuilder = new CsvBuilder();
            csvBuilder.SetUpColumns("Mod number", "AuthorId", "AuthorName", "Mod type","Mod name\\submod", "Lod files", "Textures size[MB]", "Faces", "Score", "Path", "Workshop URL", "Warnings");

            using (var progress = new ProgressBar())
            {
                var modProcessor = new ModsProcessor(csvBuilder, progress, workshopPath, scrapUsers);
                modProcessor.Process();
            }
            try
            {
                File.WriteAllText(outputPath, csvBuilder.ToString());
            }
            catch (IOException e)
            {
                Console.WriteLine(e);
            }
        }
    }
}