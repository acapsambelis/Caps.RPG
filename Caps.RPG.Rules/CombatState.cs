using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caps.RPG.CombatEngine.Creatures;

namespace Caps.RPG.CombatEngine
{
    public class CombatState
    {
        public class TeamedCreature
        {
            public readonly Creature Creature;
            public string Team;

            public TeamedCreature(Creature creature, string team)
            {
                this.Creature = creature;
                this.Team = team;
            }
            public override string ToString()
            {
                return Team + " - " + Creature.ToString();
            }

            public static Creature[] GetTeam(string name, TeamedCreature[] creatures)
            {
                return creatures.Where(c => c.Team == name).Select(c => c.Creature).ToArray();
            }
        }

        public List<TeamedCreature> Teams { get; internal set; }
        public TeamedCreature[] CombatOrder { get; internal set; }
        public CombatState()
        {
            Teams = new List<TeamedCreature>();
            CombatOrder = Array.Empty<TeamedCreature>();
        }

        public void AddCombattant(string team, Creature creature)
        {
            Teams.Add(new TeamedCreature(creature, team));
        }
        public void RemoveCombattant(Creature creature)
        {
            foreach (TeamedCreature c in Teams)
            {
                if (c.Creature == creature)
                {
                    Teams.Remove(c);
                    break;
                }
            }
        }

        public bool HasNoVictor()
        {
            string aliveTeamName = string.Empty;
            foreach(TeamedCreature c in Teams)
            {
                if (c.Creature.Status == Creature.CombatStatus.Alive)
                {
                    if (aliveTeamName.Equals(string.Empty))
                    {
                        aliveTeamName = c.Team;
                    }
                    if (!aliveTeamName.Equals(c.Team))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public string? GetVictorTeamName()
        {
            return Teams.Where(c => c.Creature.Status == Creature.CombatStatus.Alive).Select(c => c.Team).FirstOrDefault();
        }

        public void BuildCombatOrder()
        {
            Dictionary<TeamedCreature, int> initiative = new Dictionary<TeamedCreature, int>();
            foreach (TeamedCreature c in Teams)
            {
                initiative[c] = c.Creature.Initiative;
            }
            CombatOrder = initiative.OrderBy(kv => kv.Value).Reverse().Select(kv => kv.Key).ToArray();
        }

    }
}
