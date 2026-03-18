namespace Caps.RPG.World.Models.Graphics
{
    /// <summary>
    /// Lightweight 2D point with double precision used for map geometry and rendering.
    /// Replaces the old deleted Point2 type.
    /// </summary>
    public struct Point2
    {
        public double X { get; set; }
        public double Y { get; set; }

        public Point2(double x, double y)
        {
            X = x;
            Y = y;
        }

        public override string ToString() => $"({X:F2},{Y:F2})";

        public bool Equals(Point2 other) => X == other.X && Y == other.Y;

        public override bool Equals(object? obj) => obj is Point2 p && Equals(p);

        public override int GetHashCode() => System.HashCode.Combine(X, Y);

        public static bool operator ==(Point2 a, Point2 b) => a.Equals(b);
        public static bool operator !=(Point2 a, Point2 b) => !a.Equals(b);

        public double DistanceTo(Point2 other)
        {
            var dx = X - other.X;
            var dy = Y - other.Y;
            return System.Math.Sqrt(dx * dx + dy * dy);
        }

        public static double Distance(Point2 a, Point2 b) => a.DistanceTo(b);
    }
}
