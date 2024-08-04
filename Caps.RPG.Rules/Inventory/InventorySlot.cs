using Caps.RPG.Engine.Modifiers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Caps.RPG.Rules.Inventory
{
    public class InventorySlot
    {
        private Item? item;
        public ItemType ItemType { get; }
        public InventorySlot(ItemType type)
        {
            this.item = null;
            this.ItemType = type;
        }

        public Modifier[] SetItem(Item item)
        {
            if (ItemType == ItemType.Any || ItemType == ItemType.None || item.Type == ItemType)
            {
                this.item = item;
                return item.GetModifiers();
            }
            return [];
        }
    }
}
