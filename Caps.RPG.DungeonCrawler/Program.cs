using Caps.Util.IO;
using Caps.Util.Lua;
using SNS.Data.DataSerializer.XmlExtensions;
using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Caps.RPG.DungeonCrawler
{
    internal static class Program
    {
        public static readonly string VERSION = "1.0.0";
        public static readonly string GAME_NAME = "DungeonCrawler";

        public static readonly LuaEnvironment LuaEnvironment = new("Caps.RPG.Rules");

        private static void Main()
        {
            // load config
            var gameDataPath = ProgramFilesWrapper.GetGameDataPath(GAME_NAME);
            if (!Directory.Exists(gameDataPath))
            {
                ProgramFilesWrapper.BuildGameDataStructure(gameDataPath);
                File.Copy(Path.Combine(Directory.GetCurrentDirectory(), "game.config"), Path.Combine(gameDataPath, "game.config"));
            }
            var commonConfigPath = Path.Combine(gameDataPath, "game.config");
            CommonConfig config = CommonConfig.LoadConfig(gameDataPath);
            config.GameName = GAME_NAME;


            using var game = new DungeonCrawlerClient(config);
            game.Run();


            // Write config back to the file it was loaded from
            using var writer = new StreamWriter(commonConfigPath, false);
            writer.Write(config.ToXml());
        }
    }
}
