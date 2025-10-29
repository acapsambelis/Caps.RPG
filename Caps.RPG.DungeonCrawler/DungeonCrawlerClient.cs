using Caps.RPG.DungeonCrawler.GameObjects;
using Caps.RPG.DungeonCrawler.Scenes;
using Caps.RPG.MonoGame;
using Caps.RPG.MonoGame.Graphics;
using Microsoft.Xna.Framework;

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

            TextureAtlas tileAtlas = TextureAtlas.FromFile(Content, "images/tiles-definition.xml");
            TextureRegion grassland = tileAtlas.GetRegion("grassland");
            Map.Grassland = new Sprite(grassland, new Vector2(3));
            TextureRegion rock = tileAtlas.GetRegion("rock");
            CommonTileFeatures.Rock = new Sprite(rock, new Vector2(3));
            TextureRegion highlight = tileAtlas.GetRegion("highlight");
            CommonTileFeatures.Highlight = new Sprite(highlight, new Vector2(3));
        }
    }
}
