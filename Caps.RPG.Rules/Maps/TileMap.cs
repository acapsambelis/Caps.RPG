using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Caps.RPG.Rules.Maps
{
    public abstract class TileMap
    {
        public readonly Dictionary<Helpers.Vector2D, NodeBase> Tiles = [];

        public NodeBase RandomTile()
        {
            var random = new Random();
            var randomIndex = random.Next(Tiles.Count);
            return Tiles.Values.ElementAt(randomIndex);
        }

        public abstract void PrintToConsole();
        public abstract void PrintWithPath(List<NodeBase> path);
    }
}
