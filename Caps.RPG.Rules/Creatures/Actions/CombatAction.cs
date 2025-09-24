using SNS.Data.DataSerializer;
using System.Reflection;
using Caps.RPG.Rules.Maps;
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
                                Execution = (Func<Combattant, TileBase[], ActionResult>)Delegate.CreateDelegate(typeof(Func<Combattant, TileBase?, ActionResult>), method.IsStatic ? null : Activator.CreateInstance(type), method);
                            }
                        }
                    }
                }
            }
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

    public struct ActionSetup
    {
        public enum SourceType
        {
            None,
            SingleCreature,
            MultipleCreature,
            SingleTile,
            Area,
            Custom,
        }

        private bool needsSource;
        private double sourcerange = 0;
        private PropertyInfo? sourceRangeProperty;
        private SourceType sourcetype;
        private MapShape mapShape;
        private double distanceFromSource;
        private bool needsEmptyTile;

        [DataProperty("NeedsTarget")]
        public bool NeedsTarget
        {
            get { return needsSource; }
            set { needsSource = value; }
        }
        [DataProperty("SourceType")]
        public SourceType TargetType
        {
            get { return sourcetype; }
            set { sourcetype = value; }
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

        public ActionSetup(
            bool needsSource = false,
            double sourcerange = 0,
            SourceType sourcetype = SourceType.None,
            MapShape mapShape = MapShape.None,
            double distanceFromSource = 0,
            bool needsEmptyTile = false
        )
        {
            this.needsSource = needsSource;
            this.sourcerange = sourcerange;
            this.sourcetype = sourcetype;
            this.mapShape = mapShape;
            this.distanceFromSource = distanceFromSource;
            this.needsEmptyTile = needsEmptyTile;
        }
        public ActionSetup(
            bool needsSource = false,
            PropertyInfo? sourceRangeProperty = null,
            SourceType sourcetype = SourceType.None,
            MapShape mapShape = MapShape.None,
            double distanceFromSource = 0,
            bool needsEmptyTile = false
        )
        {
            this.needsSource = needsSource;
            this.sourceRangeProperty = sourceRangeProperty;
            this.sourcetype = sourcetype;
            this.mapShape = mapShape;
            this.distanceFromSource = distanceFromSource;
            this.needsEmptyTile = needsEmptyTile;
        }

        public double GetRange(Combattant source)
        {
            if (needsSource && sourcerange > 0)
            {
                return sourcerange;
            }
            if (needsSource && sourceRangeProperty != null)
            {
                return GetRangeFromProperty(source);
            }
            return distanceFromSource;
        }

        private double GetRangeFromProperty(Combattant source)
        {
            if (sourceRangeProperty == null)
                throw new InvalidOperationException("sourceRangeProperty is not set.");
            object? value = sourceRangeProperty.GetValue(source) ?? throw new InvalidOperationException("The property value is null.");
            if (value is double d)
                return d;
            if (value is int i)
                return i;
            throw new InvalidCastException($"Cannot convert property value of type {value.GetType()} to double.");
        }

        public override string ToString()
        {
            StringBuilder sb = new();
            switch (TargetType)
            {
                case SourceType.None:
                    sb.Append("No Targeting");
                    break;
                case SourceType.SingleCreature:
                    sb.Append("a single creature");
                    break;
                case SourceType.MultipleCreature:
                    sb.Append("a multiple creature");
                    break;
                case SourceType.SingleTile:
                    sb.Append("a single tile");
                    break;
                case SourceType.Area:
                    sb.Append("an area");
                    break;
                case SourceType.Custom:
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
                   needsSource == setup.needsSource &&
                   sourcerange == setup.sourcerange &&
                   sourcetype == setup.sourcetype &&
                   mapShape == setup.mapShape &&
                   distanceFromSource == setup.distanceFromSource;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(needsSource, sourcerange, sourcetype, mapShape, distanceFromSource);
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
