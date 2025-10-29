using Caps.RPG.DungeonCrawler.GameObjects;
using Caps.RPG.MonoGame;
using Caps.RPG.MonoGame.Graphics;
using Caps.RPG.Rules.Creatures;
using GeonBit.UI.Entities;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Caps.RPG.DungeonCrawler.UI
{
    public class InitiativeTracker : IUIEntity
    {
        private readonly static int topPanelHeight = 128;

        private Sprite characterSprite;
        private readonly Combattant data;
        private readonly Map mapData;
        private readonly Panel panel;
        private readonly ProgressBar healthBar;
        private bool isFocused = false;

        public Panel Panel => panel;

        public Sprite CharacterSprite
        {
            get => characterSprite;
            set
            {
                characterSprite = value;
                panel.Children.Where(c => c is Image).ToList().ForEach(child =>
                {
                    panel.RemoveChild(child);
                    panel.AddChild(new Image(
                        characterSprite.GetTextureWithColor(),
                        size: new Vector2(topPanelHeight - 25),
                        anchor: Anchor.Center
                    ));
                });
            }
        }

        public InitiativeTracker(Sprite characterSprite, ref Combattant data, ref Map map) : base()
        {
            this.characterSprite = characterSprite;
            this.data = data;
            this.mapData = map;

            // Container for image and health bar
            panel = new(new Vector2(topPanelHeight), PanelSkin.Fancy, Anchor.AutoInline)
            {
                Padding = Vector2.Zero,
                FillColor = ConsoleColorToXnaColor(data.Team.Color)
            };
            panel.OnClick += CenterCamera;
            panel.OnMouseEnter += HighlightCharacter;
            panel.OnMouseLeave += UnhighlightCharacter;

            // Character image
            Image character = new(
                characterSprite.GetTextureWithColor(),
                size: new Vector2(topPanelHeight - 25),
                anchor: Anchor.Center
            );
            panel.AddChild(character, true);
            // Health bar
            float healthPercent = data.Health / (float)Math.Max(1, data.Creature.MaxHealth);
            healthBar = new ProgressBar(0, 100)
            {
                Anchor = Anchor.BottomCenter,
                Size = new Vector2(topPanelHeight - 20, 16),
                FillColor = GetColorForHealth(data.Health, data.Creature.MaxHealth),
                Padding = new Vector2(0, 4),
                Value = (int)(healthPercent * 100)
            };
            healthBar.ProgressFill.FillColor = healthBar.FillColor;
            healthBar.ToolTipText = $"HP: {data.Health} / {data.Creature.MaxHealth}";
            panel.AddChild(healthBar, true);
        }

        private void CenterCamera(Entity entity)
        {
            Core.Camera.FollowPosition = mapData.GetTilePosition(data.Position);
            Core.Camera.Mode = CameraMoveMode.Follow;
            Core.Camera.DirectOrder = true;
        }

        private void HighlightCharacter(Entity entity)
        {
            data.Position.Highlighted = true;
        }

        private void UnhighlightCharacter(Entity entity)
        {
            data.Position.Highlighted = false;
        }

        public void Update()
        {
            healthBar.Value = (int)(data.Health / (float)Math.Max(1, data.Creature.MaxHealth) * 100);
            healthBar.ToolTipText = $"HP: {data.Health} / {data.Creature.MaxHealth}";
            healthBar.FillColor = GetColorForHealth(data.Health, data.Creature.MaxHealth);
            healthBar.ProgressFill.FillColor = healthBar.FillColor;
            panel.FillColor = isFocused ? Color.Gold : ConsoleColorToXnaColor(data.Team.Color);
        }

        private static Color GetColorForHealth(int current, int max)
        {
            float healthPercent = current / (float)Math.Max(1, max);
            return healthPercent > 0.5f ? Color.LimeGreen : (healthPercent > 0.25f ? Color.Orange : Color.Red);
        }

        private static Color ConsoleColorToXnaColor(ConsoleColor color)
        {
            return color switch
            {
                ConsoleColor.Black => Color.Black,
                ConsoleColor.DarkBlue => Color.DarkBlue,
                ConsoleColor.DarkGreen => Color.DarkGreen,
                ConsoleColor.DarkCyan => Color.DarkCyan,
                ConsoleColor.DarkRed => Color.DarkRed,
                ConsoleColor.DarkMagenta => Color.Purple,
                ConsoleColor.DarkYellow => Color.Olive,
                ConsoleColor.Gray => Color.Gray,
                ConsoleColor.DarkGray => Color.DarkGray,
                ConsoleColor.Blue => Color.Blue,
                ConsoleColor.Green => Color.Green,
                ConsoleColor.Cyan => Color.Cyan,
                ConsoleColor.Red => Color.Red,
                ConsoleColor.Magenta => Color.Magenta,
                ConsoleColor.Yellow => Color.Yellow,
                ConsoleColor.White => Color.White,
                _ => Color.Pink
            };
        }
    }
}
