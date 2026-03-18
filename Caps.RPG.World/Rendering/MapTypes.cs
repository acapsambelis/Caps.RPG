#nullable enable
using System;
using SkiaSharp;
using Caps.RPG.World.Models.Graphics;

namespace Caps.RPG.World.Rendering
{
    /// <summary>
    /// Viewport for controlling map view, zoom, and pan
    /// </summary>
    public class MapViewport
    {
        public double Scale { get; set; } = 1.0;
        public Point2 Offset { get; set; } = new(0, 0);
        public SKRect ViewBounds { get; set; }
        public double MinScale { get; set; } = 0.1;
        public double MaxScale { get; set; } = 10.0;

        public SKPoint WorldToScreen(Point2 worldPos)
        {
            return new SKPoint(
                (float)((worldPos.X + Offset.X) * Scale),
                (float)((worldPos.Y + Offset.Y) * Scale)
            );
        }

        public Point2 ScreenToWorld(SKPoint screenPos)
        {
            return new Point2(
                screenPos.X / Scale - Offset.X,
                screenPos.Y / Scale - Offset.Y
            );
        }

        public void ZoomAt(SKPoint screenPoint, double zoomFactor)
        {
            var worldPoint = ScreenToWorld(screenPoint);
            Scale = Math.Clamp(Scale * zoomFactor, MinScale, MaxScale);
            
            // Adjust offset to keep the zoom point stationary
            var newScreenPoint = WorldToScreen(worldPoint);
            Offset = new Point2(
                Offset.X + (screenPoint.X - newScreenPoint.X) / Scale,
                Offset.Y + (screenPoint.Y - newScreenPoint.Y) / Scale
            );
        }

        public void Pan(SKPoint delta)
        {
            Offset = new Point2(
                Offset.X + delta.X / Scale,
                Offset.Y + delta.Y / Scale
            );
        }
    }

    /// <summary>
    /// Map rendering layers
    /// </summary>
    [Flags]
    public enum MapLayer
    {
        None = 0,
        Terrain = 1,
        Political = 2,
        Cultural = 4,
        Religion = 256,
        Settlements = 8,
        Rivers = 16,
        Routes = 32,
        Labels = 64,
        StateAreas = 128,
        StateBorders = 512,
        ProvinceBorders = 1024,
        All = Terrain | Political | Cultural | Religion | Settlements | Rivers | Routes | Labels | StateAreas | StateBorders | ProvinceBorders
    }

    /// <summary>
    /// High-level map display modes (similar to grand-strategy map modes)
    /// </summary>
    public enum MapMode
    {
        Political = 0,
        Physical = 1,
        Cultural = 2,
        Religion = 3
    }

    /// <summary>
    /// Event arguments for rendering status updates
    /// </summary>
    public class RenderStatusEventArgs : EventArgs
    {
        public string Status { get; }
        public string Layer { get; }
        public bool IsEnabled { get; }
        public int? Count { get; }

        public RenderStatusEventArgs(string status, string layer, bool isEnabled, int? count = null)
        {
            Status = status;
            Layer = layer;
            IsEnabled = isEnabled;
            Count = count;
        }
    }
}
