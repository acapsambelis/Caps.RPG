using Caps.RPG.DungeonCrawler.Scenes;
using Caps.RPG.MonoGame;

namespace Caps.RPG.DungeonCrawler
{
    public class DungeonCrawlerClient : Core
    {
        protected CommonConfig Config;

        public DungeonCrawlerClient(CommonConfig config) : base("Dungeon Crawler")
        {
            Config = config;
        }

        protected override void Initialize()
        {
            base.Initialize();
            ChangeScene(new TitleScene(Config));
        }

        protected override void LoadContent()
        {
            // TODO: use this.Content to load your game content here
            base.LoadContent();
        }
    }
}
