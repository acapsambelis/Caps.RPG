using Caps.RPG.DungeonCrawler.UI;
using Caps.RPG.MonoGame.Graphics;
using Caps.RPG.Rules.Creatures;
using Caps.RPG.Rules.Maps;
using Microsoft.Xna.Framework.Graphics;
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
        
        private readonly Map map;
        private readonly Texture2D icon;
        private readonly Sprite deathSprite;
        private readonly Texture2D deathIcon;

        public CombattantEntity(ref Combattant combattant, Sprite sprite, ref Map map, GeonBit.UI.EventCallback ActionClicked, GeonBit.UI.EventCallback EndTurn)
        {
            Combattant = combattant;
            Combattant.OnPositionChanged += Moved;
            Combattant.OnHealthChanged += HealthChanged;
            Combattant.OnUnconsious += Unconsious;

            Sprite = sprite;
            icon = Sprite.GetTextureWithColor();
            deathSprite = CommonTileFeatures.CreateDeathSprite(sprite);
            deathIcon = deathSprite.GetTextureWithColor();

            InitiativeTracker = new(sprite, ref combattant, ref map);
            CharacterControlsPanel = new(combattant, icon, map, ActionClicked, EndTurn);

            this.map = map;
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

        public bool SetVisibility(Combattant combattant)
        {
            CharacterControlsPanel.Panel.Visible = Combattant == combattant;
            return Combattant == combattant;
        }
    }
}
