using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Caps.RPG.MonoGame;
using Caps.RPG.MonoGame.Graphics;
using Caps.RPG.Rules.Creatures;
using Caps.RPG.Rules.Maps;
using Microsoft.Xna.Framework;

namespace Caps.RPG.DungeonCrawler.GameObjects
{
    public class Map
    {
        private readonly TileMap mapData;
        private readonly Tile[,] tiles;
        private readonly Dictionary<string, Sprite> characterSprites;

        public static Sprite Grassland { get; set; }

        public Map(TileMap mapData, Dictionary<string, Sprite> characterSprites)
        {
            this.mapData = mapData;
            tiles = new Tile[mapData.GridWidth, mapData.GridDepth];
            this.characterSprites = characterSprites;

            // Calculate offset so that the center of the map is at (0, 0)
            int centerX = mapData.GridWidth / 2;
            int centerY = mapData.GridDepth / 2;

            for (int x = 0; x < mapData.GridWidth; x++)
            {
                for (int y = 0; y < mapData.GridDepth; y++)
                {
                    if (mapData[x, y].Features.Any(f => f.Value is Combattant && characterSprites.ContainsKey(f.Value.Name + " " + (f.Value as Combattant).Team)))
                    {
                        var feature = mapData[x, y].Features.First(f => characterSprites.ContainsKey(f.Value.Name + " " + (f.Value as Combattant).Team));
                        tiles[x, y] = new Tile(Grassland, mapData[x, y], x - centerX, y - centerY, characterSprites[feature.Value.Name + " " + (feature.Value as Combattant).Team]);
                    }
                    else
                    {
                        tiles[x, y] = new Tile(Grassland, mapData[x, y], x - centerX, y - centerY);
                    }
                }
            }
        }

        public void Update()
        {
            var tile = GetTileAtMousePosition();
            if (tile != null)
                tile.Color = Color.Gray;
            foreach (Tile t in tiles)
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
            for (int x = 0; x < tiles.GetLength(0); x++)
            {
                for (int y = 0; y < tiles.GetLength(1); y++)
                {
                    var d = Vector2.Distance(MousePosition, tiles[x, y].Position);
                    if (d < minD)
                    {
                        minD = d;
                        selected = tiles[x, y];
                    }
                }
            }

            if (minD < Math.Max(selected.BaseSprite.Origin.Y, selected.BaseSprite.Origin.X)) return selected;

            return null;
        }

        public void Draw()
        {
            for (int x = 0; x < tiles.GetLength(0); x++)
            {
                for (int y = 0; y < tiles.GetLength(1); y++)
                {
                    tiles[x, y].Draw();
                }
            }
        }
    }
}
