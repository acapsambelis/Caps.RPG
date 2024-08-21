

namespace Caps.RPG.Rules.Creatures.Actions
{
    public class CombatAction
    {
        #region privateMembers
        private string name;
        private string description;
        private int cost;
        private Func<Creature, Creature?, ActionResult> action;
        private bool needsTarget;
        private double distance;
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
        public Func<Creature, Creature?, ActionResult> Execution
        {
            get { return action; }
            set { action = value; }
        }
        public bool NeedsTarget
        {
            get { return needsTarget; }
            set { needsTarget = value; }
        }
        public double Distance
        {
            get { return distance; }
            set { distance = value; }
        }
        #endregion

        #region Constructors
        public CombatAction(string name, string description, int cost, Func<Creature, Creature?, ActionResult> action, bool needsTarget, double distance)
        {
            this.name = name;
            this.description = description;
            this.cost = cost;
            this.action = action;
            this.needsTarget = needsTarget;
            this.distance = distance;
        }
        #endregion

        #region GenericMethods
        public override string ToString()
        {
            return name + " : " + description;
        }
        #endregion

    }
}
