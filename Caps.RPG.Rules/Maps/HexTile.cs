using Caps.RPG.Rules.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Caps.RPG.Rules.Maps
{
    public class HexTile : NodeBase
    {
        internal HexCoords.Cube CubeCoords;

        public HexTile(bool walkable, HexCoords coords)
        {
            Coords = coords;
            Walkable = walkable;
            CubeCoords = coords.ToCube();
        }

        public override void CacheNeighbors(TileMap map)
        {
            Neighbors = map.Tiles.Where(t => t.Value != null && Coords.GetDistance(t.Value.Coords) == 1).Select(t => t.Value).ToList();
        }

        public override List<NodeBase> GetLineTo(NodeBase target, TileMap map)
        {
            if (target is not HexTile)
            {
                throw new ArgumentException("Incompatible NodeBase type");
            }

            List<NodeBase> line = [];
            //line.Add(this);
            var n = HexCoords.CubeDistance(this, (HexTile)target);

            for (int i = 0; i < n; i++)
            {
                var temp = HexCoords.CubeLerp(this, (HexTile)target, 1.0 / n * i, (HexMap)map);
                if (temp != null)
                {
                    line.Add(temp);
                }
            }

            line.Add(target);
            return line;
        }

        public override string ToString()
        {
            return $"HexTile at {Coords.ToString()} (Walkable: {Walkable})";
        }
    }

    public struct HexCoords : ICoords
    {
        private static readonly float Sqrt3 = (float)Math.Sqrt(3);

        private readonly int q; // Column
        private readonly int r; // Row

        public Vector2D Pos { get; set; }

        public HexCoords(int q, int r)
        {
            this.q = q;
            this.r = r;
            Pos = q * new Vector2D(Sqrt3, 0) + r * new Vector2D(Sqrt3 / 2, 1.5f);
        }

        public override string ToString()
        {
            return $"r {r} + q {q}";
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

        internal Cube ToCube()
        {
            Cube ret = new()
            {
                x = q,
                z = r
            };
            ret.y = -ret.x - ret.z;
            return ret;
        }

        public static HexCoords operator -(HexCoords a, HexCoords b)
        {
            return new HexCoords(a.q - b.q, a.r - b.r);
        }

        internal struct Cube
        {
            public double x;
            public double y;
            public double z;

            public Cube(double x, double y, double z)
            {
                this.x = x;
                this.y = y;
                this.z = z;
            }

            internal Cube CubeRound()
            {
                int rx = (int)Math.Round(x);
                int ry = (int)Math.Round(y);
                int rz = (int)Math.Round(z);

                int x_diff = (int)Math.Abs(rx - x);
                int y_diff = (int)Math.Abs(ry - y);
                int z_diff = (int)Math.Abs(rz - z);

                if (x_diff > y_diff && x_diff > z_diff)
                    rx = -ry - rz;
                else if (y_diff > z_diff)
                    ry = -rx - rz;
                else
                    rz = -rx - ry;

                return new Cube(rx, ry, rz);
            }

            public (double r, double q) ToAxial()
            {
                return (x, z);
            }

            public override bool Equals(object? obj)
            {
                return obj is Cube cube &&
                       x == cube.x &&
                       y == cube.y &&
                       z == cube.z;
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(x, y, z);
            }

            public static bool operator ==(Cube left, Cube right)
            {
                return left.Equals(right);
            }

            public static bool operator !=(Cube left, Cube right)
            {
                return !(left == right);
            }
        }

        internal static NodeBase? CubeLerp(HexTile a, HexTile b, double t, HexMap map)
        {
            Cube target = new Cube(NodeBase.LinearInterp(a.CubeCoords.x, b.CubeCoords.x, t),
                                   NodeBase.LinearInterp(a.CubeCoords.y, b.CubeCoords.y, t),
                                   NodeBase.LinearInterp(a.CubeCoords.z, b.CubeCoords.z, t)).CubeRound();
            return map[new HexCoords((int)target.x, (int)target.z).Pos];
        }

        internal static double CubeDistance(HexTile a, HexTile b)
        {
            return (Math.Abs(a.CubeCoords.x - b.CubeCoords.x) + Math.Abs(a.CubeCoords.y - b.CubeCoords.y) + Math.Abs(a.CubeCoords.z - b.CubeCoords.z)) / 2;
        }
    }
}
