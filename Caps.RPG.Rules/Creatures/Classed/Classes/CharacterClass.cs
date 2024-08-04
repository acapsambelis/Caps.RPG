using Caps.RPG.Rules.Creatures.Actions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Caps.RPG.Rules.Creatures.Classed.Classes
{
    public class CharacterClass
    {
        public CharacterClass() { }

        #region GenericMethods
        public override string ToString()
        {
            return "Class";
        }
        #endregion

        public static List<CombatAction> GetCombatActionsForClass()
        {
            return new List<CombatAction>();
        }
    }
}
