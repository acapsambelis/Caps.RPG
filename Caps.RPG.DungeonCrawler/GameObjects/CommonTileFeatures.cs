using Caps.RPG.MonoGame;
using Caps.RPG.MonoGame.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Caps.RPG.DungeonCrawler.GameObjects
{
    public class CommonTileFeatures
    {
        public static Sprite Highlight { get; set; }
        public static Sprite Rock { get; set; }

        private static Sprite _neonMagentaDebug;
        public static Sprite NeonMagentaDebug {
            get
            {
                _neonMagentaDebug ??= Sprite.CreateTextureSprite(Core.GraphicsDevice, 16, 16, Microsoft.Xna.Framework.Color.Magenta);
                return _neonMagentaDebug;
            }
        }

        public static Sprite CreateDeathSprite(Sprite alive)
        {
            return new Sprite(alive)
            {
                Color = Microsoft.Xna.Framework.Color.Gray,
                Rotation = (float)(Math.PI / 2)
            };
        }

        public static Sprite GetCommonSprite(string name)
        {
            var property = typeof(CommonTileFeatures).GetProperty(name, System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
            if (property != null && property.PropertyType == typeof(Sprite))
            {
                return (Sprite)property.GetValue(null);
            }
            return null;
        }
    }
}
