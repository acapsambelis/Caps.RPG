using Caps.RPG.Rules.Modifiers;
using Caps.RPG.Rules.Creatures.Actions;
using Caps.RPG.Rules.Helpers;
using Caps.RPG.Rules.Maps;
using Caps.Util;
using SNS.Data.DataSerializer;
using Caps.RPG.Rules.Creatures.Unclassed;

namespace Caps.RPG.Rules.Creatures
{
    [DataClass("Combattants")]
    public class Combattant : TileFeature, IGenericDataObject<Combattant>
    {
        private Creature _creature;
        private string _team;
        private TileBase _position;
        private bool _wasLoaded = false;

        public event EventHandler<PositionChangedEventArgs>? OnPositionChanged;
        public event EventHandler<HealthChangedEventArgs>? OnHealthChanged;
        public event EventHandler? OnUnconsious;

        [SubDataObject("Creature")]
        public Creature Creature { get { return _creature; } set { _creature = value; } }
        [DataProperty("Team")]
        public string Team { get { return _team; } set { _team = value; } }
        [SubDataObject("Position")]
        public TileBase Position
        {
            get { return _position; }
            set
            {
                var eventArgs = new PositionChangedEventArgs(_position);
                _position = value;
                OnPositionChanged?.Invoke(this, eventArgs);
            }
        }
        public int ActionCounts { get => 3; }

        public bool WasLoaded { get { return _wasLoaded; } set { _wasLoaded = value; } }

        public new string Name
        {
            get { return Creature.Name; }
        }

        public int Health
        {
            get { return Creature.Health; }
            set {
                int delta = value - Creature.Health;
                Creature.Health = value;
                OnHealthChanged?.Invoke(this, new HealthChangedEventArgs(delta));
                if (Creature.Status == Creature.HealthStatus.Unconsious)
                {
                    OnUnconsious?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public int MoveSpeed
        {
            get { return Creature.MoveSpeed; }
        }

        public Combattant() : base(" ", true) { }

        public Combattant(Creature creature, string team, TileBase position) : base(creature.Name, false)
        {
            _creature = creature;
            _team = team;
            _position = position;
            position.Features.Add(0, this);
        }

        public List<CombatAction> GetCombatActions()
        {
            return Creature.GetCombatActions();
        }

        public void Move(TileBase newPosition)
        {
            var feature = _position.Features.Where(f => f.Value is Combattant comb && comb == this).FirstOrDefault();
            if (feature.Value != null)
            {
                _position.Features.Remove(feature.Key);
            }
            Position = newPosition;
            newPosition.Features.Add(feature.Key, this);
        }

        public bool IsComputerControlled()
        {
            return _creature is IComputerControlled;
        }

        public void HealMax()
        {
            Health = Creature.MaxHealth;
        }

        public static Creature[] GetTeam(string name, Combattant[] creatures)
        {
            return creatures.Where(c => c.Team == name).Select(c => c.Creature).ToArray();
        }

        #region Actions

        public readonly static List<CombatAction> ActionList =
        [
            new CombatAction(
                name:        "Attack",
                description: "Attack one target with a physical attack.",
                cost:        1,
                action:      Attack,
                setup:       new ActionSetup(
                    needsSource: true,
                    sourcerange: 1,
                    sourcetype: ActionSetup.TargetType.SingleCreature
                )
                {
                    Tags = [ActionSetup.ActionTags.Attack]
                }
            ),
            new CombatAction(
                name:        "Move",
                description: "Move your speed.",
                cost:        1,
                action:      Move,
                setup:       new ActionSetup(
                    needsSource:         true,
                    sourceRangeProperty: typeof(Creature).GetProperty(nameof(Creature.MoveSpeed)),
                    sourcetype:          ActionSetup.TargetType.SingleTile,
                    mapShape:            MapShape.Tile,
                    needsEmptyTile:      true
                )
                {
                    Tags = [ActionSetup.ActionTags.Movement]
                }
            ),
        ];
        public static List<CombatAction> GetGenericList()
        {
            return ActionList;
        }

        // Generic Actions
        public static ActionResult Attack(Combattant source, TileBase[] targets)
        {
            if (targets.Length == 1)
            {
                Combattant creature = targets[0].GetFeature<Combattant>();
                int damage = 0;
                int toHit = Die.D20.Roll();
                bool hits = source.Creature.AttackBonus + toHit > creature.Creature.DefenseClass;
                if (hits)
                {
                    damage = Modifier.SumAll(source.Creature.Modifiers[ModifiedValue.AttackDamage], source.Creature.Attributes);
                    creature.Health -= damage;
                }
                return new ActionResult(source.Name + " attacked " + creature.Name + " with a " + (source.Creature.AttackBonus + toHit) + "(" + toHit + " + " + source.Creature.AttackBonus + ") to hit. " + damage + " was delt.");
            }
            return new ActionResult(source.Name + " attacked an invalid target");
        }

        public static ActionResult Move(Combattant source, TileBase[] targets)
        {
            if (targets.Length == 1)
            {
                source.Move(targets[0]);
                return new ActionResult(source.Name + " moved to " + source.Position.ToString());
            }
            throw new Exception("Invalid target format");
        }

        #endregion

        #region GenericMethods

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

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        #endregion
    }

    public class PositionChangedEventArgs : EventArgs
    {
        public TileBase? OldPosition { get; }

        public PositionChangedEventArgs(TileBase? oldPosition)
        {
            OldPosition = oldPosition;
        }
    }

    public class HealthChangedEventArgs : EventArgs
    {
        public int? HealthDelta { get; }

        public HealthChangedEventArgs(int? healthDelta)
        {
            HealthDelta = healthDelta;
        }
    }
}
