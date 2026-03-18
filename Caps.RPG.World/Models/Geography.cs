using Caps.RPG.World.Models.Graphics;
using System.Collections.Generic;

namespace Caps.RPG.World.Models
{
    public class River : BaseEntity
    {
        public string? Name { get; set; }
        public string? Type { get; set; }
        public int Source { get; set; }
        public int Mouth { get; set; }
        public int Parent { get; set; }
        public int Basin { get; set; }
        public int[]? Cells { get; set; }
        public Point2[]? Points { get; set; }
        public double Discharge { get; set; }
        public double Length { get; set; }
        public double Width { get; set; }
        public double SourceWidth { get; set; }
    }

    public class Marker : BaseEntity
    {
        public int Icon { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public int Cell { get; set; }
        public string? Type { get; set; }
        public int? Size { get; set; }
        public string? Fill { get; set; }
        public string? Stroke { get; set; }
        public string? Pin { get; set; }
        public bool? Pinned { get; set; }
        public double? Dx { get; set; }
        public double? Dy { get; set; }
        public int? Px { get; set; }
        public bool? Lock { get; set; }
    }

    public class Route : BaseEntity
    {
        // points: [x,y,cellId]
        public double[][]? Points { get; set; }
        public int Feature { get; set; }
        public string? Group { get; set; }
        public double? Length { get; set; }
        public string? Name { get; set; }
        public bool? Lock { get; set; }
    }

    public class Zone : BaseEntity
    {
        public string? Name { get; set; }
        public string? Type { get; set; }
        public string? Color { get; set; }
        public int[]? Cells { get; set; }
        public bool? Lock { get; set; }
        public bool? Hidden { get; set; }
    }

    public class IceElement : BaseEntity
    {
        public string? Type { get; set; }
        public double[]? Offset { get; set; }
        public Point2[]? Points { get; set; }
    }
}
