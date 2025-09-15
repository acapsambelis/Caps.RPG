using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Caps.RPG.MonoGame;
using Caps.RPG.MonoGame.Graphics;
using Caps.RPG.Rules.Maps;

namespace Caps.RPG.DungeonCrawler.GameObjects
{
    public class Tile
    {
        public Sprite BaseSprite { get; }
        public Color Color = Color.White;
        public readonly Vector2 Position;
        public readonly TileBase TileBase;
        public SortedList<int, Sprite> TileFeatureSprites = [];

        public Tile(Sprite baseSprite, TileBase data, int x, int y, Sprite uniqueSprite = null)
        {
            BaseSprite = baseSprite;
            Position = GetPosition(baseSprite, x, y);
            TileBase = data;
            foreach (var feature in data.Features)
            {
                Sprite commonSprite = CommonTileFeatures.GetCommonSprite(feature.Value.Name);
                if (commonSprite != null)
                {
                    TileFeatureSprites.Add(feature.Key, commonSprite);
                }
                else
                {
                    TileFeatureSprites.Add(feature.Key, uniqueSprite);
                }
            }
        }

        private static Vector2 GetPosition(Sprite sprite, int x, int y)
        {
            if (sprite.Height > sprite.Width)
                return new(
                     x * sprite.Width + (y % 2 * sprite.Width / 2) + sprite.Width / 2,
                     y * 0.75f * sprite.Height + sprite.Height / 2);
            else
                return new(
                    x * 0.75f * sprite.Width + sprite.Width / 2,
                    y * sprite.Height + (x % 2 * sprite.Height / 2) + sprite.Height / 2);
        }

        public void Draw()
        {
            BaseSprite.Draw(Core.SpriteBatch, Position);
            foreach (int index in TileFeatureSprites.Keys)
            {
                if (TileFeatureSprites[index] != null)
                    TileFeatureSprites[index].Draw(Core.SpriteBatch, Position);
            }
        }
    }
}
