using Caps.RPG.Rules.Attributes;
using Caps.RPG.Rules.Creatures.Actions;
using Caps.RPG.Engine.Modifiers;
using Caps.RPG.Rules.Inventory;

namespace Caps.RPG.Rules.Creatures
{
    public class Creature
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
        private Dictionary<Modifier.TargetType, List<Modifier>> modifiers;
        private int defenseClass;
        private int attackBonus;

        #endregion

        #region PublicMembers
        // Basics
        public string Name
        {
            get { return name; }
            set { name = value; }
        }
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
        public AttributeSet Attributes
        {
            get { return attributes; }
            set { attributes = value; }
        }

        // Combat
        public HealthStatus Status
        {
            get { return status; }
            set { status = value; }
        }
        public int DefenseClass
        {
            get
            {
                if (defenseClassChanged)
                {
                    this.defenseClass = Modifier.SumAll(this.modifiers[Modifier.TargetType.DefenseClass], this.Attributes);
                    this.defenseClassChanged = false;
                }
                return this.defenseClass;
            }
        }
        public int AttackBonus
        {
            get
            {
                if (attackBonusChanged)
                {
                    this.attackBonus = Modifier.SumAll(this.modifiers[Modifier.TargetType.AttackBonus], this.Attributes);
                    this.attackBonusChanged = false;
                }
                return this.attackBonus;
            }
        }
        public int Initiative
        {
            get { return new Random().Next(1, 21) + Attributes.InitiativeModifier(); }
        }
        public int MoveSpeed
        {
            get { return Attributes.MoveSpeed(); }
        }

        // Inventory
        public CreatureInventory Inventory
        {
            get { return inv; }
        }

        // Modifiers
        public Dictionary<Modifier.TargetType, List<Modifier>> Modifiers
        {
            get { return modifiers; }
        }
        #endregion

        #region Constructors
        public Creature(string name, AttributeSet attributes)
        {
            this.status = HealthStatus.Alive;
            this.name = name;
            this.attributes = attributes;
            this.maxHealth = attributes.GetMaxHealth();
            this.health = MaxHealth;
            this.combatActions = Combattant.GetGenericList();

            this.inv = new CreatureInventory(this);
            modifiers = Modifier.GetCreatureModifiers();
        }
        #endregion

        public virtual List<CombatAction> GetCombatActions()
        {
            return combatActions;
        }

        public void AddModifier(Modifier modifier, object? source)
        {
            if (modifier.Target == Modifier.TargetType.DefenseClass)
            {
                this.defenseClassChanged = true;
            }
            if (modifier.Target == Modifier.TargetType.AttackBonus)
            {
                this.attackBonusChanged = true;
            }

            List<Modifier> targetList = modifiers[modifier.Target];
            if (source is Item)
            {
                Item s = (Item)source;
                foreach (var mod in targetList)
                {
                    if (mod.Source is Item && ((Item)mod.Source).Type == s.Type)
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
        #endregion
    }
}
