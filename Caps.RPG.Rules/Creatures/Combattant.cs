using Caps.RPG.Rules.Modifiers;
using Caps.RPG.Rules.CombatMap;
using Caps.RPG.Rules.Creatures.Actions;
using Caps.RPG.Rules.Helpers;
using SNS.Data.DataSerializer;


namespace Caps.RPG.Rules.Creatures
{
    [DataClass("Combattants")]
    public class Combattant : IGenericDataObject<Combattant>
    {
        private Creature _creature;
        private string _team;
        private Vector2D position;
        public readonly char ShortName;
        public MapTile[,] fieldOfView;
        public Map fullMap;
        private bool _wasLoaded = false;

        [SubDataObject("Creature")]
        public Creature Creature { get { return _creature; } set { _creature = value; } }
        [DataProperty("Team")]
        public string Team { get { return _team; } set { _team = value; } }
        [SubDataObject("Position")]
        public Vector2D Position
        {
            get { return position; }
            set
            {
                if (position != null)
                {
                    fieldOfView[position.IntX, position.IntY].RemoveContent(this);
                    fieldOfView[value.IntX, value.IntY].AddContent(this);
                    fieldOfView = fullMap.GetVisionRange(Position, Creature.VisionRange);
                }
                position = value;
            }
        }

        public bool WasLoaded { get { return _wasLoaded; } set { _wasLoaded = value; } }

        public string Name
        {
            get { return Creature.Name; }
        }

        public int Health
        {
            get { return Creature.Health; }
            set { Creature.Health = value; }
        }

        public Combattant() { }
        
        public Combattant(Creature creature, string team, Vector2D position, ref Map fullMap)
        {
            Creature = creature;
            Team = team;
            Position = position;
            
            ShortName = creature.Name[0];
            this.fullMap = fullMap;
        }

        public List<CombatAction> GetCombatActions()
        {
            return Creature.GetCombatActions();
        }

        public override string ToString()
        {
            return Team + " - " + Creature.ToString();
        }

        public override bool Equals(object? obj)
        {
            if (obj == null) return false;
            if (obj is Combattant other)
            {
                return this.ToString().Equals(other.ToString());
            }
            return false;
        }

        public void Move(Vector2D newPosition)
        {
            Position = newPosition;
        }

        public static Creature[] GetTeam(string name, Combattant[] creatures)
        {
            return creatures.Where(c => c.Team == name).Select(c => c.Creature).ToArray();
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public readonly static List<CombatAction> ActionList =
            [
                new CombatAction("Attack", "Attack one target with a physical attack.", 1, Attack, true, false, 1),
                new CombatAction("Pass", "Do nothing.", 99, Pass, false, false, 0),
                new CombatAction("Move", "Move your speed.", 1, Move, false, true, -1),
            ];
        public static List<CombatAction> GetGenericList()
        {
            return ActionList;
        }

        // Generic Actions
        public static ActionResult Attack(Combattant source, Combattant? target = null, Vector2D? location = null)
        {
            if (target != null)
            {
                int damage = 0;
                int toHit = Die.D20.Roll();
                bool hits = source.Creature.AttackBonus + toHit > target.Creature.DefenseClass;
                if (hits)
                {
                    damage = Modifier.SumAll(source.Creature.Modifiers[TargetType.AttackDamage], source.Creature.Attributes);
                    target.Health -= damage;
                }
                return new ActionResult(source.Name + " attacked " + target.Name + " with a " + (source.Creature.AttackBonus + toHit) + "(" + toHit + " + " + source.Creature.AttackBonus + ") to hit. " + damage + " was delt.");
            }
            return new ActionResult(source.Name + " attacked an invalid target");
        }

        public static ActionResult Pass(Combattant source, Combattant? target = null, Vector2D? location = null)
        {
            return new ActionResult();
        }

        public static ActionResult Move(Combattant source, Combattant? target = null, Vector2D? location = null)
        {
            if (location != null)
            {
                // Use the pathfinding algorithm to calculate the path
                List<Vector2D> path = Pathfinding.Search(source.Position, location, source.fullMap);

                if (path == null || path.Count == 0)
                {
                    return new ActionResult(source.Name + " could not find a path to the destination.");
                }

                int remainingMovement = source.Creature.MoveSpeed;

                // Follow the path step by step
                foreach (var step in path)
                {
                    if (remainingMovement <= 0)
                        break;

                    // Move to the next step in the path
                    source.Position = step;
                    remainingMovement--;
                }
            }

            return new ActionResult(source.Name + " moved to " + source.Position.ToString());
        }
    }
}
