using System;
using System.IO;
using SNS.Data.DataSerializer;
using SNS.Data.DataSerializer.XmlExtensions;

namespace Caps.RPG.DungeonCrawler
{
    [DataClass("CommonConfig")]
    public class CommonConfig : IGenericDataObject<CommonConfig>
    {
        private string gameName;
        private string lastSaveFile;
        private int partySize;

        public string GameName
        {
            get => gameName;
            set
            {
                gameName = value;
                GameDataDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), gameName);
            }
        }
        public string GameDataDirectory { get; internal set; }
        public string ScreenshotsDirectory => Path.Combine(GameDataDirectory, "Screenshots");
        public string SavesDirectory => Path.Combine(GameDataDirectory, "Saves");
        public string ConfigsDirectory => Path.Combine(GameDataDirectory, "Configs");

        [DataProperty("LastSaveFile")]
        public string LastSaveFile
        {
            get => lastSaveFile;
            set => lastSaveFile = value;
        }

        [DataProperty("PartySize")]
        public int PartySize
        {
            get { return partySize; }
            set { partySize = value; }
        } 


        private bool wasLoaded;
        public bool WasLoaded { get => wasLoaded; set => wasLoaded = value; }

        public CommonConfig() { }

        public static CommonConfig LoadConfig(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentException("Config path cannot be null or empty.", nameof(path));

            string xmlData = File.ReadAllText(Path.Combine(path, "game.config"));
            return Xml<CommonConfig>.LoadOneFromXml(xmlData) ?? throw new InvalidOperationException($"Failed to load CommonConfig from '{path}'.");
        }
    }
}
