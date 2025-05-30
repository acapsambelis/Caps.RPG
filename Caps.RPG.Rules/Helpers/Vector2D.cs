using SNS.Data.DataSerializer;

namespace Caps.RPG.Rules.Helpers
{
    [DataClass("Vector2D")]
    public class Vector2D : IGenericDataObject<Vector2D>
    {
        [DataProperty("X")]
        public double x { get; set; }
        [DataProperty("Y")]
        public double y { get; set; }

        private bool _wasLoaded = false;
        public bool WasLoaded { get { return _wasLoaded; } set { _wasLoaded = value; } }

        public Vector2D() { }
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

        public static Vector2D operator *(Vector2D a, double scalar)
        {
            return new Vector2D(a.x * scalar, a.y * scalar);
        }

        public static Vector2D operator +(Vector2D a, Vector2D b)
        {
            return new Vector2D(a.x + b.x, a.y + b.y);
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

        public double TileDistance(Vector2D other)
        {
            return Math.Abs(this.IntX - other.IntX) + Math.Abs(this.IntY - other.IntY);
        }

        public static double Distance(Vector2D one, Vector2D two)
        {
            var xDif = one.x - two.x;
            var yDif = one.y - two.y;
            return Math.Sqrt(xDif * xDif + yDif * yDif);
        }

        public static double TileDistance(Vector2D one, Vector2D two)
        {
            return Math.Abs(one.IntX - two.IntX) + Math.Abs(one.IntY - two.IntY);
        }
    }
}
