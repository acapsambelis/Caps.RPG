using System;
using Caps.RPG.MonoGame;
using Caps.RPG.MonoGame.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Caps.RPG.DungeonCrawler.GameObjects
{
    public class Map
    {
        public Tile[,] Tiles { get; private set; }

        public static Sprite Grassland { get; set; }
        public static Sprite Rock { get; set; }

        public Map(int width, int height)
        {
            Tiles = new Tile[width, height];

            // Calculate offset so that the center of the map is at (0, 0)
            int centerX = width / 2;
            int centerY = height / 2;

            for (int x = 0; x < Tiles.GetLength(0); x++)
            {
                for (int y = 0; y < Tiles.GetLength(1); y++)
                {
                    // Offset tile positions by subtracting center coordinates
                    Tiles[x, y] = new Tile(Grassland, x - centerX, y - centerY);
                }
            }
        }

        public void Update()
        {
            var tile = GetTileAtMousePosition();
            if (tile != null)
                tile.Color = Color.Gray;
            foreach (Tile t in Tiles)
            {
                if (t != tile)
                {
                    t.Color = Color.White;
                }
            }
        }

        private Tile GetTileAtMousePosition()
        {
            var MousePosition = Core.GetCursorPosition();
            float minD = float.MaxValue;
            Tile selected = null;
            for (int x = 0; x < Tiles.GetLength(0); x++)
            {
                for (int y = 0; y < Tiles.GetLength(1); y++)
                {
                    var d = Vector2.Distance(MousePosition, Tiles[x, y].Position);
                    if (d < minD)
                    {
                        minD = d;
                        selected = Tiles[x, y];
                    }
                }
            }

            if (minD < Math.Max(selected.Sprite.Origin.Y, selected.Sprite.Origin.X)) return selected;

            return null;
        }

        public void Draw()
        {
            for (int x = 0; x < Tiles.GetLength(0); x++)
            {
                for (int y = 0; y < Tiles.GetLength(1); y++)
                {
                    Tiles[x, y].Draw();
                }
            }
        }
    }
}
