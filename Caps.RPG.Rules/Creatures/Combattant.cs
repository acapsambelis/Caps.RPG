using Caps.RPG.Engine.Modifiers;
using Caps.RPG.Rules.Creatures.Actions;
using Caps.RPG.Rules.Helpers;


namespace Caps.RPG.Rules.Creatures
{
    public class Combattant
    {
        public readonly Creature Creature;
        public string Team;
        public Vector2D Position;
        public readonly char ShortName;

        public Combattant(Creature creature, string team, Vector2D position)
        {
            this.Creature = creature;
            this.Team = team;
            this.Position = position;
            ShortName = creature.Name[0];
        }
        public override string ToString()
        {
            return Team + " - " + Creature.ToString();
        }

        public override bool Equals(object? obj)
        {
            if (obj == null) return false;
            if (obj is Combattant)
            {
                Combattant other = (Combattant)obj;
                return this.ToString().Equals(other.ToString());
            }
            return false;
        }

        public void Move(Vector2D newPosition)
        {
            this.Position = newPosition;
        }

        public static Creature[] GetTeam(string name, Combattant[] creatures)
        {
            return creatures.Where(c => c.Team == name).Select(c => c.Creature).ToArray();
        }

        public override int GetHashCode()
        {
            throw new NotImplementedException();
        }

        public readonly static List<CombatAction> ActionList = new List<CombatAction>()
            {
                new CombatAction("Attack", "Attack one target with a physical attack.", 1, Attack, true, 1),
                new CombatAction("Pass", "Do nothing.", 99, Pass, false, 0),
                new CombatAction("Dash", "Move your speed again.", 1, Dash, false, 0),
            };
        public static List<CombatAction> GetGenericList()
        {
            return ActionList;
        }

        // Generic Actions
        public static void Attack(Creature source, Creature? target)
        {
            if (target != null)
            {
                bool hits = source.AttackBonus + new Die.DTwenty().Roll() > target.DefenseClass;
                if (hits)
                {
                    target.Health -= Modifier.SumAll(source.Modifiers[Modifier.TargetType.AttackDamage], source.Attributes);
                }
            }
        }

        public static void Pass(Creature source, Creature? target)
        {

        }

        public static void Dash(Creature source, Creature? target)
        {

        }
    }
}
