using Caps.RPG.Rules.Maps;
using SNS.Data.DataSerializer;
using System.Reflection;
using System.Text;

namespace Caps.RPG.Rules.Creatures.Actions
{
    [DataClass("CombatActions")]
    public class CombatAction : IGenericDataObject<CombatAction>
    {
        #region privateMembers
        private string name;
        private string description;
        private int cost;
        private Func<Combattant, TileBase[], ActionResult> action;
        private ActionSetup setup;
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
        public Func<Combattant, TileBase[], ActionResult> Execution
        {
            get { return action; }
            set { action = value; }
        }

        [SubDataObject("Setup")]
        public ActionSetup Setup
        {
            get { return setup; }
            set { setup = value; }
        }

        public bool WasLoaded { get { return _wasLoaded; } set { _wasLoaded = value; } }
        #endregion

        #region Constructors
        public CombatAction() { }
        
        public CombatAction(
            string name,
            string description,
            int cost,
            Func<Combattant, TileBase[], ActionResult> action,
            ActionSetup setup = default
        )
        {
            this.name = name;
            this.description = description;
            this.cost = cost;
            this.action = action;
            this.setup = setup;
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
            isEqual &= EqualityComparer<Func<Combattant, TileBase[], ActionResult>>.Default.Equals(action, other.action);
            isEqual &= setup == other.setup;

            return isEqual;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(name, description, cost, action, setup);
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

    public class ActionSetup
    {
        public enum TargetType
        {
            None,
            SingleCreature,
            MultipleCreature,
            SingleTile,
            Area,
            Custom,
        }

        public enum ActionTags
        {
            Attack,
            Movement,
            Healing,
            Buff,
            Debuff,
            Utility,
        }

        private bool needsTarget;
        private double targetRange;
        private PropertyInfo? targetRangeProperty;
        private TargetType target;
        private MapShape mapShape;
        private double distanceFromSource;
        private bool needsEmptyTile;
        private ActionTags[] tags = [];

        [DataProperty("NeedsTarget")]
        public bool NeedsTarget
        {
            get { return needsTarget; }
            set { needsTarget = value; }
        }
        [DataProperty("TargetRange")]
        public double TargetRange
        {
            get { return targetRange; }
            set { targetRange = value; }
        }
        [DataProperty("Target")]
        public TargetType Target
        {
            get { return target; }
            set { target = value; }
        }
        [DataProperty("MapShape")]
        public MapShape Shape
        {
            get { return mapShape; }
            set { mapShape = value; }
        }
        [DataProperty("DistanceFromSource")]
        public double DistanceFromSource
        {
            get { return distanceFromSource; }
            set { distanceFromSource = value; }
        }
        [DataProperty("NeedsEmptyTile")]
        public bool NeedsEmptyTile
        {
            get { return needsEmptyTile; }
            set { needsEmptyTile = value; }
        }
        [DataProperty("Tags")]
        public ActionTags[] Tags
        {
            get { return tags; }
            set { tags = value; }
        }

        public ActionSetup() { }

        public ActionSetup(
            bool needsTarget = false,
            double targetRange = 0,
            PropertyInfo? targetRangeProperty = null,
            TargetType sourcetype = TargetType.None,
            MapShape mapShape = MapShape.None,
            double distanceFromSource = 0,
            bool needsEmptyTile = false)
        {
            this.needsTarget = needsTarget;
            this.targetRange = targetRange;
            this.targetRangeProperty = targetRangeProperty;
            this.target = sourcetype;
            this.mapShape = mapShape;
            this.distanceFromSource = distanceFromSource;
            this.needsEmptyTile = needsEmptyTile;
            this.tags = [];
        }

        public double GetRange(Creature source)
        {
            if (needsTarget && targetRange > 0)
            {
                return targetRange;
            }
            if (needsTarget && targetRangeProperty != null && source != null)
            {
                return GetRangeFromProperty(source);
            }
            return distanceFromSource;
        }

        private double GetRangeFromProperty(Creature source)
        {
            if (targetRangeProperty == null)
                throw new InvalidOperationException("sourceRangeProperty is not set.");
            object? value = targetRangeProperty.GetValue(source) ?? throw new InvalidOperationException("The property value is null.");
            if (value is double d)
                return d;
            if (value is int i)
                return i;
            throw new InvalidCastException($"Cannot convert property value of type {value.GetType()} to double.");
        }

        public override string ToString()
        {
            StringBuilder sb = new();
            switch (Target)
            {
                case TargetType.None:
                    sb.Append("No Targeting");
                    break;
                case TargetType.SingleCreature:
                    sb.Append("a single creature");
                    break;
                case TargetType.MultipleCreature:
                    sb.Append("a multiple creature");
                    break;
                case TargetType.SingleTile:
                    sb.Append("a single tile");
                    break;
                case TargetType.Area:
                    sb.Append("an area");
                    break;
                case TargetType.Custom:
                    sb.Append("a custom target");
                    break;
            }
            if (Shape != MapShape.None)
            {
                sb.Append($" that is the ");
                switch (Shape)
                {
                    case MapShape.None:
                        sb.Append("No Target");
                        break;
                    case MapShape.Circle:
                        sb.Append($"center of a circle of size {distanceFromSource}");
                        break;
                    case MapShape.Cone:
                        sb.Append($"edge centerpoint of a cone of size {distanceFromSource}");
                        break;
                    case MapShape.FromSourceLine:
                        sb.Append($"endpoint for a line of length {distanceFromSource} starting at the current tile");
                        break;
                    case MapShape.FreestandingLine:
                        sb.Append($"start/end point defining a line of length {distanceFromSource}");
                        break;
                    case MapShape.Custom:
                        sb.Append("Custom Targeting");
                        break;
                }
            }
            return sb.ToString();
        }

        public override bool Equals(object? obj)
        {
            return obj is ActionSetup setup &&
                   needsTarget == setup.needsTarget &&
                   targetRange == setup.targetRange &&
                   target == setup.target &&
                   mapShape == setup.mapShape &&
                   distanceFromSource == setup.distanceFromSource;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(needsTarget, targetRange, target, mapShape, distanceFromSource);
        }

        public static bool operator ==(ActionSetup left, ActionSetup right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(ActionSetup left, ActionSetup right)
        {
            return !(left == right);
        }
    }
}
