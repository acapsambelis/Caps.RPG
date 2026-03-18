#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using SkiaSharp;
using Caps.RPG.World.Models;
using Caps.RPG.World.Models.Graphics;

namespace Caps.RPG.World.Rendering
{
    public partial class WorldMapRenderer
    {
        public override void Dispose()
        {
            base.Dispose();
            // Do not write perf log here; use FlushPerfLog() called explicitly before application exit.
        }

        /// <summary>
        /// Write the accumulated perf log to a file. If <paramref name="outputDir"/> is null
        /// the system temp directory is used. This should be called explicitly (for example
        /// from the main window Closed handler) to ensure deterministic log file creation.
        /// </summary>
        public override void FlushPerfLog(string? outputDir = null)
        {
            if (!_perfEnabled) return;
            try
            {
                var dir = !string.IsNullOrEmpty(outputDir) ? outputDir : Path.Combine(Path.GetTempPath(), "Caps.RPG", "diagnostic");
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                var fname = $"WorldMapRenderer_perflog_{DateTime.Now:yyyyMMdd_HHmmss}.txt";
                var path = Path.Combine(dir, fname);
                lock (_perfLock)
                {
                    File.WriteAllLines(path, _perfLog);
                }
                Debug.WriteLine($"Perf log written to: {path}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to write perf log: {ex.Message}");
            }
        }


        private Point2 CalculatePolygonCentroid(List<Point2> points)
        {
            if (points == null || points.Count == 0) return new Point2(0, 0);
            double x = 0, y = 0;
            foreach (var p in points) { x += p.X; y += p.Y; }
            return new Point2(x / points.Count, y / points.Count);
        }

        /// <summary>
        /// Returns the index of the Voronoi cell whose cached screen-space centre is nearest to
        /// <paramref name="screenPos"/>. Returns -1 when no cell cache is available.
        /// </summary>
        public int FindCellAtScreenPoint(SKPoint screenPos)
        {
            if (_cachedCellCenters == null || _cachedCellCenters.Length == 0) return -1;
            int best = -1;
            float bestDistSq = float.MaxValue;
            for (int i = 0; i < _cachedCellCenters.Length; i++)
            {
                var c = _cachedCellCenters[i];
                float dx = screenPos.X - c.X;
                float dy = screenPos.Y - c.Y;
                float distSq = dx * dx + dy * dy;
                if (distSq < bestDistSq) { bestDistSq = distSq; best = i; }
            }
            return best;
        }

        private void RenderSvgPoliticalPolygons(SKCanvas canvas, WorldMap pack)
        {
            if (pack.PoliticalPolygons == null || _defaultPaint == null) return;

            int rendered = 0;
            foreach (var polygon in pack.PoliticalPolygons)
            {
                if (polygon.Points == null || polygon.Points.Count < 3) continue;

                var screenPoints = polygon.Points.Select(_viewport.WorldToScreen).ToArray();
                if (screenPoints.Length < 3) continue;

                // Choose color based on polygon id or position
                SKColor fillColor = SKColors.LightGray;
                if (!string.IsNullOrEmpty(polygon.Id) && int.TryParse(polygon.Id, out _))
                {
                    fillColor = GeneratePasstelColorFromId(polygon.Id).WithAlpha(180);
                }
                else
                {
                    var c = CalculatePolygonCentroid(polygon.Points);
                    var hash = ((int)c.X * 73 + (int)c.Y * 37) % 1000;
                    fillColor = GeneratePasstelColorFromId(hash.ToString()).WithAlpha(180);
                }

                _defaultPaint.Color = fillColor;
                _defaultPaint.Style = SKPaintStyle.Fill;

                using var path = new SKPath();
                path.MoveTo(screenPoints[0]);
                for (int i = 1; i < screenPoints.Length; i++) path.LineTo(screenPoints[i]);
                path.Close();
                canvas.DrawPath(path, _defaultPaint);
                rendered++;
            }

            ShortDebugWrite($"Rendered {rendered} political polygons (simple)");
        }
    }
}
