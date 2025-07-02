using System;
using Caps.RPG.MonoGame;
using Caps.RPG.MonoGame.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGameGum;
using MonoGameGum.GueDeriving;
using MonoGameGum.Forms.Controls;
using Caps.RPG.DungeonCrawler.UI;

namespace Caps.RPG.DungeonCrawler.Scenes
{
    public class TitleScene : BaseScene
    {
        private const string FIRST_LINE_TEXT = "Dungeon";
        private Vector2 _dungeonTextPos;
        private Vector2 _dungeonTextOrigin;
        private const string SECOND_LINE_TEXT = "Crawler";
        private Vector2 _slimeTextPos;
        private Vector2 _slimeTextOrigin;
        private const string BYLINE_TEXT = "a game by Alex Capsambelis";
        private Vector2 _bylineTextPos;
        private Vector2 _bylineTextOrigin;

        // The font used to render the title text.
        private SpriteFont _titleFont;

        private Panel _titleScreenButtonsPanel;
        private Panel _optionsPanel;

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
            Vector2 size = _titleFont.MeasureString(FIRST_LINE_TEXT);
            _dungeonTextPos = new Vector2(640, 200);
            _dungeonTextOrigin = size * 0.5f;

            // Set the position and origin for the Slime text.
            size = _titleFont.MeasureString(SECOND_LINE_TEXT);
            _slimeTextPos = new Vector2(640, 307);
            _slimeTextOrigin = size * 0.5f;

            // Set the position and origin for the by line text.
            size = Font.MeasureString(BYLINE_TEXT);
            _bylineTextPos = new Vector2(640, 414);
            _bylineTextOrigin = size * 0.5f;

            InitializeUI();
        }

        private void InitializeUI()
        {
            CreateTitlePanel();
            CreateOptionsPanel();
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
            GumService.Default.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            if (_titleScreenButtonsPanel.IsVisible)
            {
                // Begin the sprite batch to prepare for rendering.
                Core.SpriteBatch.Begin(samplerState: SamplerState.PointClamp);

                // The color to use for the drop shadow text.
                Color dropShadowColor = Color.Black * 0.5f;

                // Draw the Dungeon text
                Core.SpriteBatch.DrawString(_titleFont, FIRST_LINE_TEXT, _dungeonTextPos + new Vector2(10, 10), dropShadowColor, 0.0f, _dungeonTextOrigin, 1.0f, SpriteEffects.None, 1.0f); // shadow
                Core.SpriteBatch.DrawString(_titleFont, FIRST_LINE_TEXT, _dungeonTextPos, Color.White, 0.0f, _dungeonTextOrigin, 1.0f, SpriteEffects.None, 1.0f);

                // Draw the Slime text
                Core.SpriteBatch.DrawString(_titleFont, SECOND_LINE_TEXT, _slimeTextPos + new Vector2(10, 10), dropShadowColor, 0.0f, _slimeTextOrigin, 1.0f, SpriteEffects.None, 1.0f); // shadow
                Core.SpriteBatch.DrawString(_titleFont, SECOND_LINE_TEXT, _slimeTextPos, Color.White, 0.0f, _slimeTextOrigin, 1.0f, SpriteEffects.None, 1.0f);

                // Draw the byline text
                Core.SpriteBatch.DrawString(Font, BYLINE_TEXT, _bylineTextPos, Color.White, 0.0f, _bylineTextOrigin, 1.0f, SpriteEffects.None, 1.0f);

                // Always end the sprite batch when finished.
                Core.SpriteBatch.End();
            }

            GumService.Default.Draw();
        }


        private void CreateTitlePanel()
        {
            // Create a container to hold all of our buttons
            _titleScreenButtonsPanel = new Panel();
            _titleScreenButtonsPanel.Dock(Gum.Wireframe.Dock.Fill);
            _titleScreenButtonsPanel.AddToRoot();

            BlueButton startButton = new BlueButton(_atlas, "Start", "fonts/alagard.fnt");
            startButton.Anchor(Gum.Wireframe.Anchor.BottomLeft);
            startButton.Visual.X = 50;
            startButton.Visual.Y = -12;
            startButton.Visual.Width = 70;
            startButton.Click += HandleStartClicked;
            _titleScreenButtonsPanel.AddChild(startButton);

            _optionsButton = new RustButton(_atlas, "Options", "fonts/alagard.fnt");
            _optionsButton.Anchor(Gum.Wireframe.Anchor.BottomRight);
            _optionsButton.Visual.X = -50;
            _optionsButton.Visual.Y = -12;
            _optionsButton.Visual.Width = 70;
            _optionsButton.Click += HandleOptionsClicked;
            _titleScreenButtonsPanel.AddChild(_optionsButton);

            startButton.IsFocused = true;
        }

        private void HandleStartClicked(object sender, EventArgs e)
        {
            // Change to the game scene to start the game.
            Core.ChangeScene(new LoadDungeonScene());
        }

        private void HandleOptionsClicked(object sender, EventArgs e)
        {
            // Set the title panel to be invisible.
            _titleScreenButtonsPanel.IsVisible = false;

            // Set the options panel to be visible.
            _optionsPanel.IsVisible = true;

            // Give the back button on the options panel focus.
            _optionsBackButton.IsFocused = true;
        }

        private void CreateOptionsPanel()
        {
            _optionsPanel = new Panel();
            _optionsPanel.Dock(Gum.Wireframe.Dock.Fill);
            _optionsPanel.IsVisible = false;
            _optionsPanel.AddToRoot();

            TextRuntime optionsText = new TextRuntime();
            optionsText.X = 10;
            optionsText.Y = 10;
            optionsText.Text = "OPTIONS";
            optionsText.UseCustomFont = true;
            optionsText.FontScale = 0.5f;
            optionsText.CustomFontFile = @"fonts/alagard.fnt";
            _optionsPanel.AddChild(optionsText);

            OptionsSlider musicSlider = new OptionsSlider(_atlas);
            musicSlider.Name = "MusicSlider";
            musicSlider.Text = "MUSIC";
            musicSlider.Anchor(Gum.Wireframe.Anchor.Top);
            musicSlider.Visual.Y = 30f;
            musicSlider.Minimum = 0;
            musicSlider.Maximum = 1;
            musicSlider.Value = Core.Audio.SongVolume;
            musicSlider.SmallChange = .1;
            musicSlider.LargeChange = .2;
            musicSlider.ValueChanged += HandleMusicSliderValueChanged;
            musicSlider.ValueChangeCompleted += HandleMusicSliderValueChangeCompleted;
            _optionsPanel.AddChild(musicSlider);

            OptionsSlider sfxSlider = new OptionsSlider(_atlas);
            sfxSlider.Name = "SfxSlider";
            sfxSlider.Text = "SFX";
            sfxSlider.Anchor(Gum.Wireframe.Anchor.Top);
            sfxSlider.Visual.Y = 93;
            sfxSlider.Minimum = 0;
            sfxSlider.Maximum = 1;
            sfxSlider.Value = Core.Audio.SoundEffectVolume;
            sfxSlider.SmallChange = .1;
            sfxSlider.LargeChange = .2;
            sfxSlider.ValueChanged += HandleSfxSliderChanged;
            sfxSlider.ValueChangeCompleted += HandleSfxSliderChangeCompleted;
            _optionsPanel.AddChild(sfxSlider);

            _optionsBackButton = new BlueButton(_atlas, "Back", "fonts/alagard.fnt");
            _optionsBackButton.Text = "BACK";
            _optionsBackButton.Anchor(Gum.Wireframe.Anchor.BottomRight);
            _optionsBackButton.X = -28f;
            _optionsBackButton.Y = -10f;
            _optionsBackButton.Click += HandleOptionsButtonBack;
            _optionsPanel.AddChild(_optionsBackButton);
        }


        private void HandleSfxSliderChanged(object sender, EventArgs args)
        {
            // Intentionally not playing the UI sound effect here so that it is not
            // constantly triggered as the user adjusts the slider's thumb on the
            // track.

            // Get a reference to the sender as a Slider.
            var slider = (Slider)sender;

            // Set the global sound effect volume to the value of the slider.;
            Core.Audio.SoundEffectVolume = (float)slider.Value;
        }

        private void HandleSfxSliderChangeCompleted(object sender, EventArgs e)
        {
            // Play the UI Sound effect so the player can hear the difference in audio.
            Core.Audio.PlaySoundEffect(Click);
        }

        private void HandleMusicSliderValueChanged(object sender, EventArgs args)
        {
            // Intentionally not playing the UI sound effect here so that it is not
            // constantly triggered as the user adjusts the slider's thumb on the
            // track.

            // Get a reference to the sender as a Slider.
            var slider = (Slider)sender;

            // Set the global song volume to the value of the slider.
            Core.Audio.SongVolume = (float)slider.Value;
        }

        private void HandleMusicSliderValueChangeCompleted(object sender, EventArgs args)
        {
            // A UI interaction occurred, play the sound effect
            Core.Audio.PlaySoundEffect(Click);
        }

        private void HandleOptionsButtonBack(object sender, EventArgs e)
        {
            // Set the title panel to be visible.
            _titleScreenButtonsPanel.IsVisible = true;

            // Set the options panel to be invisible.
            _optionsPanel.IsVisible = false;

            // Give the options button on the title panel focus since we are coming
            // back from the options screen.
            _optionsButton.IsFocused = true;
        }
    }
}
