using Caps.RPG.CombatEngine.Creatures.Actions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Caps.RPG.CombatEngine.Creatures.Classed.Classes
{
    public class Fighter : CharacterClass
    {

        public Fighter() { }

        #region GenericMethods
        public override string ToString()
        {
            return "Fighter";
        }
        #endregion

        public static List<CombatAction> GetCombatActionsForClass(int classCount)
        {
            return actionDictionary.Where(kvp => kvp.Key >= classCount).Select(kvp => kvp.Value).ToList();
        }

        public readonly static Dictionary<int, CombatAction> actionDictionary = new Dictionary<int, CombatAction>()
        {
            { 1, new CombatAction("Second Wind", "You regain Health equal to 5 times your Fighter level.", 1, SecondWind, false) },
        };

        public static void SecondWind(Creature source, Creature? target = null)
        {
            ClassedCharacter? sourceClassed = source as ClassedCharacter;
            if (sourceClassed != null)
            {
                sourceClassed.Health += sourceClassed.GetLevels(typeof(Fighter)) * 5;
            }
        }
    }
}
