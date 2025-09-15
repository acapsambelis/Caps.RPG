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
            //_point = CreateTexture(Core.GraphicsDevice, 8, 8, Color.White);

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
                Matrix transform = Core.Camera.GetTranslation();
                Core.SpriteBatch.Begin(SpriteSortMode.Immediate, transformMatrix: transform);

                _map.Draw();

                //draw the world and camera coordinates
                //DrawWorldAndCameraCoordinates();
                Core.SpriteBatch.End();
            });
        }

        private Texture2D _point;

        private void DrawWorldAndCameraCoordinates()
        {
            Core.SpriteBatch.Draw(_point, Vector2.Zero - Vector2.One * 4, Color.White);
            Core.SpriteBatch.DrawString(Font, "World center: (0,0)", new Vector2(15, -12), Color.White);
            Core.SpriteBatch.Draw(_point, Core.Camera.CameraCenter - Vector2.One * 4, Color.Red);
            Core.SpriteBatch.DrawString(Font, $"Camera center: ({Core.Camera.CameraCenter.X:0.},{Core.Camera.CameraCenter.Y:0.})", Core.Camera.CameraCenter + Vector2.One * 20, Color.Red);
        }

        //Helpermethod for creating a texture of a specified size and color
        private static Texture2D CreateTexture(GraphicsDevice device, int width, int height, Color color)
        {
            //initialize a texture
            Texture2D texture = new Texture2D(device, width, height);

            //the array holds the color for each pixel in the texture
            Color[] data = new Color[width * height];
            for (int pixel = 0; pixel < data.Count(); pixel++)
            {
                //the function applies the color according to the specified pixel
                data[pixel] = color;
            }

            //set the color
            texture.SetData(data);

            return texture;
        }
    }
}
