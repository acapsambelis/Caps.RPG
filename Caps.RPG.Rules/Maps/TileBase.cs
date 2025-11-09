namespace Caps.RPG.Rules.Maps
{
    public abstract class TileBase
    {
        internal TileBase? Connection { get; private set; }
        internal float G { get; private set; }
        internal float H { get; private set; }
        internal float F => G + H;

        public List<TileBase?> Neighbors { get; protected set; }
        public bool Walkable { get { return !Features.Values.Any(feature => !feature.Walkable); } }
        public ICoords Coords;
        public SortedList<int, TileFeature> Features { get; protected set; } = [];
        public bool Highlighted { get; set; } = false;

        public TileBase(ICoords coords)
        {
            Coords = coords;
            Neighbors = [];
            Connection = null;
            G = 0;
            H = 0;
        }

        public float GetDistance(TileBase other) => Coords.GetDistance(other.Coords); // Helper to reduce noise in pathfinding
        internal abstract void CacheNeighbors(TileMap map);

        internal void SetConnection(TileBase connection) => Connection = connection;
        internal void SetG(float g) => G = g;
        internal void SetH(float h) => H = h;

        public abstract List<TileBase> GetLineTo(TileBase target, TileMap map);

        public static bool IsWalkable(IEnumerable<TileBase> path)
        {
            return path.All(n => n.Walkable);
        }

        internal static double LinearInterp(double a, double b, double t)
        {
            return a + (b - a) * t;
        }

        public T? GetFeature<T>() where T : TileFeature
        {
            return Features.Values.OfType<T>().FirstOrDefault();
        }

        public bool IsEmpty()
        {
            return Features.Count == 0;
        }

        public List<TileBase> FindPath(TileBase targetNode)
        {
            if (targetNode.Walkable)
                return Pathfinding.FindPathToEmpty(this, targetNode);
            else
                return Pathfinding.FindPathToFilled(this, targetNode);
        }
    }

    public interface ICoords
    {
        public float GetDistance(ICoords other);
        public Helpers.Vector2D Pos { get; set; }
    }
}
