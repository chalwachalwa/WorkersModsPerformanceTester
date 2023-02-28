# WorkersModsPerformanceTester

Hello this is comunity project, join our modders discord: https://discord.gg/gaREV5AU
Reporting issues https://github.com/chalwachalwa/WorkersModsPerformanceTester/issues

How to use: 
  - Select latest release https://github.com/chalwachalwa/WorkersModsPerformanceTester/tags
  - Download 
  - Unzip
  - Run .exe
  - Open .csv file with spreadsheet like Libre Office, MS Excel or Google docs
  - Order records by selected column 

To run console parameters use cmd.exe or open windows right mouse button menu on .exe file, go to properties and set arguments in file name textbox.

For custom path use:
--path "YOUR_CUSTOM_PATH\Steam\steamapps\workshop\content\784150"

To process without scraping users from external site use:
--nousers

How to build from source code:
- Download and install .NET SDK (at least 6.0) - https://dotnet.microsoft.com/en-us/download
- Select working directory with .sln file in cmd.exe 
- type "dotnet build"
