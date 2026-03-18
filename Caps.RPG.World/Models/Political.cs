using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Caps.RPG.World.Models
{
    public class Culture : NamedEntity
    {
        public int Base { get; set; }
        public int[]? Origins { get; set; }
        public string? Shield { get; set; }
        public int Center { get; set; }
        public string? Code { get; set; }
        public string? Color { get; set; }
        public double Expansionism { get; set; }
        public string? Type { get; set; }
        public int Area { get; set; }
        public int Cells { get; set; }
        public float Rural { get; set; }
        public float Urban { get; set; }
        public bool Lock { get; set; }
        public bool Removed { get; set; }
    }

    public class Burg : NamedEntity
    {
        public int Cell { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public int Culture { get; set; }
        public int State { get; set; }
        public int Feature { get; set; }
        public float Population { get; set; }
        public string? Type { get; set; }
        public int? MFCG { get; set; }
        public string? Link { get; set; }
        public bool Capital { get; set; }
        public int Port { get; set; }
        public bool Citadel { get; set; }
        public bool Plaza { get; set; }
        public bool Shanty { get; set; }
        public bool Temple { get; set; }
        public bool Walls { get; set; }
        public bool Lock { get; set; }
        public bool Removed { get; set; }
    }

    /// <summary>
    /// Military regiment data
    /// </summary>
    public class Regiment : BaseEntity
    {
        /// <summary>
        /// Regiment x coordinate
        /// </summary>
        public double X { get; set; }

        /// <summary>
        /// Regiment y coordinate
        /// </summary>
        public double Y { get; set; }

        /// <summary>
        /// Regiment base x coordinate
        /// </summary>
        [JsonPropertyName("bx")]
        public double BaseX { get; set; }

        /// <summary>
        /// Regiment base y coordinate
        /// </summary>
        [JsonPropertyName("by")]
        public double BaseY { get; set; }

        /// <summary>
        /// Regiment rotation angle in degrees
        /// </summary>
        public double Angle { get; set; }

        /// <summary>
        /// Unicode character to serve as an icon
        /// </summary>
        public int Icon { get; set; }

        /// <summary>
        /// Original regiment cell id
        /// </summary>
        public int Cell { get; set; }

        /// <summary>
        /// Regiment state id
        /// </summary>
        public int State { get; set; }

        /// <summary>
        /// Regiment name
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// 1 if regiment is a separate unit (like naval units), 0 if not
        /// </summary>
        [JsonPropertyName("n")]
        public bool IsSeparateUnit { get; set; }

        /// <summary>
        /// Regiment content object - unit types and their counts
        /// </summary>
        [JsonPropertyName("u")]
        public Dictionary<string, int>? UnitComposition { get; set; }
    }

    public class State : NamedEntity
    {
        public string? Form { get; set; }
        public string? FormName { get; set; }
        public string? FullName { get; set; }
        public string? Color { get; set; }
        public int Center { get; set; }
        public double[]? Pole { get; set; }
        public int Culture { get; set; }
        public string? Type { get; set; }
        public double Expansionism { get; set; }
        public int Area { get; set; }
        public int Burgs { get; set; }
        public int Cells { get; set; }
        public float Rural { get; set; }
        public float Urban { get; set; }
        public int[]? Neighbors { get; set; }
        public int[]? Provinces { get; set; }
        public string[]? Diplomacy { get; set; }
        public int Alert { get; set; }
        public List<Regiment>? Military { get; set; }
        public bool Lock { get; set; }
        public bool Removed { get; set; }
    }

    public class Province : NamedEntity
    {
        public string? FormName { get; set; }
        public string? FullName { get; set; }
        public string? Color { get; set; }
        public int Center { get; set; }
        public double[]? Pole { get; set; }
        public int Area { get; set; }
        public int? Burg { get; set; }
        public int[]? Burgs { get; set; }
        public int Cells { get; set; }
        public float Rural { get; set; }
        public float Urban { get; set; }
        public bool Lock { get; set; }
        public bool Removed { get; set; }
    }

    public class Religion : NamedEntity
    {
        public string? Type { get; set; }
        public string? Form { get; set; }
        public string? Deity { get; set; }
        public string? Color { get; set; }
        public string? Code { get; set; }
        public int[]? Origins { get; set; }
        public int Center { get; set; }
        public int Culture { get; set; }
        public double Expansionism { get; set; }
        public string? Expansion { get; set; }
        public int Area { get; set; }
        public int Cells { get; set; }
        public float Rural { get; set; }
        public float Urban { get; set; }
        public bool Lock { get; set; }
        public bool Removed { get; set; }
    }
}