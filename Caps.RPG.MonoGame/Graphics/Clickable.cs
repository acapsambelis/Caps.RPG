using Caps.RPG.MonoGame.Scenes;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Caps.RPG.MonoGame.Graphics
{
    public abstract class Clickable
    {
        public event EventHandler OnClick;

        public abstract bool IsInside(Vector2 point);
        public void FireClick(EventArgs empty)
        {
            OnClick?.Invoke(this, empty);
        }
    }
}
