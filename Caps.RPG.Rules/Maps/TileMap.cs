using Caps.RPG.Rules.Creatures;
using Caps.RPG.Rules.Helpers;

namespace Caps.RPG.Rules.Maps
{
    public abstract class TileMap
    {
        private static readonly Random random = new(0);
        public readonly Dictionary<Vector2D, TileBase> Tiles = [];

        protected int _gridDepth;
        protected int _gridWidth;

        public int GridWidth => _gridWidth;
        public int GridDepth => _gridDepth;

        public TileMap(int gridWidth, int gridDepth)
        {
            _gridWidth = gridWidth;
            _gridDepth = gridDepth;
        }

        public virtual TileBase this[Vector2D pos]
        {
            get
            {
                if (!Tiles.TryGetValue(pos, out TileBase? value))
                    throw new ArgumentException($"Tile does not exist: {pos.x} + {pos.y}");
                return value;
            }
            set
            {
                Tiles[pos] = value;
            }
        }

        public TileBase this[double x, double y]
        {
            get { return this[new Vector2D(x, y)]; }
            set { this[new Vector2D(x, y)] = value; }
        }

        public TileBase this[TileFeature feature]
        {
            get
            {
                foreach (var tile in Tiles.Values)
                {
                    if (tile.Features.Any(f => f.Value == feature))
                        return tile;
                }
                throw new ArgumentException("No tile with the specified feature exists.");
            }
        }

        public TileBase this[Creature creature]
        {
            get
            {
                foreach (var tile in Tiles.Values)
                {
                    if (tile.Features.Any(f => f.Value is Combattant c && c.Creature == creature))
                        return tile;
                }
                throw new ArgumentException("No tile with the specified feature exists.");
            }
        }

        public TileBase RandomEmptyTile()
        {
            List<TileBase> tiles = [.. Tiles.Values.Where(t => t.IsEmpty())];
            var randomIndex = random.Next(tiles.Count);
            return tiles.ElementAt(randomIndex);
        }

        public TileBase RandomTile(bool requireWalkable = false)
        {
            List<TileBase> tiles = [.. Tiles.Values];
            if (requireWalkable)
            {
                tiles = [.. Tiles.Values.Where(t => t?.Walkable == true)];
            }
            var randomIndex = random.Next(tiles.Count);
            return tiles.ElementAt(randomIndex);
        }

        public List<TileBase> NodesInRange(TileBase center, float range)
        {
            return [.. Tiles.Values.Where(t => t?.GetDistance(center) <= range)];
        }

        public virtual void PrintToConsole(Dictionary<TileBase, ConsoleColor?>? highlights = null)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("  ");
            for (int x = 0; x < _gridWidth; x++)
            {
                Console.Write((x % 10).ToString() + " ");
            }
            Console.WriteLine();
            for (int r = 0; r < _gridDepth; r++)
            {
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write((r % 10).ToString() + " ");
                Console.Write(GetPrintingOffset(r));
                int rOffset = r >> 1;
                for (int q = -rOffset; q < _gridWidth - rOffset; q++)
                {
                    var coords = new HexCoords(q, r);
                    if (Tiles.TryGetValue(coords.Pos, out var node) && node != null)
                    {
                        ConsoleColor backgroundColor;
                        ConsoleColor foregroundColor;
                        if (highlights != null && highlights.TryGetValue(node, out ConsoleColor? value))
                        {
                            foregroundColor = value ?? node.HighlightColor;
                            backgroundColor = value == null ? node.Color : ConsoleColor.Black;
                        }
                        else
                        {
                            foregroundColor = node.Color;
                            backgroundColor = ConsoleColor.Black;
                        }
                        PrintColor(node.TextRepresentation().ToString(), foregroundColor, backgroundColor);
                        Console.Write(' ');
                    }
                    else
                    {
                        Console.Write("  ");
                    }
                }
                Console.WriteLine();
            }
            Console.ForegroundColor = ConsoleColor.White;
        }

        private static void PrintColor(string text, ConsoleColor foreground, ConsoleColor background)
        {
            Console.ForegroundColor = foreground;
            Console.BackgroundColor = background;
            Console.Write(text);
            Console.ResetColor();
        }

        public virtual List<TileBase> GetLineOfSight(TileBase start, int range)
        {
            var visibleTiles = new List<TileBase>();
            var candidates = NodesInRange(start, range);
            foreach (var tile in candidates)
            {
                // Get line from center to tile
                var line = start.GetLineTo(tile, this);
                if (TileBase.IsWalkable(line))
                    visibleTiles.Add(tile);
                else
                    continue;
            }
            return visibleTiles;
        }

        protected abstract string GetPrintingOffset(int rowNumber);

        public virtual TileBase[] GetFeaturesWithinRange(TileBase source, bool considerLOS, double? range, Func<TileFeature, bool>? filter = null)
        {
            List<TileBase> tilesInRange;
            if (considerLOS)
            {
                tilesInRange = GetLineOfSight(source, (int)(range ?? Math.Max(_gridWidth, _gridDepth)));
            }
            else
            {
                tilesInRange = range != null
                    ? NodesInRange(source, (float)range.Value)
                    : [.. Tiles.Values];
            }
            var resultTiles = new List<TileBase>();
            foreach (var tile in tilesInRange)
            {
                if (tile.Features.Count > 0)
                {
                    if (filter != null)
                    {
                        if (tile.Features.Any(f => filter(f.Value)))
                        {
                            resultTiles.Add(tile);
                        }
                    }
                    else
                    {
                        resultTiles.Add(tile);
                    }
                }
            }
            return resultTiles.ToArray();
        }

        public virtual TileBase[] GetNearestFeature(TileBase source, Type featureType, bool considerLOS = false, Func<TileFeature, bool>? filter = null)
        {
            var tilesWithFeature = GetFeaturesWithinRange(
                source,
                considerLOS,
                Math.Max(_gridWidth, _gridDepth),
                f => f.GetType() == featureType && (filter == null || filter(f))
            );
            if (tilesWithFeature.Length == 0)
                return [];
            double nearestDistance = double.MaxValue;
            TileBase? nearestTile = null;
            foreach (var tile in tilesWithFeature)
            {
                double distance = source.GetDistance(tile);
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearestTile = tile;
                }
            }
            if (nearestTile != null)
                return [nearestTile];
            return [];
        }

        public virtual TileBase[] GetNearestEnemy(TileBase source, bool considerLOS = false)
        {
            Combattant sourceCombattant = source.GetFeature<Combattant>();
            var tilesWithFeature = GetFeaturesWithinRange(
                source,
                considerLOS,
                Math.Max(_gridWidth, _gridDepth),
                f => f is Combattant c && c.Team != sourceCombattant.Team
            );
            if (tilesWithFeature.Length == 0)
                return [];
            double nearestDistance = double.MaxValue;
            TileBase? nearestTile = null;
            foreach (var tile in tilesWithFeature)
            {
                double distance = source.GetDistance(tile);
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearestTile = tile;
                }
            }
            if (nearestTile != null)
                return [nearestTile];
            return [];
        }

        #region Shape Methods

        public TileBase[] GetTiles(TileBase source, double range)
        {
            return GetCircle(source, range);
        }

        public TileBase[] GetTiles(TileBase source, MapShape shape, double range)
        {
            return shape switch
            {
                MapShape.None => [source],
                MapShape.Tile => [source],
                MapShape.Circle => GetCircle(source, range),
                MapShape.Cone => GetCone(source, range),
                MapShape.FromSourceLine => GetFromSourceLine(source, range),
                MapShape.FreestandingLine => GetFreestandingLine(source, range),
                MapShape.Radius => GetCircle(source, range),
                _ => throw new NotImplementedException($"Shape {shape} is not implemented.")
            };
        }
        
        public abstract TileBase[] GetCircle(TileBase source, double range);
        public abstract TileBase[] GetFreestandingLine(TileBase source, double range);
        public abstract TileBase[] GetCone(TileBase source, double range);
        public abstract TileBase[] GetFromSourceLine(TileBase source, double range);

        #endregion
    }
}
