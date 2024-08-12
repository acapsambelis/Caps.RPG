
namespace Caps.RPG.Rules.Helpers
{
    public class Vector2D
    {
        public double x; public double y;
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
