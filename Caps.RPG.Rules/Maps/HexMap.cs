using Caps.RPG.Rules.Helpers;

namespace Caps.RPG.Rules.Maps
{
    public class HexMap : TileMap
    {
        public HexMap() : base(gridWidth: 16, gridDepth: 9) { }

        public override TileBase this[Vector2D pos]
        {
            get
            {
                //0, 3
                //q, r
                // input = odd-r
                // odd-r => cube => axial

                var q = pos.IntX - (pos.IntY - (pos.IntY & 1)) / 2;
                var r = pos.IntY;


                if (!Tiles.TryGetValue(new HexCoords(q, r).Pos, out TileBase? value))
                    throw new ArgumentException($"Tile does not exist: {pos.x} + {pos.y}");
                return value;
            }
            set
            {
                Tiles[pos] = value;
            }
        }

        protected override string GetPrintingOffset(int rowNumber)
        {
            return new string(' ', rowNumber % 2 == 0 ? 0 : 1);
        }

        #region Shapes

        public override TileBase[] GetCircle(TileBase source, double range)
        {
            return NodesInRange(source, (float)range).ToArray();
        }

        public override TileBase[] GetFreestandingLine(TileBase source, double range)
        {
            throw new NotImplementedException("Freestanding lines are not implemented for HexMap.");
        }

        public override TileBase[] GetCone(TileBase source, double range)
        {
            throw new NotImplementedException("Freestanding lines are not implemented for HexMap.");
        }

        public override TileBase[] GetFromSourceLine(TileBase source, double range)
        {
            return [.. source.GetLineTo(this[source.Coords.Pos], this).Where(t => t.GetDistance(source) <= range)];
        }

        #endregion

        public static HexMap GenerateRandomMap(int seed, int obstacleWeight)
        {
            var map = new HexMap();
            var random = new Random(seed);
            for (var r = 0; r < map._gridDepth; r++)
            {
                var rOffset = r >> 1;
                for (var q = -rOffset; q < map._gridWidth - rOffset; q++)
                {
                    var tile = new HexTile(new HexCoords(q, r));
                    if (random.Next(1, 20) <= obstacleWeight)
                        tile.Features.Add(0, new TileFeature("#Obstacle", false, Util.TerminalColors.Gray));
                    map.Tiles.Add(tile.Coords.Pos, tile);
                }
            }

            foreach (var tile in map.Tiles.Values) tile?.CacheNeighbors(map);

            return map;
        }
    }
}
