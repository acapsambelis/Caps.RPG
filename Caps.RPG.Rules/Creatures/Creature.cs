using Caps.RPG.Rules.Attributes;
using Caps.RPG.Rules.Creatures.Actions;
using Caps.RPG.Rules.Modifiers;
using Caps.RPG.Rules.Inventory;
using SNS.Data.DataSerializer;

namespace Caps.RPG.Rules.Creatures
{
    [DataClass("Creatures")]
    public class Creature : IGenericDataObject<Creature>
    {
        public enum HealthStatus
        {
            None,
            Alive,
            Dead,
            Unconsious
        }

        #region privateMembers

        // Basics
        private string name;
        private int maxHealth;
        private int health;
        private AttributeSet attributes;

        // Combat
        private HealthStatus status;
        private List<CombatAction> combatActions;

        // Inventory
        private CreatureInventory inv;

        // Modifiers
        private bool defenseClassChanged = true;
        private bool attackBonusChanged = true;
        private bool initiativeChanged = true;
        private bool moveSpeedChanged = true;
        private Dictionary<TargetType, List<Modifier>> modifiers;
        private int defenseClass;
        private int attackBonus;
        private int initiativeBonus;
        private int moveSpeed;

        private bool _wasLoaded = false;
        #endregion

        #region PublicMembers
        // Basics
        [DataProperty("Name")]
        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        [DataProperty("MaxHealth")]
        public int MaxHealth
        {
            get { return maxHealth; }
            set { maxHealth = value; }
        }
        public int Health
        {
            get { return health; }
            set
            { 
                health = value;
                if (health <= 0)
                {
                    health = 0;
                    status = HealthStatus.Unconsious;
                }
                if (health > maxHealth)
                {
                    health = maxHealth;
                }
            }
        }

        [SubDataObject("Attributes")]
        public AttributeSet Attributes
        {
            get { return attributes; }
            set { attributes = value; }
        }

        // Combat
        [DataProperty("Status")]
        public HealthStatus Status
        {
            get { return status; }
            set { status = value; }
        }
        public int DefenseClass
        {
            get
            {
                if (defenseClassChanged && modifiers.TryGetValue(TargetType.DefenseClass, out List<Modifier>? value))
                {
                    this.defenseClass = Modifier.SumAll(value, Attributes);
                    this.defenseClassChanged = false;
                }
                return this.defenseClass;
            }
        }
        public int AttackBonus
        {
            get
            {
                if (attackBonusChanged && modifiers.TryGetValue(TargetType.AttackBonus, out List<Modifier>? value))
                {
                    this.attackBonus = Modifier.SumAll(value, this.Attributes);
                    this.attackBonusChanged = false;
                }
                return this.attackBonus;
            }
        }
        public int InitiativeModifier
        {
            get {
                if (initiativeChanged && modifiers.TryGetValue(TargetType.Initiative, out List<Modifier>? value))
                {
                    this.initiativeBonus = Modifier.SumAll(value, Attributes);
                    this.initiativeChanged = false;
                }
                return this.initiativeBonus + Attributes.InitiativeModifier();
            }
        }
        public int MoveSpeed
        {
            get
            {
                if (moveSpeedChanged && modifiers.TryGetValue(TargetType.MovementSpeed, out List<Modifier>? value))
                {
                    this.moveSpeed = Modifier.SumAll(value, Attributes);
                    this.moveSpeedChanged = false;
                }
                return this.moveSpeed + Attributes.MoveSpeed();
            }
        }

        [SubDataObject("Inventory")]
        public CreatureInventory Inventory
        {
            get { return inv; }
            set { inv = value; }
        }

        [DataProperty("Modifiers")]
        public Dictionary<TargetType, List<Modifier>> Modifiers
        {
            get { return modifiers; }
            set { modifiers = value; }
        }
        [DataProperty("CombatActions")]
        public List<CombatAction> CombatActions
        {
            get { return combatActions; }
            set { combatActions = value; }
        }

        public bool WasLoaded { get { return _wasLoaded; } set { _wasLoaded = value; } }
        #endregion

        #region Constructors
        public Creature() { }
        public Creature(string name, AttributeSet attributes)
        {
            this.status = HealthStatus.Alive;
            this.name = name;
            this.attributes = attributes;
            this.maxHealth = attributes.GetMaxHealth();
            this.health = MaxHealth;
            this.combatActions = Combattant.GetGenericList();

            this.inv = new CreatureInventory();
            modifiers = Modifier.GetCreatureModifiers();
        }
        #endregion

        public virtual List<CombatAction> GetCombatActions()
        {
            return combatActions;
        }

        public void Equip(Item item)
        {
            Inventory.Equip(item);
            if (item is not null)
            {
                foreach (Modifier m in item.Modifiers)
                {
                    AddModifier(m, item);
                }
            }
        }

        public void AddModifier(Modifier modifier, object? source)
        {
            switch (modifier.Target)
            {
                case TargetType.Strength:
                case TargetType.Agility:
                case TargetType.Constitution:
                case TargetType.Intellect:
                case TargetType.Arcana:
                case TargetType.Wisdom:
                case TargetType.Charisma:
                case TargetType.Presence:
                    this.attributes.AddModifier(modifier, source);
                    return;
                case TargetType.DefenseClass:
                    this.defenseClassChanged = true;
                    break;
                case TargetType.AttackBonus:
                    this.attackBonusChanged = true;
                    break;
                case TargetType.Initiative:
                    this.initiativeChanged = true;
                    break;
                case TargetType.MovementSpeed:
                    this.moveSpeedChanged = true;
                    break;
                default:
                    break;
            }

            List<Modifier> targetList = modifiers[modifier.Target];
            if (source is Item s)
            {
                foreach (var mod in targetList)
                {
                    if (mod.Source.IsItem() && mod.Source.ItemType() == s.Type)
                    {
                        targetList.Remove(mod);
                        break;
                    }
                }
            }

            modifiers[modifier.Target].Add(modifier);
        }

        #region GenericMethods
        public override string ToString()
        {
            return Name;
        }

        public override bool Equals(object? obj)
        {
            if (obj is not Creature creature)
                return false;

            bool isEqual = true;
            isEqual &= name == creature.name;
            isEqual &= maxHealth == creature.maxHealth;
            isEqual &= health == creature.health;
            isEqual &= EqualityComparer<AttributeSet>.Default.Equals(attributes, creature.attributes);
            isEqual &= status == creature.status;
            foreach (CombatAction ca in combatActions)
            {
                if (!creature.combatActions.Contains(ca))
                {
                    return false;
                }
            }
            //isEqual &= EqualityComparer<List<CombatAction>>.Default.Equals(combatActions, creature.combatActions);
            isEqual &= inv == creature.inv;
            isEqual &= Modifiers.Count == creature.Modifiers.Count;
            // Check if all keys and their corresponding values are equal
            foreach (var key in Modifiers.Keys)
            {
                if (!creature.Modifiers.TryGetValue(key, out List<Modifier>? value))
                {
                    return false;
                }

                // Compare the lists of modifiers for each key
                var thisModifiers = value;
                var otherModifiers = creature.Modifiers[key];

                if (thisModifiers.Count != otherModifiers.Count ||
                    !thisModifiers.SequenceEqual(otherModifiers))
                {
                    return false;
                }
            }

            isEqual &= defenseClass == creature.defenseClass;
            isEqual &= attackBonus == creature.attackBonus;
            isEqual &= initiativeBonus == creature.initiativeBonus;
            isEqual &= moveSpeed == creature.moveSpeed;

            return isEqual;
        }

        public override int GetHashCode()
        {
            HashCode hash = new HashCode();
            hash.Add(name);
            hash.Add(maxHealth);
            hash.Add(health);
            hash.Add(attributes);
            hash.Add(status);
            hash.Add(combatActions);
            hash.Add(inv);
            hash.Add(modifiers);
            hash.Add(defenseClass);
            hash.Add(attackBonus);
            hash.Add(initiativeBonus);
            hash.Add(moveSpeed);
            return hash.ToHashCode();
        }

        public static bool operator ==(Creature? left, Creature? right)
        {
            return EqualityComparer<Creature>.Default.Equals(left, right);
        }

        public static bool operator !=(Creature? left, Creature? right)
        {
            return !(left == right);
        }

        #endregion
    }
}
