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
        private bool maxHealthChanged = true;
        private int maxHealth;
        private int health;
        private AttributeSet attributes;
        private CreatureType creatureType;

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
        private Dictionary<ModifiedValue, List<Modifier>> modifiers = [];
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
        public int MaxHealth
        {
            get => GetModifierValue(ModifiedValue.MaxHealth, ref maxHealthChanged, ref maxHealth) + attributes.SumModifiers(Stat.Constitution) * 10 + 10;
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
                if (health > MaxHealth)
                {
                    health = MaxHealth;
                }
            }
        }
        [DataProperty("VisionRange")]
        public int SightRange
        {
            get { return 5; }
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
            get => GetModifierValue(ModifiedValue.DefenseClass, ref defenseClassChanged, ref defenseClass);
        }

        public int AttackBonus
        {
            get => GetModifierValue(ModifiedValue.AttackBonus, ref attackBonusChanged, ref attackBonus) ;
        }

        public int InitiativeModifier
        {
            get => GetModifierValue(ModifiedValue.Initiative, ref initiativeChanged, ref initiativeBonus) + Attributes.SumModifiers(Stat.Agility);
        }

        public int MoveSpeed
        {
            get => GetModifierValue(ModifiedValue.MovementSpeed, ref moveSpeedChanged, ref moveSpeed) + 5 + (Attributes.SumModifiers(Stat.Agility) / 2);
        }
        
        private int GetModifierValue(ModifiedValue targetType, ref bool changedFlag, ref int cachedValue)
        {
            if (changedFlag)
            {
                UpdateAllModifiers();
                if (!modifiers.TryGetValue(targetType, out List<Modifier>? value))
                    cachedValue = 0;
                else
                    cachedValue = Modifier.SumAll(value, this.Attributes);
                changedFlag = false;
            }
            return cachedValue;
        }

        [SubDataObject("CreatureType")]
        public CreatureType CreatureType
        {
            get { return creatureType; }
            set {
                if (creatureType != null) throw new Exception("CreatureType cannot be changed once set.");
                creatureType = value;
                foreach (var mod in creatureType.Modifiers)
                {
                    AddModifier(mod, mod.Source);
                }
                foreach (var action in creatureType.CombatActions)
                {
                    if (!combatActions.Contains(action))
                    {
                        combatActions.Add(action);
                    }
                }
            }
        }

        [SubDataObject("Inventory")]
        public CreatureInventory Inventory
        {
            get { return inv; }
            set { inv = value; }
        }

        [DataProperty("Modifiers")]
        public Dictionary<ModifiedValue, List<Modifier>> Modifiers
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
        public Creature()
        {
            name = "";
            creatureType = null;
            attributes = new AttributeSet();
            combatActions = Combattant.GetGenericList();
            inv = new CreatureInventory();
            modifiers = Modifier.GetCreatureModifiers();
            status = HealthStatus.Alive;
        }
        public Creature(string name, CreatureType creatureType, AttributeSet attributes)
        {
            this.inv = new CreatureInventory();
            this.status = HealthStatus.Alive;
            this.name = name;
            this.creatureType = creatureType;
            this.attributes = attributes;
            this.health = MaxHealth;
            this.combatActions = Combattant.GetGenericList();

            modifiers = Modifier.GetCreatureModifiers();
        }
        #endregion

        public virtual List<CombatAction> GetCombatActions()
        {
            return [.. combatActions, .. Inventory.GetCombatActions()];
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
                case ModifiedValue.Strength:
                case ModifiedValue.Agility:
                case ModifiedValue.Constitution:
                case ModifiedValue.Intellect:
                case ModifiedValue.Arcana:
                case ModifiedValue.Wisdom:
                case ModifiedValue.Charisma:
                case ModifiedValue.Presence:
                    break;
                case ModifiedValue.DefenseClass:
                    this.defenseClassChanged = true;
                    break;
                case ModifiedValue.AttackBonus:
                    this.attackBonusChanged = true;
                    break;
                case ModifiedValue.Initiative:
                    this.initiativeChanged = true;
                    break;
                case ModifiedValue.MovementSpeed:
                    this.moveSpeedChanged = true;
                    break;
                default:
                    break;
            }
            if (!modifiers.ContainsKey(modifier.Target))
                modifiers[modifier.Target] = [];
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

        private void UpdateAllModifiers()
        {
            if (Inventory.EquipmentChanged)
            {
                var inventoryModifiers = Inventory.GetModifiers();
                foreach (var kvp in inventoryModifiers)
                {
                    foreach (var mod in kvp.Value)
                    {
                        AddModifier(mod, mod.Source);
                    }
                }
                Inventory.EquipmentChanged = false;
            }
            
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

        public virtual string Description(bool full=false)
        {
            return $"{Name}: A creature {Name}.";
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
