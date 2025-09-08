using Caps.RPG.MonoGame;
using Caps.RPG.MonoGame.Graphics;
using GeonBit.UI;
using GeonBit.UI.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Caps.RPG.DungeonCrawler.Scenes
{
    public class LoadDungeonScene(CommonConfig config) : BaseScene(config)
    {
        private string[] dungeonFiles = [];

        public override void Initialize()
        {
            base.Initialize();
            Core.ExitOnEscape = true;
            int windowWidth = Core.GraphicsDevice.Viewport.Width;
            int windowHeight = Core.GraphicsDevice.Viewport.Height;
            base.Initialize();

            // load filenames
            dungeonFiles = System.IO.Directory.GetFiles(
                System.AppDomain.CurrentDomain.BaseDirectory,
                "*.dungeon",
                System.IO.SearchOption.TopDirectoryOnly
            );

            // populate listbox items
            foreach (var file in dungeonFiles)
            {
                string fileName = System.IO.Path.GetFileNameWithoutExtension(file);
                //_dungeonListBox.Items.Add(fileName);
            }

            Panel panel = new(new Vector2(windowWidth / 3, windowHeight / 2), PanelSkin.Default, anchor: Anchor.Center)
            {
                Draggable = false
            };
            UserInterface.Active.AddEntity(panel);

            // add title and text
            RichParagraph title = new("Dungeon\nCrawler2", Anchor.TopCenter)
            {
                Scale = 4f,
                Offset = new Vector2(0, 20)
            };
            panel.AddChild(title);
        }
    }
}
