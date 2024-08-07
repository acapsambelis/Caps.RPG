using Caps.RPG.Engine.Modifiers;
using Caps.RPG.Rules.Helpers;


namespace Caps.RPG.Rules.Creatures.Actions
{
    public class CombatAction
    {
        #region privateMembers
        private string name;
        private string description;
        private int cost;
        private Action<Creature, Creature?> action;
        private bool needsTarget;
        #endregion

        #region PublicMembers
        public string Name
        {
            get { return name; }
            set { name = value; }
        }
        public string Description
        {
            get { return description; }
            set { description = value; }
        }
        public int Cost
        {
            get { return cost; }
            set { cost = value; }
        }
        public Action<Creature, Creature?> Execution
        {
            get { return action; }
            set { action = value; }
        }
        public bool NeedsTarget
        {
            get { return needsTarget; }
            set { needsTarget = value; }
        }
        #endregion

        #region Constructors
        public CombatAction(string name, string description, int cost, Action<Creature, Creature?> action, bool needsTarget)
        {
            this.name = name;
            this.description = description;
            this.cost = cost;
            this.action = action;
            this.needsTarget = needsTarget;
        }
        #endregion

        #region GenericMethods
        public override string ToString()
        {
            return name + " : " + description;
        }
        #endregion

        public readonly static List<CombatAction> ActionList = new List<CombatAction>()
        {
            new CombatAction("Attack", "Attack one target with a physical attack.", 1, Attack, true),
            new CombatAction("Move", "Move your speed along the board. (Not implemented)", 1, Move, false),
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

        public static void Move(Creature source, Creature? target = null)
        {

        }

    }
}
