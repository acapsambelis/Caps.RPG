using Caps.RPG.MonoGame;
using Caps.RPG.MonoGame.Graphics;
using Caps.RPG.Rules.Creatures;
using Caps.RPG.Rules.Maps;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Caps.RPG.DungeonCrawler.GameObjects
{
    public class Map
    {
        public static Sprite Grassland { get; set; }

        private readonly TileMap mapData;
        private readonly Tile[,] tiles;
        private readonly Dictionary<string, Sprite> characterSprites;
        private Tile clickedTile;

        public Tile[,] Tiles => tiles;

        public Tile this[TileBase tileBase]
        {
            get
            {
                for (int x = 0; x < tiles.GetLength(0); x++)
                {
                    for (int y = 0; y < tiles.GetLength(1); y++)
                    {
                        if (tiles[x, y].TileBase == tileBase)
                        {
                            return tiles[x, y];
                        }
                    }
                }
                throw new ArgumentException("TileBase not found in map.");
            }
        }

        public Tile ClickedTile
        {
            get
            {
                var ret = clickedTile;
                clickedTile = null;
                return ret;
            }
            private set
            {
                clickedTile = value;
            }
        }

        public Map(TileMap mapData, Dictionary<string, Sprite> characterSprites)
        {
            this.mapData = mapData;
            tiles = new Tile[mapData.GridWidth, mapData.GridDepth];
            this.characterSprites = characterSprites;

            for (int x = 0; x < mapData.GridWidth; x++)
            {
                for (int y = 0; y < mapData.GridDepth; y++)
                {
                    if (mapData[x, y].Features.Any(f => f.Value is Combattant && characterSprites.ContainsKey(f.Value.Name + " " + (f.Value as Combattant).Team)))
                    {
                        var feature = mapData[x, y].Features.First(f => characterSprites.ContainsKey(f.Value.Name + " " + (f.Value as Combattant).Team));
                        tiles[x, y] = new Tile(Grassland, mapData[x, y], x, y, characterSprites[feature.Value.Name + " " + (feature.Value as Combattant).Team]);
                    }
                    else
                    {
                        tiles[x, y] = new Tile(Grassland, mapData[x, y], x, y);
                    }
                    tiles[x, y].OnClick += TileClicked;
                }
            }
        }

        private void TileClicked(object sender, EventArgs e)
        {
            ClickedTile = sender as Tile;
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
                t.Update();
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

        public System.Drawing.RectangleF Draw()
        {
            Core.SpriteBatch.Begin(SpriteSortMode.Immediate, transformMatrix: Core.Camera.GetTranslation());
            for (int x = 0; x < tiles.GetLength(0); x++)
            {
                for (int y = 0; y < tiles.GetLength(1); y++)
                {
                    tiles[x, y].Draw();
                }
            }
            Core.SpriteBatch.End();

            return GetBoundingBox();
        }
        
        /// <summary>
        /// Gets the axis-aligned bounding box that surrounds all tiles in world coordinates.
        /// </summary>
        /// <returns>A RectangleF representing the bounding box.</returns>
        public System.Drawing.RectangleF GetBoundingBox()
        {
            if (tiles == null || tiles.Length == 0)
                return System.Drawing.RectangleF.Empty;

            float minX = float.MaxValue, minY = float.MaxValue;
            float maxX = float.MinValue, maxY = float.MinValue;

            for (int x = 0; x < tiles.GetLength(0); x++)
            {
                for (int y = 0; y < tiles.GetLength(1); y++)
                {
                    var tile = tiles[x, y];
                    var pos = tile.Position;
                    var sprite = tile.BaseSprite;
                    float left = pos.X - sprite.Origin.X;
                    float top = pos.Y - sprite.Origin.Y;
                    float right = left + sprite.Width;
                    float bottom = top + sprite.Height;

                    if (left < minX) minX = left;
                    if (top < minY) minY = top;
                    if (right > maxX) maxX = right;
                    if (bottom > maxY) maxY = bottom;
                }
            }

            return new System.Drawing.RectangleF(minX, minY, maxX - minX, maxY - minY);
        }

        public Vector2 GetTilePosition(TileBase tile)
        {
            for (int x = 0; x < tiles.GetLength(0); x++)
            {
                for (int y = 0; y < tiles.GetLength(1); y++)
                {
                    if (tiles[x, y].TileBase == tile)
                    {
                        return tiles[x, y].Position;
                    }
                }
            }
            throw new ArgumentException("Tile not found in map.");
        }
    }
}
