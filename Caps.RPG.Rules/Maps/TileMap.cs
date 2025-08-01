namespace Caps.RPG.Rules.Maps
{
    public abstract class TileMap
    {
        public readonly Dictionary<Helpers.Vector2D, NodeBase> Tiles = [];

        protected int _gridDepth;
        protected int _gridWidth;

        public TileMap(int gridWidth, int gridDepth)
        {
            _gridWidth = gridWidth;
            _gridDepth = gridDepth;
        }

        public NodeBase this[Helpers.Vector2D pos]
        {
            get { return Tiles[pos]; }
            set { Tiles[pos] = value; }
        }

        public NodeBase RandomTile(bool walkable = false)
        {
            var random = new Random();
            List<NodeBase> tiles = [.. Tiles.Values];
            if (walkable)
            {
                tiles = [.. Tiles.Values.Where(t => t.Walkable == true)];
            }
            var randomIndex = random.Next(tiles.Count());
            return tiles.ElementAt(randomIndex);
        }
        public List<NodeBase> NodesInRange(NodeBase center, float range)
        {
            return Tiles.Values.Where(t => t.GetDistance(center) <= range).ToList();
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
                    if (Tiles.TryGetValue(coords.Pos, out var node))
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

        public virtual void PrintWithPath(List<NodeBase> path)
        {
            for (int r = 0; r < _gridDepth; r++)
            {
                // Indent every other row for hex alignment
                Console.Write(GetOffset(r));
                int rOffset = r >> 1;
                for (int q = -rOffset; q < _gridWidth - rOffset; q++)
                {
                    var coords = new HexCoords(q, r);
                    if (Tiles.TryGetValue(coords.Pos, out var node))
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
                Console.Write(GetOffset(r));
                int rOffset = r >> 1;
                for (int q = -rOffset; q < _gridWidth - rOffset; q++)
                {
                    var coords = new HexCoords(q, r);
                    if (Tiles.TryGetValue(coords.Pos, out var node))
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

        protected abstract string GetOffset(int rowNumber);
    }
}
