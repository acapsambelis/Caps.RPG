using Caps.RPG.Rules.Modifiers;
using Caps.RPG.Rules.Creatures;

namespace Caps.RPG.Rules.Inventory
{
    public class CreatureInventory : Inventory
    {
        private Creature? _creature;

        public Equipment EquipedItems { get; }

        public CreatureInventory(Creature creature, int size = 15) : base(size)
        {
            this._creature = creature;
            EquipedItems = new Equipment();
        }
        public CreatureInventory(int size) : base(size)
        {
            _creature = null;
            EquipedItems = new Equipment();
        }

        public void Equip(Item i)
        {
            var possibleNull = EquipedItems.GetSlotForItem(i);
            InventorySlot slot = possibleNull != null ? possibleNull : new InventorySlot(ItemType.None);
            Modifier[] modifiers = slot.SetItem(i);
            foreach (Modifier m in modifiers)
            {
                _creature?.AddModifier(m, i);
            }
        }

        public void AssignCreature(Creature creature)
        {
            _creature = creature;
        }
    }
}
