using Caps.RPG.Rules.Creatures.Classed;

namespace Caps.RPG.Rules.Creatures
{
    public class Party
    {
        public string Name;
        public List<ClassedCharacter> Characters;

        public Party(string name)
        {
            Name = name;
            Characters = new List<ClassedCharacter>();
        }

        public void AddCharacter(ClassedCharacter character)
        {
            Characters.Add(character);
        }

        public void RemoveCharacter(ClassedCharacter character)
        {
            Characters.Remove(character);
        }
    }
}
