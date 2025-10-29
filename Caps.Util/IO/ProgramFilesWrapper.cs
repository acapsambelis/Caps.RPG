using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Caps.Util.IO
{
    public class ProgramFilesWrapper
    {
        //public static string GetProgramFilesPath()
        //{
        //    if (Environment.OSVersion.Platform == PlatformID.Win32NT)
        //        return Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
        //    else
        //        throw new NotSupportedException("Other operating systems are not supported.");
        //}

        //public static string GetProgramFilesX86Path()
        //{
        //    if (Environment.OSVersion.Platform == PlatformID.Win32NT)
        //        return Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
        //    else
        //        throw new NotSupportedException("Other operating systems are not supported.");
        //}

        //public static string GetTempPath()
        //{
        //    return Path.GetTempPath();
        //}

        public static string GetAppDataPath()
        {
            return Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        }

        public static void BuildGameDataStructure(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            string screenshotsPath = Path.Combine(path, "Screenshots");
            if (!Directory.Exists(screenshotsPath))
            {
                Directory.CreateDirectory(screenshotsPath);
            }
            string savesPath = Path.Combine(path, "Saves");
            if (!Directory.Exists(savesPath))
            {
                Directory.CreateDirectory(savesPath);
            }
            string configsPath = Path.Combine(path, "Configs");
            if (!Directory.Exists(configsPath))
            {
                Directory.CreateDirectory(configsPath);
            }
        }

        public static string GetGameDataPath(string gameName)
        {
            string appDataPath = GetAppDataPath();
            string gameDataPath = Path.Combine(appDataPath, gameName);
            BuildGameDataStructure(gameDataPath);
            return gameDataPath;
        }

        public static string GetScreenshotsPath(string gameName)
        {
            return Path.Combine(GetGameDataPath(gameName), "Screenshots");
        }

        public static string GetSavesPath(string gameName)
        {
            return Path.Combine(GetGameDataPath(gameName), "Saves");
        }

        public static string GetConfigsPath(string gameName)
        {
            return Path.Combine(GetGameDataPath(gameName), "Configs");
        }
    }
}
