#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using SkiaSharp;
using Caps.RPG.World.Models;
using System.Diagnostics;
using System.IO;
using Caps.RPG.World.Models.Graphics;

namespace Caps.RPG.World.Rendering
{
    public partial class WorldMapRenderer : MapRendererBase
    {
        // Polygons actually drawn for landmask in the last render pass (world coordinates).
        // Use this for clipping rivers to ensure clipping geometry matches drawing geometry.
        // Moved to base class as RenderedLandmask property.
        

        // Rendering caches to avoid recomputing screen-space geometry every frame
        private string? _viewportCacheKey;
        private WorldMap? _cachedPack;
        private SKPoint[][]? _cachedCellScreenPolygons;
        private SKRect[]? _cachedCellBounds;      // per-cell screen-space AABB for O(1) culling
        private SKPoint[]? _cachedCellCenters;
        private SKPoint[][]? _cachedLandmassScreenPolygons;
        private double _cachedMinX = double.MaxValue;
        private double _cachedMaxX = double.MinValue;
        private double _cachedMinY = double.MaxValue;
        private double _cachedMaxY = double.MinValue;
        // Track last viewport values to allow cheap translation of cached screen-space points
        private double _lastViewportScale = 1.0;
        private Point2 _lastViewportOffset = new(0, 0);
        // Cached river smoothed world-space polylines (one per river index) - independent of viewport
        private List<List<Point2>>? _cachedRiverSmoothWorlds;
        // Cached world-space axis-aligned bounds for each cached river (parallel to _cachedRiverSmoothWorlds)
        private List<SkiaSharp.SKRect>? _cachedRiverWorldAABBs;
        // Cached screen-space river polygons (one per river) to avoid per-frame geometry rebuilds
        private SkiaSharp.SKPoint[][]? _cachedRiverScreenPolygons;
        // World-space Voronoi coast-edge vertices at each river's mouth â€” null for tributaries and
        // rivers where no Voronoi edge was found.  Cached per-pack so scale-change rebuilds can
        // re-project them into the new screen space (parallel to _cachedRiverSmoothWorlds).
        private Point2?[]? _cachedRiverCoastEdgeV1;
        private Point2?[]? _cachedRiverCoastEdgeV2;

        // Shared paint and path objects for river drawing to avoid per-river allocations
        

        // Simple in-memory perf log; written to disk in Dispose(). Enabled by default.

        public enum LogLevel { None = 0, Error = 1, Warn = 2, Info = 3, Debug = 4, Trace = 5 }
        public LogLevel Verbosity { get; set; } = LogLevel.Warn;

        public WorldMapRenderer()
        {
        }

        

        // UpdateStatus and paints provided by MapRendererBase

        // Unconditional helpers so diagnostics emitted from other partial files are not removed by
        // the C# Conditional attribute (which is evaluated per compilation unit).
        private void RiverDebug(string message) => ShortDebugWrite(message, LogLevel.Trace);

        private void RiverStatus(string status, string layer, bool isEnabled, int? count = null) => UpdateStatus(status, layer, isEnabled, count);

        private void ShortDebugWrite(string? message, LogLevel level = LogLevel.Info)
        {
            if (string.IsNullOrEmpty(message)) return;
            if (level > Verbosity) return;
            const int maxLen = 300;
            if (message!.Length > maxLen)
            {
                Debug.WriteLine(message.Substring(0, maxLen) + " ...(truncated)");
            }
            else
            {
                Debug.WriteLine(message);
            }
        }

        // Perf logging helpers (thread-safe)
        private void PerfLog(string message)
        {
            if (!_perfEnabled) return;
            lock (_perfLock)
            {
                _perfLog.Add($"{DateTime.UtcNow:O} {message}");
            }
        }

        private void PerfLogDuration(string label, long ms)
        {
            PerfLog($"DURATION {label}: {ms} ms");
        }

        // Update cached screen-space geometry when the viewport or pack changes.
        private void UpdateCachesIfNeeded(WorldMap pack)
        {
            if (pack == null) return;
            // If pack is same and scale unchanged we can either reuse or translate cached screen points
            if (ReferenceEquals(pack, _cachedPack) && Math.Abs(_viewport.Scale - _lastViewportScale) < 1e-9)
            {
                // If only offset changed, translate cached screen points cheaply instead of rebuilding
                if (!(_viewport.Offset.X == _lastViewportOffset.X && _viewport.Offset.Y == _lastViewportOffset.Y))
                {
                    var dx = (float)((_viewport.Offset.X - _lastViewportOffset.X) * _viewport.Scale);
                    var dy = (float)((_viewport.Offset.Y - _lastViewportOffset.Y) * _viewport.Scale);

                    if (_cachedCellCenters != null)
                    {
                        for (int i = 0; i < _cachedCellCenters.Length; i++) _cachedCellCenters[i] = new SKPoint(_cachedCellCenters[i].X + dx, _cachedCellCenters[i].Y + dy);
                    }
                    if (_cachedCellScreenPolygons != null)
                    {
                        for (int i = 0; i < _cachedCellScreenPolygons.Length; i++)
                        {
                            var arr = _cachedCellScreenPolygons[i];
                            if (arr == null) continue;
                            for (int k = 0; k < arr.Length; k++) arr[k] = new SKPoint(arr[k].X + dx, arr[k].Y + dy);
                        }
                    }
                    if (_cachedCellBounds != null)
                    {
                        for (int i = 0; i < _cachedCellBounds.Length; i++)
                            _cachedCellBounds[i] = new SKRect(_cachedCellBounds[i].Left + dx, _cachedCellBounds[i].Top + dy, _cachedCellBounds[i].Right + dx, _cachedCellBounds[i].Bottom + dy);
                    }
                    if (_cachedLandmassScreenPolygons != null)
                    {
                        for (int i = 0; i < _cachedLandmassScreenPolygons.Length; i++)
                        {
                            var arr = _cachedLandmassScreenPolygons[i];
                            if (arr == null) continue;
                            for (int k = 0; k < arr.Length; k++) arr[k] = new SKPoint(arr[k].X + dx, arr[k].Y + dy);
                        }
                    }
                    if (_cachedRiverScreenPolygons != null)
                    {
                        for (int i = 0; i < _cachedRiverScreenPolygons.Length; i++)
                        {
                            var arr = _cachedRiverScreenPolygons[i];
                            if (arr == null) continue;
                            for (int k = 0; k < arr.Length; k++) arr[k] = new SKPoint(arr[k].X + dx, arr[k].Y + dy);
                        }
                    }

                    _lastViewportOffset = pack != null ? _viewport.Offset : new Point2(0, 0);
                    _lastViewportScale = _viewport.Scale;
                }

                return;
            }

            // Full rebuild for new pack or scale change
            _viewportCacheKey = $"{_viewport.Scale:F6}:{_viewport.Offset.X:F3}:{_viewport.Offset.Y:F3}:{pack.GetHashCode()}";
            _cachedPack = pack;
            _lastViewportScale = _viewport.Scale;
            _lastViewportOffset = _viewport.Offset;

            // Reset caches
            _cachedCellScreenPolygons = null;
            _cachedCellBounds = null;
            _cachedCellCenters = null;
            _cachedLandmassScreenPolygons = null;
            _cachedMinX = double.MaxValue; _cachedMaxX = double.MinValue; _cachedMinY = double.MaxValue; _cachedMaxY = double.MinValue;

            // Precompute cell center screen points if available
            try
            {
                if (pack.Cells?.Coordinates != null)
                {
                    _cachedCellCenters = new SKPoint[pack.Cells.Coordinates.Length];
                    for (int i = 0; i < pack.Cells.Coordinates.Length; i++)
                    {
                        var p = pack.Cells.Coordinates[i];
                        var s = _viewport.WorldToScreen(p);
                        _cachedCellCenters[i] = s;

                        if (p.X < _cachedMinX) _cachedMinX = p.X;
                        if (p.X > _cachedMaxX) _cachedMaxX = p.X;
                        if (p.Y < _cachedMinY) _cachedMinY = p.Y;
                        if (p.Y > _cachedMaxY) _cachedMaxY = p.Y;
                    }
                }
            }
            catch { /* best-effort cache build */ }

            // Precompute vertex-indexed cell polygons (screen-space) when vertex data available
            try
            {
                if (pack.Cells?.VertexIndexes != null && pack.Vertices?.Coordinates != null)
                {
                    var verts = pack.Vertices.Coordinates;
                    _cachedCellScreenPolygons = new SKPoint[pack.Cells.VertexIndexes.Length][];
                    for (int ci = 0; ci < pack.Cells.VertexIndexes.Length; ci++)
                    {
                        var vidx = pack.Cells.VertexIndexes[ci];
                        if (vidx == null || vidx.Length < 3) continue;
                        var arr = new SKPoint[vidx.Length];
                        bool valid = true;
                        for (int k = 0; k < vidx.Length; k++)
                        {
                            var vi = vidx[k];
                            if (vi < 0 || vi >= verts.Length) { valid = false; break; }
                            arr[k] = _viewport.WorldToScreen(verts[vi]);
                        }
                        if (valid) _cachedCellScreenPolygons[ci] = arr;
                    }

                    // Build per-cell AABB cache for O(1) culling â€” avoids 4Ã— LINQ traversals per cell per frame
                    _cachedCellBounds = new SKRect[_cachedCellScreenPolygons.Length];
                    for (int ci = 0; ci < _cachedCellScreenPolygons.Length; ci++)
                    {
                        var arr = _cachedCellScreenPolygons[ci];
                        if (arr == null || arr.Length == 0) continue;
                        float mnX = arr[0].X, mxX = arr[0].X, mnY = arr[0].Y, mxY = arr[0].Y;
                        for (int k = 1; k < arr.Length; k++)
                        {
                            if (arr[k].X < mnX) mnX = arr[k].X;
                            if (arr[k].X > mxX) mxX = arr[k].X;
                            if (arr[k].Y < mnY) mnY = arr[k].Y;
                            if (arr[k].Y > mxY) mxY = arr[k].Y;
                        }
                        _cachedCellBounds[ci] = new SKRect(mnX, mnY, mxX, mxY);
                    }
                }
            }
            catch { }

            // Precompute landmass polygons in screen space
            try
            {
                if (pack.LandmassPolygons != null)
                {
                    _cachedLandmassScreenPolygons = new SKPoint[pack.LandmassPolygons.Count][];
                    for (int i = 0; i < pack.LandmassPolygons.Count; i++)
                    {
                        var poly = pack.LandmassPolygons[i];
                        if (poly?.Points == null || poly.Points.Count < 3) continue;
                        var arr = new SKPoint[poly.Points.Count];
                        for (int j = 0; j < poly.Points.Count; j++) arr[j] = _viewport.WorldToScreen(poly.Points[j]);
                        _cachedLandmassScreenPolygons[i] = arr;

                        // update bounds from polygon points
                        for (int j = 0; j < poly.Points.Count; j++)
                        {
                            var p = poly.Points[j];
                            if (p.X < _cachedMinX) _cachedMinX = p.X;
                            if (p.X > _cachedMaxX) _cachedMaxX = p.X;
                            if (p.Y < _cachedMinY) _cachedMinY = p.Y;
                            if (p.Y > _cachedMaxY) _cachedMaxY = p.Y;
                        }
                    }
                }
            }
            catch { }

            // If bounds still empty, try vertices
            try
            {
                if ((double.IsInfinity(_cachedMinX) || _cachedMinX == double.MaxValue) && pack.Vertices?.Coordinates != null)
                {
                    foreach (var v in pack.Vertices.Coordinates)
                    {
                        if (v.X < _cachedMinX) _cachedMinX = v.X;
                        if (v.X > _cachedMaxX) _cachedMaxX = v.X;
                        if (v.Y < _cachedMinY) _cachedMinY = v.Y;
                        if (v.Y > _cachedMaxY) _cachedMaxY = v.Y;
                    }
                }
            }
            catch { }

            // Last resort: settlements
            try
            {
                if ((_cachedMinX == double.MaxValue || _cachedMinY == double.MaxValue) && pack.Burgs != null && pack.Burgs.Count > 0)
                {
                    foreach (var b in pack.Burgs)
                    {
                        if (b.X < _cachedMinX) _cachedMinX = b.X;
                        if (b.X > _cachedMaxX) _cachedMaxX = b.X;
                        if (b.Y < _cachedMinY) _cachedMinY = b.Y;
                        if (b.Y > _cachedMaxY) _cachedMaxY = b.Y;
                    }
                }
            }
            catch { }
        }

        public override void RenderMap(SKCanvas canvas, WorldMap map, int width, int height)
        {
            ShortDebugWrite("RenderMap called", LogLevel.Info);
            if (map == null) { ShortDebugWrite("Map or Pack is null!", LogLevel.Error); return; }

            var swTotal = Stopwatch.StartNew();

            canvas.Clear(ParseColor("#4682b4"));
            _viewport.ViewBounds = new SKRect(0, 0, width, height);

            if (_viewport.Scale == 1.0) FitMapToViewport(map, width, height);

            // Always update the cell/geometry cache so tooltip hit-testing works in all map modes,
            // not just the ones that happen to call RenderTerrain or DrawRiversIfPresent.
            UpdateCachesIfNeeded(map);

            try
            {
                // Render terrain when the terrain layer is enabled. In Political mode we still
                // want to draw terrain for context, but we must also draw state areas *in addition*
                // when the StateAreas layer is enabled. The previous logic skipped rendering
                // state areas when the Terrain flag was present which caused the political mode
                // to be effectively disabled (and in some cases led to confusing behavior).
                if (EnabledLayers.HasFlag(MapLayer.Terrain))
                {
                    var sw = Stopwatch.StartNew();
                    RenderTerrain(canvas, map);
                    sw.Stop();
                    PerfLogDuration("RenderTerrain", sw.ElapsedMilliseconds);
                }

                // If political/state areas are enabled, render them as well. This ensures
                // Political map mode can display state boundaries on top of terrain.
                if (EnabledLayers.HasFlag(MapLayer.StateAreas))
                {
                    try { RenderStateAreas(canvas, map); }
                    catch (Exception ex) { Debug.WriteLine($"STATE AREAS CRASHED: {ex.Message}"); }
                }

                // Cultural coloring mode (per-culture areas or cell colors)
                if (EnabledLayers.HasFlag(MapLayer.Cultural))
                {
                    try { RenderCultureAreas(canvas, map); }
                    catch (Exception ex) { Debug.WriteLine($"CULTURE AREAS CRASHED: {ex.Message}"); }
                }

                // Religion coloring mode (per-religion areas or cell colors)
                if (EnabledLayers.HasFlag(MapLayer.Religion))
                {
                    try { RenderReligionAreas(canvas, map); }
                    catch (Exception ex) { Debug.WriteLine($"RELIGION AREAS CRASHED: {ex.Message}"); }
                }

                // State border outlines (soft/thin), used by Physical, Cultural, and Religion modes.
                // Political mode draws its own borders internally via RenderStateAreas.
                if (EnabledLayers.HasFlag(MapLayer.StateBorders))
                {
                    try { DrawStateBorders(canvas, map); }
                    catch (Exception ex) { Debug.WriteLine($"DrawStateBorders failed: {ex.Message}"); }
                }

                // Draw rivers when the layer is enabled regardless of high-level map mode.
                // Previously rivers were only drawn from inside the StateAreas branch which
                // caused DrawRiversIfPresent to be skipped in common cases (e.g. Physical mode).
                if (EnabledLayers.HasFlag(MapLayer.Rivers))
                {
                    var sw = Stopwatch.StartNew();
                    try { DrawRiversIfPresent(canvas, map); }
                    catch (Exception ex) { Debug.WriteLine($"DrawRiversIfPresent failed: {ex.Message}"); }
                    sw.Stop();
                    PerfLogDuration("DrawRiversIfPresent", sw.ElapsedMilliseconds);
                }

                if (EnabledLayers.HasFlag(MapLayer.Settlements)) RenderEnhancedSettlements(canvas, map);
                if (EnabledLayers.HasFlag(MapLayer.Labels)) RenderEnhancedLabels(canvas, map);

                // Debug overlay: draw current rendering state for reliable diagnostics
                try
                {
                    if (_textPaint != null)
                    {
                        var packInfo = map;
                        var lines = new List<string>
                        {
                            $"Mode: {CurrentMapMode}",
                            $"Layers: {EnabledLayers}",
                            $"Cells: {packInfo?.Cells?.Coordinates?.Length ?? 0}",
                            $"Vertices: {packInfo?.Vertices?.Coordinates?.Length ?? 0}",
                            $"Grid points: {packInfo?.Grid?.Points?.Length ?? 0}",
                            $"Cultures: {packInfo?.Cultures?.Count ?? 0}",
                            $"States: {packInfo?.States?.Count ?? 0}"
                        };

                        using var overlayBg = new SKPaint { IsAntialias = true, Color = SKColors.White.WithAlpha(180), Style = SKPaintStyle.Fill };
                        using var overlayText = new SKPaint { IsAntialias = true, Color = SKColors.Black, TextSize = 12, Typeface = _textPaint.Typeface };

                        float padding = 6f;
                        float lineHeight = overlayText.TextSize * 1.25f;
                        float boxW = 0f;
                        foreach (var l in lines) boxW = Math.Max(boxW, overlayText.MeasureText(l));
                        boxW += padding * 2;
                        float boxH = lineHeight * lines.Count + padding * 2;

                        var boxRect = new SKRect(10, 10, 10 + boxW, 10 + boxH);
                        canvas.DrawRoundRect(boxRect, 4, 4, overlayBg);

                        for (int i = 0; i < lines.Count; i++)
                        {
                            var y = 10 + padding + (i + 1) * lineHeight - (lineHeight - overlayText.TextSize) / 2;
                            canvas.DrawText(lines[i], 10 + padding, y, overlayText);
                        }
                    }
                }
                catch { }

                // Province tooltip overlay — rendered last so it sits above all other layers
                try { RenderTooltipOverlay(canvas, map); }
                catch { }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"RENDER MAP CRASHED: {ex.Message}");
            }
            finally
            {
                swTotal.Stop();
                PerfLogDuration("RenderMap total", swTotal.ElapsedMilliseconds);
            }
        }

        private void FitMapToViewport(WorldMap map, int width, int height)
        {
            if (map?.Cells?.Coordinates != null && map.Cells.Coordinates.Length > 0)
            {
                var points = map.Cells.Coordinates;
                var minX = points.Min(p => p.X);
                var maxX = points.Max(p => p.X);
                var minY = points.Min(p => p.Y);
                var maxY = points.Max(p => p.Y);
                var mapWidth = maxX - minX;
                var mapHeight = maxY - minY;
                var scaleX = width * 0.85 / mapWidth;
                var scaleY = height * 0.85 / mapHeight;
                _viewport.Scale = Math.Min(scaleX, scaleY);
                _viewport.Offset = new Point2(-minX + (width / _viewport.Scale - mapWidth) / 2, -minY + (height / _viewport.Scale - mapHeight) / 2);
                return;
            }

            if (map?.Grid?.Points != null && map.Grid.Points.Length > 0)
            {
                var points = map.Grid.Points;
                var minX = points.Min(p => p.X);
                var maxX = points.Max(p => p.X);
                var minY = points.Min(p => p.Y);
                var maxY = points.Max(p => p.Y);
                var mapWidth = maxX - minX;
                var mapHeight = maxY - minY;
                var scaleX = width * 0.85 / mapWidth;
                var scaleY = height * 0.85 / mapHeight;
                _viewport.Scale = Math.Min(scaleX, scaleY);
                _viewport.Offset = new Point2(-minX + (width / _viewport.Scale - mapWidth) / 2, -minY + (height / _viewport.Scale - mapHeight) / 2);
                return;
            }

            if (map?.LandmassPolygons?.Count > 0)
            {
                var minX = double.MaxValue; var maxX = double.MinValue; var minY = double.MaxValue; var maxY = double.MinValue;
                foreach (var polygon in map.LandmassPolygons)
                {
                    var bounds = polygon.GetBounds(); if (bounds.Min.X < minX) minX = bounds.Min.X; if (bounds.Max.X > maxX) maxX = bounds.Max.X; if (bounds.Min.Y < minY) minY = bounds.Min.Y; if (bounds.Max.Y > maxY) maxY = bounds.Max.Y;
                }
                var mapWidth = maxX - minX; var mapHeight = maxY - minY;
                var scaleX = width * 0.9 / mapWidth; var scaleY = height * 0.9 / mapHeight; _viewport.Scale = Math.Min(scaleX, scaleY);
                _viewport.Offset = new Point2(-minX + (width / _viewport.Scale - mapWidth) / 2, -minY + (height / _viewport.Scale - mapHeight) / 2);
                return;
            }

            if (map?.Burgs?.Count > 0)
            {
                var settlements = map.Burgs.Where(b => b.X > 0 && b.Y > 0).ToList(); if (!settlements.Any()) return;
                var minX = settlements.Min(b => b.X); var maxX = settlements.Max(b => b.X); var minY = settlements.Min(b => b.Y); var maxY = settlements.Max(b => b.Y);
                var mapWidth = maxX - minX; var mapHeight = maxY - minY;
                var scaleX = width * 0.9 / mapWidth; var scaleY = height * 0.9 / mapHeight; _viewport.Scale = Math.Min(scaleX, scaleY);
                _viewport.Offset = new Point2(-minX + (width / _viewport.Scale - mapWidth) / 2, -minY + (height / _viewport.Scale - mapHeight) / 2);
            }
        }

        private void RenderEnhancedSettlements(SKCanvas canvas, WorldMap pack)
        {
            if (pack.Burgs?.Count == 0 || _settlementPaint == null || _borderPaint == null) { UpdateStatus("No settlement data", "Settlements", false); return; }
            float lodMinPop = GetMinPopulationForLod(_viewport.Scale);
            int settlementsRendered = 0; int capitalsRendered = 0;
            foreach (var burg in pack.Burgs!)
            {
                if (burg.Population <= 0) continue;
                if (!burg.Capital && burg.Population < lodMinPop && burg.Id != HoveredBurgId) continue;
                var screenPoint = _viewport.WorldToScreen(new Point2(burg.X, burg.Y));
                var size = GetEnhancedSettlementSize(burg);
                if (burg.Id == HoveredBurgId) size *= 1.8f;
                var color = burg.Capital ? SKColors.Gold : SKColors.DarkRed;
                _settlementPaint.Color = color; canvas.DrawCircle(screenPoint, size, _settlementPaint);
                _borderPaint.Color = burg.Capital ? SKColors.DarkGoldenrod : SKColors.Black; _borderPaint.Style = SKPaintStyle.Stroke; _borderPaint.StrokeWidth = burg.Capital ? 2.5f : 1.5f; canvas.DrawCircle(screenPoint, size, _borderPaint);
                if (burg.Capital) capitalsRendered++; settlementsRendered++;
            }
            UpdateStatus($"Enhanced Settlements: {settlementsRendered} settlements ({capitalsRendered} capitals)", "Settlements", true, settlementsRendered);
        }

        private void RenderEnhancedLabels(SKCanvas canvas, WorldMap pack)
        {
            if (pack.Burgs?.Count == 0 || _viewport.Scale < 1.0 || _textPaint == null) { Debug.WriteLine($"Enhanced Labels: Scale too low ({_viewport.Scale:F2}) or no data"); return; }
            float lodMinPop = GetMinPopulationForLod(_viewport.Scale);
            int labelsRendered = 0;
            foreach (var burg in pack.Burgs!)
            {
                if (burg.Population <= 0 || string.IsNullOrEmpty(burg.Name)) continue;
                if (!burg.Capital && burg.Population < Math.Max(lodMinPop, 50)) continue;
                var screenPoint = _viewport.WorldToScreen(new Point2(burg.X, burg.Y)); var textSize = (float)(14 * Math.Min(_viewport.Scale, 2.0));
                _textPaint.TextSize = textSize; _textPaint.Color = burg.Capital ? SKColors.DarkBlue : SKColors.Black; _textPaint.Typeface = SKTypeface.FromFamilyName("Arial", burg.Capital ? SKFontStyle.Bold : SKFontStyle.Normal);
                var offset = GetEnhancedSettlementSize(burg) + 8; canvas.DrawText(burg.Name, screenPoint.X + offset, screenPoint.Y, _textPaint); labelsRendered++;
            }
            Debug.WriteLine($"Enhanced Labels: Rendered {labelsRendered} major settlement labels");
        }

        private float GetEnhancedSettlementSize(Burg burg)
        {
            var baseSize = burg.Capital ? 10 : 5; var populationFactor = Math.Log10(burg.Population + 1); var scaleFactor = Math.Min(_viewport.Scale, 2.0); return (float)((baseSize + populationFactor * 2) * scaleFactor);
        }

        /// <summary>
        /// ID of the burg currently under the cursor. Set externally to trigger a hover highlight.
        /// Use -1 (the default) to indicate no burg is hovered.
        /// </summary>
        public int HoveredBurgId { get; set; } = -1;

        /// <summary>
        /// Index of the Voronoi cell currently under the cursor. Set externally; -1 means none.
        /// </summary>
        public int HoveredCellIndex { get; set; } = -1;

        /// <summary>
        /// Screen-space cursor position used to anchor the province tooltip overlay.
        /// </summary>
        public SKPoint TooltipScreenPosition { get; set; }

        private void RenderTooltipOverlay(SKCanvas canvas, WorldMap map)
        {
            if (HoveredCellIndex < 0 || _textPaint == null) return;
            int i = HoveredCellIndex;

            string stateName = "–";
            string cultureName = "–";
            string religionName = "–";
            string elevationStr = "–";

            if (map.StateIndexes != null && i < map.StateIndexes.Length)
            {
                int sid = map.StateIndexes[i];
                stateName = map.States?.FirstOrDefault(s => s.Id == sid)?.Name ?? (sid > 0 ? $"State {sid}" : "–");
            }
            if (map.CultureIndexes != null && i < map.CultureIndexes.Length)
            {
                int cid = map.CultureIndexes[i];
                cultureName = map.Cultures?.FirstOrDefault(c => c.Id == cid)?.Name ?? (cid > 0 ? $"Culture {cid}" : "–");
            }
            if (map.ReligionIndexes != null && i < map.ReligionIndexes.Length)
            {
                int rid = map.ReligionIndexes[i];
                religionName = map.Religions?.FirstOrDefault(r => r.Id == rid)?.Name ?? (rid > 0 ? $"Religion {rid}" : "–");
            }
            if (map.Elevation != null && i < map.Elevation.Length)
                elevationStr = map.Elevation[i].ToString();

            var lines = new List<string>
            {
                $"State: {stateName}",
                $"Culture: {cultureName}",
                $"Religion: {religionName}",
                $"Elevation: {elevationStr}",
            };

            using var tooltipBg = new SKPaint { IsAntialias = true, Color = SKColors.White.WithAlpha(220), Style = SKPaintStyle.Fill };
            using var tooltipBorder = new SKPaint { IsAntialias = true, Color = SKColors.DarkGray.WithAlpha(180), Style = SKPaintStyle.Stroke, StrokeWidth = 1f };
            using var tooltipText = new SKPaint { IsAntialias = true, Color = SKColors.Black, TextSize = 13, Typeface = _textPaint.Typeface };

            float padding = 7f;
            float lineHeight = tooltipText.TextSize * 1.4f;
            float boxW = 0f;
            foreach (var l in lines) boxW = Math.Max(boxW, tooltipText.MeasureText(l));
            boxW += padding * 2;
            float boxH = lineHeight * lines.Count + padding * 2;

            float x = TooltipScreenPosition.X + 16f;
            float y = TooltipScreenPosition.Y - boxH - 8f;
            if (x + boxW > _viewport.ViewBounds.Width) x = TooltipScreenPosition.X - boxW - 8f;
            if (y < 0) y = TooltipScreenPosition.Y + 20f;

            var boxRect = new SKRect(x, y, x + boxW, y + boxH);
            canvas.DrawRoundRect(boxRect, 5, 5, tooltipBg);
            canvas.DrawRoundRect(boxRect, 5, 5, tooltipBorder);

            for (int li = 0; li < lines.Count; li++)
            {
                float ty = y + padding + (li + 1) * lineHeight - (lineHeight - tooltipText.TextSize) / 2;
                canvas.DrawText(lines[li], x + padding, ty, tooltipText);
            }
        }

        /// <summary>
        /// Returns the minimum burg population that should be rendered as a dot at the given
        /// viewport scale. Capitals are always rendered regardless of this value.
        /// </summary>
        public static float GetMinPopulationForLod(double scale)
        {
            if (scale >= 1.5) return 0f;
            if (scale >= 0.8) return 5f;
            if (scale >= 0.4) return 30f;
            return 200f;
        }



        private float GetSettlementSize(Burg burg)
        {
            var baseSize = burg.Capital ? 8 : 4; var populationFactor = Math.Log10(burg.Population + 1); return (float)((baseSize + populationFactor * 2) * _viewport.Scale);
        }
    }
}
