using Caps.RPG.MonoGame;
using Caps.RPG.MonoGame.Scenes;
using GeonBit.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Caps.RPG.DungeonCrawler.Scenes
{
    public abstract class BaseScene(CommonConfig config, CameraSceneMode cameraMode) : Scene()
    {
        protected CommonConfig config = config;
        protected CameraSceneMode cameraMode = cameraMode;

        private static SpriteFont _font;
        private static SoundEffect _uiSoundEffect;
        private static SoundEffect _hoverEffect;

        public static SpriteFont Font => _font;
        public static SoundEffect Click => _uiSoundEffect;
        public static SoundEffect Hover => _hoverEffect;

        public override void Initialize()
        {
            base.Initialize();
        }

        public override void LoadContent()
        {
            base.LoadContent();

            _font = Core.Content.Load<SpriteFont>("fonts/alagard_standard");
            _uiSoundEffect = Core.Content.Load<SoundEffect>("audio/ui");
            _hoverEffect = Core.Content.Load<SoundEffect>("audio/ui_hover_change");
        }

        public override void Draw(GameTime gameTime)
        {
            UserInterface.Active.Draw(Core.SpriteBatch);
            Core.GraphicsDevice.Clear(new Color(196, 196, 196, 255));
            UserInterface.Active.DrawMainRenderTarget(Core.SpriteBatch);
        }

        public void Draw(GameTime gameTime, Action drawAction)
        {
            UserInterface.Active.Draw(Core.SpriteBatch);
            Core.GraphicsDevice.Clear(new Color(196, 196, 196, 255));
            drawAction();
            UserInterface.Active.DrawMainRenderTarget(Core.SpriteBatch);
        }

        public override void Update(GameTime gameTime)
        {
            if (cameraMode != CameraSceneMode.FullScreen)
            {
                if (Core.Input.Mouse.WasButtonJustPressed(Caps.RPG.MonoGame.Input.MouseButton.Left))
                    Core.Camera.Mode = CameraMoveMode.Drag;
                if (Core.Input.Mouse.WasButtonJustReleased(Caps.RPG.MonoGame.Input.MouseButton.Left))
                    Core.Camera.Mode = CameraMoveMode.Static;
                if (Core.Input.Mouse.WasButtonJustPressed(Caps.RPG.MonoGame.Input.MouseButton.Middle))
                    Core.Camera.Mode = CameraMoveMode.Point;
                if (Core.Input.Mouse.WasButtonJustPressed(Caps.RPG.MonoGame.Input.MouseButton.Right))
                    Core.Camera.Mode = CameraMoveMode.Follow;

                Core.Camera.MoveCamera(gameTime);
            }
        }
    }
}
