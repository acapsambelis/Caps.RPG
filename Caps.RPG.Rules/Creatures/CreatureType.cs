using Caps.RPG.Rules.Creatures.Actions;
using Caps.RPG.Rules.Modifiers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Caps.RPG.Rules.Creatures
{
    public class CreatureType
    {
        string name;
        private List<Modifier> modifiers = [];
        private List<CombatAction> combatActions = [];

        public string Name
        {
            get { return name; }
            set { name = value; }
        }
        public List<Modifier> Modifiers
        {
            get{ return modifiers; }
            set { modifiers = value; }
        }
        public List<CombatAction> CombatActions
        {
            get { return combatActions; }
            set { combatActions = value; }
        }

        public CreatureType() { }

        #region GenericMethods
        public override string ToString()
        {
            return Name;
        }
        #endregion
    }
}
