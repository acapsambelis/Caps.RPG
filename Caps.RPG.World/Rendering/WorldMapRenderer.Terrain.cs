#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using SkiaSharp;
using Caps.RPG.World.Models;
using System.Diagnostics;
using Caps.RPG.World.Models.Graphics;

namespace Caps.RPG.World.Rendering
{
    public partial class WorldMapRenderer
    {
        private void RenderTerrain(SKCanvas canvas, WorldMap pack)
        {
            if (_defaultPaint == null) return;

            ShortDebugWrite("  Terrain: Checking available data...", LogLevel.Debug);

            // If we have full Voronoi data, skip terrain layer - ocean depth is rendered by StateAreas layer
            if (pack.Cells?.VertexIndexes != null && pack.Vertices?.Coordinates != null)
            {
                // If we have full vertex data and we're in Physical mode, render physical overlays (biomes/elevation)
                if (CurrentMapMode == MapMode.Physical)
                {
                    ShortDebugWrite("  Terrain: Full Voronoi data available - rendering physical overlays (biome/elevation)", LogLevel.Debug);
                    RenderPhysicalOverlays(canvas, pack);
                    return;
                }

                // Otherwise, let state-area rendering handle ocean/coast visuals
                if (pack.TerrainType != null)
                {
                    ShortDebugWrite("  Terrain: Full Voronoi data available - ocean depth handled by StateAreas layer", LogLevel.Debug);
                    return; // Ocean depth shading is done in RenderIndividualVoronoiCells
                }
            }

            // Use SVG landmass polygons if available
            if (pack.LandmassPolygons?.Count > 0)
            {
                ShortDebugWrite($"  Terrain: Using {pack.LandmassPolygons.Count} SVG landmass polygons", LogLevel.Debug);
                RenderSvgLandmasses(canvas, pack);
            }

            else
            {
                ShortDebugWrite("  Terrain: No SVG landmass polygons available", LogLevel.Debug);

                // Only use simplified terrain as last resort (and only if no Voronoi data)
                if (pack.Cells?.VertexIndexes == null && pack.Burgs?.Count > 0)
                {
                    ShortDebugWrite($"  Terrain: Falling back to simplified terrain with {pack.Burgs.Count} settlements", LogLevel.Debug);
                    RenderSimplifiedTerrain(canvas, pack);
                }
                else
                {
                    ShortDebugWrite("  Terrain: Skipping simplified terrain - using Voronoi cells", LogLevel.Debug);
                }
            }
        }

        /// <summary>
        /// Render biome and elevation fills per Voronoi cell when in Physical mode
        /// </summary>
        private void RenderPhysicalOverlays(SKCanvas canvas, WorldMap pack)
        {
            if (_defaultPaint == null) return;

            // Ensure caches are up-to-date for this pack/viewport to avoid recomputing screen-space geometry
            UpdateCachesIfNeeded(pack);

            // Prefer vertex polygons for accurate cell shapes
            if (pack.Cells?.VertexIndexes != null && pack.Vertices?.Coordinates != null)
            {
                using var path = new SKPath();
                for (int cellIndex = 0; cellIndex < pack.Cells.VertexIndexes.Length; cellIndex++)
                {
                    var screenPoints = _cachedCellScreenPolygons != null && cellIndex < _cachedCellScreenPolygons.Length
                        ? _cachedCellScreenPolygons[cellIndex]
                        : null;
                    if (screenPoints == null || screenPoints.Length < 3) continue;

                    // Quick culling: use precomputed per-cell AABB when available (O(1)), fall back to vertex scan
                    bool cellVisible;
                    if (_cachedCellBounds != null && cellIndex < _cachedCellBounds.Length)
                    {
                        var b = _cachedCellBounds[cellIndex];
                        cellVisible = !(b.Right < 0 || b.Left > _viewport.ViewBounds.Width || b.Bottom < 0 || b.Top > _viewport.ViewBounds.Height);
                    }
                    else
                    {
                        cellVisible = IsPolygonVisible(screenPoints);
                    }
                    if (!cellVisible) continue;

                    // Choose fill color: biome if available, otherwise terrain/elevation
                    SKColor fillColor;
                    if (pack.BiomeIndexes != null && cellIndex < pack.BiomeIndexes.Length)
                    {
                        var bId = pack.BiomeIndexes[cellIndex];
                        var habitability = pack.Biomes?.GetHabitability(bId);
                        var alpha = habitability.HasValue ? (byte)(60 + habitability.Value * 160 / 100) : (byte)220;
                        fillColor = GetBiomeColor(bId, pack.Biomes).WithAlpha(alpha);
                    }
                    else
                    {
                        fillColor = GetTerrainColor(cellIndex, pack).WithAlpha(220);
                    }

                    _defaultPaint.Color = fillColor;
                    _defaultPaint.Style = SKPaintStyle.Fill;

                    path.Reset();
                    path.MoveTo(screenPoints[0]);
                    for (int i = 1; i < screenPoints.Length; i++) path.LineTo(screenPoints[i]);
                    path.Close();
                    canvas.DrawPath(path, _defaultPaint);
                }

                // Rivers are rendered by the MapLayer.Rivers pass in RenderMap.
                RenderSmoothOceanOverlay(canvas, pack);
                return;
            }

            // Fallback: use grid points / circle rendering tinted by biome/elevation
            if (pack.Cells?.Coordinates != null && pack.BiomeIndexes != null)
            {
                var radius = CalculateAverageCellSpacing(pack.Cells.Coordinates) * 0.6;
                for (int i = 0; i < Math.Min(pack.Cells.Coordinates.Length, pack.BiomeIndexes.Length); i++)
                {
                    var screenPt = _cachedCellCenters != null && i < _cachedCellCenters.Length ? _cachedCellCenters[i] : _viewport.WorldToScreen(pack.Cells.Coordinates[i]);
                    var screenR = (float)(radius * _viewport.Scale);
                    if (screenPt.X + screenR < -50 || screenPt.X - screenR > _viewport.ViewBounds.Width + 50 ||
                        screenPt.Y + screenR < -50 || screenPt.Y - screenR > _viewport.ViewBounds.Height + 50)
                        continue;

                    var bId = pack.BiomeIndexes[i];
                    var habitability = pack.Biomes?.GetHabitability(bId);
                    var alpha = habitability.HasValue ? (byte)(60 + habitability.Value * 160 / 100) : (byte)200;
                    var fillColor = GetBiomeColor(bId, pack.Biomes).WithAlpha(alpha);
                    _defaultPaint.Color = fillColor;
                    canvas.DrawCircle(screenPt, screenR, _defaultPaint);
                }
                // Rivers are rendered by the MapLayer.Rivers pass in RenderMap.
            }
        }

        private void RenderSvgLandmasses(SKCanvas canvas, WorldMap pack)
        {
            if (pack.LandmassPolygons == null || _defaultPaint == null) return;

            // Ensure caches for polygons/screen points
            UpdateCachesIfNeeded(pack);

            for (int polyIndex = 0; polyIndex < pack.LandmassPolygons.Count; polyIndex++)
            {
                var polygon = pack.LandmassPolygons[polyIndex];
                if (polygon?.Points == null || polygon.Points.Count < 3) continue;

                var screenPoints = _cachedLandmassScreenPolygons != null && polyIndex < _cachedLandmassScreenPolygons.Length
                    ? _cachedLandmassScreenPolygons[polyIndex]
                    : null;

                if (screenPoints == null || screenPoints.Length < 3) continue;

                // Quick culling check
                if (screenPoints[0].X < 0 && screenPoints.All(p => p.X < 0)) continue;
                if (screenPoints.All(p => p.X > _viewport.ViewBounds.Width)) continue;
                if (screenPoints.All(p => p.Y < 0)) continue;
                if (screenPoints.All(p => p.Y > _viewport.ViewBounds.Height)) continue;

                // Determine fill color
                var fillColor = GetPolygonFillColor(polygon);
                _defaultPaint.Color = fillColor;

                // Create and draw the path
                using var path = new SKPath();
                if (polygon.BezierSegments != null && polygon.PathStartPoint.HasValue && polygon.BezierSegments.Count > 0)
                {
                    // Use native cubic bezier segments for smooth coastlines (matches FMG's curveBasisClosed rendering)
                    var startScreen = _viewport.WorldToScreen(polygon.PathStartPoint.Value);
                    path.MoveTo((float)startScreen.X, (float)startScreen.Y);
                    foreach (var seg in polygon.BezierSegments)
                    {
                        var scp1 = _viewport.WorldToScreen(seg.CP1);
                        var scp2 = _viewport.WorldToScreen(seg.CP2);
                        var send = _viewport.WorldToScreen(seg.End);
                        path.CubicTo((float)scp1.X, (float)scp1.Y, (float)scp2.X, (float)scp2.Y, (float)send.X, (float)send.Y);
                    }
                }
                else
                {
                    path.MoveTo(screenPoints[0]);
                    for (int i = 1; i < screenPoints.Length; i++) path.LineTo(screenPoints[i]);
                }
                path.Close();
                canvas.DrawPath(path, _defaultPaint);
            }

            ShortDebugWrite($"Rendered {pack.LandmassPolygons.Count} SVG landmass polygons");
        }

        private SKColor GetPolygonFillColor(SvgPolygon polygon)
        {
            // Use the original fill color if available
            if (!string.IsNullOrEmpty(polygon.Fill))
            {
                try
                {
                    var color = ParseColor(polygon.Fill);
                    if (color != SKColors.Gray) // Only use if parsing was successful
                        return color;
                }
                catch
                {
                    // Fall through to default coloring
                }
            }

            // Default terrain colors based on polygon type and position
            if (polygon.Type == "landmass")
            {
                // Use a hash of the polygon's first point for consistent coloring
                var firstPoint = polygon.Points.FirstOrDefault();
                var hash = ((int)firstPoint.X * 73 + (int)firstPoint.Y * 37) % 1000;

                if (hash < 300) return SKColor.Parse("#90EE90"); // Grassland  
                else if (hash < 500) return SKColor.Parse("#8FBC8F"); // Forest
                else if (hash < 700) return SKColor.Parse("#d2d082"); // Plains
                else if (hash < 900) return SKColor.Parse("#A0522D"); // Hills
                else return SKColor.Parse("#696969"); // Mountains
            }

            return SKColor.Parse("#90EE90"); // Default green
        }

        private void RenderSimplifiedTerrain(SKCanvas canvas, WorldMap pack)
        {
            if (pack.Burgs == null) return;

            // Get map bounds from settlements
            var settlements = pack.Burgs.Where(b => b.X > 0 && b.Y > 0).ToList();
            if (!settlements.Any()) return;

            var minX = settlements.Min(b => b.X) - 100;
            var maxX = settlements.Max(b => b.X) + 100;
            var minY = settlements.Min(b => b.Y) - 100;  
            var maxY = settlements.Max(b => b.Y) + 100;

            // Draw a grid of terrain tiles
            var tileSize = 50.0; // Adjust this for detail level
            int tilesRendered = 0;

            for (double x = minX; x < maxX; x += tileSize)
            {
                for (double y = minY; y < maxY; y += tileSize)
                {
                    var point = new Point2(x, y);
                    var screenPoint = _viewport.WorldToScreen(point);
                    var tileScreenSize = (float)(tileSize * _viewport.Scale);

                    if (tileScreenSize < 1) continue; // Skip tiny tiles

                    // Determine terrain color based on position relative to water/land
                    var isWater = DetermineIfWater(point, settlements);
                    var color = isWater ? SKColor.Parse("#4682b4") : GetLandColor(point, settlements);

                    _defaultPaint!.Color = color;

                    canvas.DrawRect(screenPoint.X, screenPoint.Y, tileScreenSize, tileScreenSize, _defaultPaint);
                    tilesRendered++;
                }
            }

            ShortDebugWrite($"Rendered {tilesRendered} terrain tiles");
        }

        private bool DetermineIfWater(Point2 point, List<Burg> settlements)
        {
            // Simple heuristic: if far from any settlement, likely water
            var nearestDistance = settlements.Min(s => 
                Math.Sqrt(Math.Pow(point.X - s.X, 2) + Math.Pow(point.Y - s.Y, 2)));

            return nearestDistance > 200; // Adjust this threshold
        }

        private SKColor GetLandColor(Point2 point, List<Burg> settlements)
        {
            // Simple terrain variation based on position
            var hash = ((int)point.X * 73 + (int)point.Y * 37) % 1000;

            if (hash < 200) return SKColor.Parse("#90EE90"); // Grassland  
            else if (hash < 400) return SKColor.Parse("#8FBC8F"); // Forest
            else if (hash < 600) return SKColor.Parse("#d2d082"); // Plains
            else if (hash < 800) return SKColor.Parse("#A0522D"); // Hills
            else return SKColor.Parse("#696969"); // Mountains
        }

        private void RenderHexagonalCell(SKCanvas canvas, SKPoint center, float radius)
        {
            if (radius < 1) return;

            // Create hexagonal approximation of Voronoi cell
            var hexPoints = new SKPoint[6];
            for (int i = 0; i < 6; i++)
            {
                var angle = (float)(Math.PI / 3 * i); // 60 degree increments
                hexPoints[i] = new SKPoint(
                    center.X + radius * (float)Math.Cos(angle),
                    center.Y + radius * (float)Math.Sin(angle)
                );
            }

            using var path = new SKPath();
            path.MoveTo(hexPoints[0]);
            for (int i = 1; i < 6; i++)
            {
                path.LineTo(hexPoints[i]);
            }
            path.Close();

            canvas.DrawPath(path, _defaultPaint);
        }

        private SKColor GetTerrainColor(int cellIndex, WorldMap pack)
        {
            // Try biome-based coloring first
            if (pack.BiomeIndexes != null && cellIndex < pack.BiomeIndexes.Length)
            {
                var biomeId = pack.BiomeIndexes[cellIndex];
                return GetBiomeColor(biomeId, pack.Biomes);
            }

            // Fall back to elevation-based coloring
            if (pack.Elevation != null && cellIndex < pack.Elevation.Length)
            {
                var elevation = pack.Elevation[cellIndex];

                if (elevation < 20) // Water
                    return SKColor.Parse("#4682b4");
                else if (elevation < 30) // Coast/Beach
                    return SKColor.Parse("#f4d03f");
                else if (elevation < 40) // Lowlands
                    return SKColor.Parse("#90EE90");
                else if (elevation < 50) // Hills  
                    return SKColor.Parse("#8FBC8F");
                else if (elevation < 65) // Mountains
                    return SKColor.Parse("#A0522D");
                else if (elevation < 80) // High mountains
                    return SKColor.Parse("#696969");
                else // Snow peaks
                    return SKColor.Parse("#ffffff");
            }

            return SKColor.Parse("#90EE90"); // Default green
        }

        /// <summary>
        /// After drawing Voronoi biome cells, paint a smooth ocean overlay using an even-odd fill:
        /// a full-viewport rectangle with smooth land-feature polygons punched out.
        /// This replaces the jagged cell coastline edges with smooth curveBasisClosed curves.
        /// </summary>
        private void RenderSmoothOceanOverlay(SKCanvas canvas, WorldMap pack)
        {
            if (_defaultPaint == null || pack.Features == null || pack.Vertices?.Coordinates == null) return;

            var coords = pack.Vertices.Coordinates;

            using var oceanPath = new SKPath();
            oceanPath.FillType = SKPathFillType.EvenOdd;

            // Outer bound: ocean fills the entire viewport by default
            oceanPath.AddRect(new SKRect(
                -1, -1,
                (float)_viewport.ViewBounds.Width + 1,
                (float)_viewport.ViewBounds.Height + 1));

            // For each land feature, punch a smooth hole so the land biome colors show through
            foreach (var feature in pack.Features)
            {
                if (!feature.Land || feature.Vertices == null || feature.Vertices.Length < 3) continue;

                var verts = feature.Vertices;
                int n = verts.Length;
                var ring = new SKPoint[n];
                bool valid = true;
                for (int i = 0; i < n; i++)
                {
                    int vi = verts[i];
                    if (vi < 0 || vi >= coords.Length) { valid = false; break; }
                    var s = _viewport.WorldToScreen(coords[vi]);
                    ring[i] = new SKPoint((float)s.X, (float)s.Y);
                }
                if (!valid) continue;

                DrawBSplineClosedOnPath(oceanPath, ring);
            }

            var oceanBiomeId = pack.Biomes?.FindIdByName("Marine") ?? 0;
            _defaultPaint.Color = GetBiomeColor(oceanBiomeId, pack.Biomes);
            _defaultPaint.Style = SKPaintStyle.Fill;
            _defaultPaint.IsAntialias = true;
            canvas.DrawPath(oceanPath, _defaultPaint);
            _defaultPaint.IsAntialias = false; // restore default
        }

        /// <summary>
        /// Appends a closed uniform cubic B-spline (D3 curveBasisClosed) to <paramref name="path"/>.
        /// Matches FMG's smooth feature boundary rendering.
        /// </summary>
        private static void DrawBSplineClosedOnPath(SKPath path, SKPoint[] pts)
        {
            int n = pts.Length;
            if (n < 3) return;

            // For each segment i → i+1, the 4 influencing points are pts[i-1..i+2]:
            //   segStart = (p[-1] + 4·p[0] + p[1]) / 6
            //   cp1 = (2·p[0] + p[1]) / 3
            //   cp2 = (p[0] + 2·p[1]) / 3
            //   segEnd  = (p[0] + 4·p[1] + p[2]) / 6
            static SKPoint BasisMid(SKPoint a, SKPoint b, SKPoint c) =>
                new((a.X + 4 * b.X + c.X) / 6f, (a.Y + 4 * b.Y + c.Y) / 6f);

            path.MoveTo(BasisMid(pts[n - 1], pts[0], pts[1]));
            for (int i = 0; i < n; i++)
            {
                var p1 = pts[i];
                var p2 = pts[(i + 1) % n];
                var p3 = pts[(i + 2) % n];
                path.CubicTo(
                    new SKPoint((2 * p1.X + p2.X) / 3f, (2 * p1.Y + p2.Y) / 3f),
                    new SKPoint((p1.X + 2 * p2.X) / 3f, (p1.Y + 2 * p2.Y) / 3f),
                    BasisMid(p1, p2, p3));
            }
            path.Close();
        }

        private SKColor GetBiomeColor(int biomeId, BiomesData? biomes = null)
        {
            if (biomes?.Colors != null && biomeId >= 0 && biomeId < biomes.Colors.Length)
            {
                var hex = biomes.Colors[biomeId];
                if (!string.IsNullOrEmpty(hex) && SKColor.TryParse(hex, out var loaded))
                    return loaded;
            }

            // Fallback: standard FMG biome colors
            return biomeId switch
            {
                0 => SKColor.Parse("#4682b4"), // Marine/Ocean
                1 => SKColor.Parse("#fbe79f"), // Hot desert
                2 => SKColor.Parse("#b5b887"), // Cold desert
                3 => SKColor.Parse("#d2d082"), // Savanna
                4 => SKColor.Parse("#c8d68f"), // Grassland
                5 => SKColor.Parse("#b6d95d"), // Tropical seasonal forest
                6 => SKColor.Parse("#29bc56"), // Temperate deciduous forest
                7 => SKColor.Parse("#7dcb35"), // Tropical rainforest
                8 => SKColor.Parse("#409c43"), // Temperate rainforest
                9 => SKColor.Parse("#4b6b32"), // Taiga
                10 => SKColor.Parse("#96784b"), // Tundra
                11 => SKColor.Parse("#d5e7eb"), // Glacier
                12 => SKColor.Parse("#0b9131"), // Wetland
                _ => SKColor.Parse("#90EE90")  // Default
            };
        }
    }
}
