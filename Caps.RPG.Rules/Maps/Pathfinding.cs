using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Caps.RPG.Rules.Maps
{
    public class Pathfinding
    {
        public static List<NodeBase> FindPath(NodeBase startNode, NodeBase targetNode)
        {
            var toSearch = new List<NodeBase>() { startNode };
            var processed = new List<NodeBase>();

            while (toSearch.Count != 0)
            {
                var current = toSearch[0];
                foreach (var t in toSearch)
                {
                    if (t.F < current.F || (t.F == current.F && t.H < current.H))
                    {
                        current = t;
                    }
                }

                processed.Add(current);
                toSearch.Remove(current);

                if (current == targetNode)
                {
                    var currentrPathTile = targetNode;
                    var path = new List<NodeBase>();
                    // build path walking from target to start
                    while (currentrPathTile != startNode)
                    {
                        path.Add(currentrPathTile);
                        currentrPathTile = currentrPathTile.Connection;
                    }

                    return path;
                }

                foreach (var neighbor in current.Neighbors.Where(t => t.Walkable && !processed.Contains(t)))
                {
                    var inSearch = toSearch.Contains(neighbor);
                    var costToNeighbor = current.G + current.GetDistance(neighbor);

                    if (!inSearch || costToNeighbor < neighbor.G)
                    {
                        neighbor.SetG(costToNeighbor);
                        neighbor.SetConnection(current);

                        if (!inSearch)
                        {
                            neighbor.SetH(neighbor.GetDistance(targetNode));
                            toSearch.Add(neighbor);
                        }
                    }
                }
            }

            return [];
        }
    }
}
