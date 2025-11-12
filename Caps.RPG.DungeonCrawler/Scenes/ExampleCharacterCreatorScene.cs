using Caps.RPG.DungeonCrawler.GameObjects;
using Caps.RPG.MonoGame;
using GeonBit.UI;
using GeonBit.UI.Entities;
using GeonBit.UI.Entities.TextValidators;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Caps.RPG.DungeonCrawler.Scenes
{
    public class ExampleCharacterCreatorScene(CommonConfig config) : BaseScene(config, CameraSceneMode.FullScreen)
    {
        public override void Initialize()
        {
            base.Initialize();
            InitializeUI();
        }

        private void InitializeUI()
        {
            // example: character build page - final
            {
                int panelWidth = 730;

                // create panel and add to list of panels and manager
                Panel panel = new Panel(new Vector2(panelWidth, -1));
                UserInterface.Active.AddEntity(panel);

                // add title and text
                panel.AddChild(new Header("Create New Character"));
                panel.AddChild(new HorizontalLine());

                // create an internal panel to align components better - a row that covers the entire width split into 3 columns (left, center, right)
                // first the container panel
                Panel entitiesGroup = new Panel(new Vector2(0, 250), PanelSkin.None, Anchor.Auto);
                entitiesGroup.Padding = Vector2.Zero;
                panel.AddChild(entitiesGroup);

                // create grid
                var columnPanels = GeonBit.UI.Utils.PanelsGrid.GenerateColums(2, entitiesGroup);
                foreach (var column in columnPanels) { column.Padding = Vector2.Zero; }
                Panel leftPanel = columnPanels[0];
                Panel rightPanel = columnPanels[1];

                // create the class selection list
                leftPanel.AddChild(new Label(@"Class", Anchor.AutoCenter));
                SelectList classTypes = new SelectList(new Vector2(0, 208), Anchor.Auto);
                classTypes.AddItem("Warrior");
                classTypes.AddItem("Mage");
                classTypes.AddItem("Ranger");
                classTypes.AddItem("Monk");
                classTypes.SelectedIndex = 0;
                leftPanel.AddChild(classTypes);
                classTypes.OnValueChange = (Entity entity) =>
                {
                    string texture = ((SelectList)(entity)).SelectedValue.ToLower();
                };

                // create color selection buttons
                rightPanel.AddChild(new Label(@"Color", Anchor.AutoCenter));
                int colorPickSize = 24;
                var colorPicker = new Panel(Vector2.One * colorPickSize * 9, PanelSkin.Fancy, Anchor.AutoCenter) { Padding = Vector2.Zero };
                rightPanel.AddChild(colorPicker);
                Color[] colors = { Color.White, Color.Red, Color.Green, Color.Blue, Color.Yellow, Color.Purple, Color.Cyan, Color.Brown, Color.Orange };
                foreach (Color baseColor in colors)
                {
                    for (int i = 0; i < 9; ++i)
                    {
                        Color color = baseColor * (1.0f - (i * 2 / 16.0f)); color.A = 255;
                        ColoredRectangle currColorButton = new ColoredRectangle(color, Vector2.One * colorPickSize, i == 0 ? Anchor.Auto : Anchor.AutoInlineNoBreak);
                        currColorButton.Padding = currColorButton.SpaceAfter = currColorButton.SpaceBefore = Vector2.Zero;
                        currColorButton.OnClick = (Entity entity) =>
                        {
                            //previewImageColor.FillColor = entity.FillColor;
                        };
                        colorPicker.AddChild(currColorButton);
                    }
                }

                // gender selection (radio buttons)
                panel.AddChild(new RadioButton("Male", Anchor.Auto, new Vector2(180, 60), isChecked: true));
                panel.AddChild(new RadioButton("Female", Anchor.AutoInline, new Vector2(240, 60)));

                // hardcore mode
                Button hardcore = new Button("Hardcore", ButtonSkin.Fancy, Anchor.AutoInline, new Vector2(220, 60));
                hardcore.ButtonParagraph.Scale = 0.8f;
                hardcore.SpaceBefore = new Vector2(24, 0);
                hardcore.ToggleMode = true;
                panel.AddChild(hardcore);
                panel.AddChild(new HorizontalLine());

                // add character name, last name, and age
                // first add the labels
                panel.AddChild(new Label(@"First Name: ", Anchor.AutoInline, size: new Vector2(0.4f, -1)));
                panel.AddChild(new Label(@"Last Name: ", Anchor.AutoInline, size: new Vector2(0.4f, -1)));
                panel.AddChild(new Label(@"Age: ", Anchor.AutoInline, size: new Vector2(0.2f, -1)));

                // now add the text inputs

                // first name
                TextInput firstName = new TextInput(false, new Vector2(0.4f, -1), anchor: Anchor.Auto);
                firstName.PlaceholderText = "Name";
                firstName.Validators.Add(new EnglishCharactersOnly(true));
                firstName.Validators.Add(new OnlySingleSpaces());
                firstName.Validators.Add(new MakeTitleCase());
                panel.AddChild(firstName);

                // last name
                TextInput lastName = new TextInput(false, new Vector2(0.4f, -1), anchor: Anchor.AutoInline);
                lastName.PlaceholderText = "Surname";
                lastName.Validators.Add(new EnglishCharactersOnly(true));
                lastName.Validators.Add(new OnlySingleSpaces());
                lastName.Validators.Add(new MakeTitleCase());
                panel.AddChild(lastName);

                // age
                TextInput age = new TextInput(false, new Vector2(0.2f, -1), anchor: Anchor.AutoInline);
                age.Validators.Add(new NumbersOnly(false, 0, 80));
                age.Value = "20";
                age.ValueWhenEmpty = "20";
                panel.AddChild(age);
            }
        }
    }
}
