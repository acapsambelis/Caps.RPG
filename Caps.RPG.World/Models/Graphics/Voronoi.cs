using System.Collections.Generic;

namespace Caps.RPG.World.Models.Graphics
{
    /// <summary>
    /// Represents Voronoi diagram cell data for both grid and pack objects
    /// </summary>
    public class VoronoiCells
    {
        /// <summary>
        /// Cell indexes array (Uint16Array or Uint32Array depending on cells number)
        /// </summary>
        public int[]? Indexes { get; set; }

        /// <summary>
        /// Cell coordinates [x, y] after repacking. Numbers rounded to 2 decimals
        /// </summary>
        public Point2[]? Coordinates { get; set; }

        /// <summary>
        /// Indexes of cells adjacent to each cell (neighboring cells)
        /// </summary>
        public int[][]? AdjacentCells { get; set; }

        /// <summary>
        /// Indexes of vertices of each cell
        /// </summary>
        public int[][]? VertexIndexes { get; set; }

        /// <summary>
        /// Indicates if cell borders map edge: 1 if true, 0 if false (integers, not booleans)
        /// </summary>
        public int[]? BorderFlags { get; set; }

        /// <summary>
        /// Pack-only: Indexes of source cells in grid. The only way to find correct grid cell parent for pack cells
        /// </summary>
        public int[]? GridSourceIndexes { get; set; }
    }

    /// <summary>
    /// Represents Voronoi diagram vertex data
    /// </summary>
    public class VoronoiVertices
    {
        /// <summary>
        /// Vertices coordinates [x, y], integers
        /// </summary>
        public Point2[]? Coordinates { get; set; }

        /// <summary>
        /// Indexes of cells adjacent to each vertex. Each vertex has 3 adjacent cells
        /// </summary>
        public int[][]? AdjacentCells { get; set; }

        /// <summary>
        /// Indexes of vertices adjacent to each vertex. Most vertices have 3 neighboring vertices,
        /// bordering vertices have only 2, while the third is still added as -1
        /// </summary>
        public int[][]? AdjacentVertices { get; set; }
    }

    /// <summary>
    /// Base class for Voronoi diagram structures (Grid and Pack)
    /// </summary>
    public abstract class VoronoiGraph
    {
        public VoronoiCells? Cells { get; set; }
        public VoronoiVertices? Vertices { get; set; }
    }
}