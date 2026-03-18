namespace Caps.RPG.World.Models
{
    public class Feature : BaseEntity
    {
        public bool Land { get; set; }
        public bool Border { get; set; }
        public string? Type { get; set; }

        // pack-only fields
        public string? Group { get; set; }

        /// <summary>
        /// Number of cells in feature
        /// </summary>
        public int Cells { get; set; }

        /// <summary>
        /// Index of the first (top left) cell in feature
        /// </summary>
        public int FirstCell { get; set; }

        /// <summary>
        /// Indexes of vertices around the feature (perimetric vertices)
        /// </summary>
        public int[]? Vertices { get; set; }

        /// <summary>
        /// Name, available for lake type only
        /// </summary>
        public string? Name { get; set; }
    }
}