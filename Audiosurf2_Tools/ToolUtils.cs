using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

#if !LINUX
using Microsoft.Win32;
#endif

namespace Audiosurf2_Tools;

public class ToolUtils
{
    public static async Task<string> GetGameDirectoryAsync()
    {
        string directory = "";
#if LINUX
        var userLocation = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        if (Directory.Exists(Path.Combine(userLocation, ".steam/steam/steamapps/common/Audiosurf 2")))
        {
            Console.WriteLine("Found in steam");
            directory = Path.Combine(userLocation, ".steam/steam/steamapps/common/Audiosurf 2");
        }
        else
        {
            //guessing SteamPath on Linux
            var steamLocationConfigPath = Path.Combine(userLocation, "/.steam/steam/config/libraryfolders.vdf");
            if (!File.Exists(steamLocationConfigPath))
            {
                return "";
            }
            try
            {
                var vdfText = await File.ReadAllLinesAsync(steamLocationConfigPath);
                var gameLineText = vdfText.FirstOrDefault(x => x.Contains("\"235800\"")) ??
                    throw new DirectoryNotFoundException("Game isn't listed as installed in Steam");
                var lineIndex = Array.IndexOf(vdfText, gameLineText);
                for (int i = lineIndex - 1; i >= 0; i--)
                {
                    if (vdfText[i].Contains("\"path\""))
                    {
                        vdfText[i] = vdfText[i].Replace("\"path\"", "");
                        var libraryPath = vdfText[i][(vdfText[i].IndexOf('/') + 1)..vdfText[i].LastIndexOf('/')];
                        //libraryPath = libraryPath.Replace("\\\\", "\\");
                        directory = Path.Combine(libraryPath, "steamapps/common/Audiosurf 2");
                        break;
                    }
                }

                if (string.IsNullOrWhiteSpace(directory))
                    throw new DirectoryNotFoundException("Unable to find Audiosurf 2 directory");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
#else   
        if (Directory.Exists("C:\\Program Files (x86)\\Steam\\steamapps\\common\\Audiosurf 2"))
        {
            directory = "C:\\Program Files (x86)\\Steam\\steamapps\\common\\Audiosurf 2";
        }
        else
        {
            try
            {
                //GameID: 235800
                object steamPath =
                    Registry.GetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\Wow6432Node\\Valve\\Steam", "InstallPath", null) ??
                    throw new KeyNotFoundException("Steam installation not found");
                var vdfText = await File.ReadAllLinesAsync(Path.Combine(
                    steamPath.ToString() ?? throw new InvalidOperationException("Invalid Steam install path"),
                    "config\\libraryfolders.vdf"));
                var gameLineText = vdfText.FirstOrDefault(x => x.Contains("\"235800\"")) ??
                                   throw new DirectoryNotFoundException("Game isn't listed as installed in Steam");
                var lineIndex = Array.IndexOf(vdfText, gameLineText);
                for (int i = lineIndex - 1; i >= 0; i--)
                {
                    if (vdfText[i].Contains("\"path\""))
                    {
                        vdfText[i] = vdfText[i].Replace("\"path\"", "");
                        var libraryPath = vdfText[i][(vdfText[i].IndexOf('\"') + 1)..vdfText[i].LastIndexOf('\"')];
                        libraryPath = libraryPath.Replace("\\\\", "\\");
                        directory = Path.Combine(libraryPath, "steamapps\\common\\Audiosurf 2");
                        break;
                    }
                }

                if (string.IsNullOrWhiteSpace(directory))
                    throw new DirectoryNotFoundException("Unable to find Audiosurf 2 directory");
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        if (!string.IsNullOrWhiteSpace(directory))
        {
            if (!Directory.Exists(directory))
                return "";
            var dirInfo = new DirectoryInfo(directory);
            var files = dirInfo.GetFiles();
            if (!files.Any(x => x.Name.Contains("Audiosurf2.exe")))
            {
                return "";
            }
        }
#endif
        
        return directory;
    }
}