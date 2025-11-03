using Caps.RPG.Rules.Attributes;

namespace Caps.RPG.Rules.Creatures.Unclassed
{
    public class MonsterBlueprint
    {
        public string Name;
        public AttributeSet Attributes;

        public MonsterBlueprint()
        {
            Attributes = new AttributeSet();
        }
        public MonsterBlueprint(string name, AttributeSet attributeSet)
        {
            Name = name;
            Attributes = attributeSet;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}