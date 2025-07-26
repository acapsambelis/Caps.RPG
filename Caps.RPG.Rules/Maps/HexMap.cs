using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Caps.RPG.Rules.Maps
{
    public class HexMap : TileMap
    {
        private static readonly int _gridWidth = 16;
        private static readonly int _gridDepth = 9;

        public static HexMap GenerateRandom(int seed, int obstacleWeight)
        {
            var map = new HexMap();
            var random = new Random(seed);
            for (var r = 0; r < _gridDepth; r++)
            {
                var rOffset = r >> 1;
                for (var q = -rOffset; q < _gridWidth -rOffset; q++)
                {
                    var tile = new HexTile(random.Next(1, 20) > obstacleWeight, new HexCoords(q, r));
                    map.Tiles.Add(tile.Coords.Pos, tile);
                }
            }

            foreach (var tile in map.Tiles.Values) tile.CacheNeighbors(map);

            return map;
        }
        public override void PrintToConsole()
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

        public override void PrintWithPath(List<NodeBase> path)
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
    }
}
