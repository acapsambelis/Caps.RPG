using Caps.RPG.DungeonCrawler.GameObjects;
using Caps.RPG.MonoGame;
using Caps.RPG.MonoGame.Graphics;
using Caps.RPG.Rules.Creatures;
using Caps.RPG.Rules.Creatures.Actions;
using Caps.RPG.Rules.Maps;
using GeonBit.UI;
using GeonBit.UI.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Caps.RPG.DungeonCrawler.UI
{
    public class CharacterControlsPanel : IUIEntity
    {
        public static readonly Dictionary<string, Sprite> ActionIcons = [];

        private readonly Map map;
        private readonly Combattant combattant;

        private readonly Label nameLabel;
        private readonly Image characterPortrait;
        private readonly ProgressBar healthBar;
        private readonly List<Button> actionButtons = [];
        private readonly Panel actionCounters;

        public Combattant Combattant => combattant;
        public Panel Panel { get; }

        public CharacterControlsPanel(Combattant combattant, Texture2D characterIcon, Map map, EventCallback actionSelection)
        {
            this.map = map;
            this.combattant = combattant;
            float healthPercent = combattant.Health / (float)Math.Max(1, combattant.Creature.MaxHealth);
            Panel = new Panel(new Vector2(500, 120), PanelSkin.None, Anchor.TopCenter)
            {
                Padding = new Vector2(10)
            };

            int panelPadding = 10;
            Panel detailsPanel = new(new Vector2((Panel.Size.X - panelPadding) * 2 / 3, 0), PanelSkin.None, anchor: Anchor.TopRight)
            {
                Padding = new Vector2(panelPadding)
            };
            detailsPanel.OnClick += CenterCamera;
            detailsPanel.OnMouseEnter += HighlightCharacter;
            detailsPanel.OnMouseLeave += UnhighlightCharacter;

            nameLabel = new Label(combattant.Name, Anchor.AutoCenter)
            {
                Scale = 1.5f,
                Padding = new Vector2(0, 5),
            };
            detailsPanel.AddChild(nameLabel, true);
            Panel.AddChild(detailsPanel);

            Panel portraitPanel = new(new Vector2((Panel.Size.X - panelPadding) / 3), PanelSkin.None, anchor: Anchor.TopLeft)
            {
                Padding = new Vector2(panelPadding),
            };
            portraitPanel.OnClick += CenterCamera;
            portraitPanel.OnMouseEnter += HighlightCharacter;
            portraitPanel.OnMouseLeave += UnhighlightCharacter;
            characterPortrait = new Image(characterIcon, new Vector2(portraitPanel.Size.X - panelPadding), anchor: Anchor.Center);
            portraitPanel.AddChild(characterPortrait, true);
            Panel.AddChild(portraitPanel);

            healthBar = new ProgressBar(0, 100, Anchor.AutoCenter)
            {
                Size = new Vector2(0, 40),
                Value = combattant.Health,
                FillColor = healthPercent > 0.5f ? Color.LimeGreen : (healthPercent > 0.25f ? Color.Orange : Color.Red),
                Padding = new Vector2(0, 10)
            };
            healthBar.ProgressFill.FillColor = healthBar.FillColor;
            Panel.AddChild(healthBar);

            //actionCounters = new Panel(new Vector2(0, 300), PanelSkin.Alternative, Anchor.AutoCenter)
            //{
            //    Padding = new Vector2(5)
            //};
            //for (int i = 0; i < combattant.ActionCounts; i++)
            //{
            //    Anchor anchor = i == 0 ? Anchor.Auto : Anchor.AutoInline;
            //    actionCounters.AddChild(new RadioButton("", anchor)
            //    {
            //        Checked = true,
            //        Padding = new Vector2(5, 0),
            //        Locked = true,
            //        Tag = i.ToString()
            //    });
            //}
            //Panel.AddChild(actionCounters);

            // Calculate the number of rows needed for the ability buttons
            var actions = combattant.Creature.GetCombatActions();
            int buttonsPerRow = 5;
            int buttonSize = (int)((Panel.Size.X - new Vector2(5).X) / buttonsPerRow) - (int)(new Vector2(5).X);

            // Set the abilitiesPanel height to fit all rows
            Panel abilitiesPanel = new(new Vector2(0, 0), PanelSkin.Simple, Anchor.AutoCenter)
            {
                Padding = new Vector2(5)
            };

            // Add a button with an icon for each combat action, 5 per row
            abilitiesPanel.ClearChildren();

            Panel rowPanel = null;
            var actionButtonPadding = Vector2.Zero;
            for (int i = 0; i < actions.Count; i++)
            {
                if (i % buttonsPerRow == 0)
                {
                    rowPanel = new Panel(new Vector2(0, buttonSize), PanelSkin.None, Anchor.AutoCenter)
                    {
                        Padding = new Vector2(0, 2)
                    };
                    abilitiesPanel.Size = new Vector2(abilitiesPanel.Size.X, abilitiesPanel.Size.Y + buttonSize + actionButtonPadding.Y);
                    abilitiesPanel.AddChild(rowPanel);
                }

                var action = actions[i];
                Button actionButton = new("", ButtonSkin.Default, Anchor.AutoInlineNoBreak, size: new Vector2(buttonSize))
                {
                    ToolTipText = $"{action.Name} (Cost: {action.Cost})",
                    Tag = action.Name,
                    ToggleMode = true
                };
                actionButtonPadding = actionButton.Padding;
                actionButton.OnClick += ActionClicked;
                actionButton.OnClick += actionSelection;

                if (ActionIcons.TryGetValue(action.Name.ToLower(), out var icon) && icon != null)
                {
                    Image iconImage = new(icon.GetTextureWithColor(), new Vector2(buttonSize - 10), anchor: Anchor.Center);
                    actionButton.AddChild(iconImage, true);
                }
                else
                {
                    Label iconLabel = new("?", Anchor.Center, size: new Vector2(buttonSize - 10))
                    {
                        Scale = 3.0f,
                        Padding = Vector2.Zero
                    };
                    actionButton.AddChild(iconLabel, true);
                }
                        
                actionButtons.Add(actionButton);
                rowPanel.AddChild(actionButton);
            }
            Panel.AddChild(new HorizontalLine());
            Panel.AddChild(abilitiesPanel);

            nameLabel.Text = combattant.Name;
            healthBar.Value = (int)(healthPercent * 100);
            healthBar.ToolTipText = $"HP: {combattant.Health} / {combattant.Creature.MaxHealth}";
        }

        private void CenterCamera(Entity entity)
        {
            Core.Camera.FollowPosition = map.GetTilePosition(combattant.Position);
            Core.Camera.Mode = CameraMoveMode.Follow;
            Core.Camera.DirectOrder = true;
        }

        private void HighlightCharacter(Entity entity)
        {
            combattant.Position.Highlighted = true;
        }

        private void UnhighlightCharacter(Entity entity)
        {
            combattant.Position.Highlighted = false;
        }

        public void Update()
        {

        }

        public void SetActionNumber(int number)
        {
            //foreach (CheckBox box in actionCounters.Children.Where(c => c is CheckBox).Cast<CheckBox>())
            //{
            //    int index = int.Parse(box.Tag);
            //    box.Checked = index < number;
            //}
        }

        public void ActionClicked(Entity entity)
        {
            string actionName = ((Button)entity).Tag;
            if (string.IsNullOrEmpty(actionName)) return;

            var actions = combattant.Creature.GetCombatActions();
            CombatAction action = actions.FirstOrDefault(a => a.Name == actionName);
            TileBase[] validTiles = ActionSetupManager.ValidTiles(map.MapData, combattant, action.Setup);
            foreach (var tile in validTiles)
                tile.Highlighted = true;
        }

        public void ToggleAbilityButtons()
        {
            foreach (var button in actionButtons)
            {
                button.Checked = false;
            }
        }
    }
}
