using System.IO;

namespace WorkersModsPerformanceTester
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello, World!");

            var csvBuilder = new CsvBuilder();
            csvBuilder.SetUpColumns("Mod number", "Mod name\\submod", "Lod files", "Path", "Warnings");
            
            var workshopPath = "C:\\Program Files (x86)\\Steam\\steamapps\\workshop\\content\\784150";
            var modFolders = Directory.GetDirectories(workshopPath);

            int i = 1;

            // map      - WORKSHOP_ITEMTYPE_LANDSCAPE
            // vehicle  - WORKSHOP_ITEMTYPE_VEHICLE
            // building - WORKSHOP_ITEMTYPE_BUILDING
            var modTypes = new[] { "WORKSHOP_ITEMTYPE_BUILDING", "WORKSHOP_ITEMTYPE_VEHICLE" };

            foreach (var modFolder in modFolders)
            {
                Console.WriteLine($"{i++}/{modFolders.Length}");
                var modNumber = Path.GetFileName(Path.GetDirectoryName(modFolder));
                string modName;
                string modType;
                
                try
                {
                    modName = GetModName(modFolder, out modType);
                }
                catch(ApplicationException e)
                {
                    csvBuilder.AddRow(modNumber,"","","", modFolder, e.Message);
                    continue;
                }

                if (!modTypes.Contains(modType)) continue;

                var subfolders = Directory.GetDirectories(modFolder);
                foreach(var subfolder in subfolders)
                {
                    var appendSubmod = subfolders.Length < 1;
                     
                    var filesWithLod = Directory.GetFiles(subfolder, "*.nmf").Where(x => x.Contains("LOD", StringComparison.InvariantCultureIgnoreCase)).Count();

                    csvBuilder.AddRow(modNumber, modName, filesWithLod.ToString(), subfolder);
                };
            }

            Console.Write(csvBuilder.ToString());
        }

        private static string GetModName(string modPath, out string modType)
        {
            string modTypeValue = null;

            string configPath = Path.Combine(modPath, "workshopconfig.ini");
            try
            {
                using (var fileReader = new StreamReader(configPath))
                {
                    while (!fileReader.EndOfStream)
                    {
                        var line = fileReader.ReadLine();
                        if (line.Contains("$ITEM_TYPE"))
                        {
                            modTypeValue = line.Replace("\"", "").Replace("$ITEM_TYPE", "").Trim();
                        }

                        if (line.Contains("$ITEM_NAME"))
                        {
                            modType = string.IsNullOrEmpty(modTypeValue) ? "Invalid" : modTypeValue;
                            return line.Replace("\"", "").Replace("$ITEM_NAME", "").Trim();
                        }
                    }
                }
            }
            catch(FileNotFoundException e)
            {
                throw new ApplicationException("Invalid mod - no workshopconfig.ini", e);
            }
            throw new ApplicationException("Invalid mod - no name");
        }
    }
}