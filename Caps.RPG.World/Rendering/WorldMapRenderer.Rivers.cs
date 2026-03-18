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
        // Debugging / pixel-diff controls to reduce runtime overhead when inspecting alignment
        private bool _enablePixelDiff = false;
        public bool EnablePixelDiff { get => _enablePixelDiff; set => _enablePixelDiff = value; }
        private bool _showLandmaskOverlay = false;
        public bool ShowLandmaskOverlay { get => _showLandmaskOverlay; set => _showLandmaskOverlay = value; }
        private int _pixelDiffMaxSize = 1024; // max dimension for diff bitmap to limit cost
        public int PixelDiffMaxSize { get => _pixelDiffMaxSize; set => _pixelDiffMaxSize = Math.Max(64, value); }
        private int _pixelDiffThrottleMs = 1000; // recompute at most once per X ms
        private int _lastPixelDiffTick = 0;
        private SKImage? _cachedPixelDiffImage = null;
        // Reusable lists to avoid per-frame allocations when converting/processing points
        private readonly List<Point2> _sharedScreenPoints = new List<Point2>();

        
        // Runtime perf debugging for rivers
        private bool _enableRiverPerfDebug = false;
        public bool EnableRiverPerfDebug { get => _enableRiverPerfDebug; set => _enableRiverPerfDebug = value; }

        // Per-frame counters (reset each DrawRiversIfPresent)
        private int _pipCallsThisFrame = 0; // IsPointInPolygon / polygon checks
        private int _segIntCallsThisFrame = 0; // segment intersection checks
        private int _tailTrimOpsThisFrame = 0; // number of rivers that ran tail-trim logic
        private long _lastEnsureRenderedLandmaskMs = 0;
        private long _lastEnsureRiverWorldCacheMs = 0;
        // Track viewport used when screen-space river polygons were cached so
        // we can invalidate them when the viewport (scale/offset) changes.
        private double _cachedRiverScreenViewportScale = double.NaN;
        private Point2 _cachedRiverScreenViewportOffset = new Point2(double.NaN, double.NaN);

        private void DrawRiversIfPresent(SKCanvas canvas, WorldMap pack)
        {
            if (_borderPaint == null) return;

            try
            {
                var swTotal = Stopwatch.StartNew();
                // Ensure we have a rendered landmask available (build from Voronoi cells if necessary)
                // Measure landmask build time for perf diagnosis
                var swEnsureLand = Stopwatch.StartNew();
                try
                {
                    EnsureRenderedLandmask(pack);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"DIAG: EnsureRenderedLandmask failed: {ex.Message}");
                }
                finally
                {
                    swEnsureLand.Stop();
                    _lastEnsureRenderedLandmaskMs = swEnsureLand.ElapsedMilliseconds;
                }

                // Guaranteed diagnostics to help trace whether this method runs
                try
                {
                        if (Verbosity >= LogLevel.Trace)
                        Debug.WriteLine($"DIAG: Enter DrawRiversIfPresent - Rivers:{pack.Rivers?.Count ?? 0}, RenderedLandmask:{RenderedLandmask?.Count ?? 0}, PackLandmass:{pack.LandmassPolygons?.Count ?? 0}");
                }
                catch (Exception ex)
                {
                    if (Verbosity >= LogLevel.Error) Debug.WriteLine($"DIAG: DrawRiversIfPresent logging failed: {ex.Message}");
                }
                // DIAGNOSTICS: log landmass polygon availability and a small sample
                try
                {
                    int landCount = pack.LandmassPolygons?.Count ?? 0;
                    if (Verbosity >= LogLevel.Trace)
                    {
                        RiverDebug($"DIAG: LandmassPolygons.Count={landCount}");
                        if (landCount > 0)
                        {
                            var first = pack.LandmassPolygons[0];
                            if (first?.Points != null)
                            {
                                RiverDebug($"DIAG: First polygon points sample={Math.Min(5, first.Points.Count)}");
                                for (int i = 0; i < Math.Min(5, first.Points.Count); i++)
                                {
                                    var p = first.Points[i];
                                    RiverDebug($"DIAG: poly0.pt[{i}] = ({p.X:F2},{p.Y:F2})");
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    RiverDebug($"DIAG: Landmask logging failed: {ex.Message}");
                }

                // DIAGNOSTICS: draw landmask outline overlay to visually compare clipping behavior
                try
                {
                    var landmask = RenderedLandmask != null && RenderedLandmask.Count > 0 ? RenderedLandmask : pack.LandmassPolygons;
                    if (ShowLandmaskOverlay && landmask != null && landmask.Count > 0)
                    {
                        using var debugPaintFill = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Fill, Color = SKColors.Magenta.WithAlpha(30) };
                        using var debugPaintStroke = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = 1f, Color = SKColors.Magenta };

                        foreach (var poly in landmask)
                        {
                            if (poly == null || poly.Points == null || poly.Points.Count < 3) continue;
                            using var path = new SKPath();
                            var sp = _viewport.WorldToScreen(poly.Points[0]);
                            path.MoveTo(new SKPoint((float)sp.X, (float)sp.Y));
                            for (int pi = 1; pi < poly.Points.Count; pi++)
                            {
                                var s = _viewport.WorldToScreen(poly.Points[pi]);
                                path.LineTo(new SKPoint((float)s.X, (float)s.Y));
                            }
                            path.Close();
                            canvas.DrawPath(path, debugPaintFill);
                            canvas.DrawPath(path, debugPaintStroke);
                        }
                    }

                    // If we have both a rendered landmask and pack landmass polygons, produce a pixel-diff
                    try
                    {
                        if (EnablePixelDiff && RenderedLandmask != null && RenderedLandmask.Count > 0 && pack.LandmassPolygons != null && pack.LandmassPolygons.Count > 0)
                        {
                            DrawPixelDiffOverlay(canvas, RenderedLandmask, pack.LandmassPolygons);
                        }
                    }
                    catch (Exception ex)
                    {
                        RiverDebug($"DIAG: Pixel-diff overlay failed: {ex.Message}");
                    }
                }
                catch (Exception ex)
                {
                    RiverDebug($"DIAG: Landmask overlay draw failed: {ex.Message}");
                }

                // Configure paint for rivers
                _borderPaint.Style = SKPaintStyle.Stroke;
                _borderPaint.Color = SKColors.DeepSkyBlue.WithAlpha(200);
                _borderPaint.StrokeCap = SKStrokeCap.Round;

                // If explicit rivers list available, use it (best fidelity)
                if (pack.Rivers != null && pack.Rivers.Count > 0)
                {
                    // Ensure heavy world-space river computations are cached per-pack so viewport moves
                    // only require cheap world->screen transforms rather than rebuilding splines/meanders.
                    var swEnsureCache = Stopwatch.StartNew();
                    EnsureRiverWorldCache(pack);
                    swEnsureCache.Stop();
                    _lastEnsureRiverWorldCacheMs = swEnsureCache.ElapsedMilliseconds;

                    UpdateStatus($"Rendering {pack.Rivers.Count} rivers", "Rivers", true, pack.Rivers.Count);
                    int riverIndex = 0;
                    var swRivers = Stopwatch.StartNew();
                    // reset per-frame perf counters when enabled
                    if (EnableRiverPerfDebug)
                    {
                        _pipCallsThisFrame = 0;
                        _segIntCallsThisFrame = 0;
                        _tailTrimOpsThisFrame = 0;
                        _lastEnsureRenderedLandmaskMs = 0;
                        _lastEnsureRiverWorldCacheMs = 0;
                    }

                    // World-space polyline cache is viewport-independent; valid as long as pack hasn't changed.
                    bool worldCacheValid = ReferenceEquals(pack, _cachedPack) && _cachedRiverSmoothWorlds != null;

                    // If the viewport scale changed, the screen-space polygon cache is stale.
                    // Pan is handled by the cheap translation pass in UpdateCachesIfNeeded, so only
                    // scale changes need a full screen-polygon rebuild here.
                    if (worldCacheValid && _cachedRiverSmoothWorlds != null &&
                        !double.IsNaN(_cachedRiverScreenViewportScale) &&
                        _cachedRiverScreenViewportScale != _viewport.Scale)
                    {
                        _cachedRiverScreenPolygons = new SKPoint[_cachedRiverSmoothWorlds.Count][];
                        for (int ri = 0; ri < _cachedRiverSmoothWorlds.Count; ri++)
                        {
                            var sw2 = _cachedRiverSmoothWorlds[ri];
                            if (sw2 == null || sw2.Count < 2) continue;
                            var river2 = ri < pack.Rivers.Count ? pack.Rivers[ri] : null;
                            var centers2 = new List<Point2>(sw2.Count);
                            for (int si = 0; si < sw2.Count; si++) { var s = _viewport.WorldToScreen(sw2[si]); centers2.Add(new Point2(s.X, s.Y)); }
                            var halfWidths2 = river2 != null ? ComputeRiverHalfWidths(river2, centers2.Count, _viewport.Scale) : null;
                            var cv1 = _cachedRiverCoastEdgeV1 != null && ri < _cachedRiverCoastEdgeV1.Length ? _cachedRiverCoastEdgeV1[ri] : null;
                            var cv2 = _cachedRiverCoastEdgeV2 != null && ri < _cachedRiverCoastEdgeV2.Length ? _cachedRiverCoastEdgeV2[ri] : null;
                            var (mouthLeft2, mouthRight2) = ComputeMouthCorners(cv1, cv2, centers2);
                            var poly2 = halfWidths2 != null ? BuildRiverPolygon(centers2, halfWidths2, mouthLeft2, mouthRight2) : BuildRiverPolygon(centers2, 0.5f);
                            if (poly2 != null && poly2.Length >= 3)
                            {
                                var arr2 = new SKPoint[poly2.Length];
                                for (int k = 0; k < poly2.Length; k++) arr2[k] = new SKPoint((float)poly2[k].X, (float)poly2[k].Y);
                                _cachedRiverScreenPolygons[ri] = arr2;
                            }
                        }
                        _cachedRiverScreenViewportScale = _viewport.Scale;
                        _cachedRiverScreenViewportOffset = _viewport.Offset;
                    }

                    for (int rivIdx = 0; rivIdx < pack.Rivers.Count; rivIdx++)
                    {
                        riverIndex++;
                        var river = pack.Rivers[rivIdx];
                        if (Verbosity >= LogLevel.Trace) RiverDebug($"Rivers: processing river #{riverIndex} (cells: {river?.Cells?.Length ?? 0}, width: {river?.Width}, discharge: {river?.Discharge})");
                        RiverStatus($"Processing river {riverIndex}", "Rivers", true, river?.Cells?.Length);

                        var swRiver = Stopwatch.StartNew();

                        var smoothWorld = worldCacheValid && rivIdx < _cachedRiverSmoothWorlds!.Count ? _cachedRiverSmoothWorlds[rivIdx] : null;

                        if (smoothWorld == null || smoothWorld.Count < 2)
                        {
                            RiverDebug("  skipping: no smooth polyline cached");
                            swRiver.Stop();
                            continue;
                        }

                        // Frustum culling: skip building/drawing if river's world AABB is outside viewport
                        try
                        {
                            if (_cachedRiverWorldAABBs != null && rivIdx < _cachedRiverWorldAABBs.Count)
                            {
                                var r = _cachedRiverWorldAABBs[rivIdx];
                                // compute current viewport bounds in world space
                                var vb = _viewport.ViewBounds;
                                var viewWorld = new SKRect(
                                    (float)(vb.Left / _viewport.Scale - _viewport.Offset.X),
                                    (float)(vb.Top / _viewport.Scale - _viewport.Offset.Y),
                                    (float)(vb.Right / _viewport.Scale - _viewport.Offset.X),
                                    (float)(vb.Bottom / _viewport.Scale - _viewport.Offset.Y)
                                );
                                if (!r.IsEmpty && !viewWorld.IntersectsWith(r))
                                {
                                    if (Verbosity >= LogLevel.Trace) RiverDebug($"  skipping river #{riverIndex}: offscreen (AABB)");
                                    swRiver.Stop();
                                    continue;
                                }
                            }
                        }
                        catch { /* best-effort culling */ }

                        // Stroke width: prefer explicit river width, fallback to discharge/flux
                        float stroke = 1f;
                        try
                        {
                            if (river.Discharge > 0) stroke = Math.Max(1f, (float)(Math.Sqrt(river.Discharge) * 0.12 * _viewport.Scale));
                            else if (river.Width > 0) stroke = Math.Max(1f, (float)(river.Width * 0.25 * _viewport.Scale));
                        }
                        catch
                        {
                            stroke = 1f;
                        }

                        _borderPaint.StrokeWidth = stroke;
                        if (Verbosity >= LogLevel.Trace) RiverDebug($"  computed stroke: {stroke}, halfWidth: {stroke*0.5f}");
                        var halfWidth = stroke * 0.5f;

                        // Prefer cached screen-space polygon for this river to avoid per-frame geometry rebuilds.
                        SKPoint[]? polyToDraw = _cachedRiverScreenPolygons != null && rivIdx < _cachedRiverScreenPolygons.Length
                            ? _cachedRiverScreenPolygons[rivIdx]
                            : null;

                        if (polyToDraw != null && polyToDraw.Length >= 3)
                        {
                            // reuse shared path
                            _sharedPath?.Reset();
                            var path = _sharedPath!;
                            path.MoveTo(polyToDraw[0]);
                            for (int pi = 1; pi < polyToDraw.Length; pi++) path.LineTo(polyToDraw[pi]);
                            path.Close();
                            if (_riverFillPaint != null) canvas.DrawPath(path, _riverFillPaint);

                            // Outline
                            if (_borderPaint != null)
                            {
                                _borderPaint.Style = SKPaintStyle.Stroke;
                                _borderPaint.Color = SKColors.Black.WithAlpha(160);
                                _borderPaint.StrokeWidth = Math.Max(1f, halfWidth * 0.15f);
                                canvas.DrawPath(path, _borderPaint);
                            }
                        }
                        else
                        {
                            // Fallback to polyline draw using screen-converted centers
                            var screenPts = _sharedScreenPoints;
                            screenPts.Clear();
                            for (int spi = 0; spi < smoothWorld.Count; spi++) { var p = smoothWorld[spi]; var s = _viewport.WorldToScreen(p); screenPts.Add(new Point2(s.X, s.Y)); }
                            _sharedPath?.Reset();
                            var path = _sharedPath!;
                            path.MoveTo(new SKPoint((float)screenPts[0].X, (float)screenPts[0].Y));
                            for (int i = 1; i < screenPts.Count; i++) path.LineTo(new SKPoint((float)screenPts[i].X, (float)screenPts[i].Y));
                            // Restore paint state — polygon-path iterations change Color to Black for the outline;
                            // without this reset, fallback rivers inherit that mutation and appear black.
                            _borderPaint.Style = SKPaintStyle.Stroke;
                            _borderPaint.Color = SKColors.DeepSkyBlue.WithAlpha(200);
                            canvas.DrawPath(path, _borderPaint);
                            _sharedPath?.Reset();
                        }
                        swRiver.Stop();
                        PerfLogDuration($"River#{riverIndex}", swRiver.ElapsedMilliseconds);
                    }
                    swRivers.Stop();
                    PerfLogDuration("Rivers total", swRivers.ElapsedMilliseconds);

                    // Diagnostic summary for river perf
                    if (EnableRiverPerfDebug || Verbosity >= LogLevel.Trace)
                    {
                        RiverDebug($"DIAG: Rivers perf - total={swTotal.ElapsedMilliseconds}ms, EnsureLandmask={_lastEnsureRenderedLandmaskMs}ms, EnsureCache={_lastEnsureRiverWorldCacheMs}ms, PIPCalls={_pipCallsThisFrame}, SegIntChecks={_segIntCallsThisFrame}, TailTrimOps={_tailTrimOpsThisFrame}");
                    }

                    return;
                }

                // Fallback: build rivers from per-cell river indexes if available
                if (pack.RiverIndexes != null && pack.Cells?.Coordinates != null)
                {
                    // Group cells by river id
                    var groups = new Dictionary<int, List<int>>();
                    for (int i = 0; i < pack.RiverIndexes.Length && i < pack.Cells.Coordinates.Length; i++)
                    {
                        var rid = pack.RiverIndexes[i];
                        if (rid <= 0) continue;
                        if (!groups.ContainsKey(rid)) groups[rid] = new List<int>();
                        groups[rid].Add(i);
                    }

                    foreach (var kv in groups)
                    {
                        var cellList = kv.Value;
                        if (cellList.Count < 2) continue;

                        // Try to order cells along the river by increasing flux (approximate upstream->downstream)
                        if (pack.WaterFlux != null && pack.WaterFlux.Length == pack.Cells.Coordinates.Length)
                        {
                            cellList = cellList.OrderBy(c => pack.WaterFlux[c]).ToList();
                        }

                        var skPts = cellList.Where(ci => ci >= 0 && ci < pack.Cells.Coordinates.Length)
                                             .Select(ci => {
                                                 var s = _viewport.WorldToScreen(pack.Cells.Coordinates[ci]);
                                                 return new Point2(s.X, s.Y);
                                             })
                                             .ToList();

                        if (skPts.Count < 2) continue;

                        var maxFlux = 0;
                        if (pack.WaterFlux != null)
                            maxFlux = cellList.Where(c => c >= 0 && c < pack.WaterFlux.Length).Select(c => (int)pack.WaterFlux[c]).DefaultIfEmpty(0).Max();

                        _borderPaint.StrokeWidth = Math.Max(1f, (float)(Math.Log10(maxFlux + 1) * _viewport.Scale));

                        using var path = new SKPath();
                        path.MoveTo(new SKPoint((float)skPts[0].X, (float)skPts[0].Y));
                        for (int i = 1; i < skPts.Count; i++) path.LineTo(new SKPoint((float)skPts[i].X, (float)skPts[i].Y));
                        canvas.DrawPath(path, _borderPaint);
                    }
                    return;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"DrawRiversIfPresent failed: {ex.Message}");
            }
        }

        // Renderer-level helpers ported from TS behavior
        private Point2 GetBorderPointForCell(WorldMap pack, int cellIndex)
        {
            // Use cached bounds where possible to avoid repeated Min/Max calculations
            if (pack.Cells?.Coordinates == null || pack.Cells.Coordinates.Length == 0) return new Point2(0, 0);
            if (cellIndex < 0 || cellIndex >= pack.Cells.Coordinates.Length) return new Point2(0, 0);

            // Ensure caches are populated
            UpdateCachesIfNeeded(pack);

            var p = pack.Cells.Coordinates[cellIndex];
            var minX = double.IsFinite(_cachedMinX) && _cachedMinX != double.MaxValue ? _cachedMinX : pack.Cells.Coordinates.Min(pt => pt.X);
            var maxX = double.IsFinite(_cachedMaxX) && _cachedMaxX != double.MinValue ? _cachedMaxX : pack.Cells.Coordinates.Max(pt => pt.X);
            var minY = double.IsFinite(_cachedMinY) && _cachedMinY != double.MaxValue ? _cachedMinY : pack.Cells.Coordinates.Min(pt => pt.Y);
            var maxY = double.IsFinite(_cachedMaxY) && _cachedMaxY != double.MinValue ? _cachedMaxY : pack.Cells.Coordinates.Max(pt => pt.Y);

            var distTop = p.Y - minY;
            var distBottom = maxY - p.Y;
            var distLeft = p.X - minX;
            var distRight = maxX - p.X;

            var minDist = Math.Min(Math.Min(distTop, distBottom), Math.Min(distLeft, distRight));
            if (minDist == distTop) return new Point2(p.X, minY);
            if (minDist == distBottom) return new Point2(p.X, maxY);
            if (minDist == distLeft) return new Point2(minX, p.Y);
            return new Point2(maxX, p.Y);
        }

        // Build a conservative rendered landmask from Voronoi cell vertex data when
        // no SVG landmass polygons or previously captured rendered polygons exist.
        // This helps clipping and diagnostics in Physical mode where RenderTerrain
        // draws coastlines from cell geometry.
        private void EnsureRenderedLandmask(WorldMap pack)
        {
            try
            {
                if (RenderedLandmask != null && RenderedLandmask.Count > 0) return;
                if (pack == null) return;

                // Prefer coarse SVG landmass polygons when available. There are typically only
                // tens of these outlines vs. thousands of per-cell Voronoi polygons, making every
                // subsequent IsPointInsideAnyPolygon / FindSegmentPolygonIntersection call orders
                // of magnitude cheaper.
                if (pack.LandmassPolygons?.Count > 0)
                {
                    RenderedLandmask = pack.LandmassPolygons;
                    Debug.WriteLine($"DIAG: EnsureRenderedLandmask using {pack.LandmassPolygons.Count} SVG landmass polygons");
                    return;
                }

                if (pack.Cells?.VertexIndexes == null || pack.Vertices?.Coordinates == null) return;

                var verts = pack.Vertices.Coordinates;
                var list = new List<SvgPolygon>();

                for (int ci = 0; ci < pack.Cells.VertexIndexes.Length; ci++)
                {
                    var vertexIndices = pack.Cells.VertexIndexes[ci];
                    if (vertexIndices == null || vertexIndices.Length < 3) continue;

                    // Only include land cells to approximate the coastline
                    if (!IsLandCell(ci, pack)) continue;

                    var polyPts = new List<Point2>();
                    bool valid = true;
                    foreach (var vi in vertexIndices)
                    {
                        if (vi < 0 || vi >= verts.Length) { valid = false; break; }
                        polyPts.Add(verts[vi]);
                    }

                    if (!valid || polyPts.Count < 3) continue;

                    list.Add(new SvgPolygon { Points = polyPts });
                }

                if (list.Count > 0)
                {
                    RenderedLandmask = list;
                    Debug.WriteLine($"DIAG: EnsureRenderedLandmask populated with {list.Count} Voronoi cell polygons (SVG landmass unavailable)");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"DIAG: EnsureRenderedLandmask error: {ex.Message}");
            }
        }

        // Precompute heavy world-space river geometry (meander + smoothing + clipping) per-pack so
        // viewport moves don't recompute splines on every frame.
        private void EnsureRiverWorldCache(WorldMap pack)
        {
            try
            {
                if (pack == null || pack.Rivers == null || pack.Rivers.Count == 0) { _cachedRiverSmoothWorlds = null; return; }
                if (ReferenceEquals(pack, _cachedPack) && _cachedRiverSmoothWorlds != null) return;

                // Ensure landmask available for clipping
                EnsureRenderedLandmask(pack);

                _cachedRiverSmoothWorlds = new List<List<Point2>>(pack.Rivers.Count);
                _cachedRiverWorldAABBs = new List<SKRect>(pack.Rivers.Count);
                _cachedRiverScreenPolygons = new SKPoint[pack.Rivers.Count][];
                _cachedRiverCoastEdgeV1 = new Point2?[pack.Rivers.Count];
                _cachedRiverCoastEdgeV2 = new Point2?[pack.Rivers.Count];

                for (int ri = 0; ri < pack.Rivers.Count; ri++)
                {
                    var river = pack.Rivers[ri];
                    if (river == null || river.Cells == null || river.Cells.Length == 0) { _cachedRiverSmoothWorlds.Add(new List<Point2>()); continue; }

                    // Build raw points from river cell centers (model-space Point2)
                    var rawPoints = new List<Point2>();
                    var rawCellIndices = new List<int>();
                    foreach (var ci in river.Cells)
                    {
                        rawCellIndices.Add(ci);
                        if (ci == -1)
                        {
                            rawPoints.Add(GetBorderPointForCell(pack, rawCellIndices.Count > 1 ? rawCellIndices[rawCellIndices.Count - 2] : 0));
                            break;
                        }
                        if (ci < 0 || pack.Cells?.Coordinates == null || ci >= pack.Cells.Coordinates.Length) continue;
                        rawPoints.Add(pack.Cells.Coordinates[ci]);
                    }

                    if (rawPoints.Count < 2) { _cachedRiverSmoothWorlds.Add(new List<Point2>()); continue; }

                    // Trim to coast in world-space.
                    // coastEdgeV1/V2 are the Voronoi shared-edge vertices when the Voronoi method
                    // succeeded — passed into the backward scan so it clips at the same boundary.
                    var trimmedRaw = TrimRiverPointsToCoast(rawPoints, rawCellIndices, pack,
                        out var coastEdgeV1, out var coastEdgeV2);
                    if (trimmedRaw == null || trimmedRaw.Count < 2) { _cachedRiverSmoothWorlds.Add(new List<Point2>()); continue; }

                    var trimmedCellIndices = rawCellIndices.Take(trimmedRaw.Count).ToList();
                    var meandered = AddMeandering(trimmedCellIndices, pack, trimmedRaw, 0.5);
                    if (meandered == null || meandered.Count < 2) { _cachedRiverSmoothWorlds.Add(new List<Point2>()); continue; }

                    var meanderPoints = meandered.Select(m => m.Point).ToList();

                    var smoothWorld = GenerateCatmullRomSplineWorldAlpha(meanderPoints, 8, 0.1);
                    if (smoothWorld == null || smoothWorld.Count < 2)
                    {
                        smoothWorld = GenerateCatmullRomSplineWorld(meanderPoints, 8);
                    }

                    // Clip the spline tail to the coast.
                    // When TrimRiverPointsToCoast found the coast via a Voronoi shared edge, use
                    // that same edge for the backward scan — it is the exact visual boundary.
                    // The SVG polygon used by FindSegmentPolygonIntersection lies INLAND of the
                    // Voronoi coast on many rivers, causing the scan to wrongly clip back inland.
                    // For rivers where no Voronoi edge was found (fallback cases), keep the SVG
                    // polygon scan as the best available option.
                    bool hasVoronoiEdge = coastEdgeV1.HasValue && coastEdgeV2.HasValue;

                    // Only clip to the coast when this river actually enters the ocean.
                    // Tributary rivers end at a land cell (their mouth joins another river),
                    // so their last cell is land.  The backward scan uses RenderedLandmask —
                    // state-area polygons that also exist at inland state borders — so it
                    // incorrectly clips tributaries short before they reach their junction.
                    bool riverEndsInOcean = false;
                    for (int rk = 0; rk < rawCellIndices.Count - 1 && !riverEndsInOcean; rk++)
                    {
                        int rci = rawCellIndices[rk], rni = rawCellIndices[rk + 1];
                        if (rci >= 0 && IsLandCell(rci, pack) && (rni == -1 || (rni >= 0 && !IsLandCell(rni, pack))))
                            riverEndsInOcean = true;
                    }

                    var landmaskPolygons = RenderedLandmask != null && RenderedLandmask.Count > 0 ? RenderedLandmask : pack.LandmassPolygons;
                    bool canScan = riverEndsInOcean && (hasVoronoiEdge || (landmaskPolygons != null && landmaskPolygons.Count > 0));
                    if (canScan && smoothWorld.Count > 1)
                    {
                        // Voronoi-trimmed rivers need only a small window — the spline should end
                        // at or very near the coast; only catch slight meandering overshoot.
                        // Fallback rivers may end at the ocean-cell centre so allow a larger window.
                        int tailWindow = hasVoronoiEdge ? 15 : 50;
                        int scanFrom = Math.Max(0, smoothWorld.Count - 2 - tailWindow);
                        bool foundClip = false;
                        for (int si = smoothWorld.Count - 2; si >= scanFrom; si--)
                        {
                            Point2? ip = hasVoronoiEdge
                                ? SegmentIntersection(smoothWorld[si], smoothWorld[si + 1],
                                    coastEdgeV1.Value, coastEdgeV2.Value, out _)
                                : FindSegmentPolygonIntersection(smoothWorld[si], smoothWorld[si + 1],
                                    landmaskPolygons);
                            if (ip.HasValue)
                            {
                                if (EnableRiverPerfDebug)
                                    Debug.WriteLine($"[River#{ri}] backward-clip si={si}/{smoothWorld.Count - 1} clippedTail={smoothWorld.Count - 1 - si}pts end=({ip.Value.X:F1},{ip.Value.Y:F1}) method={(hasVoronoiEdge ? "voronoi" : "svgpoly")}");
                                var newList = smoothWorld.Take(si + 1).ToList();
                                newList[newList.Count - 1] = ip.Value;
                                smoothWorld = newList;
                                foundClip = true;
                                break;
                            }
                        }
                        if (EnableRiverPerfDebug && !foundClip)
                            Debug.WriteLine($"[River#{ri}] no backward-clip crossing in last {Math.Min(tailWindow, smoothWorld.Count - 1)} segs (hasVoronoiEdge={hasVoronoiEdge})");
                    }

                    // Belt-and-suspenders: when we have an authoritative Voronoi coast crossing,
                    // snap the final spline point to it to eliminate any residual floating-point
                    // or dedup drift.  Only do this for hasVoronoiEdge=true rivers because for
                    // fallback_b rivers trimmedRaw.Last() is the ocean cell centre, not the coast —
                    // snapping to it would push the river endpoint past the shoreline into the ocean.
                    // For those rivers the backward-scan SVG clip already gives the correct endpoint.
                    if (hasVoronoiEdge && smoothWorld != null && smoothWorld.Count > 0 && trimmedRaw.Count > 0)
                    {
                        var coastPt = trimmedRaw[trimmedRaw.Count - 1];
                        smoothWorld[smoothWorld.Count - 1] = coastPt;
                        if (EnableRiverPerfDebug)
                            Debug.WriteLine($"[River#{ri}] final-snap end=({coastPt.X:F1},{coastPt.Y:F1})");
                    }

                    _cachedRiverSmoothWorlds.Add(smoothWorld ?? new List<Point2>());

                    // Cache the Voronoi coast-edge vertices (world space) so scale-change rebuilds
                    // can re-project them without re-running trimming.
                    _cachedRiverCoastEdgeV1![ri] = hasVoronoiEdge ? coastEdgeV1 : null;
                    _cachedRiverCoastEdgeV2![ri] = hasVoronoiEdge ? coastEdgeV2 : null;

                    // Build and cache screen-space polygon for this river to avoid per-frame rebuilds
                    try
                    {
                        if (smoothWorld != null && smoothWorld.Count > 0)
                        {
                            var screenCenters = new List<Point2>(smoothWorld.Count);
                            for (int si = 0; si < smoothWorld.Count; si++)
                            {
                                var sp = _viewport.WorldToScreen(smoothWorld[si]);
                                screenCenters.Add(new Point2(sp.X, sp.Y));
                            }

                            var halfWidths = ComputeRiverHalfWidths(river, screenCenters.Count, _viewport.Scale);
                            var (mouthLeft, mouthRight) = ComputeMouthCorners(coastEdgeV1, coastEdgeV2, screenCenters);
                            var poly = BuildRiverPolygon(screenCenters, halfWidths, mouthLeft, mouthRight);
                            if (poly != null && poly.Length >= 3)
                            {
                                var arr = new SKPoint[poly.Length];
                                for (int k = 0; k < poly.Length; k++) arr[k] = new SKPoint((float)poly[k].X, (float)poly[k].Y);
                                _cachedRiverScreenPolygons[ri] = arr;
                            }
                        }
                    }
                    catch { /* best-effort cache */ }

                    // Compute and store conservative world-space AABB for this river
                    if (smoothWorld != null && smoothWorld.Count > 0)
                    {
                        double minX = double.MaxValue, minY = double.MaxValue, maxX = double.MinValue, maxY = double.MinValue;
                        foreach (var p in smoothWorld)
                        {
                            if (p.X < minX) minX = p.X;
                            if (p.X > maxX) maxX = p.X;
                            if (p.Y < minY) minY = p.Y;
                            if (p.Y > maxY) maxY = p.Y;
                        }
                        _cachedRiverWorldAABBs.Add(new SKRect((float)minX, (float)minY, (float)maxX, (float)maxY));
                    }
                    else
                    {
                        _cachedRiverWorldAABBs.Add(SKRect.Empty);
                    }
                }
                // Record the viewport parameters used to create any screen-space caches so
                // they can be invalidated when the viewport changes (zoom/pan).
                _cachedRiverScreenViewportScale = _viewport.Scale;
                _cachedRiverScreenViewportOffset = _viewport.Offset;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"EnsureRiverWorldCache failed: {ex.Message}");
                _cachedRiverSmoothWorlds = null;
            }
        }

        private List<Point2> TrimRiverPointsToCoast(List<Point2> rawPoints, List<int> cellIndices, WorldMap pack,
            out Point2? coastEdgeV1, out Point2? coastEdgeV2)
        {
            coastEdgeV1 = null;
            coastEdgeV2 = null;
            if (rawPoints == null || rawPoints.Count < 2) return rawPoints;

            for (int i = 0; i < cellIndices.Count - 1 && i < rawPoints.Count - 1; i++)
            {
                var ci = cellIndices[i];
                var ni = cellIndices[i + 1];

                bool currentIsLand = ci >= 0 && IsLandCell(ci, pack);
                bool nextIsLand = ni >= 0 && IsLandCell(ni, pack);

                if (currentIsLand && !nextIsLand)
                {
                    var a = rawPoints[i];
                    var b = rawPoints[i + 1];
                    Point2? ip = null;
                    string method = "fallback_b";

                    // PRIMARY: Voronoi shared edge — the two vertices shared by the land cell ci and
                    // ocean cell ni define the exact visual coast boundary (terrain is rendered from
                    // the same Voronoi tessellation, so this is pixel-accurate).
                    if (ci >= 0 && ni >= 0 &&
                        pack.Cells?.VertexIndexes != null &&
                        ci < pack.Cells.VertexIndexes.Length &&
                        ni < pack.Cells.VertexIndexes.Length &&
                        pack.Vertices?.Coordinates != null)
                    {
                        var ciVerts = pack.Cells.VertexIndexes[ci];
                        var niVerts = pack.Cells.VertexIndexes[ni];
                        if (ciVerts != null && niVerts != null)
                        {
                            var ciSet = new HashSet<int>(ciVerts);
                            int sv1 = -1, sv2 = -1;
                            foreach (var v in niVerts)
                            {
                                if (ciSet.Contains(v))
                                {
                                    if (sv1 < 0) sv1 = v;
                                    else if (sv2 < 0) { sv2 = v; break; }
                                }
                            }
                            if (sv1 >= 0 && sv2 >= 0 &&
                                sv1 < pack.Vertices.Coordinates.Length &&
                                sv2 < pack.Vertices.Coordinates.Length)
                            {
                                var v1 = pack.Vertices.Coordinates[sv1];
                                var v2 = pack.Vertices.Coordinates[sv2];
                                ip = SegmentIntersection(a, b, v1, v2, out _);
                                if (!ip.HasValue)
                                {
                                    // Cell coordinates may not cross the shared Voronoi edge directly
                                    // (e.g. when pack.Cells.Coordinates are centroids rather than sites).
                                    // Extend the segment 5× in the same direction to force the crossing.
                                    var extB = new Point2(a.X + (b.X - a.X) * 5, a.Y + (b.Y - a.Y) * 5);
                                    ip = SegmentIntersection(a, extB, v1, v2, out _);
                                }
                                if (ip.HasValue)
                                {
                                    method = "voronoi_edge";
                                    coastEdgeV1 = v1;
                                    coastEdgeV2 = v2;
                                }
                                if (EnableRiverPerfDebug)
                                    Debug.WriteLine($"[TrimRiver] i={i} ci={ci} ni={ni}: Voronoi sv1={sv1} sv2={sv2} ip={ip?.X:F1},{ip?.Y:F1}");
                            }
                            else if (EnableRiverPerfDebug)
                                Debug.WriteLine($"[TrimRiver] i={i} ci={ci} ni={ni}: no shared Voronoi edge (sv1={sv1} sv2={sv2})");
                        }
                    }

                    // FALLBACK: SVG landmass polygon — may not align precisely with visual coast
                    if (!ip.HasValue && pack.LandmassPolygons?.Count > 0)
                    {
                        ip = FindSegmentPolygonIntersection(a, b, pack.LandmassPolygons);
                        if (ip.HasValue)
                        {
                            method = "svg_direct";
                        }
                        else
                        {
                            var dx = b.X - a.X;
                            var dy = b.Y - a.Y;
                            ip = FindSegmentPolygonIntersection(a, new Point2(a.X + dx * 10, a.Y + dy * 10), pack.LandmassPolygons);
                            if (ip.HasValue) method = "svg_extended";
                        }
                        if (EnableRiverPerfDebug)
                            Debug.WriteLine($"[TrimRiver] i={i}: SVG fallback ip={ip?.X:F1},{ip?.Y:F1} method={method}");
                    }

                    var trimList = new List<Point2>();
                    for (int k = 0; k <= i; k++) trimList.Add(rawPoints[k]);
                    trimList.Add(ip ?? b);
                    if (EnableRiverPerfDebug)
                        Debug.WriteLine($"[TrimRiver] FINAL i={i}: a=({a.X:F1},{a.Y:F1}) b=({b.X:F1},{b.Y:F1}) end=({(ip ?? b).X:F1},{(ip ?? b).Y:F1}) method={method}");
                    return trimList;
                }
            }

            return rawPoints;
        }

        private List<(Point2 Point, double Flux)> AddMeandering(List<int> riverCells, WorldMap pack, List<Point2>? riverPoints = null, double meandering = 0.5)
        {
            var result = new List<(Point2, double)>();
            if (riverCells == null || riverCells.Count == 0) return result;

            // Build base points list
            var basePoints = new List<Point2>();
            if (riverPoints != null && riverPoints.Count > 0)
            {
                basePoints.AddRange(riverPoints);
            }
            else
            {
                for (int idx = 0; idx < riverCells.Count; idx++)
                {
                    var cell = riverCells[idx];
                    if (cell == -1)
                    {
                        var prev = idx > 0 ? riverCells[idx - 1] : 0;
                        basePoints.Add(GetBorderPointForCell(pack, prev));
                        break;
                    }
                    if (pack.Cells?.Coordinates != null && cell >= 0 && cell < pack.Cells.Coordinates.Length)
                        basePoints.Add(pack.Cells.Coordinates[cell]);
                    else
                        basePoints.Add(new Point2(0, 0));
                }
            }

            var flArray = pack.WaterFlux; // ushort[]
            var elevation = pack.Elevation; // byte[]

            int lastStep = riverCells.Count - 1;
            int step = 10;
            if (riverCells.Count > 0)
            {
                var source = riverCells[0];
                if (source >= 0 && elevation != null && source < elevation.Length && elevation[source] < 20) step = 1;
            }

            for (int i = 0; i <= lastStep; i++, step++)
            {
                var cell = riverCells[i];
                bool isLast = i == lastStep;
                var p1 = i < basePoints.Count ? basePoints[i] : new Point2(0, 0);
                double flux = 0;
                if (flArray != null && cell >= 0 && cell < flArray.Length) flux = flArray[cell];

                result.Add((p1, flux));
                if (isLast) break;

                var nextCell = riverCells[i + 1];
                var p2 = (i + 1) < basePoints.Count ? basePoints[i + 1] : new Point2(0, 0);

                if (nextCell == -1)
                {
                    result.Add((p2, flux));
                    break;
                }

                var dx = p2.X - p1.X;
                var dy = p2.Y - p1.Y;
                var dist2 = dx * dx + dy * dy;
                if (dist2 <= 25 && riverCells.Count >= 6) continue;

                double mev = meandering + 1.0 / step + Math.Max(meandering - step / 100.0, 0);
                var angle = Math.Atan2(p2.Y - p1.Y, p2.X - p1.X);
                var sinMeander = Math.Sin(angle) * mev;
                var cosMeander = Math.Cos(angle) * mev;

                if (step < 20 && (dist2 > 64 || (dist2 > 36 && riverCells.Count < 5)))
                {
                    var p1x = (p1.X * 2 + p2.X) / 3 + -sinMeander;
                    var p1y = (p1.Y * 2 + p2.Y) / 3 + cosMeander;
                    var p2x = (p1.X + p2.X * 2) / 3 + sinMeander / 2;
                    var p2y = (p1.Y + p2.Y * 2) / 3 - cosMeander / 2;
                    result.Add((new Point2(p1x, p1y), 0));
                    result.Add((new Point2(p2x, p2y), 0));
                }
                else if (dist2 > 25 || riverCells.Count < 6)
                {
                    var mx = (p1.X + p2.X) / 2 + -sinMeander;
                    var my = (p1.Y + p2.Y) / 2 + cosMeander;
                    result.Add((new Point2(mx, my), 0));
                }
            }

            return result;
        }

        private List<Point2> GenerateCatmullRomSplineWorldAlpha(List<Point2> pts, int segmentsPerSpan = 6, double alpha = 0.1)
        {
            var result = new List<Point2>();
            if (pts == null || pts.Count == 0) return result;
            if (pts.Count == 1) { result.Add(pts[0]); return result; }

            var p = new List<Point2>();
            p.Add(pts[0]);
            p.AddRange(pts);
            p.Add(pts[pts.Count - 1]);

            // Safe division: when the phantom start/end duplicate produces a zero knot
            // interval (tj == tk because the two bracketing points are identical), any
            // interpolation over that degenerate span equals the shared endpoint.
            // Without this guard the division produces NaN which the dedup step silently
            // removes — causing the spline to terminate at pts[n-2] instead of pts[n-1]
            // and leaving rivers visually disconnected from the coast.
            static double S(double num, double den) => Math.Abs(den) < 1e-9 ? 0.0 : num / den;

            for (int i = 0; i < p.Count - 3; i++)
            {
                var p0 = p[i];
                var p1 = p[i + 1];
                var p2 = p[i + 2];
                var p3 = p[i + 3];

                double tj0 = 0.0;
                double tj1 = tj0 + Math.Pow(Distance(p0, p1), alpha);
                double tj2 = tj1 + Math.Pow(Distance(p1, p2), alpha);
                double tj3 = tj2 + Math.Pow(Distance(p2, p3), alpha);

                for (int j = 0; j <= segmentsPerSpan; j++)
                {
                    double t = tj1 + (tj2 - tj1) * j / (double)segmentsPerSpan;

                    Point2 A1 = Interpolate(p0, p1, S(t - tj0, tj1 - tj0));
                    Point2 A2 = Interpolate(p1, p2, S(t - tj1, tj2 - tj1));
                    Point2 A3 = Interpolate(p2, p3, S(t - tj2, tj3 - tj2));

                    Point2 B1 = Interpolate(A1, A2, S(t - tj0, tj2 - tj0));
                    Point2 B2 = Interpolate(A2, A3, S(t - tj1, tj3 - tj1));

                    Point2 C = Interpolate(B1, B2, S(t - tj1, tj2 - tj1));
                    result.Add(C);
                }
            }

            var dedup = new List<Point2>();
            Point2? last = null;
            foreach (var pt in result)
            {
                if (last == null || Math.Abs(pt.X - last.Value.X) > 1e-6 || Math.Abs(pt.Y - last.Value.Y) > 1e-6)
                {
                    dedup.Add(pt);
                    last = pt;
                }
            }
            return dedup;
        }

        private Point2 Interpolate(Point2 a, Point2 b, double t)
        {
            return new Point2(a.X + (b.X - a.X) * t, a.Y + (b.Y - a.Y) * t);
        }

        // Distance(Point2,Point2) is defined in helpers; use that implementation to avoid duplication.

        // Compute per-vertex half-widths (screen pixels) for a river by linearly interpolating
        // from SourceWidth (upstream, index 0) to Width (mouth, index n-1).
        // When SourceWidth is unset (0) a small fraction of the mouth width is used so the river
        // tapers to a thin line at its headwaters rather than starting full-width.
        private float[] ComputeRiverHalfWidths(River river, int n, double scale)
        {
            if (n <= 0) return Array.Empty<float>();

            double mouthPx, sourcePx;
            if (river.Discharge > 0)
            {
                // Discharge (38–600+) gives good natural variation; Width values (0.03–0.21)
                // are SVG micro-units too small to differentiate rivers at any usable coefficient.
                mouthPx  = Math.Sqrt(river.Discharge) * 0.12 * scale;
                sourcePx = mouthPx * 0.15;      // taper to ~15 % at headwaters
            }
            else if (river.Width > 0)
            {
                mouthPx  = river.Width * 0.25 * scale;
                sourcePx = river.SourceWidth > 0
                    ? river.SourceWidth * 0.25 * scale
                    : mouthPx * 0.15;
            }
            else
            {
                mouthPx  = 1.0;
                sourcePx = 0.5;
            }

            float mouthHalf  = Math.Max(0.5f,  (float)mouthPx)  * 0.5f;
            float sourceHalf = Math.Max(0.25f, (float)sourcePx) * 0.5f;

            var result = new float[n];
            for (int i = 0; i < n; i++)
            {
                float t = n > 1 ? i / (float)(n - 1) : 1f;
                result[i] = sourceHalf + (mouthHalf - sourceHalf) * t;
            }
            return result;
        }

        // Given world-space Voronoi coast-edge vertices and the current screen-space centerline,
        // returns the screen-space polygon corners that align with the coast edge, correctly
        // assigned as (mouthLeft, mouthRight) relative to the river's flow direction at the mouth.
        private (Point2? mouthLeft, Point2? mouthRight) ComputeMouthCorners(
            Point2? coastV1World, Point2? coastV2World, List<Point2> screenCenters)
        {
            if (!coastV1World.HasValue || !coastV2World.HasValue || screenCenters.Count < 2)
                return (null, null);
            var s1 = _viewport.WorldToScreen(coastV1World.Value);
            var s2 = _viewport.WorldToScreen(coastV2World.Value);
            var v1 = new Point2(s1.X, s1.Y);
            var v2 = new Point2(s2.X, s2.Y);
            int n = screenCenters.Count;
            var center = screenCenters[n - 1];
            // Signed cross product: > 0 means v1 is on the left side of the flow direction
            var flowX = screenCenters[n - 1].X - screenCenters[n - 2].X;
            var flowY = screenCenters[n - 1].Y - screenCenters[n - 2].Y;
            var cross1 = flowX * (v1.Y - center.Y) - flowY * (v1.X - center.X);
            return cross1 >= 0 ? (v1, v2) : (v2, v1);
        }

        // Build a filled polygon approximating a stroke around the centerline points.
        // halfWidth is in screen pixels (uniform across all vertices).
        private Point2[] BuildRiverPolygon(List<Point2> center, float halfWidth)
        {
            if (center == null || center.Count < 2 || halfWidth <= 0) return Array.Empty<Point2>();
            var widths = new float[center.Count];
            Array.Fill(widths, halfWidth);
            return BuildRiverPolygon(center, widths);
        }

        // Build a filled polygon with per-vertex half-widths so the river tapers from source to mouth.
        // mouthLeft/mouthRight, when provided, snap the mouth corners to the Voronoi coast-edge
        // endpoints so the river polygon aligns exactly with the shoreline.
        private Point2[] BuildRiverPolygon(List<Point2> center, float[] halfWidths, Point2? mouthLeft = null, Point2? mouthRight = null)
        {
            if (center == null || center.Count < 2 || halfWidths == null || halfWidths.Length == 0) return Array.Empty<Point2>();

            int n = center.Count;
            var normals = new Point2[Math.Max(1, n - 1)];

            for (int i = 0; i < n - 1; i++)
            {
                var dx = center[i + 1].X - center[i].X;
                var dy = center[i + 1].Y - center[i].Y;
                var len = Math.Sqrt(dx * dx + dy * dy);
                if (len <= 1e-9)
                {
                    normals[i] = new Point2(0, 0);
                }
                else
                {
                    var nx = -dy / len;
                    var ny = dx / len;
                    normals[i] = new Point2(nx, ny);
                }
            }

            var left = new List<Point2>(n);
            var right = new List<Point2>(n);

            for (int i = 0; i < n; i++)
            {
                Point2 nrm;
                if (i == 0) nrm = normals[0];
                else if (i == n - 1) nrm = normals[n - 2];
                else
                {
                    var a = normals[i - 1];
                    var b = normals[i];
                    var sx = a.X + b.X;
                    var sy = a.Y + b.Y;
                    var slen = Math.Sqrt(sx * sx + sy * sy);
                    if (slen <= 1e-9)
                    {
                        nrm = (Math.Abs(b.X) + Math.Abs(b.Y) > 0) ? b : a;
                    }
                    else
                    {
                        nrm = new Point2(sx / slen, sy / slen);
                    }
                }

                // If normal is degenerate, fallback to previous or next
                if (Math.Abs(nrm.X) < 1e-9 && Math.Abs(nrm.Y) < 1e-9)
                {
                    if (i > 0) nrm = new Point2(center[i].X - center[i - 1].X, center[i].Y - center[i - 1].Y);
                    if (Math.Abs(nrm.X) < 1e-9 && Math.Abs(nrm.Y) < 1e-9 && i < n - 1) nrm = new Point2(center[i + 1].X - center[i].X, center[i + 1].Y - center[i].Y);
                    var ln = Math.Sqrt(nrm.X * nrm.X + nrm.Y * nrm.Y);
                    if (ln > 1e-9) nrm = new Point2(-nrm.Y / ln, nrm.X / ln);
                    else nrm = new Point2(0, 1);
                }

                float hw = i < halfWidths.Length ? halfWidths[i] : halfWidths[halfWidths.Length - 1];
                left.Add(new Point2(center[i].X + nrm.X * hw, center[i].Y + nrm.Y * hw));
                right.Add(new Point2(center[i].X - nrm.X * hw, center[i].Y - nrm.Y * hw));
            }

            // Align the mouth with the Voronoi coast edge, clamped to river half-width.
            // - When the Voronoi edge is wider than the river: corners sit at ±hw from
            //   center along the coast direction (no fan).
            // - When the river is wider than the Voronoi gap: corners snap to the exact
            //   Voronoi vertices so the polygon closes flush against the shoreline.
            if (mouthLeft.HasValue && mouthRight.HasValue)
            {
                float hw = (n - 1) < halfWidths.Length ? halfWidths[n - 1] : halfWidths[halfWidths.Length - 1];
                var c  = center[n - 1];
                var ml = mouthLeft.Value;
                var mr = mouthRight.Value;

                // Coast-edge direction: from mr toward ml (normalized)
                var edgeX = ml.X - mr.X;
                var edgeY = ml.Y - mr.Y;
                var edgeLen = Math.Sqrt(edgeX * edgeX + edgeY * edgeY);

                if (edgeLen > 1e-9)
                {
                    var dx = edgeX / edgeLen;
                    var dy = edgeY / edgeLen;

                    // Signed distances from the centerline crossing to each corner,
                    // projected along the coast direction.  tLeft >= 0, tRight <= 0.
                    var tLeft  = Math.Max(0.0, (ml.X - c.X) * dx + (ml.Y - c.Y) * dy);
                    var tRight = Math.Min(0.0, (mr.X - c.X) * dx + (mr.Y - c.Y) * dy);

                    // Clamp to river half-width to prevent fan / funnel shapes
                    tLeft  = Math.Min(tLeft,   hw);
                    tRight = Math.Max(tRight, -hw);

                    left[n - 1]  = new Point2(c.X + dx * tLeft,  c.Y + dy * tLeft);
                    right[n - 1] = new Point2(c.X + dx * tRight, c.Y + dy * tRight);
                }
                // Degenerate edge: the main loop already placed good corners; leave them.
            }

            // Build polygon: left side then reversed right side
            var poly = new List<Point2>(left.Count + right.Count);
            poly.AddRange(left);
            for (int i = right.Count - 1; i >= 0; i--) poly.Add(right[i]);

            // Remove consecutive duplicates
            var cleaned = new List<Point2>();
            Point2? last = null;
            foreach (var p in poly)
            {
                if (last == null || Math.Abs(p.X - last.Value.X) > 0.001 || Math.Abs(p.Y - last.Value.Y) > 0.001)
                {
                    cleaned.Add(p);
                    last = p;
                }
            }

            // Return cleaned polygon (consecutive duplicates already removed) without expensive Distinct call
            return cleaned.ToArray();
        }

        // World-space Catmull-Rom spline for Point2 (uniform parameterisation).
        // Used as a fallback when the chord-length variant returns fewer than 2 points.
        private List<Point2> GenerateCatmullRomSplineWorld(List<Point2> points, int segmentsPerSpan = 6)
        {
            var result = new List<Point2>();
            if (points == null || points.Count == 0) return result;
            if (points.Count == 1) { result.Add(points[0]); return result; }

            var p = new List<Point2>();
            p.Add(points[0]);
            p.AddRange(points);
            p.Add(points[points.Count - 1]);

            for (int i = 0; i < p.Count - 3; i++)
            {
                var p0 = p[i];
                var p1 = p[i + 1];
                var p2 = p[i + 2];
                var p3 = p[i + 3];

                for (int j = 0; j <= segmentsPerSpan; j++)
                {
                    double t = j / (double)segmentsPerSpan;
                    double t2 = t * t;
                    double t3 = t2 * t;

                    double x = 0.5 * ((2 * p1.X) + (-p0.X + p2.X) * t + (2 * p0.X - 5 * p1.X + 4 * p2.X - p3.X) * t2 + (-p0.X + 3 * p1.X - 3 * p2.X + p3.X) * t3);
                    double y = 0.5 * ((2 * p1.Y) + (-p0.Y + p2.Y) * t + (2 * p0.Y - 5 * p1.Y + 4 * p2.Y - p3.Y) * t2 + (-p0.Y + 3 * p1.Y - 3 * p2.Y + p3.Y) * t3);

                    result.Add(new Point2(x, y));
                }
            }

            var dedup = new List<Point2>();
            Point2? last = null;
            foreach (var pt in result)
            {
                if (last == null || Math.Abs(pt.X - last.Value.X) > 1e-6 || Math.Abs(pt.Y - last.Value.Y) > 1e-6)
                {
                    dedup.Add(pt);
                    last = pt;
                }
            }
            return dedup;
        }

        // Return true if point is inside ANY of the provided polygons (ray-casting)
        private bool IsPointInsideAnyPolygon(Point2 pt, List<SvgPolygon> polygons)
        {
            foreach (var poly in polygons)
            {
                if (EnableRiverPerfDebug) _pipCallsThisFrame++;
                if (IsPointInPolygon(pt, poly.Points)) return true;
            }
            return false;
        }

        // Ray-casting point-in-polygon (winding) for list of Point2
        private bool IsPointInPolygon(Point2 p, List<Point2> poly)
        {
            bool inside = false;
            int j = poly.Count - 1;
            double px = p.X, py = p.Y;
            for (int i = 0; i < poly.Count; j = i++)
            {
                var pi = poly[i];
                var pj = poly[j];
                if (((pi.Y > py) != (pj.Y > py)) &&
                    (px < (pj.X - pi.X) * (py - pi.Y) / (pj.Y - pi.Y + 0.0) + pi.X))
                {
                    inside = !inside;
                }
            }
            return inside;
        }

        // Find intersection point between segment a-b and any polygon edge; returns first (closest t)
        private Point2? FindSegmentPolygonIntersection(Point2 a, Point2 b, List<SvgPolygon> polygons)
        {
            double bestT = double.MaxValue;
            Point2? best = null;

            foreach (var poly in polygons)
            {
                var pts = poly.Points;
                for (int i = 0; i < pts.Count; i++)
                {
                    if (EnableRiverPerfDebug) _segIntCallsThisFrame++;
                    var c = pts[i];
                    var d = pts[(i + 1) % pts.Count];
                    var ip = SegmentIntersection(a, b, c, d, out double t);
                    if (ip.HasValue && t >= 0 && t <= 1)
                    {
                        if (t < bestT)
                        {
                            bestT = t;
                            best = ip;
                        }
                    }
                }
            }

            return best;
        }

        // Segment intersection: returns intersection point and t along segment a->b
        private Point2? SegmentIntersection(Point2 a, Point2 b, Point2 c, Point2 d, out double tOut)
        {
            tOut = 0;
            double x1 = a.X, y1 = a.Y;
            double x2 = b.X, y2 = b.Y;
            double x3 = c.X, y3 = c.Y;
            double x4 = d.X, y4 = d.Y;

            double denom = (y4 - y3) * (x2 - x1) - (x4 - x3) * (y2 - y1);
            if (Math.Abs(denom) < 1e-9) return null; // parallel

            double ua = ((x4 - x3) * (y1 - y3) - (y4 - y3) * (x1 - x3)) / denom;
            double ub = ((x2 - x1) * (y1 - y3) - (y2 - y1) * (x1 - x3)) / denom;

            if (ua >= 0 && ua <= 1 && ub >= 0 && ub <= 1)
            {
                tOut = ua;
                return new Point2(x1 + ua * (x2 - x1), y1 + ua * (y2 - y1));
            }

            return null;
        }

        // Draw a pixel-diff overlay between two polygon sets (both in world coordinates).
        // Differences are shown in semi-transparent red on the provided canvas.
        private void DrawPixelDiffOverlay(SKCanvas targetCanvas, List<SvgPolygon> aPolys, List<SvgPolygon> bPolys)
        {
            // Respect EnablePixelDiff and throttle/caching to avoid heavy per-frame work
            if (!EnablePixelDiff) return;

            if (_viewport.ViewBounds.Width <= 0 || _viewport.ViewBounds.Height <= 0) return;

            int tick = Environment.TickCount;
            // If we have a cached image that's recent enough, draw it and skip recompute
            if (_cachedPixelDiffImage != null && tick - _lastPixelDiffTick < _pixelDiffThrottleMs)
            {
                using var paintDrawCached = new SKPaint { IsAntialias = true, FilterQuality = SKFilterQuality.None, Color = SKColors.Red.WithAlpha(200) };
                targetCanvas.DrawImage(_cachedPixelDiffImage, new SKRect(0, 0, _viewport.ViewBounds.Width, _viewport.ViewBounds.Height), paintDrawCached);
                return;
            }

            // Compute scaled bitmap size to limit cost
            double vw = Math.Max(1.0, _viewport.ViewBounds.Width);
            double vh = Math.Max(1.0, _viewport.ViewBounds.Height);
            double scaleFactor = Math.Min(1.0, (double)PixelDiffMaxSize / Math.Max(vw, vh));
            int w = Math.Max(1, (int)Math.Round(vw * scaleFactor));
            int h = Math.Max(1, (int)Math.Round(vh * scaleFactor));

            using var bmpA = new SKBitmap(w, h, true);
            using var bmpB = new SKBitmap(w, h, true);
            bmpA.Erase(SKColors.Transparent);
            bmpB.Erase(SKColors.Transparent);

            // Helper to rasterize polygon list into bitmap (scale canvas so world->screen coords fit)
            void Rasterize(List<SvgPolygon> polys, SKBitmap bmp)
            {
                using var c = new SKCanvas(bmp);
                c.Clear(SKColors.Transparent);
                // Scale down drawing so world->screen coords map into reduced bitmap
                float s = (float)scaleFactor;
                c.Scale(s, s);
                using var paint = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Fill, Color = SKColors.White };
                foreach (var poly in polys)
                {
                    if (poly?.Points == null || poly.Points.Count < 3) continue;
                    using var path = new SKPath();
                    var sp0 = _viewport.WorldToScreen(poly.Points[0]);
                    path.MoveTo(new SKPoint((float)sp0.X, (float)sp0.Y));
                    for (int i = 1; i < poly.Points.Count; i++)
                    {
                        var sp = _viewport.WorldToScreen(poly.Points[i]);
                        path.LineTo(new SKPoint((float)sp.X, (float)sp.Y));
                    }
                    path.Close();
                    c.DrawPath(path, paint);
                }
                c.Flush();
            }

            Rasterize(aPolys, bmpA);
            Rasterize(bPolys, bmpB);

            // Compute diff into a new bitmap
            using var diffBmp = new SKBitmap(w, h, true);
            int diffCount = 0;
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    var pa = bmpA.GetPixel(x, y);
                    var pb = bmpB.GetPixel(x, y);
                    bool aOn = pa.Alpha > 0 && pa.Red > 0;
                    bool bOn = pb.Alpha > 0 && pb.Red > 0;
                    if (aOn != bOn)
                    {
                        diffBmp.SetPixel(x, y, SKColors.Red.WithAlpha(200));
                        diffCount++;
                    }
                    else
                    {
                        diffBmp.SetPixel(x, y, SKColors.Transparent);
                    }
                }
            }

            double total = (double)w * h;
            double pct = total > 0 ? (double)diffCount / total * 100.0 : 0.0;
            RiverDebug($"DIAG: Pixel-diff: {diffCount} pixels ({pct:F4}%) differ between A and B");

            // Cache the diff image and draw it scaled up to viewport
            using var img = SKImage.FromBitmap(diffBmp);
            // Dispose previous cached image and replace
            try { _cachedPixelDiffImage?.Dispose(); } catch { }
            _cachedPixelDiffImage = img;
            _lastPixelDiffTick = tick;

            using var paintDraw = new SKPaint { IsAntialias = true, FilterQuality = SKFilterQuality.None, Color = SKColors.Red.WithAlpha(200) };
            targetCanvas.DrawImage(_cachedPixelDiffImage, new SKRect(0, 0, _viewport.ViewBounds.Width, _viewport.ViewBounds.Height), paintDraw);
        }
    }
}
