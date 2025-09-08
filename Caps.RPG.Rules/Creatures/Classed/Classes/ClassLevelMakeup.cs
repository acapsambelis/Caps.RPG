
namespace Caps.RPG.Rules.Creatures.Classed.Classes
{
    public class ClassLevelMakeup
    {
        #region PublicMembers
        public Dictionary<Type, int> ClassLevels { get; internal set; } = [];
        #endregion

        #region Constructors
        public ClassLevelMakeup() { }
        public ClassLevelMakeup(Dictionary<Type, int> classLevels)
        {
            ClassLevels = classLevels;
        }
        #endregion

        #region GenericMethods
        public override string ToString()
        {
            string simplified = "";
            foreach (var level in ClassLevels)
            {
                simplified += ", " + level.Key + ": " + level.Value;
            }
            return simplified;
        }
        #endregion

        public void AddClassLevel(Type toAdd)
        {
            if (!ClassLevels.ContainsKey(toAdd))
            {
                ClassLevels[toAdd] = 0;
            }
            ClassLevels[toAdd] += 1;
        }

        public int GetLevels(Type t)
        {
            if (t.IsSubclassOf(typeof(CharacterClass)))
            {
                if (ClassLevels.TryGetValue(t, out int value)) return value;
            }
            return 0;
        }
    }
}
