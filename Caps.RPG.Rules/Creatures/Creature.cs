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
        private int defenseClass;
        private AttributeSet attributes;
        private List<CombatAction> combatActions;

        private Dictionary<Modifier.ModifierTarget, List<Modifier>> modifiers;

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
            get { return defenseClass; }
            set { defenseClass = value; }
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

            modifiers = new Dictionary<Modifier.ModifierTarget, List<Modifier>>();
        }
        #endregion

        public virtual List<CombatAction> GetCombatActions()
        {
            return combatActions;
        }

        #region GenericMethods
        public override string ToString()
        {
            return Name;
        }
        #endregion
    }
}
