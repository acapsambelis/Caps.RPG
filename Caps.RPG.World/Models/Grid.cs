using Caps.RPG.World.Models.Graphics;
using System.Collections.Generic;

namespace Caps.RPG.World.Models
{
    /// <summary>
    /// Grid contains map data before repacking - the initial voronoi diagram based on jittered square grid points
    /// </summary>
    public class Grid : VoronoiGraph
    {
        /// <summary>
        /// Initial count of cells/points requested for map creation. Default: 10,000, Max: 100,000, Min: 1,000
        /// </summary>
        public int CellsDesired { get; set; }

        /// <summary>
        /// Spacing between points before jittering
        /// </summary>
        public double Spacing { get; set; }

        /// <summary>
        /// Number of cells in column
        /// </summary>
        public int CellsY { get; set; }

        /// <summary>
        /// Number of cells in row
        /// </summary>
        public int CellsX { get; set; }

        /// <summary>
        /// Coordinates [x, y] based on jittered square grid. Numbers rounded to 2 decimals
        /// </summary>
        public Point2[]? Points { get; set; }

        /// <summary>
        /// Off-canvas points coordinates used to cut the diagram approximately by canvas edges (integers)
        /// </summary>
        public Point2[]? Boundary { get; set; }

        public List<Feature>? Features { get; set; }

        #region Grid Cell Data Arrays
        /// <summary>
        /// Cells elevation in [0, 100] range, where 20 is the minimal land elevation (Uint8Array)
        /// </summary>
        public byte[]? Elevation { get; set; }

        /// <summary>
        /// Indexes of feature (Uint16Array or Uint32Array depending on cells number)
        /// </summary>
        public int[]? FeatureIndexes { get; set; }

        /// <summary>
        /// Distance field from water level: 1, 2, ... - land cells, -1, -2, ... - water cells, 0 - unmarked cell (Uint8Array)
        /// </summary>
        public sbyte[]? TerrainType { get; set; }

        /// <summary>
        /// Cells temperature in Celsius (Uint8Array)
        /// </summary>
        public byte[]? Temperature { get; set; }

        /// <summary>
        /// Cells precipitation in unspecified scale (Uint8Array)
        /// </summary>
        public byte[]? Precipitation { get; set; }
        #endregion
    }
}