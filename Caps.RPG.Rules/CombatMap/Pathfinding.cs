using Caps.RPG.Rules.Helpers;

namespace Caps.RPG.Rules.CombatMap
{
    public class Pathfinding
    {
        public static List<Vector2D> Search(Vector2D start, Vector2D goal, Map map)
        {
            var openSet = new PriorityQueue<Vector2D, int>();  // Initialize the open set (priority queue) to store nodes to explore, prioritized by their fScore
            var cameFrom = new Dictionary<Vector2D, Vector2D>();  // Dictionary to keep track of the most efficient previous step for each node
            var gScore = new Dictionary<Vector2D, int> { [start] = 0 };  // Dictionary to store the cost of the cheapest path from start to each node
            var fScore = new Dictionary<Vector2D, int> { [start] = (int)start.Distance(goal) };  // Dictionary to store the estimated total cost (gScore + heuristic) from start to goal through each node

            // Add the starting node to the open set with its fScore
            openSet.Enqueue(start, fScore[start]);

            
            while (openSet.Count > 0)  // While there are nodes to explore in the open set
            {
                // Get the node with the lowest fScore from the open set
                var current = openSet.Dequeue();

                // If the current node is the goal, reconstruct and return the path
                if (current == goal)
                {
                    return ReconstructPath(cameFrom, current);
                }

                foreach (var neighbor in map.GetNeighbors(current))  // Iterate through all neighbors of the current node
                {
                    // Calculate the tentative gScore for the neighbor
                    int tentativeGScore = gScore[current] + 1;

                    // If this path to the neighbor is better than any previously recorded path
                    if (!gScore.ContainsKey(neighbor) || tentativeGScore < gScore[neighbor])
                    {
                        cameFrom[neighbor] = current;  // Update the path to the neighbor

                        // Update the gScore and fScore for the neighbor
                        gScore[neighbor] = tentativeGScore;
                        fScore[neighbor] = gScore[neighbor] + (int)neighbor.Distance(goal);

                        // If the neighbor is not already in the open set, add it
                        if (!openSet.UnorderedItems.Any(x => x.Element == neighbor))
                        {
                            openSet.Enqueue(neighbor, fScore[neighbor]);
                        }
                    }
                }
            }

            return [];
        }

        private static List<Vector2D> ReconstructPath(Dictionary<Vector2D, Vector2D> cameFrom, Vector2D current)
        {
            var path = new List<Vector2D> { current };
            while (cameFrom.ContainsKey(current))
            {
                current = cameFrom[current];
                path.Add(current);
            }
            path.Reverse();
            return path;
        }
    }
}
