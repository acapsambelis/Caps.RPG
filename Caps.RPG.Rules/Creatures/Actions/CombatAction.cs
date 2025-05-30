

using Caps.RPG.Rules.Helpers;

namespace Caps.RPG.Rules.Creatures.Actions
{
    public class CombatAction
    {
        #region privateMembers
        private string name;
        private string description;
        private int cost;
        private Func<Combattant, Combattant?, Vector2D?, ActionResult> action;
        private bool needsTarget;
        private bool needsLocation;
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
        public Func<Combattant, Combattant?, Vector2D?, ActionResult> Execution
        {
            get { return action; }
            set { action = value; }
        }
        public bool NeedsTarget
        {
            get { return needsTarget; }
            set { needsTarget = value; }
        }
        public bool NeedsLocation
        {
            get { return needsLocation; }
            set { needsLocation = value; }
        }
        public double Distance
        {
            get { return distance; }
            set { distance = value; }
        }
        #endregion

        #region Constructors
        public CombatAction(
            string name,
            string description,
            int cost,
            Func<Combattant, Combattant?, Vector2D?, ActionResult> action,
            bool needsTarget,
            bool needsLocation,
            double distance
        )
        {
            this.name = name;
            this.description = description;
            this.cost = cost;
            this.action = action;
            this.needsTarget = needsTarget;
            this.needsLocation = needsLocation;
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
