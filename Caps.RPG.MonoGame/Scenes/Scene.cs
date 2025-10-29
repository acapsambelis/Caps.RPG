using System;
using System.Collections.Generic;
using Caps.RPG.MonoGame.Graphics;
using Caps.RPG.MonoGame.Input;
using GeonBit.UI;
using GeonBit.UI.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace Caps.RPG.MonoGame.Scenes
{
    public abstract class Scene : IDisposable
    {
        /// <summary>
        /// Gets the ContentManager used for loading scene-specific assets.
        /// </summary>
        /// <remarks>
        /// Assets loaded through this ContentManager will be automatically unloaded when this scene ends.
        /// </remarks>
        protected ContentManager Content { get; }

        /// <summary>
        /// Gets a value that indicates if the scene has been disposed of.
        /// </summary>
        public bool IsDisposed { get; private set; }

        private List<Clickable> registeredClickables = [];

        /// <summary>
        /// Creates a new scene instance.
        /// </summary>
        public Scene()
        {
            // Create a content manager for the scene
            Content = new ContentManager(Core.Content.ServiceProvider);

            // Set the root directory for content to the same as the root directory
            // for the game's content.
            Content.RootDirectory = Core.Content.RootDirectory;
        }

        // Finalizer, called when object is cleaned up by garbage collector.
        ~Scene() => Dispose(false);

        /// <summary>
        /// Initializes the scene.
        /// </summary>
        /// <remarks>
        /// When overriding this in a derived class, ensure that base.Initialize()
        /// still called as this is when LoadContent is called.
        /// </remarks>
        public virtual void Initialize()
        {
            LoadContent();
        }

        /// <summary>
        /// Override to provide logic to load content for the scene.
        /// </summary>
        public virtual void LoadContent() { }

        /// <summary>
        /// Add a clickable object to the list
        /// </summary>
        /// <param name="clickable"></param>
        public void RegisterClickable(Clickable clickable)
        {
            registeredClickables.Add(clickable);
        }

        /// <summary>
        /// Unloads scene-specific content.
        /// </summary>
        public virtual void UnloadContent()
        {
            Content.Unload();
        }

        /// <summary>
        /// Updates this scene.
        /// </summary>
        /// <param name="gameTime">A snapshot of the timing values for the current frame.</param>
        public virtual void Update(GameTime gameTime)
        {
            var mousePos = Core.Camera.GetWorldPosition(Core.Input.Mouse.Position.ToVector2());
            if (!Core.Input.Mouse.WasButtonJustPressed(MouseButtons.Left) || registeredClickables == null || UserInterface.Active.ActiveEntity is not RootPanel)
                return;
            foreach (Clickable clickable in registeredClickables)
            {
                if (clickable.IsInside(mousePos))
                {
                    clickable.FireClick(EventArgs.Empty);
                }
            }
        }



        /// <summary>
        /// Draws this scene.
        /// </summary>
        /// <param name="gameTime">A snapshot of the timing values for the current frame.</param>
        public virtual void Draw(GameTime gameTime) { }

        /// <summary>
        /// Disposes of this scene.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes of this scene.
        /// </summary>
        /// <param name="disposing">'
        /// Indicates whether managed resources should be disposed.  This value is only true when called from the main
        /// Dispose method.  When called from the finalizer, this will be false.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            if (IsDisposed)
            {
                return;
            }

            if (disposing)
            {
                UnloadContent();
                Content.Dispose();
            }
        }

    }
}
