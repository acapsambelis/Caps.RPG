using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caps.RPG.CombatEngine.Attributes;
using Caps.RPG.CombatEngine.Creatures.Actions;
using Caps.RPG.Engine.Modifiers;

namespace Caps.RPG.CombatEngine.Creatures
{
    public class Creature
    {
        public enum CombatStatus
        {
            None,
            Alive,
            Dead,
            Unconsious
        }

        #region privateMembers
        private string name;
        private CombatStatus status;
        private int maxHealth;
        private int health;
        private AttributeSet attributes;
        private List<CombatAction> combatActions;


        private bool defenseClassChanged = true;
        private bool attackBonusChanged = true;
        private Dictionary<Modifier.TargetType, List<Modifier>> modifiers;
        private int defenseClass;
        private int attackBonus;

        #endregion

        #region PublicMembers
        public string Name
        {
            get { return name; }
            set { name = value; }
        }
        public CombatStatus Status
        {
            get { return status; }
            set { status = value; }
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
                    status = CombatStatus.Unconsious;
                }
                if (health > maxHealth)
                {
                    health = maxHealth;
                }
            }
        }
        public int DefenseClass
        {
            get
            {
                if (defenseClassChanged)
                {
                    this.defenseClass = Modifier.SumModifierFlat(this.modifiers[Modifier.TargetType.DefenseClass]);
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
                    this.attackBonus = Modifier.SumModifierFlat(this.modifiers[Modifier.TargetType.AttackBonus]);
                    this.attackBonusChanged = false;
                }
                return this.attackBonus;
            }
        }
        public AttributeSet Attributes
        {
            get { return attributes; }
            set { attributes = value; }
        }
        public int Initiative
        {
            get { return new Random().Next(1, 21) + Attributes.InitiativeModifier(); }
        }

        public Dictionary<Modifier.TargetType, List<Modifier>> Modifiers
        {
            get { return modifiers; }
        }
        #endregion

        #region Constructors
        public Creature(string name, AttributeSet attributes)
        {
            this.status = CombatStatus.None;
            this.name = name;
            this.attributes = attributes;
            this.maxHealth = attributes.GetMaxHealth();
            this.health = MaxHealth;
            this.combatActions = CombatAction.GetGenericList();

            modifiers = Modifier.CreatureModifiers;
        }
        #endregion

        public virtual List<CombatAction> GetCombatActions()
        {
            return combatActions;
        }

        public void AddModifier(Modifier modifier)
        {
            if (modifier.Target == Modifier.TargetType.DefenseClass)
            {
                this.defenseClassChanged = true;
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
