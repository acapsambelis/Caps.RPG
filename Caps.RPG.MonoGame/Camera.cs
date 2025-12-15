using Caps.RPG.MonoGame.Input;
using GeonBit.UI;
using Microsoft.Xna.Framework;
using System;
using System.Drawing;

namespace Caps.RPG.MonoGame
{
    public enum CameraSceneMode
    {
        None,
        FullScreen,
        Panning,
        Following
    }

    public enum CameraMoveMode
    {
        None,
        FullScreen,
        Static,
        Point,
        Drag,
        Follow
    }

    /// <summary>
    /// The Camera class is basically just a location (the center of the screen)
    /// which is used to generate a translation matrix for use in the SpriteBatch.Begin method
    /// which in turn offsets everything that is drawn to the screen.
    /// The offset is a quarter of the screen size (which is the top-left corner of the screen)
    /// </summary>
    public class Camera
    {
        private Vector2 viewSize;
        private readonly MouseInfo _mouseInfo;
        private CameraMoveMode _mode = CameraMoveMode.Follow;
        private Vector2 _pointDestination;
        private Vector2 _dragStartPosition;
        private Vector2 _followPosition;
        private float _zoom = 1f;
        public Vector2 CameraCenter { get; set; }
        public CameraMoveMode Mode
        {
            get => _mode;
            set
            {
                _mode = value;
                var mouseWorldPos = GetWorldPosition(_mouseInfo.Position.ToVector2());
                switch (_mode)
                {
                    case CameraMoveMode.Drag:
                        _dragStartPosition = mouseWorldPos;
                        break;
                    case CameraMoveMode.Point:
                        _pointDestination = mouseWorldPos;
                        break;
                }
            }
        }
        public float Zoom
        {
            get => _zoom;
            set => _zoom = Math.Clamp(value, 0.1f, 5f); // Prevent extreme zooms
        }

        public Vector2 FollowPosition
        {
            get => _followPosition;
            set => _followPosition = value;
        }

        public bool DirectOrder
        {
            get;
            set;
        }

        public Vector2 GetTopLeft() => CameraCenter - viewSize;

        public Camera(Vector2 viewSize, MouseInfo mouseInfo)
        {
            _mouseInfo = mouseInfo;
            this.viewSize = viewSize;
        }

        public void MoveCamera(GameTime gameTime, RectangleF? worldSize = null)
        {
            if (UserInterface.Active.ActiveEntity is not GeonBit.UI.Entities.RootPanel && !DirectOrder)
                return;
            int pixelsMoved = 0;
            if (_mouseInfo.IsButtonDown(MouseButtons.Left))
            {
                if (_mouseInfo.XDelta != 0 || _mouseInfo.YDelta != 0)
                {
                    var newMouseWorldPosition = GetWorldPosition(_mouseInfo.Position.ToVector2());
                    var difference = _dragStartPosition - newMouseWorldPosition;
                    var targetPosition = CameraCenter + difference;
                    pixelsMoved = MoveToward(targetPosition, (float)gameTime.ElapsedGameTime.TotalMilliseconds, 0.5f);
                    _dragStartPosition = newMouseWorldPosition;
                }
            }
            if (Mode == CameraMoveMode.Point)
                pixelsMoved = MoveToward(_pointDestination, (float)gameTime.ElapsedGameTime.TotalMilliseconds, 0.1f);
            if (Mode == CameraMoveMode.Follow)
                pixelsMoved = MoveToward(_followPosition, (float)gameTime.ElapsedGameTime.TotalMilliseconds);

            if (worldSize.HasValue)
                ClampToWorld(worldSize.Value);

            // mark order complete when camera stops moving
            DirectOrder = DirectOrder && pixelsMoved != 0;
        }

        private void ClampToWorld(RectangleF worldSize)
        {
            CameraCenter = new Vector2(
                Math.Clamp(CameraCenter.X, worldSize.Left, worldSize.Right),
                Math.Clamp(CameraCenter.Y, worldSize.Top, worldSize.Bottom)
            );
        }

        public void ZoomCamera()
        {
            if (UserInterface.Active.ActiveEntity is not GeonBit.UI.Entities.RootPanel && !DirectOrder)
                return;
            var delta = _mouseInfo.ScrollWheelDelta;
            if (delta != 0)
            {
                // Adjust zoom speed as needed
                Zoom += delta * 0.001f;
            }
        }

        public void ZoomToWorldSize(RectangleF worldSize)
        {
            // Calculate the scale needed so that the world fits 75% of the screen
            float worldWidth = worldSize.Width;
            float worldHeight = worldSize.Height;

            float screenWidth = viewSize.X * 2f;
            float screenHeight = viewSize.Y * 2f;

            // 95% of the screen
            float targetScreenWidth = screenWidth * 0.95f;
            float targetScreenHeight = screenHeight * 0.95f;

            float zoomX = targetScreenWidth / worldWidth;
            float zoomY = targetScreenHeight / worldHeight;

            // Use the smaller zoom to ensure the whole world fits
            Zoom = Math.Min(zoomX, zoomY);

            // Optionally, center the camera on the world
            CameraCenter = new Vector2(
                worldSize.Left + worldWidth / 2f,
                worldSize.Top + worldHeight / 2f
            );
        }


        // Plan (pseudocode):
        // 1. The current GetTranslation() transforms world -> screen with:
        //      screenPos = ((worldPos - CameraCenter) * Zoom) + viewSize
        // 2. To get world from screen we need to invert that:
        //      worldPos = ((screenPos - viewSize) / Zoom) + CameraCenter
        // 3. Implement the inverse transformation, guarding implicitly against invalid Zoom
        //    (Zoom is already clamped elsewhere), return the computed world position.
        public Vector2 GetWorldPosition(Vector2 screenPosition)
        {
            // screenCenter is stored in `viewSize` (used as translation back in GetTranslation)
            var screenCenter = viewSize;

            // Undo translation to center, then undo scaling, then translate by camera center
            return ((screenPosition - screenCenter) / Zoom) + CameraCenter;
        }


        // returns the number of pixels moved
        public int MoveToward(Vector2 target, float deltaTimeInMs, float movePercentage = .02f)
        {
            //figure out which direction to move the camera
            Vector2 differenceInPosition = target - CameraCenter;

            //figure out how far to move in each update
            differenceInPosition *= movePercentage;

            //get a fraction how much time has passed since last update
            //so the camera moves at a constant speed
            var fractionOfPassedTime = deltaTimeInMs / 10;
            //note: dividing by 10 is an arbitrary constant,
            //which works well in this case to make the camera slower than the player

            //move the camera towards the target
            CameraCenter += differenceInPosition * fractionOfPassedTime;

            //if the camera is very close to the target, just center it on the target
            //in order to avoid "jiggling" if the camera continuously overshoots the target
            if ((target - CameraCenter).Length() < movePercentage)
            {
                CameraCenter = target;
            }

            return (int)Math.Round(Math.Sqrt(differenceInPosition.X * differenceInPosition.X + differenceInPosition.Y * differenceInPosition.Y));
        }

        public Matrix GetTranslation()
        {
            // Calculate the translation to move the camera center to the origin
            var translationToOrigin = Matrix.CreateTranslation(-CameraCenter.X, -CameraCenter.Y, 0);

            // Apply scaling (zoom) around the origin (camera center)
            var scale = Matrix.CreateScale(Zoom, Zoom, 1f);

            // Translate back to the screen center after scaling
            var screenCenter = viewSize;
            var translationBack = Matrix.CreateTranslation(screenCenter.X, screenCenter.Y, 0);

            // Combine the matrices: move to origin -> scale -> move back
            return translationToOrigin * scale * translationBack;
        }
    }
}