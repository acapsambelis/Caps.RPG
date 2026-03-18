using System.Collections.Generic;

namespace Caps.RPG.World.Models.Graphics
{
    /// <summary>
    /// A single cubic bezier segment: two control points and the end point (all world-space coordinates).
    /// Implicit start point is the end point of the previous segment (or <see cref="SvgPolygon.PathStartPoint"/>).
    /// </summary>
    public record SvgCubicSegment(Point2 CP1, Point2 CP2, Point2 End);

    /// <summary>
    /// Represents a polygon extracted from SVG path data
    /// </summary>
    public class SvgPolygon
    {
        /// <summary>
        /// Points that make up the polygon (flattened bezier approximation for fallback rendering).
        /// </summary>
        public List<Point2> Points { get; set; } = new();

        /// <summary>
        /// The starting point (MoveTo) of the original SVG path in world-space coordinates.
        /// Only populated when <see cref="BezierSegments"/> is set.
        /// </summary>
        public Point2? PathStartPoint { get; set; }

        /// <summary>
        /// Original cubic bezier segments from the SVG path in world-space coordinates.
        /// When populated, use these with <c>CubicTo</c> for smooth rendering instead of the
        /// flattened <see cref="Points"/> list.
        /// </summary>
        public List<SvgCubicSegment>? BezierSegments { get; set; }

        /// <summary>
        /// Fill color or pattern
        /// </summary>
        public string? Fill { get; set; }

        /// <summary>
        /// Type of polygon (landmass, political, etc.)
        /// </summary>
        public string? Type { get; set; }

        /// <summary>
        /// Identifier from SVG (id, class, data-id)
        /// </summary>
        public string? Id { get; set; }

        /// <summary>
        /// Bounding box for quick culling
        /// </summary>
        public (Point2 Min, Point2 Max) GetBounds()
        {
            if (Points.Count == 0)
                return (new Point2(0, 0), new Point2(0, 0));

            var minX = double.MaxValue;
            var minY = double.MaxValue;
            var maxX = double.MinValue;
            var maxY = double.MinValue;

            foreach (var point in Points)
            {
                if (point.X < minX) minX = point.X;
                if (point.Y < minY) minY = point.Y;
                if (point.X > maxX) maxX = point.X;
                if (point.Y > maxY) maxY = point.Y;
            }

            return (new Point2(minX, minY), new Point2(maxX, maxY));
        }
    }
}