using Caps.RPG.Rules.Creatures;
using Caps.RPG.Rules.Helpers;

namespace Caps.RPG.Rules
{
    public class CombatState
    {
        public List<Combattant> Teams { get; internal set; }
        public Combattant[] CombatOrder { get; internal set; }
        public CombatState()
        {
            Teams = [];
            CombatOrder = [];
        }

        public void AddCombattant(string team, Creature creature, Vector2D position)
        {
            Teams.Add(new Combattant(creature, team, position));
        }
        public void RemoveCombattant(Creature creature)
        {
            foreach (Combattant c in Teams)
            {
                if (c.Creature == creature)
                {
                    Teams.Remove(c);
                    break;
                }
            }
        }

        public Combattant[] GetNeighbors(Vector2D source, double distance)
        {
            List<Combattant> neighbors = [];
            foreach (Combattant c in Teams)
            {
                if (c.Position.Distance(source) <= distance)
                {
                    neighbors.Add(c);
                }
            }
            return neighbors.ToArray();
        }

        public bool HasNoVictor()
        {
            string aliveTeamName = string.Empty;
            foreach(Combattant c in Teams)
            {
                if (c.Creature.Status == Creature.HealthStatus.Alive)
                {
                    if (aliveTeamName.Equals(""))
                    {
                        aliveTeamName = c.Team;
                    }
                    if (!aliveTeamName.Equals(c.Team))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public string? GetVictorTeamName()
        {
            return Teams.Where(c => c.Creature.Status == Creature.HealthStatus.Alive).Select(c => c.Team).FirstOrDefault();
        }

        public void BuildCombatOrder()
        {
            Dictionary<Combattant, int> initiative = [];
            foreach (Combattant c in Teams)
            {
                initiative[c] = c.Creature.InitiativeModifier + new Die.DTwenty().Roll();
            }
            CombatOrder = initiative.OrderBy(kv => kv.Value).Reverse().Select(kv => kv.Key).ToArray();
        }

    }
}
