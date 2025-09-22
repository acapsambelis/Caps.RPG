using Caps.RPG.MonoGame.Graphics;
using Caps.RPG.Rules.Creatures;
using GeonBit.UI.Entities;
using Microsoft.Xna.Framework;
using SNS.Data.DataSerializer.DataExtensions;
using System;
using System.Collections.Generic;

namespace Caps.RPG.DungeonCrawler.UI
{
    public class CharacterControlsPanel : IUIEntity
    {
        public Panel Panel { get; }
        private Label nameLabel;
        private ProgressBar healthBar;
        private Panel statusEffectsPanel;

        public CharacterControlsPanel(Combattant combattant)
        {
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

            statusEffectsPanel = new Panel(new Vector2(0, 30), PanelSkin.Simple, Anchor.AutoCenter)
            {
                Padding = new Vector2(5)
            };
            Panel.AddChild(statusEffectsPanel);

            nameLabel.Text = combattant.Name;
            healthBar.Value = (int)(healthPercent * 100);
            healthBar.ToolTipText = $"HP: {combattant.Health} / {combattant.Creature.MaxHealth}";
        }

        public void Update()
        {

        }
    }
}
