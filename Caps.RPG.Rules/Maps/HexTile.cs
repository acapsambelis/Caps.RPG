using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Caps.RPG.Rules.Maps
{
    public class HexTile : NodeBase
    {
        public HexTile(bool walkable, HexCoords coords)
        {
            Coords = coords;
            Walkable = walkable;
        }

        public override void CacheNeighbors(TileMap map)
        {
            Neighbors = map.Tiles.Where(t => Coords.GetDistance(t.Value.Coords) == 1).Select(t => t.Value).ToList();
        }
    }

    public struct HexCoords : ICoords
    {
        private static readonly float Sqrt3 = (float)Math.Sqrt(3);

        private readonly int q; // Column
        private readonly int r; // Row

        public Helpers.Vector2D Pos { get; set; }

        public HexCoords(int q, int r)
        {
            this.q = q;
            this.r = r;
            Pos = q * new Helpers.Vector2D(Sqrt3, 0) + r * new Helpers.Vector2D(Sqrt3 / 2, 1.5f);
        }

        public float GetDistance(ICoords other) => (this - (HexCoords)other).AxialLength();

        private int AxialLength()
        {
            if (q == 0 && r == 0) return 0;
            if (q > 0 && r >= 0) return q + r;
            if (q <= 0 && r > 0) return -q < r ? r : -q;
            if (q < 0) return -q - r;
            return -r > q ? -r : q;
        }

        public static HexCoords operator -(HexCoords a, HexCoords b)
        {
            return new HexCoords(a.q - b.q, a.r - b.r);
        }
    }
}
