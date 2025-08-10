using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Caps.RPG.Rules.Maps
{
    // used for terminal maps only
    public class MapColors
    {
        public static readonly MapColor Black = new(ConsoleColor.Black, ConsoleColor.Gray);
        public static readonly MapColor Blue = new(ConsoleColor.DarkBlue, ConsoleColor.Blue);
        public static readonly MapColor Green = new(ConsoleColor.DarkGreen, ConsoleColor.Green);
        public static readonly MapColor Cyan = new(ConsoleColor.DarkCyan, ConsoleColor.Cyan);
        public static readonly MapColor Red = new(ConsoleColor.DarkRed, ConsoleColor.Red);
        public static readonly MapColor Magenta = new(ConsoleColor.DarkMagenta, ConsoleColor.Magenta);
        public static readonly MapColor Yellow = new(ConsoleColor.DarkYellow, ConsoleColor.Yellow);
        public static readonly MapColor Gray = new(ConsoleColor.DarkGray, ConsoleColor.White);
    }

    public class MapColor
    {
        public ConsoleColor Color { get; set; } = ConsoleColor.Gray;
        public ConsoleColor Highlight { get; set; } = ConsoleColor.White;
        public MapColor(ConsoleColor color, ConsoleColor highlight)
        {
            Color = color;
            Highlight = highlight;
        }

        public override string ToString()
        {
            return Color.ToString();
        }
    }
}
