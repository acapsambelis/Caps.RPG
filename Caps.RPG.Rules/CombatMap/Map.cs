using Caps.RPG.Rules.Helpers;

namespace Caps.RPG.Rules.CombatMap
{
    public class Map
    {
        private MapTile[,] tiles;

        public Map(int width, int height)
        {
            tiles = new MapTile[width, height];
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    tiles[x, y] = new MapTile { X = x, Y = y };
                }
            }
        }

        public MapTile[,] GetRange(Vector2D first, Vector2D second)
        {
            MapTile[,] ret = new MapTile[Math.Abs(first.IntX - second.IntX) + 1, Math.Abs(first.IntY - second.IntY) + 1];
            int startX = Math.Min(first.IntX, second.IntX);
            int startY = Math.Min(first.IntY, second.IntY);

            for (int x = 0; x < ret.GetLength(0); x++)
            {
                for (int y = 0; y < ret.GetLength(1); y++)
                {
                    ret[x, y] = tiles[startX + x, startY + y];
                }
            }

            return ret;
        }

        public MapTile[,] GetVisionRange(Vector2D position, int range)
        {
            MapTile[,] ret = new MapTile[range * 2, range * 2];

            for (int x = position.IntX - range; x < ret.GetLength(0); x++)
            {
                for (int y = position.IntY - range; y < ret.GetLength(1); y++)
                {
                    if (position.Distance(new Vector2D(x, y)) < range)
                    {
                        ret[x, y] = tiles[position.IntX + x, position.IntY + y];
                    }
                }
            }
            return ret;
        }

        public IEnumerable<Vector2D> GetNeighbors(Vector2D position)
        {
            var neighbors = new List<Vector2D>();
            var directions = new List<Vector2D>
        {
            new Vector2D(0, 1),  // Up
            new Vector2D(1, 0),  // Right
            new Vector2D(0, -1), // Down
            new Vector2D(-1, 0)  // Left
        };

            foreach (var direction in directions)
            {
                var neighbor = position + direction;

                // Check if the neighbor is within bounds and passable
                if (IsWithinBounds(neighbor) && IsPassable(neighbor))
                {
                    neighbors.Add(neighbor);
                }
            }

            return neighbors;
        }

        private bool IsWithinBounds(Vector2D position)
        {
            return position.IntX >= 0 && position.IntX < tiles.GetLength(0) &&
                   position.IntY >= 0 && position.IntY < tiles.GetLength(1);
        }

        private bool IsPassable(Vector2D position)
        {
            return tiles[position.IntX, position.IntY].Contents.Count() > 0; // 0 = passable, 1 = obstacle
        }
    }
}
