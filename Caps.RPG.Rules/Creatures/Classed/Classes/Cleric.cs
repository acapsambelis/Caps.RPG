using Caps.RPG.Rules.Creatures.Actions;
using Caps.RPG.Rules.Helpers;
using Caps.RPG.Rules.Maps;


namespace Caps.RPG.Rules.Creatures.Classed.Classes
{
    public class Cleric : CharacterClass
    {
        public Cleric() { }

        #region GenericMethods
        public override string ToString()
        {
            return "Cleric";
        }
        #endregion


        public static List<CombatAction> GetCombatActionsForClass(int classCount)
        {
            return actionDictionary.Where(kvp => kvp.Key >= classCount).Select(kvp => kvp.Value).ToList();
        }

        public readonly static Dictionary<int, CombatAction> actionDictionary = new Dictionary<int, CombatAction>()
        {
            { 1, new CombatAction("Healing Word", "You heal one target for Health equal to 5 times your Cleric level.", 1, HealingWord, true, false, 5) },
        };

        public static ActionResult HealingWord(Combattant source, TileBase? target = null)
        {
            if (target == null)
                return new ActionResult();

            Combattant targetCreature = (Combattant)target.Features.Values.Where(f => f is Combattant);
            ClassedCharacter? sourceClassed = source.Creature as ClassedCharacter;
            if (sourceClassed != null)
            {
                targetCreature.Health += sourceClassed.GetLevels(typeof(Cleric)) * 5;
            }
            return new ActionResult();
        }
    }
}
