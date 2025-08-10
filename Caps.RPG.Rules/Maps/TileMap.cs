using Caps.RPG.Rules.Helpers;

namespace Caps.RPG.Rules.Maps
{
    public abstract class TileMap
    {
        private static readonly Random random = new(0);
        public readonly Dictionary<Vector2D, TileBase> Tiles = [];

        protected int _gridDepth;
        protected int _gridWidth;

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

        public TileBase RandomTile(bool walkable = false)
        {
            List<TileBase> tiles = [.. Tiles.Values];
            if (walkable)
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
    }
}
