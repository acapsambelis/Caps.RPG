using Caps.RPG.Rules.Creatures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Caps.RPG.Rules.Inventory
{
    public class Inventory
    {
        public InventorySlot[] Slots { get; }

        public Inventory(int size = 15)
        {
            Slots = new InventorySlot[size];
            for (int i = 0; i < Slots.Length; i++)
            {
                Slots[i] = new InventorySlot(ItemType.Any);
            }
        }
    }
}
