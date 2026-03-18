using Caps.RPG.MonoGame;
using Caps.RPG.Rules.Saves;
using GeonBit.UI;
using GeonBit.UI.Entities;
using GeonBit.UI.Utils.Forms;
using Microsoft.Xna.Framework;
using System.IO;

namespace Caps.RPG.DungeonCrawler.Scenes
{
    public class TitleScene(CommonConfig config) : BaseScene(config, CameraSceneMode.FullScreen)
    {
        public override void Initialize()
        {
            base.Initialize();

            // While on the title screen, we can enable exit on escape
            Core.ExitOnEscape = true;

            InitializeUI();
        }

        private void InitializeUI()
        {
            int windowWidth = Core.GraphicsDevice.Viewport.Width;
            int windowHeight = Core.GraphicsDevice.Viewport.Height;
            // create panel and add to list of panels and manager
            Panel panel = new(new Vector2(windowWidth / 3, windowHeight / 1.5f), PanelSkin.None, anchor: Anchor.Center)
            {
                Draggable = false
            };
            UserInterface.Active.AddEntity(panel);

            // add title and text
            RichParagraph title = new("Dungeon\nCrawler", Anchor.TopCenter)
            {
                Scale = 6f,
                Offset = new Vector2(0, 20)
            };
            panel.AddChild(title);
            RichParagraph welcomeText = new(@"a game by Alex Capsambelis", Anchor.Center, scale:1.5f);
            panel.AddChild(welcomeText);

            Panel buttonPanel = new(new Vector2(panel.Size.X, panel.Size.Y / 2f), PanelSkin.None, Anchor.BottomCenter);
            { // brackets are for organization only and do not affect code execution
                // game buttons
                Panel gameButtonPanel = new(new Vector2(buttonPanel.Size.X, buttonPanel.Size.Y * 2 / 3), PanelSkin.None, Anchor.TopCenter);
                Button continueGame = new("Continue", ButtonSkin.Fancy, Anchor.TopCenter, size: new Vector2(panel.Size.X * 2 / 3, 80));
                continueGame.OnClick += (btn) => Continue();
                gameButtonPanel.AddChild(continueGame);
                if (config.LastSaveFile.Equals(""))
                {
                    continueGame.Visible = false;
                }

                Button newGame = new("New Game", ButtonSkin.Default, Anchor.Center, size: new Vector2(panel.Size.X * 2 / 3, 80));
                newGame.OnClick += (btn) => NewGame();
                gameButtonPanel.AddChild(newGame);

                Button loadButton = new("Load Game", ButtonSkin.Default, Anchor.BottomCenter, size: new Vector2(panel.Size.X * 2 / 3, 80));
                loadButton.OnClick += (btn) => LoadGame();
                gameButtonPanel.AddChild(loadButton);

                buttonPanel.AddChild(gameButtonPanel);

                // additional buttons
                Panel additionalButtonPanel = new(new Vector2(buttonPanel.Size.X, buttonPanel.Size.Y / 3), PanelSkin.None, Anchor.BottomCenter);

                Button optionsButton = new("Options", ButtonSkin.Default, Anchor.BottomLeft, size: new Vector2(panel.Size.X / 3, 60));
                optionsButton.OnClick += (btn) => Options();
                additionalButtonPanel.AddChild(optionsButton);

                Button creditsButton = new("Credits", ButtonSkin.Default, Anchor.BottomRight, size: new Vector2(panel.Size.X / 3, 60));
                creditsButton.OnClick += (btn) => Credits();
                additionalButtonPanel.AddChild(creditsButton);

                buttonPanel.AddChild(additionalButtonPanel);
            }
            panel.AddChild(buttonPanel);

            UserInterface.Active.AddEntity(new Paragraph("V" + Program.VERSION, Anchor.BottomLeft, scale:1.5f, offset:new Vector2(10, 10)));
            Button exitButton = new("Exit", ButtonSkin.Default, Anchor.BottomRight, size: new Vector2(windowWidth / 20, 60), offset:new Vector2(10, 10));
            exitButton.OnClick += btn => { Core.Instance.Exit(); };
            UserInterface.Active.AddEntity(exitButton);
            Button showExample = new Button("Show Example", ButtonSkin.Default, Anchor.TopRight, new Vector2(200, 75));
            showExample.OnClick += (btn) => { Core.ChangeScene(new ExampleCharacterCreatorScene(config)); };
            UserInterface.Active.AddEntity(showExample);
        }

        public void Continue()
        {
            Core.ChangeScene(new WorldMapScene(config, @"C:\Users\Alex Capsambelis\source\repos\RPG.Demo.World\RPG.Demo.World\temp\Orcelana Full 2026-03-07-01-43.json", () => Core.ChangeScene(new TitleScene(config))));
            //Core.ChangeScene(new PartyCreationScene(config));
        }

        public void NewGame()
        {
            var newForm = new Form([
                new FormFieldData(FormFieldType.TextInput, "txtSaveFileName", "Save File Name:"),
            ], null);
            GeonBit.UI.Utils.MessageBox.ShowMsgBox("Create New Save", "", "Start", extraEntities: [newForm.FormPanel], onDone: () =>
            {
                string filename = newForm.GetValue("txtSaveFileName").ToString();
                if (string.IsNullOrWhiteSpace(filename))
                {
                    GeonBit.UI.Utils.MessageBox.ShowMsgBox("Invalid Name", "Please enter a valid save file name.");
                    return;
                }
                filename = Path.Combine(config.SavesDirectory, filename + ".rpg");
                SaveState newSave = new();
                newSave.Save(filename);
                config.LastSaveFile = filename;
                GeonBit.UI.Utils.MessageBox.ShowMsgBox("Save name", string.Format("Save created: " + config.LastSaveFile));
            });
            Core.ChangeScene(new PartyCreationScene(config));
        }

        public void LoadGame()
        {
            GeonBit.UI.Utils.MessageBox.OpenLoadFileDialog(config.SavesDirectory, res =>
            {
                GeonBit.UI.Utils.MessageBox.ShowMsgBox("File Selected!", $"Selected file: '{res.FullPath}'.\n\nIn this example we just show a message box, in a real project we would use this path to load the file.");
                config.LastSaveFile = res.FullPath;
                return true;
            }, message: "It won't actually load anything so don't worry about picking any file.");
        }

        public void Options()
        {
        }

        public void Credits()
        {
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
        }
    }
}
