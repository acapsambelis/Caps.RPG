using Caps.RPG.Rules.Attributes;
using SNS.Data.DataSerializer;

namespace Caps.RPG
{
    [DataClass("Creatures")]
    public class TestCreature : IGenericDataObject<TestCreature>
    {
        private AttributeSet attributes;
        private bool _wasLoaded = false;

        [SubDataObject("Attributes")]
        public AttributeSet Attributes
        {
            get { return attributes; }
            set { attributes = value; }
        }

        public bool WasLoaded { get { return _wasLoaded; } set { _wasLoaded = value; } }

        public TestCreature() { }
        public TestCreature(string name)
        {
            this.attributes = new AttributeSet();
        }

        public override bool Equals(object? obj)
        {
            return obj is TestCreature creature &&
                   EqualityComparer<AttributeSet>.Default.Equals(attributes, creature.attributes);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(attributes);
        }

        public static bool operator ==(TestCreature? left, TestCreature? right)
        {
            return EqualityComparer<TestCreature>.Default.Equals(left, right);
        }

        public static bool operator !=(TestCreature? left, TestCreature? right)
        {
            return !(left == right);
        }
    }
}
