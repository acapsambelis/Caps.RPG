using Caps.Util.IO;
using SNS.Data.DataSerializer.XmlExtensions;
using System.IO;

namespace Caps.RPG.DungeonCrawler
{
    internal static class Program
    {
        public static readonly string VERSION = "1.0.0";
        public static readonly string GAME_NAME = "DungeonCrawler";

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
            //using var game = new GeonBitUI_Examples();
            game.Run();


            // Write config back to the file it was loaded from
            using var writer = new StreamWriter(commonConfigPath, false);
            writer.Write(config.ToXml());
        }
    }
}
