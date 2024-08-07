using Caps.RPG.Rules.Creatures.Actions;

namespace Caps.RPG.Rules.Creatures.Classed.Classes
{
    public class Wizard : CharacterClass
    {
        public Wizard() { }

        #region GenericMethods
        public override string ToString()
        {
            return "Wizard";
        }
        #endregion

        public static List<CombatAction> GetCombatActionsForClass(int classCount)
        {
            return actionDictionary.Where(kvp => kvp.Key >= classCount).Select(kvp => kvp.Value).ToList();
        }

        public readonly static Dictionary<int, CombatAction> actionDictionary = new Dictionary<int, CombatAction>()
        {
            { 1, new CombatAction("Firebolt", "You deal damage to one target equal to 5 times your Wizard level.", 1, Firebolt, true) },
        };

        public static void Firebolt(Creature source, Creature? target = null)
        {
            if (target == null)
                return;

            ClassedCharacter? sourceClassed = source as ClassedCharacter;
            if (sourceClassed != null)
            {
                target.Health -= sourceClassed.GetLevels(typeof(Wizard)) * 5;
            }
        }
    }
}
