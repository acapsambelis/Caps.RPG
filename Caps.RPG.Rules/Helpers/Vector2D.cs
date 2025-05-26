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
