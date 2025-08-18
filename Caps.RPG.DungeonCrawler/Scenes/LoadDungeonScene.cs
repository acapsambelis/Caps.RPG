using Caps.RPG.DungeonCrawler.UI;
using Caps.RPG.MonoGame;
using Caps.RPG.MonoGame.Graphics;
using GeonBit.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Caps.RPG.DungeonCrawler.Scenes
{
    public class LoadDungeonScene : BaseScene
    {
        private string[] dungeonFiles = [];

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

            // populate listbox items
            foreach (var file in dungeonFiles)
            {
                string fileName = System.IO.Path.GetFileNameWithoutExtension(file);
                //_dungeonListBox.Items.Add(fileName);
            }
        }

        public override void LoadContent()
        {

        }

        public override void Update(GameTime gameTime)
        {
            // GeonBit.UIL update UI manager
            UserInterface.Active.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {

        }
    }
}
