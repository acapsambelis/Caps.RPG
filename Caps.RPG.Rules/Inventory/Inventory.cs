
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
