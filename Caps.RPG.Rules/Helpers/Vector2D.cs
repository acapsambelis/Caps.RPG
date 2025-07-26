using SNS.Data.DataSerializer;
using System.Reflection.Metadata.Ecma335;

namespace Caps.RPG.Rules.Helpers
{
    [DataClass("Vector2D")]
    public class Vector2D : IGenericDataObject<Vector2D>
    {
        [DataProperty("X")]
        public double x { get; set; }
        [DataProperty("Y")]
        public double y { get; set; }

        private bool useIntegerEquality = true;
        [DataProperty("UseIntegerEquality")]
        public bool UseIntegerEquality { get { return useIntegerEquality; } set { useIntegerEquality = value; } }

        private bool _wasLoaded = false;
        public bool WasLoaded { get { return _wasLoaded; } set { _wasLoaded = value; } }

        public Vector2D() { }
        public Vector2D(bool useIntegerEquality) { this.useIntegerEquality = useIntegerEquality; }
        public Vector2D(double x, double y)
        {
            this.x = x; this.y = y;
        }

        public int IntX
        {
            get { return (int)x; }
        }
        public int IntY
        {
            get { return (int)y; }
        }

        public static Vector2D operator -(Vector2D a, Vector2D b)
        {
            return new Vector2D(a.x - b.x, a.y - b.y);
        }

        // Multiplication with double
        public static Vector2D operator *(Vector2D a, double scalar) => new Vector2D(a.x * scalar, a.y * scalar);
        public static Vector2D operator *(double scalar, Vector2D a) => a * scalar;

        // Multiplication with float
        public static Vector2D operator *(Vector2D a, float scalar) => a * (double)scalar;
        public static Vector2D operator *(float scalar, Vector2D a) => a * (double)scalar;

        // Multiplication with int
        public static Vector2D operator *(Vector2D a, int scalar) => a * (double)scalar;
        public static Vector2D operator *(int scalar, Vector2D a) => a * (double)scalar;

        public static Vector2D operator +(Vector2D a, Vector2D b)
        {
            return new Vector2D(a.x + b.x, a.y + b.y);
        }

        public static bool operator ==(Vector2D a, Vector2D b)
        {
            if (a is null && b is null) return true;
            if (a is null || b is null) return false;
            if (a.useIntegerEquality && b.useIntegerEquality)
            {
                return a.IntX == b.IntX && a.IntY == b.IntY;
            }
            else
            {
                return a.x == b.x && a.y == b.y;
            }
        }

        public static bool operator !=(Vector2D a, Vector2D b)
        {
            return !(a == b);
        }

        public override bool Equals(object? obj)
        {
            if (obj is Vector2D other)
            {
                return this == other;
            }
            return false;
        }

        public override int GetHashCode()
        {
            if (useIntegerEquality)
            {
                return HashCode.Combine(IntX, IntY, useIntegerEquality);
            }
            return HashCode.Combine(x, y, useIntegerEquality);
        }

        // convert to unit vector
        public Vector2D Normalize()
        {
            double length = Math.Sqrt(x * x + y * y);
            return length > 0 ? new Vector2D(x / length, y / length) : new Vector2D(0, 0);
        }

        public double Distance(Vector2D other)
        {
            var xDif = this.x - other.x;
            var yDif = this.y - other.y;
            return Math.Sqrt(xDif * xDif + yDif * yDif);
        }

        public static double Distance(Vector2D one, Vector2D two)
        {
            var xDif = one.x - two.x;
            var yDif = one.y - two.y;
            return Math.Sqrt(xDif * xDif + yDif * yDif);
        }
    }
}
