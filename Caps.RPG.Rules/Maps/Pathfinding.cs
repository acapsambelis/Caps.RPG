using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Caps.RPG.Rules.Maps
{
    public class Pathfinding
    {
        public static List<TileBase> FindPathToEmpty(TileBase startNode, TileBase targetNode)
        {
            var toSearch = new List<TileBase>() { startNode };
            var processed = new List<TileBase>();

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
                    var path = new List<TileBase>();
                    // build path walking from target to start
                    while (currentrPathTile != startNode)
                    {
                        path.Add(currentrPathTile);
                        currentrPathTile = currentrPathTile.Connection;
                    }
                    path.Add(startNode);
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

        public static List<TileBase> FindPathToFilled(TileBase startNode, TileBase targetNode)
        {
            List<TileBase> shortestPath = null;
            float shortestLength = float.MaxValue;

            foreach (var neighbor in targetNode.Neighbors.Where(t => t.Walkable))
            {
                var path = FindPathToEmpty(startNode, neighbor);
                if (path != null && path.Count > 0)
                {
                    if (path.Count < shortestLength)
                    {
                        shortestLength = path.Count;
                        shortestPath = [.. path];
                    }
                }
            }

            if (shortestPath != null)
            {
                shortestPath.Reverse();
                return shortestPath;
            }

            return [];
        }

        public static List<TileBase> FindStraightline(TileBase startNode, TileBase targetNode, TileMap map)
        {
            return startNode.GetLineTo(targetNode, map);
        }
    }
}
