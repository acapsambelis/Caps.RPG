
using Caps.RPG.Rules.Creatures.Actions;
using SNS.Data.DataSerializer;

namespace Caps.RPG.Rules.Inventory
{
    [DataClass("Inventories")]
    public class Inventory : IIDDataObject<Inventory>
    {
        private static int _idCounter;
        private int id;

        [DataProperty("ID")]
        public int ID { get { return id; } set { id = value; } }
        [DataProperty("Slots")]
        public InventorySlot[] Slots { get; set; }

        private bool _wasLoaded = false;
        public bool WasLoaded { get { return _wasLoaded; } set { _wasLoaded = value; } }

        public Inventory()
        {
            Slots = [];
        }
        public Inventory(int size = 15)
        {
            this.ID = _idCounter++;
            Slots = new InventorySlot[size];
            for (int i = 0; i < Slots.Length; i++)
            {
                Slots[i] = new InventorySlot(ItemType.Any);
            }
        }

        public override bool Equals(object? obj)
        {
            if (obj is not Inventory other)
                return false;

            bool isEqual = true;
            isEqual &= id == other.id;
            foreach (InventorySlot slot in Slots)
            {
                if (!other.Slots.Contains(slot))
                {
                    return false;
                }
            }
            return isEqual;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(id, Slots);
        }
    }
}
