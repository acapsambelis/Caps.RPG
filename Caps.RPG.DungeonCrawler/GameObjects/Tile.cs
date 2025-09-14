using Caps.RPG.MonoGame;
using Caps.RPG.MonoGame.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Caps.RPG.DungeonCrawler.GameObjects
{
    public class Tile(Sprite sprite, int x, int y)
    {
        public Sprite Sprite { get; } = sprite;
        public Color Color = Color.White;
        public Vector2 Position = GetPosition(sprite, x, y);

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
            //Core.SpriteBatch.Draw(Texture, Position, null, Color, 0f, Origin, 1f, SpriteEffects.None, 1f);
            Sprite.Draw(Core.SpriteBatch, Position);
        }
    }
}
