# WorkersModsPerformanceTester

Hello! This is comunity project, join our modders discord: https://discord.gg/gaREV5AU <br />
Reporting issues https://github.com/chalwachalwa/WorkersModsPerformanceTester/issues

This tool is running analysis on mods files in your library. It can provide informations about faces of model, number of LODs, textures size.  <br />
Scoring 0-100: 
 - 20% one LOD, 40% two LODs, 
 - next 40% - faces  
 - last 20% - textures size 
<br /><br />
Faces and textures score based on logistic curves adjusted on sampling 80% of 253 random vehilces models and 80% of 730 random buildings models; <br />
Example  https://www.wolframalpha.com/input?i=F%28x%29%3D0.5-0.5*tanh%28%28x-0.865-1.7%29%2F%282*0.641%29%29 


# How to use: 
  - Select latest release https://github.com/chalwachalwa/WorkersModsPerformanceTester/tags 
  - Download <br />
  - Unzip <br />
  - Shift + Right Mouse Button on .exe file -> copy as path
  - Click (windows) + R
  - Type: cmd.exe
  - Paste + (optional: add arguments)
  - Click enter
  - If you get information that .NET 6.0 runtime is required please download, install and retry: https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/runtime-6.0.14-windows-x64-installer
  - If you get any other prompt with unexpected error please copy and send it to me
  - Open .csv file with spreadsheet like Libre Office, MS Excel or Google docs
  - Order records by selected column 

To run console parameters use cmd.exe or follow this tutorial: https://www.digitalcitizen.life/shortcut-arguments-parameters-windows/

For custom mods path use:
--path "YOUR_CUSTOM_PATH\Steam\steamapps\workshop\content\784150"

For custom output file path:
--output "YOUR_CUSTOM_FILE.csv"

To process without scraping users from external site use:
--nousers

# How to build from source code:
- Download source code on latest release page
- Download and install .NET SDK (at least 6.0) - https://dotnet.microsoft.com/en-us/download
- Select working directory with .sln file in cmd.exe 
- type "dotnet build"

# License
MIT <br />
Feel free to fork, modify, distribute. <br />
No guarantee. 
