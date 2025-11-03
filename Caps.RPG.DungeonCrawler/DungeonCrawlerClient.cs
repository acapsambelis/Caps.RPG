using Caps.RPG.DungeonCrawler.GameObjects;
using Caps.RPG.DungeonCrawler.Scenes;
using Caps.RPG.MonoGame;
using Caps.RPG.MonoGame.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

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
            base.Initialize(); // Core.Initialize() already set the window size / borderless

#if DEBUG
            // Choose monitor - replace with e.g. Config.MonitorIndex if you add it to CommonConfig
            int monitorIndex = 1; // 0 = primary
            var screens = Screen.AllScreens;
            monitorIndex = Math.Clamp(monitorIndex, 0, screens.Length - 1);
            var screen = screens[monitorIndex];

            // ClientBounds were set by Core.MakeFullScreen, so use them to center the window
            int windowWidth = Window.ClientBounds.Width;
            int windowHeight = Window.ClientBounds.Height;
            int x = screen.WorkingArea.X + (screen.WorkingArea.Width - windowWidth) / 2;
            int y = screen.WorkingArea.Y + (screen.WorkingArea.Height - windowHeight) / 2;

            // Preferred: use MonoGame window API if available
            try
            {
                Window.Position = new Point(x, y);
            }
            catch
            {
                // Fallback: native Win32 move (Windows-only)
                SetWindowPos(Window.Handle, IntPtr.Zero, x, y, 0, 0, SWP_NOSIZE | SWP_NOZORDER);
            }
#endif

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

#if DEBUG
        private const uint SWP_NOSIZE = 0x0001;
        private const uint SWP_NOZORDER = 0x0004;
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);
#endif
    }
}
