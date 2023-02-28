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
            var arguments = args.Select((value, index) => new { value, index })
                    .GroupBy(x => x.index / 2, x => x.value);

            string workshopPath = "C:\\Program Files (x86)\\Steam\\steamapps\\workshop\\content\\784150";
            bool scrapUsers = true;
            foreach (var argument in arguments)
            {
                var a = argument.Select(x => x).ToArray();
                if (a[0] == "--path")
                {
                    workshopPath = a[1];
                }
                else if (a[0] == "--nousers")
                {
                    scrapUsers = false;
                }
                else
                {
                    workshopPath = "C:\\Program Files (x86)\\Steam\\steamapps\\workshop\\content\\784150";
                }
            }

            Console.WriteLine("Processing...");

            var csvBuilder = new CsvBuilder();
            csvBuilder.SetUpColumns("Mod number", "AuthorId", "AuthorName", "Mod type","Mod name\\submod", "Lod files", "Textures size[MB]", "Faces", "Score", "Path", "Warnings");

            using (var progress = new ProgressBar())
            {
                var modProcessor = new ModsProcessor(csvBuilder, progress, workshopPath, scrapUsers);
                modProcessor.Process();
            }
            try
            {
                File.WriteAllText("result4.csv", csvBuilder.ToString());
            }
            catch (IOException e)
            {
                Console.WriteLine(e);
            }
        }
    }
}