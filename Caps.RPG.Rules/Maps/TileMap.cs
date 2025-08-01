namespace Caps.RPG.Rules.Maps
{
    public abstract class TileMap
    {
        private static Random random = new Random(0);
        public readonly Dictionary<Helpers.Vector2D, NodeBase?> Tiles = [];

        protected int _gridDepth;
        protected int _gridWidth;

        public TileMap(int gridWidth, int gridDepth)
        {
            _gridWidth = gridWidth;
            _gridDepth = gridDepth;
        }

        public NodeBase? this[Helpers.Vector2D pos]
        {
            get {
                if (!Tiles.TryGetValue(pos, out NodeBase? value))
                    return null;
                return value;
            }
            set {
                Tiles[pos] = value;
            }
        }

        public NodeBase RandomTile(bool walkable = false)
        {
            List<NodeBase> tiles = [.. Tiles.Values];
            if (walkable)
            {
                tiles = [.. Tiles.Values.Where(t => t?.Walkable == true)];
            }
            var randomIndex = random.Next(tiles.Count());
            return tiles.ElementAt(randomIndex);
        }
        public List<NodeBase> NodesInRange(NodeBase center, float range)
        {
            return [.. Tiles.Values.Where(t => t?.GetDistance(center) <= range)];
        }

        public virtual void PrintToConsole()
        {
            for (int r = 0; r < _gridDepth; r++)
            {
                // Indent every other row for hex alignment
                Console.Write(new string(' ', r % 2 == 0 ? 0 : 2));
                int rOffset = r >> 1;
                for (int q = -rOffset; q < _gridWidth - rOffset; q++)
                {
                    var coords = new HexCoords(q, r);
                    if (Tiles.TryGetValue(coords.Pos, out NodeBase? node) && node != null)
                    {
                        // Print walkable as '.' and obstacle as '#'
                        Console.Write(node.Walkable ? ". " : "# ");
                    }
                    else
                    {
                        Console.Write("  ");
                    }
                }
                Console.WriteLine();
            }
        }

        public virtual void PrintWithTileHighlights(List<NodeBase> path)
        {
            for (int r = 0; r < _gridDepth; r++)
            {
                // Indent every other row for hex alignment
                Console.Write(GetPrintingOffset(r));
                int rOffset = r >> 1;
                for (int q = -rOffset; q < _gridWidth - rOffset; q++)
                {
                    var coords = new HexCoords(q, r);
                    if (Tiles.TryGetValue(coords.Pos, out var node) && node != null)
                    {
                        if (path.Contains(node))
                        {
                            // Print path nodes as '*'
                            Console.ForegroundColor = ConsoleColor.Blue;
                            Console.Write("* ");
                            Console.ForegroundColor = ConsoleColor.White;
                        }
                        else
                        {
                            // Print walkable as '.' and obstacle as '#'
                            Console.Write(node.Walkable ? ". " : "# ");
                        }
                    }
                    else
                    {
                        Console.Write("  ");
                    }
                }
                Console.WriteLine();
            }
        }
        public virtual void PrintWithNodesInRange(NodeBase center, float range)
        {
            var nodesInRange = NodesInRange(center, range);
            for (int r = 0; r < _gridDepth; r++)
            {
                Console.Write(GetPrintingOffset(r));
                int rOffset = r >> 1;
                for (int q = -rOffset; q < _gridWidth - rOffset; q++)
                {
                    var coords = new HexCoords(q, r);
                    if (Tiles.TryGetValue(coords.Pos, out var node) && node != null)
                    {
                        if (nodesInRange.Contains(node))
                        {
                            // Print nodes in range as '*'
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.Write(node.Walkable ? "* " : "# ");
                            Console.ForegroundColor = ConsoleColor.White;
                        }
                        else
                        {
                            // Print walkable as '.' and obstacle as '#'
                            Console.Write(node.Walkable ? ". " : "# ");
                        }
                    }
                    else
                    {
                        Console.Write("  ");
                    }
                }
                Console.WriteLine();
            }
        }

        public virtual List<NodeBase> GetLineOfSight(NodeBase start, int range)
        {
            var visibleTiles = new List<NodeBase>();
            var candidates = NodesInRange(start, range);
            foreach (var tile in candidates)
            {
                // Get line from center to tile
                var line = start.GetLineTo(tile, this);
                if (NodeBase.IsWalkable(line))
                    visibleTiles.Add(tile);
                else
                    continue;
            }
            return visibleTiles;
        }

        protected abstract string GetPrintingOffset(int rowNumber);
    }
}
