using Caps.RPG.MonoGame.Audio;
using Caps.RPG.MonoGame.Input;
using Caps.RPG.MonoGame.Scenes;
using GeonBit.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace Caps.RPG.MonoGame
{
    public class Core : Game
    {
        internal static Core s_instance;

        /// <summary>
        /// Gets a reference to the Core instance.
        /// </summary>
        public static Core Instance => s_instance;

        private static Scene s_activeScene;
        private static Scene s_nextScene;
        private static Vector2 windowSize;

        /// <summary>
        /// Gets the graphics device manager to control the presentation of graphics.
        /// </summary>
        public static GraphicsDeviceManager Graphics { get; private set; }

        /// <summary>
        /// Gets the graphics device used to create graphical resources and perform primitive rendering.
        /// </summary>
        public static new GraphicsDevice GraphicsDevice { get; private set; }

        /// <summary>
        /// Gets the sprite batch used for all 2D rendering.
        /// </summary>
        public static SpriteBatch SpriteBatch { get; private set; }

        /// <summary>
        /// Gets the content manager used to load global assets.
        /// </summary>
        public static new ContentManager Content { get; private set; }

        /// <summary>
        /// Gets a reference to the input management system.
        /// </summary>
        public static InputManager Input { get; private set; }

        /// <summary>
        /// Gets or Sets a value that indicates if the game should exit when the esc key on the keyboard is pressed.
        /// </summary>
        public static bool ExitOnEscape { get; set; }

        /// <summary>
        /// Gets a reference to the audio control system.
        /// </summary>
        public static AudioController Audio { get; private set; }

        /// <summary>
        /// Gets a reference to the camera used to offset the view of the game world.
        /// </summary>
        public static Camera Camera { get; private set; }

        /// <summary>
        /// Creates a new Core instance.
        /// </summary>
        /// <param name="title">The title to display in the title bar of the game window.</param>
        public Core(string title)
        {
            if (s_instance != null) throw new InvalidOperationException($"Only a single Core instance can be created");
            s_instance = this;

            // Create a new graphics device manager.
            Graphics = new GraphicsDeviceManager(this);

            Graphics.ApplyChanges();
            Window.Title = title;

            // Set the core's content manager to a reference of hte base Game's content manager.
            Content = base.Content;
            Content.RootDirectory = "Content";

            // Make window borderless assuming fullscreen is used
            Window.IsBorderless = true;
        }

        protected override void Initialize()
        {
            base.Initialize();
            // Set the core's graphics device to a reference of the base Game's
            // graphics device.
            GraphicsDevice = base.GraphicsDevice;
            windowSize = MakeFullScreen();

            SpriteBatch = new SpriteBatch(GraphicsDevice);
            Input = new InputManager();
            Audio = new AudioController();
            Camera = new Camera(windowSize / 2, Input.Mouse);

            InitializeUI();
            base.Initialize();
        }

        private static Vector2 MakeFullScreen()
        {
            // make the window fullscreen (but still with border and top control bar)
            int _ScreenWidth = Graphics.GraphicsDevice.Adapter.CurrentDisplayMode.Width;
            int _ScreenHeight = Graphics.GraphicsDevice.Adapter.CurrentDisplayMode.Height;
            Graphics.PreferredBackBufferWidth = _ScreenWidth;
            Graphics.PreferredBackBufferHeight = _ScreenHeight;
            Graphics.IsFullScreen = false;
            Graphics.ApplyChanges();

            return new Vector2(_ScreenWidth, _ScreenHeight);
        }

        protected void InitializeUI()
        {
            UserInterface.Initialize(Content, BuiltinThemes.hd);
            UserInterface.Active.UseRenderTarget = true;
            UserInterface.Active.IncludeCursorInRenderTarget = false;
        }


        protected override void LoadContent()
        {
            base.LoadContent();
        }

        protected override void UnloadContent()
        {
            // Dispose of the audio controller.
            Audio.Dispose();

            base.UnloadContent();
        }

        protected override void Update(GameTime gameTime)
        {
            // make sure window is focused
            if (!IsActive)
                return;

            UserInterface.Active.Update(gameTime);
            Input.Update(gameTime);

            if (ExitOnEscape && Input.Keyboard.IsKeyDown(Keys.Escape))
                Exit();

            // if there is a next scene waiting to be switch to, then transition to that scene
            if (s_nextScene != null)
                TransitionScene();

            s_activeScene?.Update(gameTime);  // If there is an active scene, update it.

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            s_activeScene?.Draw(gameTime);  // If there is an active scene, draw it.

            base.Draw(gameTime);
        }

        public static void ChangeScene(Scene next)
        {
            // Only set the next scene value if it is not the same
            // instance as the currently active scene.
            if (s_activeScene != next)
            {
                s_nextScene = next;
            }
        }

        public static Vector2 GetCursorPosition()
        {
            return Input.GetMouseWorldPosition(Camera.GetTranslation());
        }

        private static void TransitionScene()
        {
            UserInterface.Active.Clear();
            // If there is an active scene, dispose of it
            s_activeScene?.Dispose();

            // Force the garbage collector to collect to ensure memory is cleared
            GC.Collect();

            // Change the currently active scene to the new scene
            s_activeScene = s_nextScene;

            // Null out the next scene value so it does not trigger a change over and over.
            s_nextScene = null;

            // If the active scene now is not null, initialize it.
            // Remember, just like with Game, the Initialize call also calls the
            // Scene.LoadContent
            s_activeScene?.Initialize();
        }
    }
}
