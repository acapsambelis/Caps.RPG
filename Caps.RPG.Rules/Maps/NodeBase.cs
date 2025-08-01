using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Caps.RPG.Rules.Maps
{
    public abstract class NodeBase
    {
        public NodeBase Connection { get; private set; }
        public float G { get; private set; }
        public float H { get; private set; }
        public float F => G + H;
        public List<NodeBase?> Neighbors { get; protected set; }
        public bool Walkable { get; protected set; }

        public ICoords Coords;
        public float GetDistance(NodeBase other) => Coords.GetDistance(other.Coords); // Helper to reduce noise in pathfinding
        public abstract void CacheNeighbors(TileMap map);

        public void SetConnection(NodeBase connection) => Connection = connection;
        public void SetG(float g) => G = g;
        public void SetH(float h) => H = h;

        public abstract List<NodeBase> GetLineTo(NodeBase target, TileMap map);

        public static bool IsWalkable(IEnumerable<NodeBase> path)
        {
            return path.All(n => n.Walkable);
        }

        public static double LinearInterp(double a, double b, double t)
        {
            return a + (b - a) * t;
        }
    }

    public interface ICoords
    {
        public float GetDistance(ICoords other);
        public Helpers.Vector2D Pos { get; set; }
    }
}
