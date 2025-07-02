using Caps.RPG.DungeonCrawler.UI;
using Caps.RPG.MonoGame;
using Caps.RPG.MonoGame.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGameGum;
using MonoGameGum.Forms.Controls;

namespace Caps.RPG.DungeonCrawler.Scenes
{
    public class LoadDungeonScene : BaseScene
    {
        private const string _tableTitle = "Available Dungeons";
        private Vector2 _tableTitlePosition;
        private Vector2 _tableTitleOrigin;

        private Panel _mainPanel;

        private string[] dungeonFiles = [];
        private ListBox _dungeonListBox;

        public override void Initialize()
        {
            // LoadContent is called during base.Initialize().
            base.Initialize();

            // load filenames
            dungeonFiles = System.IO.Directory.GetFiles(
                System.AppDomain.CurrentDomain.BaseDirectory,
                "*.dungeon",
                System.IO.SearchOption.TopDirectoryOnly
            );

            Vector2 size = Font.MeasureString(_tableTitle);
            _tableTitlePosition = new Vector2(640, 200);
            _tableTitleOrigin = size * 0.5f;

            _mainPanel = new Panel();
            _mainPanel.Dock(Gum.Wireframe.Dock.Fill);
            _mainPanel.AddToRoot();

            // Create the list box for displaying dungeon files.
            _dungeonListBox = new ListBox
            {
                Width = 400,
                Height = 300,
                X = 640,
                Y = 300,
            };
            _mainPanel.AddChild(_dungeonListBox);

            // populate listbox items
            foreach (var file in dungeonFiles)
            {
                string fileName = System.IO.Path.GetFileNameWithoutExtension(file);
                _dungeonListBox.Items.Add(fileName);
            }
        }

        public override void LoadContent()
        {

        }

        public override void Update(GameTime gameTime)
        {
            GumService.Default.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            // Begin the sprite batch to prepare for rendering.
            Core.SpriteBatch.Begin(samplerState: SamplerState.PointClamp);

            Core.SpriteBatch.DrawString(Font, _tableTitle, _tableTitlePosition, Color.White, 0.0f, _tableTitleOrigin, 1.0f, SpriteEffects.None, 1.0f);



            // Always end the sprite batch when finished.
            Core.SpriteBatch.End();

            GumService.Default.Draw();
        }
    }
}
