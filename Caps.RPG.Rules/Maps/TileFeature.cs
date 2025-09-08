using Caps.Util;

namespace Caps.RPG.Rules.Maps
{
    public class TileFeature
    {
        public string Name { get; set; }
        public bool Walkable { get; set; }
        private TerminalColor color;
        private ConsoleColor c;
        public TileFeature(string name, bool walkable, TerminalColor color)
        {
            Name = name;
            Walkable = walkable;
            this.color = color;
            c = color.Color;
        }
        public TileFeature(string name) : this(name, true, TerminalColors.Gray) { }
        public ConsoleColor Color
        {
            get { return c; }
            set { c = value; }
        }
        public ConsoleColor HighlightColor
        {
            get { return color.Highlight; }
        }

        public char TextRepresentation()
        {
            return Walkable ? '.' : Name.ToCharArray()[0];
        }
    }
}
