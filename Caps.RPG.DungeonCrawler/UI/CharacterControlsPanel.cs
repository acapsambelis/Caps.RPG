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

        private readonly EncounterMap map;
        private readonly Combattant combattant;

        private readonly Label nameLabel;
        private readonly Image characterPortrait;
        private readonly ProgressBar healthBar;
        private readonly List<Button> actionButtons = [];
        private readonly Panel actionCounters;
        private readonly EventCallback actionSelection;

        public Combattant Combattant => combattant;
        public Panel Panel { get; }

        public CharacterControlsPanel(Combattant combattant, Texture2D characterIcon, EncounterMap map, EventCallback actionSelection, EventCallback endTurn)
        {
            this.map = map;
            this.combattant = combattant;
            this.actionSelection = actionSelection;
            float healthPercent = combattant.Health / (float)Math.Max(1, combattant.Creature.MaxHealth);
            Panel = new Panel(new Vector2(500, 730), PanelSkin.None, Anchor.TopLeft)
            {
                Padding = new Vector2(10)
            };

            //
            // character details
            //

            int panelPadding = 10;
            Panel detailsPanel = new(new Vector2((Panel.Size.X - panelPadding) * 2 / 3, (Panel.Size.X - panelPadding) / 3), PanelSkin.None, anchor: Anchor.TopRight)
            {
                Padding = new Vector2(panelPadding)
            };
            detailsPanel.OnClick += CenterCamera;
            detailsPanel.OnMouseEnter += HighlightCharacter;
            detailsPanel.OnMouseLeave += UnhighlightCharacter;

            // name
            {
                nameLabel = new Label(combattant.Name, Anchor.AutoCenter)
                {
                    Scale = 1.5f,
                    Padding = new Vector2(0, 5),
                };
                detailsPanel.AddChild(nameLabel, true);
            }

            // description
            {
                Label descriptionLabel = new(combattant.Creature.Description(), Anchor.Center)
                {
                    Padding = new Vector2(0, 5),
                    ToolTipText = combattant.Creature.Description(full: true),
                };
                detailsPanel.AddChild(descriptionLabel);
            }

            // action counters
            {
                actionCounters = new Panel(new Vector2(0, 0), PanelSkin.None, Anchor.BottomCenter)
                {
                    Padding = new Vector2(0, 5),
                };
                for (int i = 0; i < combattant.ActionCounts; i++)
                {
                    Icon ic = new(IconType.OrbRed, Anchor.AutoInlineNoBreak)
                    {
                        Locked = true,
                        Tag = i.ToString()
                    };
                    actionCounters.AddChild(ic);
                }
                actionCounters.Size = new Vector2(actionCounters.Children[0].EntityDefaultSize.X * combattant.ActionCounts, actionCounters.Children[0].EntityDefaultSize.Y);

                detailsPanel.AddChild(actionCounters);
                Panel.AddChild(detailsPanel);
            }

            // portrait
            {
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

                // healthbar
                healthBar = new ProgressBar(0, 100, Anchor.AutoCenter)
                {
                    Size = new Vector2(0, 40),
                    FillColor = healthPercent > 0.5f ? Color.LimeGreen : (healthPercent > 0.25f ? Color.Orange : Color.Red),
                    Padding = new Vector2(0, 10),
                    Value = (int)(healthPercent * 100),
                    ToolTipText = $"HP: {combattant.Health} / {combattant.Creature.MaxHealth}"
                };
                healthBar.ProgressFill.FillColor = healthBar.FillColor;
                Panel.AddChild(healthBar);
            }

            if (combattant.IsComputerControlled()) return;

            //
            // Abilities
            //
            Panel.AddChild(new HorizontalLine());
            Panel actionsWrapper = new(new Vector2(0, 0), PanelSkin.None, Anchor.Auto)
            {
                Padding = new Vector2(0),
                Tag = "actionsWrapper"
            };
            actionsWrapper.AddChild(GetActionsTabs(actionSelection));
            Panel.AddChild(actionsWrapper);

            //
            // End turn button
            //
            Panel bottomPanel = new(new Vector2(0, 80), PanelSkin.None, Anchor.BottomCenter)
            {
                Padding = new Vector2(5)
            };
            Button endTurnButton = new("End Turn", ButtonSkin.Fancy, Anchor.Center, new Vector2(160, 40))
            {
                Padding = new Vector2(5),
                ToolTipText = "End your turn immediately"
            };
            endTurnButton.OnClick += endTurn;
            bottomPanel.AddChild(endTurnButton);
            Panel.AddChild(bottomPanel);
        }

        private Panel GetActionsTabs(EventCallback actionSelection)
        {
            // Calculate the number of rows needed for the ability buttons
            var actions = combattant.GetCombatActions();
            actionButtons.Clear();
            int buttonsPerRow = 5;
            int buttonSize = (int)((Panel.Size.X - new Vector2(5).X) / buttonsPerRow) - (int)(new Vector2(5).X);

            // Set the abilitiesPanel height to fit all rows
            Panel abilitiesPanel = new(new Vector2(0, 0), PanelSkin.None, Anchor.AutoCenter)
            {
                Padding = new Vector2(5)
            };
            PanelTabs tabs = new()
            {
                BackgroundSkin = PanelSkin.None
            };
            abilitiesPanel.AddChild(tabs);

            {
                TabData tab = tabs.AddTab("All");
                tab.panel.Padding = new Vector2(5);
                for (int i = 0; i < actions.Count; i++)
                {
                    var action = actions[i];
                    Button actionButton = new("", ButtonSkin.Default, Anchor.AutoInline, size: new Vector2(buttonSize))
                    {
                        ToolTipText = $"{action.Name} (Cost: {action.Cost})",
                        Tag = action.Name,
                        ToggleMode = true
                    };
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
                    tab.panel.AddChild(actionButton);
                }
            }
            {
                TabData tab = tabs.AddTab("Common");
                tab.panel.AddChild(new Header("Tab 2"));
                tab.panel.AddChild(new Paragraph(@"Work in progress"));
            }
            {
                TabData tab = tabs.AddTab("Items");
                tab.panel.AddChild(new Header("Tab 3"));
                tab.panel.AddChild(new Paragraph(@"Work in progress"));
            }
            
            return abilitiesPanel;
        }

        public void CenterCamera(Entity entity)
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
            // if combat actions have changed, we may need to update the action buttons (e.g. new actions available)
            if (!combattant.IsComputerControlled())
            {
                int currentActionButtonCount = actionButtons.Count;
                int actualActionCount = combattant.GetCombatActions().Count;
                if (currentActionButtonCount != actualActionCount)
                {
                    // rebuild action buttons
                    Panel actionsWrapper = Panel.Children.Where(c => c is Panel p && p.Tag == "actionsWrapper").FirstOrDefault() as Panel;
                    actionsWrapper.Children[0].RemoveFromParent();  // always only one child
                    var output = GetActionsTabs(actionSelection);
                    actionsWrapper.AddChild(output);
                }
            }

            healthBar.Value = (int)(combattant.Health / (float)Math.Max(1, combattant.Creature.MaxHealth) * 100);
            healthBar.ToolTipText = $"HP: {combattant.Health} / {combattant.Creature.MaxHealth}";
            healthBar.FillColor = GetColorForHealth(combattant.Health, combattant.Creature.MaxHealth);
            healthBar.ProgressFill.FillColor = healthBar.FillColor;
        }

        public void SetActionNumber(int number)
        {
            foreach (Icon icon in actionCounters.Children.Where(c => c is Icon).Cast<Icon>())
            {
                int index = int.Parse(icon.Tag);
                icon.Enabled = index < number;
            }
        }

        public void ActionClicked(Entity entity)
        {
            if ((entity as Button).Checked == false)
            {
                foreach (var tile in map.Tiles)
                    tile.TileBase.Highlighted = false;
                return;
            }
            string actionName = ((Button)entity).Tag;
            if (string.IsNullOrEmpty(actionName)) return;

            var actions = combattant.GetCombatActions();
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

        private static Color GetColorForHealth(int current, int max)
        {
            float healthPercent = current / (float)Math.Max(1, max);
            return healthPercent > 0.5f ? Color.LimeGreen : (healthPercent > 0.25f ? Color.Orange : Color.Red);
        }
    }
}
