using Caps.RPG.Engine.Modifiers;
using Caps.RPG.Rules.Creatures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Caps.RPG.Rules.Inventory
{
    public class CreatureInventory : Inventory
    {
        private readonly Creature _creature;
        public Equipment EquipedItems { get; }

        public CreatureInventory(Creature creature, int size = 15) : base(size)
        {
            this._creature = creature;
            EquipedItems = new Equipment();
        }

        public void Equip(Item i)
        {
            var possibleNull = EquipedItems.GetSlotForItem(i);
            InventorySlot slot = possibleNull != null ? possibleNull : new InventorySlot(ItemType.None);
            Modifier[] modifiers = slot.SetItem(i);
            foreach (Modifier m in modifiers)
            {
                _creature.AddModifier(m, i);
            }
        }
    }
}
