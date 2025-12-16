using Caps.RPG.Rules.Creatures.Actions;
using Caps.RPG.Rules.Inventory;

namespace Caps.RPG.Rules.Creatures.Classed.Classes
{
    public class CharacterClass
    {
        private string name;
        private List<Item> startingInventory;

        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        public List<Item> StartingInventory
        {
            get { return startingInventory; }
            set { startingInventory = value; }
        }

        public CharacterClass() { }

        #region GenericMethods
        public override string ToString()
        {
            return Name;
        }
        #endregion

        public static List<CombatAction> GetCombatActionsForClass()
        {
            return new List<CombatAction>();
        }
    }
}
