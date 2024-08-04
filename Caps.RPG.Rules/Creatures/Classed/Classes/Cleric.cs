using Caps.RPG.CombatEngine.Creatures.Actions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Caps.RPG.CombatEngine.Creatures.Classed.Classes
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
            { 1, new CombatAction("Healing Word", "You heal one target for Health equal to 5 times your Cleric level.", 1, HealingWord, true) },
        };

        public static void HealingWord(Creature source, Creature? target = null)
        {
            if (target == null)
                return;

            ClassedCharacter? sourceClassed = source as ClassedCharacter;
            if (sourceClassed != null)
            {
                target.Health += sourceClassed.GetLevels(typeof(Cleric)) * 5;
            }
        }
    }
}
