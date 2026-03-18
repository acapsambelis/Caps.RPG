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
    public class TerritoryRenderer
    {
        private readonly MapRendererBase _owner;

        public TerritoryRenderer(MapRendererBase owner)
        {
            _owner = owner;
        }

        private SKColor ResolveColor(string? color, int id) =>
            !string.IsNullOrEmpty(color) ? _owner.ParseColor(color) : _owner.GeneratePasstelColorFromId(id.ToString());

        /// <summary>
        /// Ensures no two colors in the palette are perceptually too close together by
        /// iteratively pushing hues apart until all consecutive pairs (on the circular hue
        /// wheel) exceed the minimum separation. Saturation, value, and alpha are preserved.
        /// </summary>
        private static Dictionary<int, SKColor> SpreadColors(Dictionary<int, SKColor> colors)
        {
            if (colors.Count <= 1) return colors;

            float minSep = Math.Max(20f, 360f / colors.Count);

            var keys = colors.Keys.ToArray();
            var h = new float[keys.Length];
            var s = new float[keys.Length];
            var v = new float[keys.Length];
            var a = new byte[keys.Length];
            for (int i = 0; i < keys.Length; i++)
            {
                colors[keys[i]].ToHsv(out h[i], out s[i], out v[i]);
                a[i] = colors[keys[i]].Alpha;
            }

            int[] order = Enumerable.Range(0, keys.Length).OrderBy(i => h[i]).ToArray();

            for (int iter = 0; iter < 300; iter++)
            {
                bool changed = false;

                for (int i = 0; i < order.Length - 1; i++)
                {
                    int x = order[i], y = order[i + 1];
                    float gap = h[y] - h[x];
                    if (gap < minSep)
                    {
                        float push = (minSep - gap) * 0.5f;
                        h[x] = (h[x] - push + 360f) % 360f;
                        h[y] = (h[y] + push) % 360f;
                        changed = true;
                    }
                }

                // Circular wrap: last vs first
                int lo = order[^1], hi = order[0];
                float wrapGap = (h[hi] + 360f - h[lo]) % 360f;
                if (wrapGap < minSep)
                {
                    float push = (minSep - wrapGap) * 0.5f;
                    h[lo] = (h[lo] - push + 360f) % 360f;
                    h[hi] = (h[hi] + push) % 360f;
                    changed = true;
                }

                if (!changed) break;
                order = Enumerable.Range(0, keys.Length).OrderBy(i => h[i]).ToArray();
            }

            var result = new Dictionary<int, SKColor>(colors.Count);
            for (int i = 0; i < keys.Length; i++)
                result[keys[i]] = SKColor.FromHsv(h[i], s[i], v[i]).WithAlpha(a[i]);
            return result;
        }

        private void DrawTextWithShadow(SKCanvas canvas, string text, float x, float y)
        {
            if (_owner.TextPaint == null) return;
            using var shadowPaint = new SKPaint
            {
                IsAntialias = true,
                Color = SKColors.White.WithAlpha(180),
                TextSize = _owner.TextPaint.TextSize,
                TextAlign = _owner.TextPaint.TextAlign,
                Typeface = _owner.TextPaint.Typeface
            };
            canvas.DrawText(text, x + 1, y + 1, shadowPaint);
            canvas.DrawText(text, x - 1, y - 1, shadowPaint);
            canvas.DrawText(text, x, y, _owner.TextPaint);
        }

        public void RenderOfficialFmgCells(SKCanvas canvas, WorldMap pack)
        {
            if (_owner.DefaultPaint == null || pack.Cells?.Coordinates == null || pack.StateIndexes == null) return;

            bool hasVertexData = pack.Cells.VertexIndexes != null && pack.Vertices?.Coordinates != null;
            if (hasVertexData)
            {
                RenderIndividualVoronoiCells(canvas, pack);
            }
            else
            {
                RenderVoronoiCells(canvas, pack);
            }
        }

        public void RenderIndividualVoronoiCells(SKCanvas canvas, WorldMap pack)
        {
            if (pack.Cells?.VertexIndexes == null || pack.Vertices?.Coordinates == null) return;

            _owner.RenderedLandmask = new List<SvgPolygon>();

            var stateColors = new Dictionary<int, SKColor>();
            if (pack.States != null)
                foreach (var state in pack.States)
                    if (state.Id > 0 && !string.IsNullOrEmpty(state.Color))
                        stateColors[state.Id] = _owner.ParseColor(state.Color);

            int cellsRendered = RenderCellsWithColorResolver(canvas, pack, cellIndex =>
            {
                int stateId = pack.StateIndexes != null && cellIndex < pack.StateIndexes.Length ? pack.StateIndexes[cellIndex] : 0;
                if (stateId <= 0) return null;
                return stateColors.TryGetValue(stateId, out var c) ? c.WithAlpha(200) : _owner.GeneratePasstelColorFromId(stateId.ToString()).WithAlpha(200);
            });

            DrawStateOutlines(canvas, pack);
            RenderStateLabelsAtPoles(canvas, pack, stateColors);
            _owner.UpdateStatus($"Rendered {cellsRendered} individual Voronoi cells", "StateAreas", true, cellsRendered);
        }

        public void RenderIndividualVoronoiCellsByCulture(SKCanvas canvas, WorldMap pack)
        {
            if (pack.Cells?.VertexIndexes == null || pack.Vertices?.Coordinates == null) return;

            var noCultureColor = new SKColor(210, 200, 185).WithAlpha(180);

            var cultureColors = new Dictionary<int, SKColor>();
            if (pack.Cultures != null)
            {
                foreach (var c in pack.Cultures)
                {
                    if (c.Id == 0)
                    {
                        if (!string.IsNullOrEmpty(c.Color))
                            noCultureColor = ResolveColor(c.Color, c.Id).WithAlpha(180);
                    }
                    else if (c.Id > 0)
                    {
                        cultureColors[c.Id] = _owner.EnhanceStateColor(ResolveColor(c.Color, c.Id));
                    }
                }
            }
            cultureColors = SpreadColors(cultureColors);

            var stateToCulture = new Dictionary<int, int>();
            if (pack.States != null) foreach (var s in pack.States) stateToCulture[s.Id] = s.Culture;

            int cellsRendered = RenderCellsWithColorResolver(canvas, pack, cellIndex =>
            {
                // Use -1 as sentinel: "CultureIndexes not available" vs 0 = "explicitly wildlands".
                // When per-cell data is present, trust it and do not fall back to state heuristics.
                int cultureId = pack.CultureIndexes != null && cellIndex < pack.CultureIndexes.Length
                    ? pack.CultureIndexes[cellIndex]
                    : -1;

                if (cultureId < 0)
                {
                    // No per-cell array — try state dominant culture then nearest burg
                    cultureId = 0;
                    if (pack.StateIndexes != null && cellIndex < pack.StateIndexes.Length)
                    {
                        var sid = pack.StateIndexes[cellIndex];
                        if (stateToCulture.TryGetValue(sid, out var cid)) cultureId = cid;
                    }
                    if (cultureId <= 0 && pack.Burgs != null && pack.Cells?.Coordinates != null && cellIndex < pack.Cells.Coordinates.Length)
                    {
                        var sample = pack.Cells.Coordinates[cellIndex];
                        double bestDist = double.MaxValue; int bestCulture = 0;
                        foreach (var b in pack.Burgs) { if (b.Culture <= 0) continue; var dx = b.X - sample.X; var dy = b.Y - sample.Y; var d = dx * dx + dy * dy; if (d < bestDist) { bestDist = d; bestCulture = b.Culture; } }
                        cultureId = bestCulture;
                    }
                }

                if (cultureId <= 0) return noCultureColor;
                return cultureColors.TryGetValue(cultureId, out var fill) ? fill : _owner.GeneratePasstelColorFromId(cultureId.ToString()).WithAlpha(200);
            });

            _owner.UpdateStatus($"Rendered culture Voronoi cells: {cellsRendered}", "Cultural", cellsRendered > 0, cellsRendered);
        }

        public void RenderIndividualVoronoiCellsByReligion(SKCanvas canvas, WorldMap pack)
        {
            if (pack.Cells?.VertexIndexes == null || pack.Vertices?.Coordinates == null) return;

            // Neutral parchment fallback for land cells with no religion; overridden by the
            // FMG "No religion" entry (Id == 0) if one is present and has a color.
            var noReligionColor = new SKColor(210, 200, 185).WithAlpha(180);

            var religionColors = new Dictionary<int, SKColor>();
            if (pack.Religions != null)
            {
                foreach (var r in pack.Religions)
                {
                    if (r.Id == 0)
                    {
                        if (!string.IsNullOrEmpty(r.Color))
                            noReligionColor = ResolveColor(r.Color, r.Id).WithAlpha(180);
                    }
                    else if (r.Id > 0)
                    {
                        religionColors[r.Id] = _owner.EnhanceStateColor(ResolveColor(r.Color, r.Id));
                    }
                }
            }
            religionColors = SpreadColors(religionColors);

            int cellsRendered = RenderCellsWithColorResolver(canvas, pack, cellIndex =>
            {
                int religionId = pack.ReligionIndexes != null && cellIndex < pack.ReligionIndexes.Length ? pack.ReligionIndexes[cellIndex] : 0;
                if (religionId <= 0) return noReligionColor;
                return religionColors.TryGetValue(religionId, out var fill) ? fill : _owner.GeneratePasstelColorFromId(religionId.ToString()).WithAlpha(200);
            });

            _owner.UpdateStatus($"Rendered religion Voronoi cells: {cellsRendered}", "Religion", cellsRendered > 0, cellsRendered);
        }

        public void RenderVoronoiPolygons(SKCanvas canvas, WorldMap pack)
        {
            if (pack.Cells?.VertexIndexes == null || pack.Vertices?.Coordinates == null || pack.StateIndexes == null) return;

            var stateColors = new Dictionary<int, SKColor>();
            if (pack.States != null)
                foreach (var state in pack.States)
                    if (state.Id > 0)
                        stateColors[state.Id] = _owner.EnhanceStateColor(ResolveColor(state.Color, state.Id));

            int regionsRendered = RenderTerritoryPolygons(canvas, pack,
                t => stateColors.TryGetValue(t.StateId, out var c) ? c : _owner.GeneratePasstelColorFromId(t.StateId.ToString()).WithAlpha(200),
                alwaysDrawBorders: true,
                (cv, t, region, color) =>
                {
                    var state = pack.States?.FirstOrDefault(s => s.Id == t.StateId);
                    if (state != null && !string.IsNullOrEmpty(state.Name)) RenderStateLabel(cv, state, region, color);
                });

            _owner.UpdateStatus($"STATE TERRITORIES: Rendered {regionsRendered} territory regions", "StateAreas", true, regionsRendered);
        }

        public void RenderVoronoiPolygonsForCultures(SKCanvas canvas, WorldMap pack)
        {
            if (pack.Cells?.VertexIndexes == null || pack.Vertices?.Coordinates == null) return;

            var cultureColors = new Dictionary<int, SKColor>();
            if (pack.Cultures != null)
                foreach (var c in pack.Cultures)
                    if (c.Id > 0) cultureColors[c.Id] = ResolveColor(c.Color, c.Id);

            int regionsRendered = RenderTerritoryPolygons(canvas, pack,
                t =>
                {
                    int cultureId = 0;
                    var stateObj = pack.States?.FirstOrDefault(s => s.Id == t.StateId);
                    if (stateObj != null) cultureId = stateObj.Culture;
                    return cultureId > 0 && cultureColors.TryGetValue(cultureId, out var c) ? c : _owner.GeneratePasstelColorFromId(t.StateId.ToString()).WithAlpha(200);
                },
                alwaysDrawBorders: true);

            _owner.UpdateStatus($"CULTURE TERRITORIES: Rendered {regionsRendered} territory regions", "Cultural", regionsRendered > 0, regionsRendered);
        }

        // Render territories colored by dominant religion (computed from per-cell ReligionIndexes)
        public void RenderVoronoiPolygonsForReligions(SKCanvas canvas, WorldMap pack)
        {
            if (pack.Cells?.VertexIndexes == null || pack.Vertices?.Coordinates == null) return;

            var religionColors = new Dictionary<int, SKColor>();
            if (pack.Religions != null)
                foreach (var r in pack.Religions)
                    if (r.Id > 0) religionColors[r.Id] = ResolveColor(r.Color, r.Id);

            // State has no Religion property — derive dominant religion per state from per-cell indexes
            var stateToReligion = new Dictionary<int, int>();
            if (pack.StateIndexes != null && pack.ReligionIndexes != null)
            {
                var counts = new Dictionary<int, Dictionary<int, int>>();
                int len = Math.Min(pack.StateIndexes.Length, pack.ReligionIndexes.Length);
                for (int i = 0; i < len; i++)
                {
                    var sid = pack.StateIndexes[i];
                    var rid = pack.ReligionIndexes[i];
                    if (sid <= 0 || rid <= 0) continue;
                    if (!counts.TryGetValue(sid, out var inner)) counts[sid] = inner = new Dictionary<int, int>();
                    inner.TryGetValue(rid, out int n);
                    inner[rid] = n + 1;
                }
                foreach (var kvp in counts)
                    stateToReligion[kvp.Key] = kvp.Value.OrderByDescending(x => x.Value).First().Key;
            }

            int regionsRendered = RenderTerritoryPolygons(canvas, pack,
                t => stateToReligion.TryGetValue(t.StateId, out var rid) && religionColors.TryGetValue(rid, out var c)
                    ? c : _owner.GeneratePasstelColorFromId(t.StateId.ToString()).WithAlpha(200),
                alwaysDrawBorders: true);

            _owner.UpdateStatus($"RELIGION TERRITORIES: Rendered {regionsRendered} territory regions", "Religion", regionsRendered > 0, regionsRendered);
        }

        public void RenderStateLabelsAtPoles(SKCanvas canvas, WorldMap pack, Dictionary<int, SKColor> stateColors)
        {
            if (pack.States==null || _owner.TextPaint==null) return;
            foreach (var state in pack.States)
            {
                if (state.Id<=0 || string.IsNullOrEmpty(state.Name) || state.Removed) continue;
                if (state.Pole==null || state.Pole.Length<2) continue;
                var poleWorld = new Point2(state.Pole[0], state.Pole[1]); var poleScreen = _owner.Viewport.WorldToScreen(poleWorld);
                SKColor textColor = SKColors.Black; if (stateColors.TryGetValue(state.Id, out var sc)) textColor = _owner.GetContrastingTextColor(sc);
                var displayName = state.FullName ?? state.Name; float textSize = (float)(12 * _owner.Viewport.Scale); textSize = Math.Clamp(textSize,8f,24f);
                _owner.TextPaint.TextSize = textSize; _owner.TextPaint.Color = textColor; _owner.TextPaint.TextAlign = SKTextAlign.Center;
                DrawTextWithShadow(canvas, displayName, poleScreen.X, poleScreen.Y);
            }
        }

        private void RenderStateLabel(SKCanvas canvas, Models.State state, List<Point2> statePolygon, SKColor backgroundColor)
        {
            if (state == null || statePolygon == null || statePolygon.Count == 0 || _owner.TextPaint == null) return;

            var stateName = state.FullName ?? state.Name;
            if (string.IsNullOrEmpty(stateName)) return;

            var centroid = new Point2(statePolygon.Average(p => p.X), statePolygon.Average(p => p.Y));
            var screenCentroid = _owner.Viewport.WorldToScreen(centroid);

            var stateScreenSize = statePolygon.Select(_owner.Viewport.WorldToScreen).Select(p => _owner.Distance(new Point2(p.X, p.Y), new Point2(screenCentroid.X, screenCentroid.Y))).DefaultIfEmpty(0).Max();
            if (stateScreenSize < 30) return;

            var baseTextSize = Math.Max(10, Math.Min(18, stateScreenSize / 8));
            _owner.TextPaint.TextSize = (float)baseTextSize;
            _owner.TextPaint.Color = SKColors.Black;
            _owner.TextPaint.Typeface = SKTypeface.FromFamilyName("Arial", SKFontStyle.Bold);
            _owner.TextPaint.TextAlign = SKTextAlign.Center;

            var textBounds = new SKRect();
            _owner.TextPaint.MeasureText(stateName, ref textBounds);
            var textX = screenCentroid.X - textBounds.Width / 2;
            var textY = screenCentroid.Y + textBounds.Height / 2;

            if (baseTextSize >= 12)
            {
                var padding = 3f;
                var backgroundPaint = new SKPaint { Color = SKColors.White.WithAlpha(200), Style = SKPaintStyle.Fill };
                var backgroundRect = new SKRect(textX - padding, textY - textBounds.Height - padding, textX + textBounds.Width + padding, textY + padding);
                canvas.DrawRoundRect(backgroundRect, 2, 2, backgroundPaint);
                backgroundPaint.Dispose();
            }

            DrawTextWithShadow(canvas, stateName, screenCentroid.X, screenCentroid.Y);
        }

        public void DrawCultureBordersByCellEdges(SKCanvas canvas, WorldMap pack)
        {
            if (_owner.BorderPaint == null || pack.Cells?.VertexIndexes == null || pack.Vertices?.Coordinates == null) return;

            int cellCount = pack.Cells.VertexIndexes.Length;
            var cellCulture = new int[cellCount];
            if (pack.CultureIndexes != null && pack.CultureIndexes.Length >= cellCount)
                for (int i = 0; i < cellCount; i++) cellCulture[i] = pack.CultureIndexes[i];
            else if (pack.StateIndexes != null && pack.States != null)
            {
                var stateToCulture = new Dictionary<int, int>();
                foreach (var s in pack.States) stateToCulture[s.Id] = s.Culture;
                for (int i = 0; i < cellCount; i++)
                    if (i < pack.StateIndexes.Length && stateToCulture.TryGetValue(pack.StateIndexes[i], out var cid)) cellCulture[i] = cid;
            }

            var edgeKeys = CollectBoundaryEdges(pack, i => cellCulture[i], requireReverseEdge: true, skipWaterNeighbors: true);

            _owner.BorderPaint.Style = SKPaintStyle.Stroke;
            _owner.BorderPaint.IsAntialias = true;
            _owner.BorderPaint.Color = SKColors.Black;
            _owner.BorderPaint.StrokeWidth = Math.Max(2f, (float)(6 * Math.Min(_owner.Viewport.Scale, 2.0)));
            _owner.BorderPaint.StrokeJoin = SKStrokeJoin.Round;
            DrawEdgeKeys(canvas, pack, edgeKeys);
        }

        public void DrawStateOutlines(SKCanvas canvas, WorldMap pack, bool soft = false)
        {
            if (_owner.BorderPaint == null || pack == null || pack.Cells?.VertexIndexes == null || pack.Vertices?.Coordinates == null || pack.StateIndexes == null) return;
            try
            {
                var edgeKeys = CollectBoundaryEdges(pack, i => i < pack.StateIndexes.Length ? pack.StateIndexes[i] : 0, requireReverseEdge: false, skipWaterNeighbors: false);

                _owner.BorderPaint.Style = SKPaintStyle.Stroke;
                _owner.BorderPaint.IsAntialias = true;
                _owner.BorderPaint.StrokeCap = SKStrokeCap.Round;
                _owner.BorderPaint.StrokeJoin = SKStrokeJoin.Round;
                _owner.BorderPaint.Color = SKColors.Black.WithAlpha(120);
                _owner.BorderPaint.StrokeWidth = soft
                    ? Math.Max(1f, (float)(1.5 * Math.Min(_owner.Viewport.Scale, 2.0)))
                    : Math.Max(1.5f, (float)(3.0 * Math.Min(_owner.Viewport.Scale, 2.0)));

                DrawEdgeKeys(canvas, pack, edgeKeys);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"DrawStateOutlines failed: {ex.Message}");
            }
        }

        public void RenderVoronoiBasedStates(SKCanvas canvas, WorldMap pack)
        {
            if (pack.Grid?.Points==null || pack.StateIndexes==null || _owner.DefaultPaint==null) { Debug.WriteLine("Early exit - missing data or paint"); return; }

            var stateColors = new Dictionary<int, SKColor>();
            if (pack.States!=null) foreach (var stateData in pack.States) if (stateData.Id>0) stateColors[stateData.Id] = ResolveColor(stateData.Color, stateData.Id).WithAlpha(180);

            var stateGroups = new Dictionary<int,List<int>>(); int landCellsCount=0;
            for (int i=0;i<Math.Min(pack.Grid.Points.Length, pack.StateIndexes.Length); i++) { var stateId = pack.StateIndexes[i]; if (stateId<=0) continue; if (!_owner.IsLandCell(i, pack)) continue; landCellsCount++; if (!stateGroups.ContainsKey(stateId)) stateGroups[stateId]=new List<int>(); stateGroups[stateId].Add(i); }

            _owner.UpdateStatus($"Found {landCellsCount} land cells in {stateGroups.Count} states", "StateAreas", true, stateGroups.Count);
            if (stateGroups.Count==0) return;

            var statesToRender = _owner.DebugOnlyLargestState ? stateGroups.OrderByDescending(g=>g.Value.Count).Take(2).ToDictionary(k=>k.Key,v=>v.Value) : stateGroups;

            var stateBoundaries = TraceStateBoundariesFromVoronoi(pack, statesToRender);
            _owner.RenderedLandmask = new List<SvgPolygon>();

            int totalPolygonsDrawn=0; int boundaryIndex=0;
            foreach (var kvp in statesToRender)
            {
                var stateId = kvp.Key; var landCellIndices = kvp.Value;
                SKColor fillColor = stateColors.ContainsKey(stateId) ? stateColors[stateId] : _owner.GeneratePasstelColorFromId(stateId.ToString()).WithAlpha(180);
                _owner.DefaultPaint.Color = fillColor.WithAlpha(150); _owner.DefaultPaint.Style = SKPaintStyle.Fill;

                List<Point2> statePolygon;
                if (boundaryIndex < stateBoundaries.Count && stateBoundaries[boundaryIndex].Count >= 3) statePolygon = stateBoundaries[boundaryIndex];
                else statePolygon = CreateStatePolygonFromCells(landCellIndices, pack.Grid.Points);

                if (statePolygon.Count>=3)
                {
                    _owner.RenderedLandmask.Add(new SvgPolygon { Points = statePolygon });
                    var screenPoints = statePolygon.Select(_owner.Viewport.WorldToScreen).ToArray();
                    if (screenPoints.Length>=3 && _owner.IsPolygonVisible(screenPoints))
                    {
                        using var path = new SKPath();
                        path.MoveTo(screenPoints[0]);
                        for (int i = 1; i < screenPoints.Length; i++) path.LineTo(screenPoints[i]);
                        path.Close();
                        canvas.DrawPath(path, _owner.DefaultPaint);
                        if (_owner.BorderPaint != null)
                        {
                            _owner.BorderPaint.Style = SKPaintStyle.Stroke;
                            _owner.BorderPaint.Color = SKColors.Black;
                            _owner.BorderPaint.StrokeWidth = Math.Max(2f, (float)(6 * Math.Min(_owner.Viewport.Scale, 2.0)));
                            canvas.DrawPath(path, _owner.BorderPaint);
                        }
                        totalPolygonsDrawn++;
                    }
                }
                boundaryIndex++;
            }

            Debug.WriteLine($"Processed {stateGroups.Count} states, drew {totalPolygonsDrawn} polygons");
            _owner.UpdateStatus($"Rendering states: {stateGroups.Count} states drawn", "StateAreas", true, stateGroups.Count);
        }

        public void RenderVoronoiCells(SKCanvas canvas, WorldMap pack)
        {
            if (pack.Cells?.Coordinates==null || pack.StateIndexes==null) return;
            var stateColors = new Dictionary<int, SKColor>(); if (pack.States!=null) foreach(var state in pack.States) if (state.Id>0) stateColors[state.Id]=ResolveColor(state.Color, state.Id).WithAlpha(180);
            int cellsRendered=0; int cellsWithStates=0; var cellRadius = _owner.CalculateAverageCellSpacing(pack.Cells.Coordinates)*0.7;
            int maxIndex = Math.Min(pack.Cells.Coordinates.Length, pack.StateIndexes.Length);
            for (int i=0;i<maxIndex;i++) { var cellCenter=pack.Cells.Coordinates[i]; var stateId=(int)pack.StateIndexes[i]; if (stateId<=0) continue; var screenCenter=_owner.Viewport.WorldToScreen(cellCenter); var screenRadius=(float)(cellRadius*_owner.Viewport.Scale); if (screenCenter.X+screenRadius < -50 || screenCenter.X-screenRadius > _owner.Viewport.ViewBounds.Width+50 || screenCenter.Y+screenRadius < -50 || screenCenter.Y-screenRadius > _owner.Viewport.ViewBounds.Height+50) continue; SKColor fillColor; if (stateColors.ContainsKey(stateId)) { fillColor=stateColors[stateId]; cellsWithStates++; } else { fillColor=_owner.GeneratePasstelColorFromId(stateId.ToString()).WithAlpha(180); cellsWithStates++; } _owner.DefaultPaint.Color=fillColor; _owner.DefaultPaint.Style=SKPaintStyle.Fill; if (screenRadius>=1.0f) { canvas.DrawCircle(screenCenter, screenRadius, _owner.DefaultPaint); cellsRendered++; if (_owner.RenderedLandmask==null) _owner.RenderedLandmask=new List<SvgPolygon>(); var s=cellCenter; var r=cellRadius*0.8; var poly=new SvgPolygon{ Points = new List<Point2> { new Point2(s.X-r,s.Y-r), new Point2(s.X+r,s.Y-r), new Point2(s.X+r,s.Y+r), new Point2(s.X-r,s.Y+r) } }; _owner.RenderedLandmask.Add(poly); } }
            _owner.UpdateStatus($"FALLBACK CIRCLES: Rendered {cellsRendered} circle cells", "StateAreas", true, cellsRendered);
        }

        private double CalculatePolygonArea(List<Point2> polygon) { if (polygon.Count<3) return 0; double area=0; for (int i=0;i<polygon.Count;i++){ var j=(i+1)%polygon.Count; area+=polygon[i].X*polygon[j].Y; area-=polygon[j].X*polygon[i].Y; } return Math.Abs(area)/2.0; }

        private class StateTerritory
        {
            public int StateId { get; set; }
            public int CellCount { get; set; }
            public List<List<Point2>> Regions { get; set; } = new List<List<Point2>>();
        }

        private List<StateTerritory> CreateStateTerritories(WorldMap pack)
        {
            var territories = new List<StateTerritory>();
            if (pack.StateIndexes == null || pack.Cells?.AdjacentCells == null)
                return territories;

            var stateGroups = new Dictionary<int, List<int>>();
            for (int cellIndex = 0; cellIndex < pack.StateIndexes.Length; cellIndex++)
            {
                var stateId = (int)pack.StateIndexes[cellIndex];
                if (stateId <= 0) continue;
                if (!stateGroups.ContainsKey(stateId)) stateGroups[stateId] = new List<int>();
                stateGroups[stateId].Add(cellIndex);
            }

            foreach (var stateGroup in stateGroups)
            {
                var territory = new StateTerritory
                {
                    StateId = stateGroup.Key,
                    CellCount = stateGroup.Value.Count,
                    Regions = CreateStateRegions(stateGroup.Value, pack)
                };
                if (territory.Regions.Count > 0) territories.Add(territory);
            }
            return territories;
        }

        private List<List<Point2>> CreateStateRegions(List<int> cellIndices, WorldMap pack)
        {
            var regions = new List<List<Point2>>();
            if (pack.Cells?.AdjacentCells == null || pack.Cells?.Coordinates == null) return regions;

            var visited = new HashSet<int>();
            var cellSet = new HashSet<int>(cellIndices);

            foreach (var startCell in cellIndices)
            {
                if (visited.Contains(startCell)) continue;
                var regionCells = new List<int>();
                var queue = new Queue<int>();
                queue.Enqueue(startCell);
                visited.Add(startCell);

                while (queue.Count > 0)
                {
                    var currentCell = queue.Dequeue();
                    regionCells.Add(currentCell);
                    if (currentCell < pack.Cells.AdjacentCells.Length && pack.Cells.AdjacentCells[currentCell] != null)
                    {
                        foreach (var adjacentCell in pack.Cells.AdjacentCells[currentCell])
                        {
                            if (adjacentCell >= 0 && cellSet.Contains(adjacentCell) && !visited.Contains(adjacentCell))
                            {
                                visited.Add(adjacentCell);
                                queue.Enqueue(adjacentCell);
                            }
                        }
                    }
                }

                if (regionCells.Count > 0)
                {
                    var regionBoundary = CreateRegionBoundary(regionCells, pack);
                    if (regionBoundary.Count >= 3) regions.Add(regionBoundary);
                }
            }
            return regions;
        }

        private List<Point2> CreateRegionBoundary(List<int> regionCells, WorldMap pack)
        {
            if (pack.Cells?.Coordinates == null) return new List<Point2>();
            var cellCenters = regionCells
                .Where(i => i < pack.Cells.Coordinates.Length)
                .Select(i => pack.Cells.Coordinates[i])
                .ToList();
            if (cellCenters.Count < 3) return new List<Point2>();
            var hull = _owner.ComputeConvexHull(cellCenters);
            return ExpandPolygon(hull, _owner.CalculateAverageCellSpacing(pack.Cells.Coordinates) * 0.3);
        }

        private List<Point2> ExpandPolygon(List<Point2> polygon, double expansion)
        {
            if (polygon.Count < 3 || expansion <= 0) return polygon;
            var center = new Point2(polygon.Average(p => p.X), polygon.Average(p => p.Y));
            var expanded = new List<Point2>();
            foreach (var point in polygon)
            {
                var direction = new Point2(point.X - center.X, point.Y - center.Y);
                var length = Math.Sqrt(direction.X * direction.X + direction.Y * direction.Y);
                if (length > 0)
                {
                    var normalized = new Point2(direction.X / length, direction.Y / length);
                    expanded.Add(new Point2(point.X + normalized.X * expansion, point.Y + normalized.Y * expansion));
                }
                else expanded.Add(point);
            }
            return expanded;
        }

        private List<Point2> CreateStatePolygonFromCells(List<int> cellIndices, Point2[] gridPoints){ if (cellIndices.Count==0||gridPoints==null) return new List<Point2>(); var cellPoints = cellIndices.Where(i=>i<gridPoints.Length).Select(i=>gridPoints[i]).ToList(); if (cellPoints.Count==0) return new List<Point2>(); if (cellPoints.Count<=2) return CreateSimpleBoundingShape(cellPoints); return _owner.ComputeConvexHull(cellPoints); }
        private List<Point2> CreateSimpleBoundingShape(List<Point2> points)
        {
            if (points == null || points.Count == 0) return new List<Point2>();
            if (points.Count == 1)
            {
                var center = points[0];
                var size = 25.0;
                return new List<Point2>
                {
                    new Point2(center.X - size, center.Y - size),
                    new Point2(center.X + size, center.Y - size),
                    new Point2(center.X + size, center.Y + size),
                    new Point2(center.X - size, center.Y + size)
                };
            }
            else if (points.Count == 2)
            {
                var p1 = points[0];
                var p2 = points[1];
                var dx = p2.X - p1.X;
                var dy = p2.Y - p1.Y;
                var length = Math.Sqrt(dx * dx + dy * dy);
                if (length <= 1e-9) return new List<Point2> { p1, p1, p2, p2 };
                var normalX = -dy / length * 20;
                var normalY = dx / length * 20;
                return new List<Point2>
                {
                    new Point2(p1.X + normalX, p1.Y + normalY),
                    new Point2(p2.X + normalX, p2.Y + normalY),
                    new Point2(p2.X - normalX, p2.Y - normalY),
                    new Point2(p1.X - normalX, p1.Y - normalY)
                };
            }
            else
            {
                return _owner.ComputeConvexHull(points);
            }
        }
        private List<List<Point2>> TraceStateBoundariesFromVoronoi(WorldMap pack, Dictionary<int, List<int>> stateGroups)
        {
            var stateBoundaries = new List<List<Point2>>();
            if (pack.Grid?.Cells?.AdjacentCells == null || pack.StateIndexes == null)
            {
                foreach (var kvp in stateGroups)
                {
                    var cellIndices = kvp.Value;
                    var cellPoints = cellIndices.Where(i => i < pack.Grid.Points.Length).Select(i => pack.Grid.Points[i]).ToList();
                    if (cellPoints.Count >= 3) stateBoundaries.Add(_owner.ComputeConvexHull(cellPoints));
                }
                return stateBoundaries;
            }

            foreach (var kvp in stateGroups)
            {
                var stateId = kvp.Key;
                var stateCells = new HashSet<int>(kvp.Value);

                var boundaryPoints = new List<Point2>();

                foreach (var cellIndex in kvp.Value)
                {
                    if (cellIndex >= pack.Grid.Cells.AdjacentCells.Length) continue;
                    var neighbors = pack.Grid.Cells.AdjacentCells[cellIndex];
                    if (neighbors == null) continue;
                    var cellPos = pack.Grid.Points[cellIndex];
                    foreach (var neighborIndex in neighbors)
                    {
                        if (neighborIndex < 0 || neighborIndex >= pack.StateIndexes.Length) continue;
                        var neighborStateId = pack.StateIndexes[neighborIndex];
                        if (neighborStateId != stateId)
                        {
                            var neighborPos = pack.Grid.Points[neighborIndex];
                            var boundaryPoint = new Point2((cellPos.X + neighborPos.X)/2, (cellPos.Y + neighborPos.Y)/2);
                            boundaryPoints.Add(boundaryPoint);
                        }
                    }
                }

                if (boundaryPoints.Count >= 3)
                {
                    var uniquePoints = boundaryPoints.GroupBy(p => new { X = Math.Round(p.X, 1), Y = Math.Round(p.Y, 1) }).Select(g => g.First()).ToList();
                    if (uniquePoints.Count >= 3) stateBoundaries.Add(_owner.ComputeConvexHull(uniquePoints));
                }
                else
                {
                    var cellPoints = kvp.Value.Where(i => i < pack.Grid.Points.Length).Select(i => pack.Grid.Points[i]).ToList();
                    if (cellPoints.Count >= 3) stateBoundaries.Add(_owner.ComputeConvexHull(cellPoints));
                }
            }

            return stateBoundaries;
        }

        /// <summary>
        /// Collects the set of Voronoi edges that lie on the boundary between cells with different
        /// group IDs (as returned by <paramref name="groupIdForCell"/>). Cells whose group ID is
        /// &lt;= 0 are skipped. Pass <paramref name="requireReverseEdge"/>=true (culture borders) to
        /// only include interior edges that have a matching neighbour; pass false (state outlines) to
        /// also include coastline/map-edge edges. Pass <paramref name="skipWaterNeighbors"/>=true to
        /// exclude edges whose only neighbour is a water cell.
        /// </summary>
        private HashSet<string> CollectBoundaryEdges(WorldMap pack, Func<int, int> groupIdForCell, bool requireReverseEdge = true, bool skipWaterNeighbors = false)
        {
            var edgeKeys = new HashSet<string>();
            if (pack.Cells?.VertexIndexes == null || pack.Vertices?.Coordinates == null) return edgeKeys;

            int cellCount = pack.Cells.VertexIndexes.Length;
            for (int cellIndex = 0; cellIndex < cellCount; cellIndex++)
            {
                var verts = pack.Cells.VertexIndexes[cellIndex];
                if (verts == null || verts.Length < 2) continue;

                int myGroup = groupIdForCell(cellIndex);
                if (myGroup <= 0) continue;

                for (int vi = 0; vi < verts.Length; vi++)
                {
                    var a = verts[vi]; var b = verts[(vi + 1) % verts.Length];
                    if (a < 0 || b < 0 || a >= pack.Vertices.Coordinates.Length || b >= pack.Vertices.Coordinates.Length) continue;

                    bool reverseEdgeFound = false, sameGroupFound = false, skipDueToWater = false;
                    if (pack.Cells.AdjacentCells != null && cellIndex < pack.Cells.AdjacentCells.Length && pack.Cells.AdjacentCells[cellIndex] != null)
                    {
                        foreach (var neighbour in pack.Cells.AdjacentCells[cellIndex])
                        {
                            if (neighbour < 0 || neighbour >= cellCount) continue;
                            var nverts = pack.Cells.VertexIndexes[neighbour]; if (nverts == null) continue;
                            for (int nj = 0; nj < nverts.Length; nj++)
                            {
                                var na = nverts[nj]; var nb = nverts[(nj + 1) % nverts.Length];
                                if (na == b && nb == a)
                                {
                                    reverseEdgeFound = true;
                                    if (skipWaterNeighbors)
                                    {
                                        if (!_owner.IsLandCell(neighbour, pack)) skipDueToWater = true;
                                    }
                                    if (groupIdForCell(neighbour) == myGroup) sameGroupFound = true;
                                    break;
                                }
                            }
                            if (reverseEdgeFound) break;
                        }
                    }
                    if (sameGroupFound || skipDueToWater) continue;
                    if (requireReverseEdge && !reverseEdgeFound) continue;
                    edgeKeys.Add(a < b ? $"{a}_{b}" : $"{b}_{a}");
                }
            }
            return edgeKeys;
        }

        /// <summary>
        /// Draws a set of Voronoi edges (collected by <see cref="CollectBoundaryEdges"/>) onto
        /// <paramref name="canvas"/> using the current state of <see cref="MapRendererBase.BorderPaint"/>.
        /// Edges entirely outside the viewport (with a 200-unit margin) are culled.
        /// </summary>
        private void DrawEdgeKeys(SKCanvas canvas, WorldMap pack, HashSet<string> edgeKeys)
        {
            foreach (var key in edgeKeys)
            {
                var parts = key.Split('_'); if (parts.Length != 2) continue;
                if (!int.TryParse(parts[0], out var va) || !int.TryParse(parts[1], out var vb)) continue;
                if (va < 0 || vb < 0 || va >= pack.Vertices.Coordinates.Length || vb >= pack.Vertices.Coordinates.Length) continue;
                var p1 = _owner.Viewport.WorldToScreen(pack.Vertices.Coordinates[va]);
                var p2 = _owner.Viewport.WorldToScreen(pack.Vertices.Coordinates[vb]);
                const float margin = 200f;
                if (p1.X < -margin && p2.X < -margin) continue;
                if (p1.X > _owner.Viewport.ViewBounds.Width + margin && p2.X > _owner.Viewport.ViewBounds.Width + margin) continue;
                if (p1.Y < -margin && p2.Y < -margin) continue;
                if (p1.Y > _owner.Viewport.ViewBounds.Height + margin && p2.Y > _owner.Viewport.ViewBounds.Height + margin) continue;
                canvas.DrawLine(p1, p2, _owner.BorderPaint!);
            }
        }

        /// <summary>
        /// Shared territory polygon loop. Creates state territories, resolves a fill colour per
        /// territory via <paramref name="colorResolver"/>, draws every visible region polygon, and
        /// optionally draws borders. The <paramref name="onLargestRegion"/> callback (if provided)
        /// is invoked once per territory with the largest rendered region — use it to place labels.
        /// Returns the total number of region polygons drawn.
        /// </summary>
        private int RenderTerritoryPolygons(SKCanvas canvas, WorldMap pack, Func<StateTerritory, SKColor> colorResolver, bool alwaysDrawBorders, Action<SKCanvas, StateTerritory, List<Point2>, SKColor>? onLargestRegion = null)
        {
            var territories = CreateStateTerritories(pack);
            int regionsRendered = 0;
            foreach (var territory in territories)
            {
                if (territory.StateId <= 0) continue;
                var fillColor = colorResolver(territory);
                _owner.DefaultPaint.Color = fillColor;
                _owner.DefaultPaint.Style = SKPaintStyle.Fill;

                List<Point2>? largestRegion = null; double largestArea = 0;
                foreach (var region in territory.Regions)
                {
                    if (region.Count < 3) continue;
                    var screenPoints = region.Select(_owner.Viewport.WorldToScreen).ToArray();
                    if (!_owner.IsPolygonVisible(screenPoints)) continue;
                    using var path = new SKPath();
                    path.MoveTo(screenPoints[0]);
                    for (int i = 1; i < screenPoints.Length; i++) path.LineTo(screenPoints[i]);
                    path.Close();
                    canvas.DrawPath(path, _owner.DefaultPaint);
                    if (alwaysDrawBorders && _owner.BorderPaint != null)
                    {
                        _owner.BorderPaint.Style = SKPaintStyle.Stroke;
                        _owner.BorderPaint.Color = SKColors.Black;
                        _owner.BorderPaint.StrokeWidth = Math.Max(2f, (float)(6 * Math.Min(_owner.Viewport.Scale, 2.0)));
                        canvas.DrawPath(path, _owner.BorderPaint);
                    }
                    regionsRendered++;
                    var area = CalculatePolygonArea(region);
                    if (area > largestArea) { largestArea = area; largestRegion = region; }
                }

                if (onLargestRegion != null && largestRegion != null)
                    onLargestRegion(canvas, territory, largestRegion, fillColor);
            }
            return regionsRendered;
        }

        /// <summary>
        /// Shared Voronoi cell fill loop. Iterates every land cell, resolves a fill color via
        /// <paramref name="colorResolver"/> (return null to skip a cell), builds the screen-space
        /// polygon, draws it, and accumulates it into RenderedLandmask.
        /// </summary>
        private int RenderCellsWithColorResolver(SKCanvas canvas, WorldMap pack, Func<int, SKColor?> colorResolver)
        {
            if (_owner.DefaultPaint == null || pack.Cells?.VertexIndexes == null || pack.Vertices?.Coordinates == null) return 0;

            _owner.RenderedLandmask ??= new List<SvgPolygon>();
            int cellsRendered = 0;

            for (int cellIndex = 0; cellIndex < pack.Cells.VertexIndexes.Length; cellIndex++)
            {
                var vertexIndices = pack.Cells.VertexIndexes[cellIndex];
                if (vertexIndices == null || vertexIndices.Length < 3) continue;

                if (!_owner.IsLandCell(cellIndex, pack)) continue;

                var fill = colorResolver(cellIndex);
                if (fill == null) continue;

                var polygonWorldPoints = new List<Point2>();
                var polygonPoints = new List<SKPoint>();
                bool valid = true;
                foreach (var vi in vertexIndices)
                {
                    if (vi < 0 || vi >= pack.Vertices.Coordinates.Length) { valid = false; break; }
                    var vertex = pack.Vertices.Coordinates[vi];
                    polygonWorldPoints.Add(vertex);
                    polygonPoints.Add(_owner.Viewport.WorldToScreen(vertex));
                }
                if (!valid || polygonPoints.Count < 3) continue;
                if (!_owner.IsPolygonVisible(polygonPoints.ToArray())) continue;

                _owner.DefaultPaint.Color = fill.Value;
                _owner.DefaultPaint.Style = SKPaintStyle.Fill;

                using var path = new SKPath();
                path.MoveTo(polygonPoints[0]);
                for (int i = 1; i < polygonPoints.Count; i++) path.LineTo(polygonPoints[i]);
                path.Close();
                canvas.DrawPath(path, _owner.DefaultPaint);

                _owner.RenderedLandmask.Add(new SvgPolygon { Points = polygonWorldPoints });
                cellsRendered++;
            }

            return cellsRendered;
        }
    }
}
