using Caps.RPG.DungeonCrawler.UI;
using Caps.RPG.MonoGame;
using Caps.RPG.MonoGame.Graphics;
using GeonBit.UI;
using GeonBit.UI.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Caps.RPG.DungeonCrawler.Scenes
{
    public class TitleScene : BaseScene
    {
        private const string FIRST_LINE_TEXT = "Dungeon";
        private const string SECOND_LINE_TEXT = "Crawler";
        private const string BYLINE_TEXT = "a game by Alex Capsambelis";

        // buttons to rotate examples
        Button nextExampleButton;
        Button previousExampleButton;

        // The font used to render the title text.
        private SpriteFont _titleFont;

        // The options button used to open the options menu.
        private RustButton _optionsButton;

        // The back button used to exit the options menu back to the title menu.
        private BlueButton _optionsBackButton;

        // Reference to the texture atlas that we can pass to UI elements when they
        // are created.
        private TextureAtlas _atlas;

        public override void Initialize()
        {
            // LoadContent is called during base.Initialize().
            base.Initialize();

            // While on the title screen, we can enable exit on escape so the player
            // can close the game by pressing the escape key.
            Core.ExitOnEscape = true;

            // Set the position and origin for the Dungeon text.
            //Vector2 size = _titleFont.MeasureString(FIRST_LINE_TEXT);
            //_dungeonTextPos = new Vector2(640, 200);
            //_dungeonTextOrigin = size * 0.5f;

            // Set the position and origin for the Slime text.
            //size = _titleFont.MeasureString(SECOND_LINE_TEXT);
            //_slimeTextPos = new Vector2(640, 307);
            //_slimeTextOrigin = size * 0.5f;

            // Set the position and origin for the by line text.
            //size = Font.MeasureString(BYLINE_TEXT);
            //_bylineTextPos = new Vector2(640, 414);
            //_bylineTextOrigin = size * 0.5f;

            InitializeUI();
        }

        private void InitializeUI()
        {
            // create top panel
            int topPanelHeight = 65;
            Panel topPanel = new Panel(new Vector2(0, topPanelHeight + 2), PanelSkin.Simple, Anchor.TopCenter);
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

            // add show-get button
            Button showGitButton = new Button("Git Repo", ButtonSkin.Fancy, Anchor.TopCenter, new Vector2(280, topPanelHeight));
            showGitButton.OnClick = (btn) =>
            {
                var url = "https://github.com/RonenNess/GeonBit.UI";
                try
                {
                    System.Diagnostics.Process.Start(url);
                }
                catch
                {
                    try
                    {
                        System.Diagnostics.Process.Start("IExplore", url);
                    }
                    catch
                    {
                        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(url) { UseShellExecute = true });
                    }
                }
            };
            topPanel.AddChild(showGitButton);

            // add exit button
            Button exitBtn = new Button("Exit", anchor: Anchor.BottomRight, size: new Vector2(200, -1));
            exitBtn.OnClick = (entity) => { Exit(); };
            UserInterface.Active.AddEntity(exitBtn);
        }

        public void NextExample()
        {
        }

        public void PreviousExample()
        {
        }

        public void Exit()
        {
        }

        public override void LoadContent()
        {
            base.LoadContent();

            // Load the font for the title text
            _titleFont = Content.Load<SpriteFont>("fonts/alagard_large");

            // Load the texture atlas from the xml configuration file.
            _atlas = TextureAtlas.FromFile(Core.Content, "images/atlas-definition.xml");
        }

        public override void Update(GameTime gameTime)
        {
            // update UI
            UserInterface.Active.Update(gameTime);
            
            // call base update
            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
        }

        private void HandleStartClicked(object sender, EventArgs e)
        {
            // Change to the game scene to start the game.
            Core.ChangeScene(new LoadDungeonScene());
        }
    }
}
