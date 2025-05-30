using Caps.RPG.Rules.Helpers;

namespace Caps.RPG.Rules.CombatMap
{
    public class MapTile
    {
        private int x;
        private int y;
        private List<object> contents;

        public int X
        {
            get { return x; }
            set { x = value; }
        }
        public int Y
        {
            get { return y; }
            set { y = value; }
        }

        public Vector2D Position
        {
            get { return new Vector2D(x, y); }
        }

        public List<object> Contents
        {
            get { return contents; }
            set { contents = value; }
        }

        public MapTile()
        {
            contents = [];
        }

        public void AddContent(object content)
        {
            contents.Add(content);
        }

        public void RemoveContent(object content)
        {
            contents.Remove(content);
        }
    }
}
