using Caps.RPG.DungeonCrawler.UI;
using Caps.RPG.MonoGame;
using Caps.RPG.MonoGame.Scenes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using MonoGameGum;
using MonoGameGum.Forms.Controls;

namespace Caps.RPG.DungeonCrawler.Scenes
{
    public abstract class BaseScene : Scene
    {
        private static SpriteFont _font;
        private static SoundEffect _uiSoundEffect;
        private static SoundEffect _hoverEffect;

        public static SpriteFont Font => _font;
        public static SoundEffect Click => _uiSoundEffect;
        public static SoundEffect Hover => _hoverEffect;

        public override void Initialize()
        {
            base.Initialize();
            GumService.Default.Root.Children.Clear();
        }

        public override void LoadContent()
        {
            base.LoadContent();

            _font = Core.Content.Load<SpriteFont>("fonts/alagard_standard");
            _uiSoundEffect = Core.Content.Load<SoundEffect>("audio/ui");
            _hoverEffect = Core.Content.Load<SoundEffect>("audio/ui_hover_change");
            BlueButton.SetHoverSound(_hoverEffect);
            BlueButton.SetClickSound(_uiSoundEffect);
            RustButton.SetHoverSound(_hoverEffect);
            RustButton.SetClickSound(_uiSoundEffect);
        }

        public override void Draw(GameTime gameTime)
        {
            Core.GraphicsDevice.Clear(new Color(196, 196, 196, 255));
        }
    }
}
