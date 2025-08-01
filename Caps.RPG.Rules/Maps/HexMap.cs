using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Caps.RPG.Rules.Maps
{
    public class HexMap : TileMap
    {
        public HexMap() : base(gridWidth: 16, gridDepth: 9) { }

        public static HexMap GenerateRandom(int seed, int obstacleWeight)
        {
            var map = new HexMap();
            var random = new Random(seed);
            for (var r = 0; r < map._gridDepth; r++)
            {
                var rOffset = r >> 1;
                for (var q = -rOffset; q < map._gridWidth -rOffset; q++)
                {
                    var tile = new HexTile(random.Next(1, 20) > obstacleWeight, new HexCoords(q, r));
                    map.Tiles.Add(tile.Coords.Pos, tile);
                }
            }

            foreach (var tile in map.Tiles.Values) tile?.CacheNeighbors(map);

            return map;
        }

        protected override string GetPrintingOffset(int rowNumber)
        {
            return new string(' ', rowNumber % 2 == 0 ? 0 : 2);
        }
    }
}
