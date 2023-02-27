using System;
using System.Diagnostics;
using System.IO;
using System.Collections.Generic;

namespace WorkersModsPerformanceTester
{
    internal class Program
    {
        // map      - WORKSHOP_ITEMTYPE_LANDSCAPE
        // vehicle  - WORKSHOP_ITEMTYPE_VEHICLE
        // building - WORKSHOP_ITEMTYPE_BUILDING
        

        static void Main(string[] args)
        {
            Console.WriteLine("Processing...");

            var csvBuilder = new CsvBuilder();
            csvBuilder.SetUpColumns("Mod number", "AuthorId", "Mod type","Mod name\\submod", "Lod files", "Textures size[MB]", "Faces", "Score", "Path", "Warnings");

            using (var progress = new ProgressBar())
            {
                var modProcessor = new ModsProcessor(csvBuilder, progress);
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