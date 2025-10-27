using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Caps.RPG.MonoGame;
using Caps.RPG.MonoGame.Graphics;
using Caps.RPG.Rules.Maps;
using System;

namespace Caps.RPG.DungeonCrawler.GameObjects
{
    public class Tile : Clickable
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
                TileFeatureSprites[index]?.Draw(Core.SpriteBatch, Position);
            }
            if (TileBase.Highlighted)
            {
                CommonTileFeatures.Highlight.Draw(Core.SpriteBatch, Position);
            }
            // Draw coordinates as debug text
            var font = Scenes.BaseScene.Font;
            if (font != null)
            {
                string coordsText = $"{TileBase.Coords.Pos.IntX:0},{TileBase.Coords.Pos.IntY:0}";
                Core.SpriteBatch.DrawString(font, coordsText, Position, Color.Black);
            }
        }

        public override bool IsInside(Vector2 point)
        {
            // the tile is drawn centered at Position, with BaseSprite's width/height
            float width = BaseSprite.Width;
            float height = BaseSprite.Height;
            float left = Position.X - width / 2;
            float top = Position.Y - height / 2;
            float right = left + width;
            float bottom = top + height;

            // Rectangle check
            if (point.X < left || point.X > right || point.Y < top || point.Y > bottom)
                return false;

            // If hex tile (height > width), use hexagon hit test
            if (height > width)
            {
                // Axial hexagon math (pointy-topped)
                float relX = point.X - Position.X;
                float relY = point.Y - Position.Y;
                float q = (float)(Math.Sqrt(3) / 3 * relX - 1.0 / 3 * relY) / (height / 2);
                float r = (2.0f / 3 * relY) / (height / 2);
                return Math.Abs(q) + Math.Abs(r) + Math.Abs(-q - r) <= 1.0f;
            }
            else
            {
                // Flat-topped hex or rectangle: just use rectangle
                return true;
            }
        }

        public void Update()
        {
            
        }
    }
}
