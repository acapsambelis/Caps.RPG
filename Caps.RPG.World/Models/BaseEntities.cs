using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Caps.RPG.World.Models
{
    /// <summary>
    /// Base entity class for FMG objects with ID
    /// </summary>
    public abstract class BaseEntity
    {
        /// <summary>
        /// Entity ID, always equal to the array index in FMG data structures
        /// </summary>
        [JsonPropertyName("i")]
        public int Id { get; set; }
    }

    /// <summary>
    /// Base entity class for named FMG objects
    /// </summary>
    public abstract class NamedEntity : BaseEntity
    {
        public string? Name { get; set; }
    }
}