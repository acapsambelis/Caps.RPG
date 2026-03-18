using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Caps.RPG.World.Models
{
    /// <summary>
    /// Global biomes data structure
    /// </summary>
    public class BiomesData
    {
        /// <summary>
        /// Biome IDs
        /// </summary>
        [JsonPropertyName("i")]
        public int[]? BiomeIds { get; set; }

        /// <summary>
        /// Biome names
        /// </summary>
        [JsonPropertyName("name")]
        public string[]? Names { get; set; }

        /// <summary>
        /// Biome colors in hex or hatching pattern links
        /// </summary>
        [JsonPropertyName("color")]
        public string[]? Colors { get; set; }

        /// <summary>
        /// 2D matrix used to define cell biome by temperature and moisture
        /// </summary>
        public byte[][]? BiomesMatrix { get; set; }

        /// <summary>
        /// Biome movement cost (0 or positive). Used during cultures, states and religions growth phase
        /// </summary>
        [JsonPropertyName("cost")]
        public int[]? MovementCost { get; set; }

        /// <summary>
        /// Biome habitability (0 or positive). 0 means uninhabitable, max ~100
        /// </summary>
        public int[]? Habitability { get; set; }

        /// <summary>
        /// Non-weighted array of icons for each biome. Used for relief icons rendering
        /// </summary>
        public string[][]? Icons { get; set; }

        /// <summary>
        /// How packed icons can be for the biome (0 to 150)
        /// </summary>
        public int[]? IconsDensity { get; set; }
    }

}
