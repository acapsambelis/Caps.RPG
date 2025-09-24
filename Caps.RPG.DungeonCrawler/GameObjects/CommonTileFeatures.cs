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
