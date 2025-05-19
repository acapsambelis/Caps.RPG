using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Caps.RPG.MonoGame.Graphics
{
    public class TextureRegion
    {

        public Texture2D Texture { get; set; }

        public Rectangle SourceRectangle { get; set; }

        public int Width => SourceRectangle.Width;

        public int Height => SourceRectangle.Height;

        /// <summary>
        /// Gets the top normalized texture coordinate of this region.
        /// </summary>
        public float TopTextureCoordinate => SourceRectangle.Top / (float)Texture.Height;

        /// <summary>
        /// Gets the bottom normalized texture coordinate of this region.
        /// </summary>
        public float BottomTextureCoordinate => SourceRectangle.Bottom / (float)Texture.Height;

        /// <summary>
        ///  Gets the left normalized texture coordinate of this region.
        /// </summary>
        public float LeftTextureCoordinate => SourceRectangle.Left / (float)Texture.Width;

        /// <summary>
        /// Gets the right normalized texture coordinate of this region.
        /// </summary>
        public float RightTextureCoordinate => SourceRectangle.Right / (float)Texture.Width;


        public TextureRegion() { }

        public TextureRegion(Texture2D texture, int x, int y, int width, int height)
        {
            Texture = texture;
            SourceRectangle = new Rectangle(x, y, width, height);
        }

        /// <summary>
        /// Submit this texture region for drawing in the current batch.
        /// </summary>
        /// <param name="spriteBatch">The spritebatch instance used for batching draw calls.</param>
        /// <param name="position">The xy-coordinate location to draw this texture region on the screen.</param>
        /// <param name="color">The color mask to apply when drawing this texture region on screen.</param>
        public void Draw(SpriteBatch spriteBatch, Vector2 position, Color color)
        {
            Draw(spriteBatch, position, color, 0.0f, Vector2.Zero, Vector2.One, SpriteEffects.None, 0.0f);
        }

        /// <summary>
        /// Submit this texture region for drawing in the current batch.
        /// </summary>
        /// <param name="spriteBatch">The spritebatch instance used for batching draw calls.</param>
        /// <param name="position">The xy-coordinate location to draw this texture region on the screen.</param>
        /// <param name="color">The color mask to apply when drawing this texture region on screen.</param>
        /// <param name="rotation">The amount of rotation, in radians, to apply when drawing this texture region on screen.</param>
        /// <param name="origin">The center of rotation, scaling, and position when drawing this texture region on screen.</param>
        /// <param name="scale">The scale factor to apply when drawing this texture region on screen.</param>
        /// <param name="effects">Specifies if this texture region should be flipped horizontally, vertically, or both when drawing on screen.</param>
        /// <param name="layerDepth">The depth of the layer to use when drawing this texture region on screen.</param>
        public void Draw(SpriteBatch spriteBatch, Vector2 position, Color color, float rotation, Vector2 origin, float scale, SpriteEffects effects, float layerDepth)
        {
            Draw(
                spriteBatch,
                position,
                color,
                rotation,
                origin,
                new Vector2(scale, scale),
                effects,
                layerDepth
            );
        }

        /// <summary>
        /// Submit this texture region for drawing in the current batch.
        /// </summary>
        /// <param name="spriteBatch">The spritebatch instance used for batching draw calls.</param>
        /// <param name="position">The xy-coordinate location to draw this texture region on the screen.</param>
        /// <param name="color">The color mask to apply when drawing this texture region on screen.</param>
        /// <param name="rotation">The amount of rotation, in radians, to apply when drawing this texture region on screen.</param>
        /// <param name="origin">The center of rotation, scaling, and position when drawing this texture region on screen.</param>
        /// <param name="scale">The amount of scaling to apply to the x- and y-axes when drawing this texture region on screen.</param>
        /// <param name="effects">Specifies if this texture region should be flipped horizontally, vertically, or both when drawing on screen.</param>
        /// <param name="layerDepth">The depth of the layer to use when drawing this texture region on screen.</param>
        public void Draw(SpriteBatch spriteBatch, Vector2 position, Color color, float rotation, Vector2 origin, Vector2 scale, SpriteEffects effects, float layerDepth)
        {
            spriteBatch.Draw(
                Texture,
                position,
                SourceRectangle,
                color,
                rotation,
                origin,
                scale,
                effects,
                layerDepth
            );
        }

    }
}
