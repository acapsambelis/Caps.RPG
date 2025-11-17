using Caps.RPG.MonoGame;
using Caps.RPG.MonoGame.Graphics;
using Caps.RPG.Rules.Creatures;
using Caps.RPG.Rules.Creatures.Classed;
using Caps.RPG.Rules.Creatures.Classed.Classes;
using GeonBit.UI;
using GeonBit.UI.Entities;
using GeonBit.UI.Entities.TextValidators;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Caps.RPG.DungeonCrawler.Scenes
{
    public class PartyCreationScene(CommonConfig config) : BaseScene(config, CameraSceneMode.FullScreen)
    {
        public static readonly string PLAYER_TEAM = "PLAYER";

        private CreatureType[] creatureTypes;
        private readonly CharacterCreationData[] characterData = new CharacterCreationData[config.PartySize];

        public override void Initialize()
        {
            var characterLoader = Program.LuaEnvironment.LoadFromFolder("Characters");
            creatureTypes = [.. characterLoader.LoadComponentsFromCategory<CreatureType>("CreatureTypes.Humanoids")];

            for (int i = 0; i < config.PartySize; i++)
            {
                characterData[i] = new CharacterCreationData(i);
            }

            base.Initialize();
            InitializeUI();
        }

        public void InitializeUI()
        {
            Panel outsideBorder = new(new Vector2(1800, 1200), PanelSkin.Default, Anchor.Center);
            Panel panel = new(new Vector2(0, 0), PanelSkin.None, Anchor.TopCenter);
            PanelTabs tabs = new()
            {
                BackgroundSkin = PanelSkin.None
            };
            panel.AddChild(tabs);
            outsideBorder.AddChild(panel);

            for (int c = 0; c < config.PartySize; c++)
            {
                TabData tab = tabs.AddTab($"Character{c + 1}");
                tab.panel.Tag = c.ToString();

                // create grid
                var columnPanels = GeonBit.UI.Utils.PanelsGrid.GenerateColums(2, tab.panel);
                foreach (var column in columnPanels)
                {
                    column.Padding = Vector2.Zero;
                    column.MinSize = new Vector2(column.Size.X, (int)(outsideBorder.Size.Y / 2));
                }
                Panel leftPanel = columnPanels[0];
                Panel rightPanel = columnPanels[1];

                // create the ancestry selection list
                leftPanel.AddChild(new Label(@"Ancestry", Anchor.AutoCenter) { Scale = 1.5f });
                SelectList ancestryTypes = new(new Vector2(0, 208), Anchor.Auto);
                var creatureType = typeof(CreatureType);

                foreach (var ct in creatureTypes)
                {
                    ancestryTypes.AddItem(ct.Name);
                }
                ancestryTypes.OnValueChange += e =>
                {
                    var selectedAncestryName = ancestryTypes.SelectedValue;
                    var selectedAncestry = creatureTypes.FirstOrDefault(ct => ct.Name == selectedAncestryName);
                    characterData[int.Parse(tab.panel.Tag)].Ancestry = selectedAncestry;
                };
                ancestryTypes.SelectedIndex = 0;
                leftPanel.AddChild(ancestryTypes);

                // create the class selection list
                leftPanel.AddChild(new Label(@"Class", Anchor.AutoCenter) { Scale = 1.5f });
                SelectList classTypes = new(new Vector2(0, 208), Anchor.Auto);
                var characterClassType = typeof(CharacterClass);
                var classTypesList = AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(a => a.GetTypes())
                    .Where(t => characterClassType.IsAssignableFrom(t) && !t.IsAbstract && t != characterClassType)
                    .ToList();

                foreach (var classType in classTypesList)
                {
                    classTypes.AddItem(classType.Name);
                }
                classTypes.OnValueChange += e =>
                {
                    Type selectedClassType = classTypesList.FirstOrDefault(ct => ct.Name == classTypes.SelectedValue);
                    if (selectedClassType != null)
                        characterData[int.Parse(tab.panel.Tag)].Class = selectedClassType;
                };
                classTypes.SelectedIndex = 0;
                leftPanel.AddChild(classTypes);

                // create color selection buttons
                rightPanel.AddChild(new Label(@"Color", Anchor.AutoCenter) { Scale = 1.5f });
                int colorPickSize = 48;
                var colorPicker = new Panel(Vector2.One * colorPickSize * 9, PanelSkin.Fancy, Anchor.AutoCenter) { Padding = Vector2.Zero };
                rightPanel.AddChild(colorPicker);
                Color[] colors = [Color.White, Color.Red, Color.Green, Color.Blue, Color.Yellow, Color.Purple, Color.Cyan, Color.Brown, Color.Orange];
                foreach (Color baseColor in colors)
                {
                    for (int i = 0; i < 9; ++i)
                    {
                        Color color = baseColor * (1.0f - (i * 2 / 16.0f)); color.A = 255;
                        ColoredRectangle currColorButton = new(color, Vector2.One * colorPickSize, i == 0 ? Anchor.Auto : Anchor.AutoInlineNoBreak);
                        currColorButton.Padding = currColorButton.SpaceAfter = currColorButton.SpaceBefore = Vector2.Zero;
                        currColorButton.OnClick = entity =>
                        {
                            characterData[int.Parse(tab.panel.Tag)].Color = color;
                        };
                        colorPicker.AddChild(currColorButton);
                    }
                }

                // ability scores
                var stats = Enum.GetValues(typeof(Rules.Attributes.Stat)).Cast<Rules.Attributes.Stat>().ToArray();
                stats = [.. stats.Where(s => s != Rules.Attributes.Stat.None)];
                var abilityColumnPanels = GeonBit.UI.Utils.PanelsGrid.GenerateColums(stats.Length, tab.panel);
                foreach (var column in abilityColumnPanels) { column.Padding = Vector2.Zero; column.Anchor = Anchor.AutoInline; }
                for (int a = 0; a < stats.Length; a++)
                {
                    Panel abilityPanel = abilityColumnPanels[a];
                    abilityPanel.AddChild(new Label(stats[a].ToString(), Anchor.AutoCenter) { Scale = 1.25f });
                    // add ability score selection here
                    Panel scorePanel = new(new Vector2(100, 140), PanelSkin.None, Anchor.AutoCenter) { Padding = Vector2.Zero };
                    Button plusButton = new("+", ButtonSkin.Default, Anchor.TopCenter, new Vector2(48));
                    Label scoreLabel = new("0", Anchor.Center) { Padding = Vector2.Zero, Scale = 1.5f };
                    Button minusButton = new("-", ButtonSkin.Default, Anchor.BottomCenter, new Vector2(48));

                    plusButton.ButtonParagraph.AlignToCenter = true;
                    minusButton.ButtonParagraph.AlignToCenter = true;
                    plusButton.Padding = Vector2.Zero;
                    minusButton.Padding = Vector2.Zero;
                    plusButton.ButtonParagraph.Padding = Vector2.Zero;
                    minusButton.ButtonParagraph.Padding = Vector2.Zero;
                    plusButton.Tag = a.ToString();
                    minusButton.Tag = a.ToString();

                    minusButton.OnClick = e =>
                    {
                        int value = int.Parse(scoreLabel.Text);
                        if (value > -5) value--;
                        scoreLabel.Text = value.ToString();
                        characterData[int.Parse(tab.panel.Tag)].AbilityScores[stats[int.Parse(e.Tag)]] = value;
                    };
                    plusButton.OnClick = e =>
                    {
                        int value = int.Parse(scoreLabel.Text);
                        if (value < 10) value++;
                        scoreLabel.Text = value.ToString();
                        characterData[int.Parse(tab.panel.Tag)].AbilityScores[stats[int.Parse(e.Tag)]] = value;
                    };

                    scorePanel.AddChild(minusButton);
                    scorePanel.AddChild(scoreLabel);
                    scorePanel.AddChild(plusButton);
                    abilityPanel.AddChild(scorePanel);
                }

                // name
                tab.panel.AddChild(new HorizontalLine() { Offset = new Vector2(0, 50)});
                TextInput txtName = new(false, new Vector2(0.4f, -1), anchor: Anchor.AutoCenter) { Offset = new Vector2(0, 50) };
                txtName.TextParagraph.AlignToCenter = true;
                txtName.PlaceholderText = tab.name;
                txtName.PlaceholderParagraph.AlignToCenter = true;
                txtName.Validators.Add(new EnglishCharactersOnly(true));
                txtName.Validators.Add(new OnlySingleSpaces());
                txtName.Validators.Add(new MakeTitleCase());
                txtName.OnValueChange += e =>
                {
                    if (!string.IsNullOrWhiteSpace(txtName.Value))
                    {
                        characterData[int.Parse(tab.panel.Tag)].Name = txtName.Value;
                    }
                };

                tab.panel.AddChild(txtName);
            }

            Button start = new("Start", ButtonSkin.Default, Anchor.BottomCenter, new Vector2(150, 50))
            {
                Offset = new Vector2(0, 20)
            };
            start.OnClick += (btn) => StartGame();
            UserInterface.Active.AddEntity(start);

            UserInterface.Active.AddEntity(outsideBorder);
        }

        private void StartGame()
        {
            List<ClassedCharacter> party = [];
            Dictionary<string, Sprite> characterSprites = [];
            foreach (var cData in characterData)
            {
                party.Add(cData.ToCharacter());
                // create sprite for character
                var sprite = Sprite.CreateTextureSprite(Core.GraphicsDevice, 48, 48, cData.Color);
                characterSprites.Add(cData.Name + " " + PLAYER_TEAM, sprite);
            }


            Core.ChangeScene(new GameScene(config, [.. party], characterSprites));
        }

        private struct CharacterCreationData
        {
            public CreatureType Ancestry;
            public Type Class;
            public Color Color;
            public Dictionary<Rules.Attributes.Stat, int> AbilityScores;
            public string Name;

            public CharacterCreationData(int i)
            {
                Ancestry = null;
                Class = null;
                Color = Color.White;
                AbilityScores = [];
                Name = "Unnamed Hero" + i;

                var stats = Enum.GetValues(typeof(Rules.Attributes.Stat)).Cast<Rules.Attributes.Stat>().ToArray();
                stats = [.. stats.Where(s => s != Rules.Attributes.Stat.None)];
                foreach (Rules.Attributes.Stat stat in stats)
                {
                    AbilityScores[stat] = 0;
                }
            }

            public readonly ClassedCharacter ToCharacter()
            {
                var attrSet = new Rules.Attributes.AttributeSet(
                    AbilityScores[Rules.Attributes.Stat.Strength],
                    AbilityScores[Rules.Attributes.Stat.Agility],
                    AbilityScores[Rules.Attributes.Stat.Constitution],
                    AbilityScores[Rules.Attributes.Stat.Intellect],
                    AbilityScores[Rules.Attributes.Stat.Arcana],
                    AbilityScores[Rules.Attributes.Stat.Wisdom],
                    AbilityScores[Rules.Attributes.Stat.Presence],
                    AbilityScores[Rules.Attributes.Stat.Charisma]
                );
                var classes = new Dictionary<Type, int>
                {
                    [Class] = 1
                };
                var character = new ClassedCharacter(Name, attrSet, Ancestry, classes);
                //character.Color = Color;
                return character;
            }
        }
    }
}
