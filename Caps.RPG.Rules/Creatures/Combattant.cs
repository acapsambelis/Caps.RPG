using Caps.RPG.Rules.Modifiers;
using Caps.RPG.Rules.Creatures.Actions;
using Caps.RPG.Rules.Helpers;
using SNS.Data.DataSerializer;
using Caps.RPG.Rules.Maps;


namespace Caps.RPG.Rules.Creatures
{
    [DataClass("Combattants")]
    public class Combattant : TileFeature, IGenericDataObject<Combattant>
    {
        private Creature _creature;
        private MapColor _team;
        private TileBase position;
        public readonly char ShortName;
        public TileMap fieldOfView;
        public TileMap fullMap;
        private bool _wasLoaded = false;

        [SubDataObject("Creature")]
        public Creature Creature { get { return _creature; } set { _creature = value; } }
        [SubDataObject("Team")]
        public MapColor Team { get { return _team; } set { _team = value; } }
        [SubDataObject("Position")]
        public TileBase Position
        {
            get { return position; }
            set
            {
                //if (position != null)
                //{
                //    fieldOfView = fullMap.GetLineOfSight(position, _creature.SightRange);
                //    fieldOfView[value.IntX, value.IntY].AddContent(this);
                //    fieldOfView = fullMap.GetVisionRange(Position, Creature.VisionRange);
                //}
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

        public Combattant() : base(" ", true, MapColors.Gray) { }

        public Combattant(Creature creature, MapColor team, TileBase position) : base(creature.Name, false, team)
        {
            Creature = creature;
            Team = team;
            Position = position;
            position.Features.Add(0, this);

            ShortName = creature.Name[0];
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

        public void Move(TileBase newPosition)
        {
            var oldPosition = position.Features.Where(f => f.Value is Combattant comb && comb == this).FirstOrDefault();
            if (oldPosition.Value != null)
            {
                position.Features.Remove(oldPosition.Key);
            }
            Position = newPosition;
            newPosition.Features.Add(oldPosition.Key, this);
        }

        public static Creature[] GetTeam(MapColor name, Combattant[] creatures)
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
        public static ActionResult Attack(Combattant source, TileBase? target = null)
        {
            if (target != null)
            {
                Combattant creature = (Combattant)target.Features.Values.Where(f => f is Combattant);
                int damage = 0;
                int toHit = Die.D20.Roll();
                bool hits = source.Creature.AttackBonus + toHit > creature.Creature.DefenseClass;
                if (hits)
                {
                    damage = Modifier.SumAll(source.Creature.Modifiers[TargetType.AttackDamage], source.Creature.Attributes);
                    creature.Health -= damage;
                }
                return new ActionResult(source.Name + " attacked " + creature.Name + " with a " + (source.Creature.AttackBonus + toHit) + "(" + toHit + " + " + source.Creature.AttackBonus + ") to hit. " + damage + " was delt.");
            }
            return new ActionResult(source.Name + " attacked an invalid target");
        }

        public static ActionResult Pass(Combattant source, TileBase? target = null)
        {
            return new ActionResult();
        }

        public static ActionResult Move(Combattant source, TileBase? target = null)
        {
            if (target != null)
            {
                source.Move(target);
            }

            return new ActionResult(source.Name + " moved to " + source.Position.ToString());
        }
    }
}
