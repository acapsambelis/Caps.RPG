using Caps.RPG.DungeonCrawler.GameObjects;
using Caps.RPG.MonoGame;
using Caps.RPG.MonoGame.Graphics;
using Caps.RPG.Rules.Creatures;
using Caps.RPG.Rules.Creatures.Classed;
using Caps.RPG.Rules.Maps;
using Caps.Util;
using Caps.Util.Lua;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;

namespace Caps.RPG.DungeonCrawler.Scenes
{
    public class GameScene(CommonConfig config) : BaseScene(config, CameraSceneMode.Panning)
    {
        private readonly TileMap hexMap = HexMap.GenerateRandomMap(0, 3);
        private readonly List<Combattant> combattants = [];
        private readonly Dictionary<string, Sprite> characterSprites = [];
        private Map _map;

        public override void Initialize()
        {
            var characterLoader = new LuaEntityLoader("Characters");
            List<ClassedCharacter> blueTeam = characterLoader.LoadComponentsFromCategory<ClassedCharacter>("BlueTeam");
            List<ClassedCharacter> redTeam = characterLoader.LoadComponentsFromCategory<ClassedCharacter>("RedTeam");

            combattants.AddRange(blueTeam.Select(c => new Combattant(c, TerminalColors.Blue, hexMap.RandomTile(true))));
            combattants.AddRange(redTeam.Select(c => new Combattant(c, TerminalColors.Red, hexMap.RandomTile(true))));

            foreach (Combattant combattant in combattants)
            {
                combattant.HealAll();
            }
            base.Initialize();
        }

        public override void LoadContent()
        {
            base.LoadContent();

            TextureAtlas characterAtlas = TextureAtlas.FromFile(Core.Content, "images/characters-definition.xml");
            foreach (Combattant combattant in combattants)
            {
                TextureRegion region = characterAtlas.GetRegion(combattant.Name + " " + combattant.Team.ToString());
                characterSprites[combattant.Name + " " + combattant.Team.ToString()] = new Sprite(region, new Vector2(3));
            }

            _map = new Map(hexMap, characterSprites);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            _map.Update();
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime, () =>
            {
                Core.SpriteBatch.Begin(SpriteSortMode.Immediate, transformMatrix: Core.Camera.GetTranslation());

                _map.Draw();

                Core.SpriteBatch.End();
            });
        }
    }
}
