using System.Diagnostics;
using System.IO;

namespace WorkersModsPerformanceTester
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello, World!");

            var csvBuilder = new CsvBuilder();
            csvBuilder.SetUpColumns("Mod number", "Mod name\\submod", "Lod files", "Textures size [MB]", "Path", "Warnings");
            
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
                    csvBuilder.AddRow(modNumber,"","","","", modFolder, e.Message);
                    continue;
                }

                if (!modTypes.Contains(modType)) continue;

                var subfolders = Directory.GetDirectories(modFolder);
                foreach(var subfolder in subfolders)
                {
                    var appendSubmod = subfolders.Length > 1;
                     
                    var filesWithLod = Directory.GetFiles(subfolder, "*.nmf").Where(x => x.Contains("LOD", StringComparison.InvariantCultureIgnoreCase)).Count();
                    var texturesSize = Format.ByteSize(Directory.GetFiles(subfolder, "*.dds").Sum(x => new FileInfo(x).Length));


                    csvBuilder.AddRow(modNumber, appendSubmod ? modName + "\\" + Path.GetFileName(Path.GetDirectoryName(subfolder)) : modName
                        , filesWithLod.ToString(), texturesSize, subfolder);
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

        //Thank you stack overflow https://stackoverflow.com/questions/281640/how-do-i-get-a-human-readable-file-size-in-bytes-abbreviation-using-net/4967106#4967106
        public static class Format
        {
            static string[] sizeSuffixes = {
        "B", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };

            public static string ByteSize(long size)
            {
                Debug.Assert(sizeSuffixes.Length > 0);

                const string formatTemplate = "{0}{1:0.#} {2}";

                if (size == 0)
                {
                    return string.Format(formatTemplate, null, 0, sizeSuffixes[0]);
                }

                var absSize = Math.Abs((double)size);
                var fpPower = Math.Log(absSize, 1000);
                var intPower = (int)fpPower;
                var iUnit = intPower >= sizeSuffixes.Length
                    ? sizeSuffixes.Length - 1
                    : intPower;
                var normSize = absSize / Math.Pow(1000, iUnit);

                return string.Format(
                    formatTemplate,
                    size < 0 ? "-" : null, normSize, sizeSuffixes[iUnit]);
            }
        }

    }
}