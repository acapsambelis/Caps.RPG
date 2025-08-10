namespace Caps.RPG.Rules.Maps
{
    public class TileFeature
    {
        public string Name { get; set; }
        public bool Walkable { get; set; }
        private MapColor color;
        private ConsoleColor c;
        public TileFeature(string name, bool walkable, MapColor color)
        {
            Name = name;
            Walkable = walkable;
            this.color = color;
            c = color.Color;
        }
        public TileFeature(string name) : this(name, true, MapColors.Gray) { }
        public ConsoleColor Color
        {
            //get { return color.Color; }
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
