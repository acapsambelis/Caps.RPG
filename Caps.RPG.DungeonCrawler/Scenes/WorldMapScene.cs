using Caps.RPG.MonoGame;
using Caps.RPG.World.Models;
using Caps.RPG.World.Rendering;
using GeonBit.UI;
using GeonBit.UI.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Caps.RPG.DungeonCrawler.Scenes
{
    public class WorldMapScene(CommonConfig config, string mapFilePath, Action? onClose = null)
        : BaseScene(config, CameraSceneMode.FullScreen)
    {
        private WorldMap? _fmgMap;
        private WorldMapRenderer? _renderer;
        private Texture2D? _mapTexture;
        private bool _isDirty = true;
        private bool _isPanning;
        private bool _hasPanned;
        private int _hoveredBurgId = -1;
        private int _hoveredCellIndex = -1;   // cell currently shown in the tooltip
        private int _pendingCellIndex = -1;    // cell under the cursor (not yet shown)
        private SKPoint _lastTooltipPos = new(-1, -1);
        private double _tooltipTimer = 0.0;    // seconds the mouse has been still
        private bool _tooltipVisible = false;
        private const double TooltipDelay = 0.4;
        private Panel? _burgPopup;

        /// <summary>
        /// Raised when the user clicks a burg/settlement on the map.
        /// Subscribe to this to show custom popups or trigger game events.
        /// </summary>
        public event Action<Burg>? BurgClicked;

        public override void Initialize()
        {
            base.Initialize();
            InitializeUI();
        }

        public override void LoadContent()
        {
            base.LoadContent();

            if (System.IO.File.Exists(mapFilePath))
                _fmgMap = MapLoader.LoadFromFile(mapFilePath);

            _renderer = new WorldMapRenderer();
            _renderer.EnableRiverPerfDebug = true;
            SetMapMode(MapMode.Political);

            BurgClicked += ShowBurgPopup;
        }

        private void InitializeUI()
        {
            Panel modePanel = new(new Vector2(200, 280), PanelSkin.None, Anchor.TopRight, new Vector2(5, 5));

            Button physicalBtn = new("Physical", ButtonSkin.Default, Anchor.AutoCenter, new Vector2(180, 60));
            physicalBtn.OnClick += _ => SetMapMode(MapMode.Physical);
            modePanel.AddChild(physicalBtn);

            Button politicalBtn = new("Political", ButtonSkin.Default, Anchor.AutoCenter, new Vector2(180, 60));
            politicalBtn.OnClick += _ => SetMapMode(MapMode.Political);
            modePanel.AddChild(politicalBtn);

            Button culturalBtn = new("Cultural", ButtonSkin.Default, Anchor.AutoCenter, new Vector2(180, 60));
            culturalBtn.OnClick += _ => SetMapMode(MapMode.Cultural);
            modePanel.AddChild(culturalBtn);

            Button religionBtn = new("Religion", ButtonSkin.Default, Anchor.AutoCenter, new Vector2(180, 60));
            religionBtn.OnClick += _ => SetMapMode(MapMode.Religion);
            modePanel.AddChild(religionBtn);

            UserInterface.Active.AddEntity(modePanel);

            if (onClose != null)
            {
                Button closeBtn = new("X", ButtonSkin.Default, Anchor.TopLeft, new Vector2(50, 50));
                closeBtn.OnClick += _ => onClose();
                UserInterface.Active.AddEntity(closeBtn);
            }
        }

        private void SetMapMode(MapMode mode)
        {
            if (_renderer == null) return;

            _renderer.CurrentMapMode = mode;
            _renderer.EnabledLayers = mode switch
            {
                MapMode.Political => MapLayer.StateAreas | MapLayer.Settlements | MapLayer.Labels,
                MapMode.Physical  => MapLayer.Terrain | MapLayer.StateBorders | MapLayer.Settlements | MapLayer.Rivers | MapLayer.Routes | MapLayer.Labels,
                MapMode.Cultural  => MapLayer.Cultural | MapLayer.StateBorders | MapLayer.Settlements | MapLayer.Labels,
                MapMode.Religion  => MapLayer.Religion | MapLayer.StateBorders | MapLayer.Settlements | MapLayer.Labels,
                _                 => MapLayer.All
            };

            _isDirty = true;
        }

        private void HandleInput()
        {
            if (_renderer == null) return;

            var mouse = Core.Input.Mouse;

            // Pan via left-button drag when no UI entity is active
            if (mouse.IsButtonDown(MonoGame.Input.MouseButtons.Left)
                && UserInterface.Active.ActiveEntity is RootPanel)
            {
                if (!_isPanning)
                {
                    _isPanning = true;
                }
                else
                {
                    var delta = mouse.PositionDelta;
                    if (delta != Microsoft.Xna.Framework.Point.Zero)
                    {
                        _renderer.Viewport.Pan(new SKPoint(delta.X, delta.Y));
                        _isDirty = true;
                        _hasPanned = true;
                    }
                }
            }
            else
            {
                // Left button was just released: treat as a click if the mouse didn't actually drag
                if (_isPanning && !_hasPanned
                    && mouse.WasButtonJustReleased(MonoGame.Input.MouseButtons.Left)
                    && UserInterface.Active.ActiveEntity is RootPanel)
                {
                    HandleMapClick(new SKPoint(mouse.X, mouse.Y));
                }

                _isPanning = false;
                _hasPanned = false;
            }

            // Zoom via scroll wheel
            int scrollDelta = mouse.ScrollWheelDelta;
            if (scrollDelta != 0)
            {
                var screenPoint = new SKPoint(mouse.X, mouse.Y);
                double zoomFactor = scrollDelta > 0 ? 1.15 : 1.0 / 1.15;
                _renderer.Viewport.ZoomAt(screenPoint, zoomFactor);
                _isDirty = true;
            }

            UpdateHoveredBurg();
            UpdateHoveredCell();
        }

        private void UpdateHoveredBurg()
        {
            var mousePos = new SKPoint(Core.Input.Mouse.X, Core.Input.Mouse.Y);
            int newHovered = FindNearestBurg(mousePos)?.Id ?? -1;

            if (newHovered != _hoveredBurgId)
            {
                _hoveredBurgId = newHovered;
                _renderer.HoveredBurgId = newHovered;
                _isDirty = true;
            }
        }

        private void UpdateHoveredCell()
        {
            if (_renderer == null || _fmgMap == null) return;
            var mousePos = new SKPoint(Core.Input.Mouse.X, Core.Input.Mouse.Y);

            int cellIdx = _renderer.FindCellAtScreenPoint(mousePos);
            if (cellIdx >= 0 && _fmgMap.TerrainType != null && cellIdx < _fmgMap.TerrainType.Length
                && _fmgMap.TerrainType[cellIdx] <= 0)
                cellIdx = -1;

            float dx = mousePos.X - _lastTooltipPos.X;
            float dy = mousePos.Y - _lastTooltipPos.Y;
            bool mouseMoved = dx * dx + dy * dy > 1f;

            if (mouseMoved)
            {
                // Mouse moved — reset the delay and record new position/cell
                _lastTooltipPos = mousePos;
                _pendingCellIndex = cellIdx;
                _tooltipTimer = 0.0;

                // Hide any currently visible tooltip immediately
                if (_tooltipVisible)
                {
                    _tooltipVisible = false;
                    _hoveredCellIndex = -1;
                    _renderer.HoveredCellIndex = -1;
                    _isDirty = true;
                }
            }
            else
            {
                // Mouse is still — keep pending cell current (handles viewport pan/zoom)
                _pendingCellIndex = cellIdx;
            }
        }

        private void AdvanceTooltipTimer(GameTime gameTime)
        {
            if (_pendingCellIndex < 0 || _tooltipVisible) return;

            _tooltipTimer += gameTime.ElapsedGameTime.TotalSeconds;
            if (_tooltipTimer >= TooltipDelay)
            {
                _hoveredCellIndex = _pendingCellIndex;
                _renderer.HoveredCellIndex = _pendingCellIndex;
                _renderer.TooltipScreenPosition = _lastTooltipPos;
                _tooltipVisible = true;
                _isDirty = true;
            }
        }

        private void HandleMapClick(SKPoint screenPos)
        {
            var clicked = FindNearestBurg(screenPos);
            if (clicked != null)
                BurgClicked?.Invoke(clicked);
        }

        private Burg? FindNearestBurg(SKPoint screenPos, float hitRadius = 15f)
        {
            if (_renderer == null || _fmgMap?.Burgs == null) return null;
            Burg? nearest = null;
            float bestDist = hitRadius;
            foreach (var burg in _fmgMap.Burgs)
            {
                if (burg.Population <= 0) continue;
                var sp = _renderer.Viewport.WorldToScreen(new Caps.RPG.World.Models.Graphics.Point2(burg.X, burg.Y));
                float dist = MathF.Sqrt(MathF.Pow(screenPos.X - sp.X, 2) + MathF.Pow(screenPos.Y - sp.Y, 2));
                if (dist < bestDist) { bestDist = dist; nearest = burg; }
            }
            return nearest;
        }

        private void ShowBurgPopup(Burg burg)
        {
            // Dismiss any existing popup first
            _burgPopup?.RemoveFromParent();
            _burgPopup = null;

            var popup = new Panel(new Vector2(300, 0), PanelSkin.Default, Anchor.Center);

            popup.AddChild(new Header(burg.Name ?? "Unknown Settlement"));
            popup.AddChild(new HorizontalLine());

            if (burg.Capital)
                popup.AddChild(new Paragraph("* Capital City", Anchor.Auto, Microsoft.Xna.Framework.Color.Gold));

            if (!string.IsNullOrEmpty(burg.Type))
                popup.AddChild(new Paragraph($"Type: {burg.Type}"));

            var culture = burg.Culture > 0
                ? _fmgMap?.Cultures?.FirstOrDefault(c => c.Id == burg.Culture)
                : null;
            if (culture != null && !string.IsNullOrEmpty(culture.Name))
            {
                var swatchColor = Microsoft.Xna.Framework.Color.Gray;
                if (!string.IsNullOrEmpty(culture.Color) && SKColor.TryParse(culture.Color, out var sk))
                    swatchColor = new Microsoft.Xna.Framework.Color(sk.Red, sk.Green, sk.Blue);

                var swatch = new ColoredRectangle(swatchColor, new Vector2(18, 18), Anchor.AutoInlineNoBreak);
                swatch.SpaceAfter = new Vector2(6, 0);
                popup.AddChild(swatch);
                popup.AddChild(new Paragraph($"Culture: {culture.Name}", Anchor.AutoInline));
            }

            if (burg.Population > 0)
                popup.AddChild(new Paragraph($"Population: ~{(int)(burg.Population * 1000):N0}"));

            var features = new List<string>();
            if (burg.Port > 0)    features.Add("Port");
            if (burg.Citadel)     features.Add("Citadel");
            if (burg.Temple)      features.Add("Temple");
            if (burg.Plaza)       features.Add("Plaza");
            if (burg.Walls)       features.Add("Walls");
            if (burg.Shanty)      features.Add("Shanty Town");
            if (features.Count > 0)
                popup.AddChild(new Paragraph("Features: " + string.Join(", ", features)));

            popup.AddChild(new HorizontalLine());

            var closeBtn = new Button("Close", ButtonSkin.Default, Anchor.Auto, new Vector2(0, 50));
            closeBtn.OnClick += _ =>
            {
                _burgPopup?.RemoveFromParent();
                _burgPopup = null;
            };
            popup.AddChild(closeBtn);

            UserInterface.Active.AddEntity(popup);
            _burgPopup = popup;
        }

        private void RenderMapToTexture()
        {
            if (_renderer == null) return;

            int width = Core.GraphicsDevice.Viewport.Width;
            int height = Core.GraphicsDevice.Viewport.Height;

            using var bitmap = new SKBitmap(width, height, SKColorType.Rgba8888, SKAlphaType.Opaque);
            using var canvas = new SKCanvas(bitmap);

            if (_fmgMap != null)
                _renderer.RenderMap(canvas, _fmgMap, width, height);
            else
                canvas.Clear(SKColors.DarkSlateGray);

            var oldTexture = _mapTexture;
            _mapTexture = new Texture2D(Core.GraphicsDevice, width, height, false, SurfaceFormat.Color);
            _mapTexture.SetData(bitmap.Bytes);
            oldTexture?.Dispose();

            _isDirty = false;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            HandleInput();
            AdvanceTooltipTimer(gameTime);
            if (_isDirty)
                RenderMapToTexture();
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime, () =>
            {
                if (_mapTexture != null)
                {
                    Core.SpriteBatch.Begin(SpriteSortMode.Immediate);
                    Core.SpriteBatch.Draw(_mapTexture, Vector2.Zero, Microsoft.Xna.Framework.Color.White);
                    Core.SpriteBatch.End();
                }
                return System.Drawing.RectangleF.Empty;
            });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _renderer?.Dispose();
                _mapTexture?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
