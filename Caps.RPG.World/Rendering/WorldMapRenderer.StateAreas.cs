#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using SkiaSharp;
using Caps.RPG.World.Models;
using Caps.RPG.World.Models.Graphics;

namespace Caps.RPG.World.Rendering
{
    public partial class WorldMapRenderer
    {
        private void RenderStateAreas(SKCanvas canvas, WorldMap pack)
        {
            if (_defaultPaint == null) return;

            // Condensed diagnostics to avoid verbose logs
            ShortDebugWrite("Ã°Å¸â€Â DATA ANALYSIS: summarizing available pack data (truncated)");
            try
            {
                int cellCount = pack.Cells?.Coordinates?.Length ?? 0;
                int stateAssignments = pack.StateIndexes?.Length ?? 0;
                int vertexCells = pack.Cells?.VertexIndexes?.Length ?? 0;
                int totalVertices = pack.Vertices?.Coordinates?.Length ?? 0;
                int landmassCount = pack.LandmassPolygons?.Count ?? 0;
                int gridPoints = pack.Grid?.Points?.Length ?? 0;

                ShortDebugWrite($"Cells:{cellCount}, StateAssignments:{stateAssignments}, VertexCells:{vertexCells}, Vertices:{totalVertices}, Landmasses:{landmassCount}, GridPoints:{gridPoints}");

                if (cellCount > 0 && stateAssignments > 0)
                {
                    // Show top-level distribution summary (first 10 states by count)
                    var sampled = pack.StateIndexes.Take(2000).GroupBy(s => s).Select(g => (Id: g.Key, Count: g.Count())).OrderByDescending(t => t.Count).Take(10);
                    ShortDebugWrite("Top states (sampled): " + string.Join(", ", sampled.Select(s => $"S{s.Id}:{s.Count}")));
                }
            }
            catch (Exception ex)
            {
                ShortDebugWrite("Data analysis failed: " + ex.Message);
            }

            // PRIORITY 1: Use SVG Political Polygons if available (BEST SOURCE - already has correct boundaries!)
            if (pack.PoliticalPolygons?.Count > 0)
            {
                ShortDebugWrite($"Using SVG political polygons ({pack.PoliticalPolygons.Count})", LogLevel.Debug);
                UpdateStatus($"Using SVG Political Polygons: {pack.PoliticalPolygons.Count} polygons", "StateAreas", true, pack.PoliticalPolygons.Count);
                RenderSvgPoliticalPolygons(canvas, pack);
                return;
            }

            // PRIORITY 2: Use pack.cells with vertex data for proper Voronoi rendering
            if (pack.Cells?.Coordinates != null && pack.StateIndexes != null && 
                pack.Cells.VertexIndexes != null && pack.Vertices?.Coordinates != null)
            {
                ShortDebugWrite($"Using official FMG data model with vertex data ({pack.Cells.Coordinates.Length} cells)", LogLevel.Debug);
                UpdateStatus($"Using FMG Voronoi cells: {pack.Cells.Coordinates.Length} cells", "StateAreas", true, pack.Cells.Coordinates.Length);
                RenderOfficialFmgCells(canvas, pack);
                return;
            }

            // PRIORITY 3: Grid fallback with proper cell-state matching
            if (pack.Grid?.Points != null && pack.StateIndexes != null)
            {
                ShortDebugWrite("Using grid fallback approach (may produce horizontal bands)", LogLevel.Debug);
                UpdateStatus($"Grid fallback: {pack.Grid.Points.Length} points, {pack.StateIndexes.Length} state assignments", "StateAreas", true, pack.Grid.Points.Length);

                // Check for count mismatch warning
                if (pack.Grid.Points.Length != pack.StateIndexes.Length)
                {
                    ShortDebugWrite($"WARNING: Grid points ({pack.Grid.Points.Length}) do not match state assignments ({pack.StateIndexes.Length})", LogLevel.Warn);
                    ShortDebugWrite("Mismatch: grid includes water cells but state indexes are land-only", LogLevel.Warn);
                }

                RenderVoronoiBasedStates(canvas, pack);
                return;
            }

            // PRIORITY 4: Simple state circles as last resort
            if (pack.States != null)
            {
                ShortDebugWrite("No FMG cell data available, falling back to simple state circles", LogLevel.Debug);
                int stateCirclesDrawn = 0;
                foreach (var state in pack.States)
                {
                    if (state.Pole?.Length >= 2)
                    {
                        var center = _viewport.WorldToScreen(new Point2(state.Pole[0], state.Pole[1]));
                        var color = ParseColor(state.Color ?? "#FF0000");

                        _defaultPaint.Color = color.WithAlpha(120);
                        _defaultPaint.Style = SKPaintStyle.Fill;

                        var radius = (float)(80 * _viewport.Scale);
                        canvas.DrawCircle(center, radius, _defaultPaint);
                        stateCirclesDrawn++;
                    }
                }
                ShortDebugWrite($"Drew {stateCirclesDrawn} simple state circles as last resort", LogLevel.Debug);
                UpdateStatus($"Drew {stateCirclesDrawn} state circles (simple fallback)", "StateAreas", true, stateCirclesDrawn);
                return;
            }

            ShortDebugWrite("No state data available for rendering", LogLevel.Debug);
            UpdateStatus("No data available", "StateAreas", false);
        }

        private void RenderOfficialFmgCells(SKCanvas canvas, WorldMap pack)
        {
            var tr = new TerritoryRenderer(this);
            tr.RenderOfficialFmgCells(canvas, pack);
        }

        // Render territories but color by state->culture mapping
        private void RenderVoronoiPolygonsForCultures(SKCanvas canvas, WorldMap pack)
        {
            var tr = new TerritoryRenderer(this);
            tr.RenderVoronoiPolygonsForCultures(canvas, pack);
        }

        // Render territories but color by dominant religion per state
        private void RenderVoronoiPolygonsForReligions(SKCanvas canvas, WorldMap pack)
        {
            var tr = new TerritoryRenderer(this);
            tr.RenderVoronoiPolygonsForReligions(canvas, pack);
        }

        // Render culture map: prefer per-cell Voronoi polygons colored by culture
        private void RenderCultureAreas(SKCanvas canvas, WorldMap pack)
        {
            if (_defaultPaint == null) { UpdateStatus("No paint", "Cultural", false); return; }

            // If we have full vertex data, color each land cell by culture (preferred)
            if (pack.Cells?.VertexIndexes != null && pack.Vertices?.Coordinates != null)
            {
                var tr = new TerritoryRenderer(this);
                tr.RenderIndividualVoronoiCellsByCulture(canvas, pack);
                return;
            }

            // Fallback: if states include culture mapping, color state territories by culture
            if (pack.States != null && pack.States.Any(s => s.Culture > 0))
            {
                RenderVoronoiPolygonsForCultures(canvas, pack);
                return;
            }

            // Last fallback: use grid/grouping approach (if available)
            if (pack.Grid?.Points != null && pack.CultureIndexes != null)
            {
                // Simple per-grid coloring via circles
                var cultureColors = new Dictionary<int, SKColor>();
                if (pack.Cultures != null)
                {
                    foreach (var c in pack.Cultures) if (c.Id > 0) cultureColors[c.Id] = ParseColor(c.Color ?? GeneratePasstelColorFromId(c.Id.ToString()).ToString()).WithAlpha(180);
                }

                var points = pack.Grid.Points;
                var radius = CalculateAverageCellSpacing(points) * 0.6;
                int rendered = 0;
                for (int i = 0; i < Math.Min(points.Length, pack.CultureIndexes.Length); i++)
                {
                    var cid = pack.CultureIndexes[i];
                    if (cid <= 0) continue;
                    var screen = _viewport.WorldToScreen(points[i]);
                    var r = (float)(radius * _viewport.Scale);
                    if (!IsPolygonVisible(new SKPoint[] { screen })) continue;
                    _defaultPaint.Color = cultureColors.ContainsKey(cid) ? cultureColors[cid] : GeneratePasstelColorFromId(cid.ToString()).WithAlpha(180);
                    _defaultPaint.Style = SKPaintStyle.Fill;
                    canvas.DrawCircle(screen, r, _defaultPaint);
                    rendered++;
                }
                UpdateStatus($"Rendered culture grid cells: {rendered}", "Cultural", rendered > 0, rendered);
                return;
            }

            UpdateStatus("No culture data", "Cultural", false);
        }

        private void RenderReligionAreas(SKCanvas canvas, WorldMap pack)
        {
            if (_defaultPaint == null) { UpdateStatus("No paint", "Religion", false); return; }

            // Priority 1: Full vertex data — color each land cell by religion (most detailed)
            if (pack.Cells?.VertexIndexes != null && pack.Vertices?.Coordinates != null)
            {
                var tr = new TerritoryRenderer(this);
                tr.RenderIndividualVoronoiCellsByReligion(canvas, pack);
                return; // StateBorders layer handles outlines
            }

            // Priority 2: Territory polygons colored by dominant religion per state
            if (pack.States != null && pack.ReligionIndexes != null && pack.StateIndexes != null)
            {
                RenderVoronoiPolygonsForReligions(canvas, pack);
                return;
            }

            // Priority 3: Grid circles fallback
            if (pack.Grid?.Points != null && pack.ReligionIndexes != null)
            {
                var religionColors = new Dictionary<int, SKColor>();
                if (pack.Religions != null)
                    foreach (var r in pack.Religions)
                        if (r.Id > 0) religionColors[r.Id] = ParseColor(r.Color ?? GeneratePasstelColorFromId(r.Id.ToString()).ToString()).WithAlpha(180);

                var points = pack.Grid.Points;
                var radius = CalculateAverageCellSpacing(points) * 0.6;
                int rendered = 0;
                for (int i = 0; i < Math.Min(points.Length, pack.ReligionIndexes.Length); i++)
                {
                    var rid = pack.ReligionIndexes[i];
                    if (rid <= 0) continue;
                    var screen = _viewport.WorldToScreen(points[i]);
                    var r = (float)(radius * _viewport.Scale);
                    if (!IsPolygonVisible(new SKPoint[] { screen })) continue;
                    _defaultPaint.Color = religionColors.ContainsKey(rid) ? religionColors[rid] : GeneratePasstelColorFromId(rid.ToString()).WithAlpha(180);
                    _defaultPaint.Style = SKPaintStyle.Fill;
                    canvas.DrawCircle(screen, r, _defaultPaint);
                    rendered++;
                }
                UpdateStatus($"Rendered religion grid cells: {rendered}", "Religion", rendered > 0, rendered);
                return;
            }

            UpdateStatus("No religion data", "Religion", false);
        }

        private int FindStateIdForPolygon(SvgPolygon polygon, WorldMap pack)
        {
            // Method 1: Try polygon ID first (if it's a valid state ID)
            if (!string.IsNullOrEmpty(polygon.Id) && int.TryParse(polygon.Id, out int parsedId) && parsedId > 0)
            {
                return parsedId;
            }

            // Method 2: Use spatial lookup - find which grid cell this polygon's center belongs to
            if (pack.Grid?.Points != null && pack.StateIndexes != null)
            {
                var polygonCenter = CalculatePolygonCentroid(polygon.Points);

                // Find the nearest grid point to this polygon's center
                double nearestDistance = double.MaxValue;
                int nearestGridIndex = -1;

                for (int i = 0; i < pack.Grid.Points.Length; i++)
                {
                    var gridPoint = pack.Grid.Points[i];
                    var distance = Distance(polygonCenter, gridPoint);

                    if (distance < nearestDistance)
                    {
                        nearestDistance = distance;
                        nearestGridIndex = i;
                    }
                }

                // Get state ID from StateIndexes using the nearest grid cell
                if (nearestGridIndex >= 0 && nearestGridIndex < pack.StateIndexes.Length)
                {
                    return pack.StateIndexes[nearestGridIndex];
                }
            }

            // Method 3: Fallback to nearest settlement
            return DeterminePolygonStateFromSettlements(polygon, pack);
        }

        private int DeterminePolygonStateFromSettlements(SvgPolygon polygon, WorldMap pack)
        {
            if (pack.Burgs == null) return 0;

            // Calculate polygon centroid
            var centroid = CalculatePolygonCentroid(polygon.Points);

            // Find the nearest settlement to assign state
            var nearestSettlement = pack.Burgs
                .Where(b => b.X > 0 && b.Y > 0 && b.State > 0)
                .OrderBy(s => Math.Pow(s.X - centroid.X, 2) + Math.Pow(s.Y - centroid.Y, 2))
                .FirstOrDefault();

            return nearestSettlement?.State ?? 0;
        }

        private void RenderVoronoiBasedStates(SKCanvas canvas, WorldMap pack)
        {
            var tr = new TerritoryRenderer(this);
            tr.RenderVoronoiBasedStates(canvas, pack);
        }

        private void DrawStateBorders(SKCanvas canvas, WorldMap pack)
        {
            var tr = new TerritoryRenderer(this);
            tr.DrawStateOutlines(canvas, pack, soft: true);
        }

        private void DrawProvinceBorders(SKCanvas canvas, WorldMap pack)
        {
            var tr = new TerritoryRenderer(this);
            tr.DrawProvinceOutlines(canvas, pack);
        }
    }
}