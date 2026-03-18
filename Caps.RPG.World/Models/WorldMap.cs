using Caps.RPG.World.Models.Graphics;
using System.Collections.Generic;

namespace Caps.RPG.World.Models
{
    /// <summary>
    /// WorldMap contains the fully processed map data — the optimized Voronoi diagram enriched with
    /// geography, political entities, infrastructure, and per-cell scoring arrays.
    /// </summary>
    public class WorldMap : VoronoiGraph
    {
        public List<Feature>? Features { get; set; }
        public List<River>? Rivers { get; set; }
        public List<Burg>? Burgs { get; set; }
        public List<Culture>? Cultures { get; set; }
        public List<State>? States { get; set; }
        public List<Religion>? Religions { get; set; }
        public BiomesData? Biomes { get; set; }

        #region Cell Data Arrays
        /// <summary>
        /// Cells elevation in [0, 100] range, where 20 is the minimal land elevation (Uint8Array)
        /// </summary>
        public byte[]? Elevation { get; set; }

        /// <summary>
        /// Indexes of feature (Uint16Array or Uint32Array depending on cells number)
        /// </summary>
        public int[]? FeatureIndexes { get; set; }

        /// <summary>
        /// Distance field: 1, 2, ... - land cells, -1, -2, ... - water cells, 0 - unmarked cell (Uint8Array)
        /// </summary>
        public sbyte[]? TerrainType { get; set; }

        /// <summary>
        /// Cells score. Scoring is used to define best cells to place a burg (Uint16Array)
        /// </summary>
        public ushort[]? CellScore { get; set; }

        /// <summary>
        /// Cells biome index (Uint8Array)
        /// </summary>
        public byte[]? BiomeIndexes { get; set; }

        /// <summary>
        /// Cells burg index (Uint16Array)
        /// </summary>
        public ushort[]? BurgIndexes { get; set; }

        /// <summary>
        /// Cells culture index (Uint16Array)
        /// </summary>
        public ushort[]? CultureIndexes { get; set; }

        /// <summary>
        /// Cells state index (Uint16Array)
        /// </summary>
        public ushort[]? StateIndexes { get; set; }

        /// <summary>
        /// Cells province index (Uint16Array)
        /// </summary>
        public ushort[]? ProvinceIndexes { get; set; }

        /// <summary>
        /// Cells religion index (Uint16Array)
        /// </summary>
        public ushort[]? ReligionIndexes { get; set; }

        /// <summary>
        /// Cells area in pixels (Uint16Array)
        /// </summary>
        public ushort[]? CellAreas { get; set; }

        /// <summary>
        /// Cells population in population points (1 point = 1000 people by default) (Float32Array)
        /// </summary>
        public float[]? Population { get; set; }

        /// <summary>
        /// Cells river index (Uint16Array)
        /// </summary>
        public ushort[]? RiverIndexes { get; set; }

        /// <summary>
        /// Cells flux amount. Defines how much water flows through the cell. Used to get rivers data and score cells (Uint16Array)
        /// </summary>
        public ushort[]? WaterFlux { get; set; }

        /// <summary>
        /// Cells flux amount in confluences. Confluences are cells where rivers meet each other (Uint16Array)
        /// </summary>
        public ushort[]? RiverConfluences { get; set; }

        /// <summary>
        /// Cells harbor score. Shows how many water cells are adjacent to the cell. Used for scoring (Uint8Array)
        /// </summary>
        public byte[]? HarborScore { get; set; }

        /// <summary>
        /// Cells haven cells index. Each coastal cell has haven cells defined for correct routes building (Uint16Array or Uint32Array)
        /// </summary>
        public int[]? HavenIndexes { get; set; }
        #endregion

        /// <summary>
        /// Cell connections via routes. E.g. routes[8] = {9: 306, 10: 306} shows that cell 8 has two route connections
        /// </summary>
        public Dictionary<int, Dictionary<int, int>>? Routes { get; set; }

        /// <summary>
        /// Quadtree used for fast closest cell detection
        /// </summary>
        public object? SpatialIndex { get; set; }

        /// <summary>
        /// Original Voronoi grid data from the .map file
        /// </summary>
        public Grid? Grid { get; set; }

        /// <summary>
        /// Landmass polygons extracted from SVG
        /// </summary>
        public List<SvgPolygon>? LandmassPolygons { get; set; }

        /// <summary>
        /// Political boundary polygons extracted from SVG
        /// </summary>
        public List<SvgPolygon>? PoliticalPolygons { get; set; }
    }
}