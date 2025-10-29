using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Linq;

namespace Caps.RPG.MonoGame.Graphics
{
    public class Sprite
    {
        /// <summary>
        /// Gets or Sets the source texture region represented by this sprite.
        /// </summary>
        public TextureRegion Region { get; set; }

        /// <summary>
        /// Gets or Sets the color mask to apply when rendering this sprite.
        /// </summary>
        /// <remarks>
        /// Default value is Color.White
        /// </remarks>
        public Color Color { get; set; } = Color.White;

        /// <summary>
        /// Gets or Sets the amount of rotation, in radians, to apply when rendering this sprite.
        /// </summary>
        /// <remarks>
        /// Default value is 0.0f
        /// </remarks>
        public float Rotation { get; set; } = 0.0f;

        /// <summary>
        /// Gets or Sets the scale factor to apply to the x- and y-axes when rendering this sprite.
        /// </summary>
        /// <remarks>
        /// Default value is Vector2.One
        /// </remarks>
        public Vector2 Scale { get; set; } = Vector2.One;

        /// <summary>
        /// Gets or Sets the xy-coordinate origin point, relative to the top-left corner, of this sprite.
        /// </summary>
        /// <remarks>
        /// Default value is Vector2.Zero
        /// </remarks>
        public Vector2 Origin { get; set; } = Vector2.Zero;

        /// <summary>
        /// Gets or Sets the sprite effects to apply when rendering this sprite.
        /// </summary>
        /// <remarks>
        /// Default value is SpriteEffects.None
        /// </remarks>
        public SpriteEffects Effects { get; set; } = SpriteEffects.None;

        /// <summary>
        /// Gets or Sets the layer depth to apply when rendering this sprite.
        /// </summary>
        /// <remarks>
        /// Default value is 0.0f
        /// </remarks>
        public float LayerDepth { get; set; } = 0.0f;

        /// <summary>
        /// Gets the width, in pixels, of this sprite. 
        /// </summary>
        /// <remarks>
        /// Width is calculated by multiplying the width of the source texture region by the x-axis scale factor.
        /// </remarks>
        public float Width => Region.Width * Scale.X;

        /// <summary>
        /// Gets the height, in pixels, of this sprite.
        /// </summary>
        /// <remarks>
        /// Height is calculated by multiplying the height of the source texture region by the y-axis scale factor.
        /// </remarks>
        public float Height => Region.Height * Scale.Y;

        /// <summary>
        /// Creates a new sprite.
        /// </summary>
        public Sprite() { }

        /// <summary>
        /// Creates a new sprite using the specified source texture region.
        /// </summary>
        /// <param name="region">The texture region to use as the source texture region for this sprite.</param>
        public Sprite(TextureRegion region, bool isCentered = true)
        {
            Region = region;
            if (isCentered) CenterOrigin();
        }

        public Sprite(TextureRegion region, Vector2 scale, bool isCentered = true)
        {
            Region = region;
            Scale = scale;
            if (isCentered) CenterOrigin();
        }

        public Sprite(Sprite other)
        {
            Region = new TextureRegion(other.Region);
            //Color = other.Color;
            Color = Color.Green;
            Rotation = other.Rotation;
            Scale = other.Scale;
            Origin = other.Origin;
            Effects = other.Effects;
            LayerDepth = other.LayerDepth;
        }

        /// <summary>
        /// Sets the origin of this sprite to the center
        /// </summary>
        public void CenterOrigin()
        {
            Origin = new Vector2(Region.Width, Region.Height) * 0.5f;
        }

        /// <summary>
        /// Submit this sprite for drawing to the current batch.
        /// </summary>
        /// <param name="spriteBatch">The SpriteBatch instance used for batching draw calls.</param>
        /// <param name="position">The xy-coordinate position to render this sprite at.</param>
        public void Draw(SpriteBatch spriteBatch, Vector2 position)
        {
            Region.Draw(spriteBatch, position, Color, Rotation, Origin, Scale, Effects, LayerDepth);
        }

        public Texture2D GetTexture()
        {
            // Returns a new Texture2D containing only the region represented by this sprite.
            // If the region covers the whole texture, return the original texture.
            if (Region.SourceRectangle.X == 0 && Region.SourceRectangle.Y == 0 &&
                Region.SourceRectangle.Width == Region.Texture.Width &&
                Region.SourceRectangle.Height == Region.Texture.Height)
            {
                return Region.Texture;
            }

            // Extract the region as a new Texture2D
            var graphicsDevice = Region.Texture.GraphicsDevice;
            var sourceRect = Region.SourceRectangle;
            Color[] data = new Color[sourceRect.Width * sourceRect.Height];
            Region.Texture.GetData(0, sourceRect, data, 0, data.Length);

            Texture2D regionTexture = new Texture2D(graphicsDevice, sourceRect.Width, sourceRect.Height);
            regionTexture.SetData(data);
            return regionTexture;
        }

        
        public Texture2D GetTextureWithColor()
        {
            if (Region == null || Region.Texture == null)
                throw new InvalidOperationException("Region or region texture is null.");

            // If no color tint, reuse the existing logic which may return the original texture when possible.
            if (this.Color == Color.White)
            {
                return GetTexture();
            }

            var sourceRect = Region.SourceRectangle;
            int width = sourceRect.Width;
            int height = sourceRect.Height;

            // Read the region pixels
            Color[] data = new Color[width * height];
            Region.Texture.GetData(0, sourceRect, data, 0, data.Length);

            // Apply the color mask to each pixel
            byte maskR = this.Color.R;
            byte maskG = this.Color.G;
            byte maskB = this.Color.B;
            byte maskA = this.Color.A;

            for (int i = 0; i < data.Length; i++)
            {
                Color p = data[i];
                // Multiply channels and keep within 0..255 using integer math
                byte r = (byte)((p.R * maskR) / 255);
                byte g = (byte)((p.G * maskG) / 255);
                byte b = (byte)((p.B * maskB) / 255);
                byte a = (byte)((p.A * maskA) / 255);
                data[i] = new Color(r, g, b, a);
            }

            // Create a new texture for the tinted region
            var graphicsDevice = Region.Texture.GraphicsDevice;
            Texture2D tinted = new Texture2D(graphicsDevice, width, height);
            tinted.SetData(data);
            return tinted;
        }

        //Helpermethod for creating a texture of a specified size and color
        public static Sprite CreateTextureSprite(GraphicsDevice device, int width, int height, Color color)
        {
            Texture2D texture = CreateTexture(device, width, height, color);
            TextureRegion region = new TextureRegion(texture, 0, 0, width, height);
            return new Sprite(region);
        }

        private static Texture2D CreateTexture(GraphicsDevice device, int width, int height, Color color)
        {
            //initialize a texture
            Texture2D texture = new Texture2D(device, width, height);

            //the array holds the color for each pixel in the texture
            Color[] data = new Color[width * height];
            for (int pixel = 0; pixel < data.Count(); pixel++)
            {
                //the function applies the color according to the specified pixel
                data[pixel] = color;
            }

            //set the color
            texture.SetData(data);

            return texture;
        }
    }
}
