using SNS.Data.DataSerializer;
using System.Reflection;

using Caps.RPG.Rules.Helpers;

namespace Caps.RPG.Rules.Creatures.Actions
{
    [DataClass("CombatActions")]
    public class CombatAction : IGenericDataObject<CombatAction>
    {
        #region privateMembers
        private string name;
        private string description;
        private int cost;
        private Func<Combattant, Combattant?, Vector2D?, ActionResult> action;
        private bool needsTarget;
        private bool needsLocation;
        private double distance;
        private bool _wasLoaded = false;
        #endregion

        #region PublicMembers
        [DataProperty("Name")]
        public string Name
        {
            get { return name; }
            set { name = value; }
        }
        [DataProperty("Description")]
        public string Description
        {
            get { return description; }
            set { description = value; }
        }
        [DataProperty("Cost")]
        public int Cost
        {
            get { return cost; }
            set { cost = value; }
        }
        [DataProperty("FunctionLocation")]
        public string ExecutionLocation
        {
            get
            {
                if (Execution != null)
                {
                    var method = Execution.Method;
                    var declaringType = method.DeclaringType?.FullName ?? "Unknown";
                    var methodName = method.Name;
                    return $"{declaringType}|{methodName}";
                }
                return string.Empty;
            }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    var parts = value.Split('|');
                    if (parts.Length == 2)
                    {
                        var type = Type.GetType(parts[0]);
                        if (type != null)
                        {
                            var method = type.GetMethod(parts[1], BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
                            if (method != null)
                            {
                                Execution = (Func<Combattant, Combattant?, Vector2D?, ActionResult>)Delegate.CreateDelegate(typeof(Func<Combattant, Combattant?, Vector2D?, ActionResult>), method.IsStatic ? null : Activator.CreateInstance(type), method);
                            }
                        }
                    }
                }
            }
        }
        public Func<Combattant, Combattant?, Vector2D?, ActionResult> Execution
        {
            get { return action; }
            set { action = value; }
        }
        [DataProperty("NeedsTarget")]
        public bool NeedsTarget
        {
            get { return needsTarget; }
            set { needsTarget = value; }
        }
        [DataProperty("NeedsLocation")]
        public bool NeedsLocation
        {
            get { return needsLocation; }
            set { needsLocation = value; }
        }
        [DataProperty("Distance")]
        public double Distance
        {
            get { return distance; }
            set { distance = value; }
        }

        public bool WasLoaded { get { return _wasLoaded; } set { _wasLoaded = value; } }
        #endregion

        #region Constructors
        public CombatAction() { }
        
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

        public override bool Equals(object? obj)
        {
            if (obj is not CombatAction other)
                return false;

            bool isEqual = true;
            isEqual &= name == other.name;
            isEqual &= description == other.description;
            isEqual &= cost == other.cost;
            isEqual &= EqualityComparer<Func<Creature, Creature?, ActionResult>>.Default.Equals(action, other.action);
            isEqual &= needsTarget == other.needsTarget;
            isEqual &= distance == other.distance;

            return isEqual;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(name, description, cost, action, needsTarget, distance);
        }

        public static bool operator ==(CombatAction? left, CombatAction? right)
        {
            return EqualityComparer<CombatAction>.Default.Equals(left, right);
        }

        public static bool operator !=(CombatAction? left, CombatAction? right)
        {
            return !(left == right);
        }
        #endregion

    }
}
