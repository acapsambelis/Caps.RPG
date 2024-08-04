using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Caps.RPG.CombatEngine.Creatures.Actions
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
            new CombatAction("Dodge", "The first attack against you will miss (until your next turn).", 1, Dodge, false),
        };
        public static List<CombatAction> GetGenericList()
        {
            return ActionList;
        }

        // Generic Actions
        public static void Attack(Creature source, Creature? target)
        {

        }

        public static void Move(Creature source, Creature? target = null)
        {

        }

        public static void Dodge(Creature source, Creature? target = null)
        {

        }

    }
}
