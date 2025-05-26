using Microsoft.Xna.Framework.Graphics;

namespace Caps.RPG.MonoGame.Graphics
{
    public class CustomFont
    {
        public readonly string Filename;
        public readonly SpriteFont SpriteFont;

        public CustomFont(string filename)
        {
            Filename = filename;
            SpriteFont = Core.Content.Load<SpriteFont>(filename);
        }
    }
}
