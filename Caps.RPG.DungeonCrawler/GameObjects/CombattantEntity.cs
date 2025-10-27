using Caps.RPG.DungeonCrawler.UI;
using Caps.RPG.MonoGame.Graphics;
using Caps.RPG.Rules.Creatures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Caps.RPG.DungeonCrawler.GameObjects
{
    public class CombattantEntity : IUIEntity
    {
        public Combattant Combattant { get; }
        public Sprite Sprite { get; }
        public InitiativeTracker InitiativeTracker { get; }
        public CharacterControlsPanel CharacterControlsPanel { get; }
        
        private Map map;

        private readonly Sprite deathSprite;

        public CombattantEntity(ref Combattant combattant, Sprite sprite, Sprite deathSprite, ref InitiativeTracker initiativeTracker, ref CharacterControlsPanel characterControlsPanel, ref Map map)
        {
            Combattant = combattant;
            Combattant.OnPositionChanged += Moved;
            Combattant.OnHealthChanged += HealthChanged;
            Sprite = sprite;
            InitiativeTracker = initiativeTracker;
            CharacterControlsPanel = characterControlsPanel;
            this.map = map;
            Combattant.OnUnconsious += Unconsious;
            //deathSprite = new(sprite)
            //{
            //    Color = Microsoft.Xna.Framework.Color.Gray,
            //    Rotation = (float)(Math.PI / 2)
            //};
            this.deathSprite = deathSprite;
        }

        private void Moved(object sender, PositionChangedEventArgs e)
        {
            // Update the position of the sprite on the map
            var newTile = map[Combattant.Position];
            newTile.TileFeatureSprites.Add(0, Sprite);
            if (e.OldPosition != null)
            {
                var oldTile = map[e.OldPosition];
                oldTile.TileFeatureSprites.Remove(0);
            }
        }

        private void HealthChanged(object sender, HealthChangedEventArgs e)
        {
            // Update health bar or other UI elements if needed
            //InitiativeTracker.
        }

        private void Unconsious(object sender, EventArgs e)
        {
            var tile = map[Combattant.Position];
            tile.TileFeatureSprites.Remove(0);
            tile.TileFeatureSprites.Add(0, deathSprite);
            InitiativeTracker.CharacterSprite = deathSprite;
        }

        public void Update()
        {
            InitiativeTracker.Update();
            CharacterControlsPanel.Update();
        }
    }
}
