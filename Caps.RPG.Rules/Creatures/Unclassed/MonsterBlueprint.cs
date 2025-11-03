namespace Caps.RPG.Rules.Creatures.Unclassed
{
    public class MonsterBlueprint
    {
        public string Name;
        public MonsterBlueprint() { }
        public MonsterBlueprint(string name) { Name = name; }

        public override string ToString()
        {
            return Name;
        }
    }
}