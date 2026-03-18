#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using SkiaSharp;
using Caps.RPG.World.Models;
using Caps.RPG.World.Models.Graphics;

namespace Caps.RPG.World.Rendering
{
    public abstract class MapRendererBase : IDisposable
    {
        // Shared viewport
        protected MapViewport _viewport = new MapViewport();

        // Shared paints used by renderers
        protected SKPaint? _defaultPaint;
        protected SKPaint? _borderPaint;
        protected SKPaint? _textPaint;
        protected SKPaint? _settlementPaint;
        protected SKPaint? _riverFillPaint;
        protected SKPath? _sharedPath;

        // Perf log members left to concrete renderer
        protected readonly List<string> _perfLog = new();
        protected readonly object _perfLock = new();
        protected bool _perfEnabled = true;

        public event EventHandler<RenderStatusEventArgs>? StatusUpdate;

        public MapViewport Viewport => _viewport;
        // Expose paints for helpers
        public SKPaint? DefaultPaint => _defaultPaint;
        public SKPaint? BorderPaint => _borderPaint;
        public SKPaint? TextPaint => _textPaint;
        public SKPaint? SettlementPaint => _settlementPaint;
        public SKPaint? RiverFillPaint => _riverFillPaint;
        public SKPath? SharedPath => _sharedPath;
        public MapLayer EnabledLayers { get; set; } = MapLayer.Terrain | MapLayer.StateAreas | MapLayer.Political | MapLayer.Settlements | MapLayer.Rivers;
        public MapMode CurrentMapMode { get; set; } = MapMode.Physical;
        public bool DebugOnlyLargestState { get; set; } = false;
        // Last rendered landmask polygons (world coordinates). Renderers may populate this for clipping.
        public List<SvgPolygon>? RenderedLandmask { get; set; }

        protected MapRendererBase()
        {
            InitializePaints();
        }

        protected void InitializePaints()
        {
            _defaultPaint = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Fill };
            _borderPaint = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = 1f, Color = SKColors.Black };
            _textPaint = new SKPaint { IsAntialias = true, Color = SKColors.Black, TextSize = 12, Typeface = SKTypeface.FromFamilyName("Arial") };
            _settlementPaint = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Fill };
            _riverFillPaint = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Fill, Color = SKColors.DeepSkyBlue.WithAlpha(200) };
            _sharedPath = new SKPath();
        }

        public void UpdateStatus(string status, string layer, bool isEnabled, int? count = null)
        {
            StatusUpdate?.Invoke(this, new RenderStatusEventArgs(status, layer, isEnabled, count));
        }

        public abstract void RenderMap(SKCanvas canvas, WorldMap map, int width, int height);

        public abstract void FlushPerfLog(string? outputDir = null);

        public virtual void Dispose()
        {
            _defaultPaint?.Dispose();
            _borderPaint?.Dispose();
            _textPaint?.Dispose();
            _settlementPaint?.Dispose();
            // Keep perf log flush in concrete class
        }

        protected internal SKColor ParseColor(string? color)
        {
            if (string.IsNullOrEmpty(color)) return SKColors.Gray;
            try { return SKColor.Parse(color); }
            catch { return SKColors.Gray; }
        }

        protected internal double Distance(Point2 a, Point2 b)
        {
            var dx = a.X - b.X; var dy = a.Y - b.Y; return Math.Sqrt(dx * dx + dy * dy);
        }

        protected internal bool IsLandCell(int index, WorldMap pack)
        {
            if (pack.TerrainType != null && index >= 0 && index < pack.TerrainType.Length)
                return pack.TerrainType[index] > 0;
            if (pack.Elevation != null && index >= 0 && index < pack.Elevation.Length)
                return pack.Elevation[index] >= 20;
            return true;
        }

        protected internal bool IsPolygonVisible(SKPoint[] pts)
        {
            if (pts == null || pts.Length == 0) return false;
            float minX = pts[0].X, maxX = pts[0].X, minY = pts[0].Y, maxY = pts[0].Y;
            for (int i = 1; i < pts.Length; i++)
            {
                if (pts[i].X < minX) minX = pts[i].X;
                if (pts[i].X > maxX) maxX = pts[i].X;
                if (pts[i].Y < minY) minY = pts[i].Y;
                if (pts[i].Y > maxY) maxY = pts[i].Y;
            }
            if (maxX < 0 || minX > _viewport.ViewBounds.Width) return false;
            if (maxY < 0 || minY > _viewport.ViewBounds.Height) return false;
            return true;
        }

        protected internal double Cross(Point2 a, Point2 b, Point2 c)
        {
            return (b.X - a.X) * (c.Y - a.Y) - (b.Y - a.Y) * (c.X - a.X);
        }

        // Monotone chain convex hull (returns points in CCW order)
        protected internal List<Point2> ComputeConvexHull(List<Point2> points)
        {
            var pts = points.Distinct().ToList();
            if (pts.Count < 3) return new List<Point2>(pts);

            pts.Sort((a, b) =>
            {
                var c = a.X.CompareTo(b.X);
                return c != 0 ? c : a.Y.CompareTo(b.Y);
            });

            List<Point2> lower = new List<Point2>();
            foreach (var p in pts)
            {
                while (lower.Count >= 2 && Cross(lower[lower.Count - 2], lower[lower.Count - 1], p) <= 0)
                    lower.RemoveAt(lower.Count - 1);
                lower.Add(p);
            }

            List<Point2> upper = new List<Point2>();
            for (int i = pts.Count - 1; i >= 0; i--)
            {
                var p = pts[i];
                while (upper.Count >= 2 && Cross(upper[upper.Count - 2], upper[upper.Count - 1], p) <= 0)
                    upper.RemoveAt(upper.Count - 1);
                upper.Add(p);
            }

            lower.RemoveAt(lower.Count - 1);
            upper.RemoveAt(upper.Count - 1);
            lower.AddRange(upper);
            return lower;
        }

        protected internal double CalculateAverageCellSpacing(Point2[] points)
        {
            if (points == null || points.Length < 2) return 50.0;
            var distances = new List<double>();
            var sampleSize = Math.Min(100, points.Length);
            for (int i = 0; i < sampleSize; i++)
            {
                var point = points[i];
                double nearest = double.MaxValue;
                for (int j = 0; j < Math.Min(points.Length, 20); j++)
                {
                    if (i == j) continue;
                    var d = Distance(point, points[j]); if (d < nearest) nearest = d;
                }
                if (nearest < double.MaxValue) distances.Add(nearest);
            }
            return distances.Count > 0 ? distances.Average() : 50.0;
        }

        private static readonly SKColor[] _pastelColors = new SKColor[]
        {
            SKColor.Parse("#FFB6C1"), // Light Pink
            SKColor.Parse("#FFFFE0"), // Light Yellow
            SKColor.Parse("#98FB98"), // Pale Green
            SKColor.Parse("#87CEEB"), // Sky Blue
            SKColor.Parse("#DDA0DD"), // Plum
            SKColor.Parse("#F0E68C"), // Khaki
            SKColor.Parse("#FFA07A"), // Light Salmon
            SKColor.Parse("#20B2AA"), // Light Sea Green
            SKColor.Parse("#B0C4DE"), // Light Steel Blue
            SKColor.Parse("#F5DEB3"), // Wheat
            SKColor.Parse("#D8BFD8"), // Thistle
            SKColor.Parse("#AFEEEE"), // Pale Turquoise
            SKColor.Parse("#FFE4E1"), // Misty Rose
            SKColor.Parse("#E0FFFF"), // Light Cyan
            SKColor.Parse("#FAFAD2")  // Light Goldenrod Yellow
        };

        protected internal SKColor GeneratePasstelColorFromId(string id)
        {
            var hash = id.GetHashCode();
            return _pastelColors[Math.Abs(hash) % _pastelColors.Length];
        }

        protected internal SKColor GetContrastingTextColor(SKColor backgroundColor)
        {
            double luminance = (0.299 * backgroundColor.Red + 0.587 * backgroundColor.Green + 0.114 * backgroundColor.Blue) / 255;
            return luminance > 0.5 ? SKColors.Black : SKColors.White;
        }

        protected internal (float H, float S, float L) RgbToHsl(SKColor rgb)
        {
            float r = rgb.Red / 255f;
            float g = rgb.Green / 255f;
            float b = rgb.Blue / 255f;
            float max = Math.Max(r, Math.Max(g, b));
            float min = Math.Min(r, Math.Min(g, b));
            float h = 0, s = 0, l = (max + min) / 2;
            if (max != min)
            {
                float d = max - min;
                s = l > 0.5f ? d / (2 - max - min) : d / (max + min);
                if (max == r)
                    h = (g - b) / d + (g < b ? 6 : 0);
                else if (max == g)
                    h = (b - r) / d + 2;
                else if (max == b)
                    h = (r - g) / d + 4;
                h /= 6;
            }
            return (h, s, l);
        }

        protected internal SKColor HslToRgb((float H, float S, float L) hsl)
        {
            float r, g, b;
            if (hsl.S == 0)
            {
                r = g = b = hsl.L;
            }
            else
            {
                float hue2rgb(float p, float q, float t)
                {
                    if (t < 0) t += 1;
                    if (t > 1) t -= 1;
                    if (t < 1f / 6) return p + (q - p) * 6 * t;
                    if (t < 1f / 2) return q;
                    if (t < 2f / 3) return p + (q - p) * (2f / 3 - t) * 6;
                    return p;
                }
                float q = hsl.L < 0.5f ? hsl.L * (1 + hsl.S) : hsl.L + hsl.S - hsl.L * hsl.S;
                float p = 2 * hsl.L - q;
                r = hue2rgb(p, q, hsl.H + 1f / 3);
                g = hue2rgb(p, q, hsl.H);
                b = hue2rgb(p, q, hsl.H - 1f / 3);
            }
            return new SKColor((byte)(r * 255), (byte)(g * 255), (byte)(b * 255));
        }

        protected internal SKColor EnhanceStateColor(SKColor baseColor)
        {
            var hsl = RgbToHsl(baseColor);
            hsl.S = Math.Min(1.0f, hsl.S * 1.3f);
            if (hsl.L < 0.3f) hsl.L = 0.4f;
            if (hsl.L > 0.8f) hsl.L = 0.7f;
            return HslToRgb(hsl).WithAlpha(220);
        }
    }
}
