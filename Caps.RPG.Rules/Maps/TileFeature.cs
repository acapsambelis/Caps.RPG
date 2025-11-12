using Caps.Util;

namespace Caps.RPG.Rules.Maps
{
    public class TileFeature(string name, bool walkable)
    {
        public TileFeature(string name) : this(name, true) { }

        public string Name { get; set; } = name;
        public bool Walkable { get; set; } = walkable;

        public char TextRepresentation()
        {
            return Walkable ? '.' : Name.ToCharArray()[0];
        }
    }
}
