using Caps.RPG.MonoGame.Graphics;
using Caps.RPG.Rules.Creatures;
using Caps.RPG.Rules.Maps;
using FlatRedBall.Glue.StateInterpolation;
using GeonBit.UI.Entities;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace Caps.RPG.DungeonCrawler.UI
{
    public class CharacterControlsPanel : IUIEntity
    {
        public static readonly Dictionary<string, Sprite> ActionIcons = [];

        private readonly TileMap map;
        private readonly Combattant combattant;

        private readonly Label nameLabel;
        private readonly ProgressBar healthBar;
        private readonly Panel abilitiesPanel;

        public Panel Panel { get; }

        public CharacterControlsPanel(Combattant combattant, TileMap map)
        {
            this.map = map;
            this.combattant = combattant;
            float healthPercent = combattant.Health / (float)Math.Max(1, combattant.Creature.MaxHealth);
            Panel = new Panel(new Vector2(500, 120), PanelSkin.None, Anchor.TopCenter)
            {
                Padding = new Vector2(10)
            };

            nameLabel = new Label(combattant.Name, Anchor.AutoCenter)
            {
                Scale = 1.5f,
                Padding = new Vector2(0, 5)
            };
            Panel.AddChild(nameLabel);

            healthBar = new ProgressBar(0, 100, Anchor.AutoCenter)
            {
                Size = new Vector2(0, 40),
                Value = combattant.Health,
                FillColor = healthPercent > 0.5f ? Color.LimeGreen : (healthPercent > 0.25f ? Color.Orange : Color.Red),
                Padding = new Vector2(0, 10)
            };
            healthBar.ProgressFill.FillColor = healthBar.FillColor;
            Panel.AddChild(healthBar);

            // Calculate the number of rows needed for the ability buttons
            var actions = combattant.Creature.GetCombatActions();
            int buttonsPerRow = 5;
            int buttonSize = (int)((Panel.Size.X - new Vector2(5).X) / buttonsPerRow) - (int)(new Vector2(5).X);

            // Set the abilitiesPanel height to fit all rows
            abilitiesPanel = new Panel(new Vector2(0, 0), PanelSkin.Simple, Anchor.AutoCenter)
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
                    ToolTipText = $"{action.Name} (Cost: {action.Cost})"
                };
                actionButtonPadding = actionButton.Padding;

                actionButton.OnClick += entity =>
                {
                    TileBase[] validTiles = ActionSetupManager.ValidTiles(map, combattant, action.Setup);
                    foreach (var tile in validTiles)
                        tile.Highlighted = true;
                    var getTarget = ActionSetupManager.GetAction(action.Setup);
                    TileBase[] target = getTarget?.Invoke(map, combattant, action.Setup);
                };

                if (ActionIcons.TryGetValue(action.Name.ToLower(), out var icon) && icon != null)
                {
                    Image iconImage = new(icon.GetTexture(), new Vector2(buttonSize - 10), anchor: Anchor.Center);
                    actionButton.AddChild(iconImage, true);
                }
                else
                {
                    Label iconLabel = new("?", Anchor.AutoCenter)
                    {
                        Scale = 3.0f,
                        Padding = new Vector2(0)
                    };
                    actionButton.AddChild(iconLabel, true);
                }
                        

                rowPanel.AddChild(actionButton);
            }
            Panel.AddChild(abilitiesPanel);

            nameLabel.Text = combattant.Name;
            healthBar.Value = (int)(healthPercent * 100);
            healthBar.ToolTipText = $"HP: {combattant.Health} / {combattant.Creature.MaxHealth}";
        }

        public void Update()
        {

        }
    }
}
