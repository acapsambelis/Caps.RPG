
using Caps.RPG.MonoGame;
// using MonoGame and basic system stuff
using GeonBit.UI;
// using GeonBit UI elements
using GeonBit.UI.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;


namespace Caps.RPG.DungeonCrawler
{
    /// <summary>
    /// This is the main 'Game' instance for your game.
    /// </summary>
    public class GeonBitUI_Examples2 : Game
    {
        public static GraphicsDeviceManager Graphics { get; private set; }
        public static SpriteBatch SpriteBatch { get; private set; }

        #region UI fields
        // buttons to rotate examples
        Button nextExampleButton;
        Button previousExampleButton;

        // paragraph that shows the currently active entity
        Paragraph targetEntityShow;
        #endregion

        /// <summary>
        /// Create the game instance.
        /// </summary>
        public GeonBitUI_Examples2()
        {
            // init graphics device manager and set content root
            Graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            Window.IsBorderless = true;
        }

        /// <summary>
        /// Initialize the main application.
        /// </summary>
        protected override void Initialize()
        {
            MakeFullScreen();
            InitializeThemeAndUI(BuiltinThemes.hd);
        }

        private static void MakeFullScreen()
        {
            // make the window fullscreen (but still with border and top control bar)
            int _ScreenWidth = Graphics.GraphicsDevice.Adapter.CurrentDisplayMode.Width;
            int _ScreenHeight = Graphics.GraphicsDevice.Adapter.CurrentDisplayMode.Height;
            Graphics.PreferredBackBufferWidth = _ScreenWidth;
            Graphics.PreferredBackBufferHeight = _ScreenHeight;
            Graphics.IsFullScreen = false;
            Graphics.ApplyChanges();
        }

        private void InitializeThemeAndUI(BuiltinThemes theme)
        {
            // create and init the UI manager
            UserInterface.Initialize(Content, theme);
            UserInterface.Active.UseRenderTarget = true;

            // draw cursor outside the render target
            UserInterface.Active.IncludeCursorInRenderTarget = false;

            // Create a new SpriteBatch, which can be used to draw textures.
            SpriteBatch = SpriteBatch ?? new SpriteBatch(GraphicsDevice);

            // init ui and examples
            InitExamplesAndUI();
        }

        /// <summary>
        /// Create the top bar with next / prev buttons etc, and init all UI example panels.
        /// </summary>        
        protected void InitExamplesAndUI()
        {
            // create top panel
            int topPanelHeight = 65;
            Panel topPanel = new Panel(new Vector2(0, topPanelHeight + 2), PanelSkin.Default, Anchor.TopCenter);
            topPanel.Padding = Vector2.Zero;
            UserInterface.Active.AddEntity(topPanel);

            // add previous example button
            previousExampleButton = new Button("<- Back", ButtonSkin.Default, Anchor.Auto, new Vector2(300, topPanelHeight));
            previousExampleButton.OnClick = (btn) => { PreviousExample(); };
            topPanel.AddChild(previousExampleButton);

            // add next example button
            nextExampleButton = new Button("Next ->", ButtonSkin.Default, Anchor.TopRight, new Vector2(300, topPanelHeight));
            nextExampleButton.OnClick = (btn) => { NextExample(); };
            nextExampleButton.Identifier = "next_btn";
            topPanel.AddChild(nextExampleButton);

            // add exit button
            Button exitBtn = new Button("Exit", anchor: Anchor.BottomRight, size: new Vector2(200, -1));
            exitBtn.OnClick = (entity) => { Exit(); };
            UserInterface.Active.AddEntity(exitBtn);

            // events panel for debug
            Panel eventsPanel = new Panel(new Vector2(400, 530), PanelSkin.Simple, Anchor.CenterLeft, new Vector2(-10, 0));
            eventsPanel.Visible = false;

            // events log (single-time events)
            eventsPanel.AddChild(new Label("Events Log:"));
            SelectList eventsLog = new SelectList(size: new Vector2(-1, 280));
            eventsLog.ExtraSpaceBetweenLines = -8;
            eventsLog.ItemsScale = 0.5f;
            eventsLog.Locked = true;
            eventsPanel.AddChild(eventsLog);

            // current events (events that happen while something is true)
            eventsPanel.AddChild(new Label("Current Events:"));
            SelectList eventsNow = new SelectList(size: new Vector2(-1, 100));
            eventsNow.ExtraSpaceBetweenLines = -8;
            eventsNow.ItemsScale = 0.5f;
            eventsNow.Locked = true;
            eventsPanel.AddChild(eventsNow);

            // paragraph to show currently active panel
            targetEntityShow = new Paragraph("test", Anchor.Auto, Color.White, scale: 0.75f);
            eventsPanel.AddChild(targetEntityShow);

            // add the events panel
            UserInterface.Active.AddEntity(eventsPanel);

            // whenever events log list size changes, make sure its not too long. if it is, trim it.
            eventsLog.OnListChange = (entity) =>
            {
                SelectList list = (SelectList)entity;
                if (list.Count > 100)
                {
                    list.RemoveItem(0);
                }
            };

            // listen to all global events - one timers
            UserInterface.Active.OnClick = (entity) => { eventsLog.AddItem("Click: " + entity.GetType().Name); eventsLog.scrollToEnd(); };
            UserInterface.Active.OnRightClick = (entity) => { eventsLog.AddItem("RightClick: " + entity.GetType().Name); eventsLog.scrollToEnd(); };
            UserInterface.Active.OnMouseDown = (entity) => { eventsLog.AddItem("MouseDown: " + entity.GetType().Name); eventsLog.scrollToEnd(); };
            UserInterface.Active.OnRightMouseDown = (entity) => { eventsLog.AddItem("RightMouseDown: " + entity.GetType().Name); eventsLog.scrollToEnd(); };
            UserInterface.Active.OnMouseEnter = (entity) => { eventsLog.AddItem("MouseEnter: " + entity.GetType().Name); eventsLog.scrollToEnd(); };
            UserInterface.Active.OnMouseLeave = (entity) => { eventsLog.AddItem("MouseLeave: " + entity.GetType().Name); eventsLog.scrollToEnd(); };
            UserInterface.Active.OnMouseReleased = (entity) => { eventsLog.AddItem("MouseReleased: " + entity.GetType().Name); eventsLog.scrollToEnd(); };
            UserInterface.Active.OnMouseWheelScroll = (entity) => { eventsLog.AddItem("Scroll: " + entity.GetType().Name); eventsLog.scrollToEnd(); };
            UserInterface.Active.OnStartDrag = (entity) => { eventsLog.AddItem("StartDrag: " + entity.GetType().Name); eventsLog.scrollToEnd(); };
            UserInterface.Active.OnStopDrag = (entity) => { eventsLog.AddItem("StopDrag: " + entity.GetType().Name); eventsLog.scrollToEnd(); };
            UserInterface.Active.OnFocusChange = (entity) => { eventsLog.AddItem("FocusChange: " + entity.GetType().Name); eventsLog.scrollToEnd(); };
            UserInterface.Active.OnValueChange = (entity) => { if (entity.Parent == eventsLog) { return; } eventsLog.AddItem("ValueChanged: " + entity.GetType().Name); eventsLog.scrollToEnd(); };

            // clear the current events after every frame they were drawn
            eventsNow.AfterDraw = (entity) => { eventsNow.ClearItems(); };

            // listen to all global events - happening now
            UserInterface.Active.WhileDragging = (entity) => { eventsNow.AddItem("Dragging: " + entity.GetType().Name); eventsNow.scrollToEnd(); };
            UserInterface.Active.WhileMouseDown = (entity) => { eventsNow.AddItem("MouseDown: " + entity.GetType().Name); eventsNow.scrollToEnd(); };
            UserInterface.Active.WhileMouseHover = (entity) => { eventsNow.AddItem("MouseHover: " + entity.GetType().Name); eventsNow.scrollToEnd(); };
            eventsNow.MaxItems = 4;

            // add extra info button
            Button infoBtn = new Button("  Events", anchor: Anchor.BottomLeft, size: new Vector2(280, -1));
            infoBtn.AddChild(new Icon(IconType.Scroll, Anchor.CenterLeft), true);
            infoBtn.OnClick = (entity) =>
            {
                eventsPanel.Visible = !eventsPanel.Visible;
            };
            infoBtn.ToggleMode = true;
            infoBtn.ToolTipText = "Show events log.";
            UserInterface.Active.AddEntity(infoBtn);

            // once done init, clear events log
            eventsLog.ClearItems();

            // call base initialize
            base.Initialize();
        }

        /// <summary>
        /// Show next UI example.
        /// </summary>
        public void NextExample()
        {
        }

        /// <summary>
        /// Show previous UI example.
        /// </summary>
        public void PreviousExample()
        {
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // make sure window is focused
            if (!IsActive)
                return;

            // exit on escape
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // update UI
            UserInterface.Active.Update(gameTime);

            // show currently active entity (for testing)
            targetEntityShow.Text = "Target Entity: " + (UserInterface.Active.TargetEntity != null ? UserInterface.Active.TargetEntity.GetType().Name : "null");

            // call base update
            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            // draw ui
            UserInterface.Active.Draw(SpriteBatch);

            // clear buffer
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // finalize ui rendering
            UserInterface.Active.DrawMainRenderTarget(SpriteBatch);

            // call base draw function
            base.Draw(gameTime);
        }
    }
}