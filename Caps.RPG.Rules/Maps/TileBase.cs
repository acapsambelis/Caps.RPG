using Caps.Util;

namespace Caps.RPG.Rules.Maps
{
    public abstract class TileBase
    {
        internal TileBase? Connection { get; private set; }
        public float G { get; private set; }
        public float H { get; private set; }
        public float F => G + H;

        public List<TileBase?> Neighbors { get; protected set; }
        public bool Walkable { get { return !Features.Values.Any(feature => !feature.Walkable); } }
        public ICoords Coords;
        public SortedList<int, TileFeature> Features { get; protected set; } = [];
        public ConsoleColor Color
        {
            get { return Features.Count > 0 ? Features.First().Value.Color : ConsoleColor.DarkGreen; }
        }
        public ConsoleColor HighlightColor
        {
            get { return Features.Count > 0 ? Features.First().Value.HighlightColor : ConsoleColor.Green; }
        }

        public TileBase(ICoords coords)
        {
            Coords = coords;
            Neighbors = [];
            Connection = null;
            G = 0;
            H = 0;
        }

        public virtual char TextRepresentation()
        {
            if (Features.Count == 0) return '.';
            var sortedFeatures = Features.OrderBy(f => f.Key).ToList();
            if (sortedFeatures.Last().Value is TileFeature feature)
            {
                return feature.TextRepresentation();
            }
            return '#';
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

        public T GetFeature<T>() where T : TileFeature
        {
            return Features.Values.OfType<T>().FirstOrDefault() ?? throw new InvalidOperationException($"No feature of type {typeof(T).Name} found.");
        }

        public bool IsEmpty()
        {
            return Features.Count == 0;
        }
    }

    public interface ICoords
    {
        public float GetDistance(ICoords other);
        public Helpers.Vector2D Pos { get; set; }
    }
}
