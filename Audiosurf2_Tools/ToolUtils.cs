using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace Audiosurf2_Tools;

public class ToolUtils
{
    public static async Task<string> GetGameDirectoryAsync()
    {
        string directory = "";
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
        return directory;
    }
}