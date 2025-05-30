using Caps.RPG.Rules.Creatures.Actions;
using SNS.Data.DataSerializer;

namespace Caps.RPG.Rules.Inventory
{
    [DataClass("CreatureInventories")]
    public class CreatureInventory : Inventory, IGenericDataObject<CreatureInventory>
    {
        [SubDataObject("EquippedItems")]
        public Equipment EquipedItems { get; set; }
        
        public CreatureInventory() : base (15)
        {
            EquipedItems = new Equipment();
        }
        public CreatureInventory(int size = 15) : base(size)
        {
            EquipedItems = new Equipment();
        }

        internal void Equip(Item i)
        {
            var possibleNull = EquipedItems.GetSlotForItem(i);
            InventorySlot slot = possibleNull ?? new InventorySlot(ItemType.None);
            slot.SetItem(i);
        }

        public override bool Equals(object? obj)
        {
            if (obj is not CreatureInventory other)
                return false;

            bool isEqual = true;
            isEqual &= base.Equals(obj);
            isEqual &= ID == other.ID;
            isEqual &= EquipedItems == other.EquipedItems;
            return isEqual;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(base.GetHashCode(), ID, Slots, EquipedItems);
        }

        public static bool operator ==(CreatureInventory? left, CreatureInventory? right)
        {
            return EqualityComparer<CreatureInventory>.Default.Equals(left, right);
        }

        public static bool operator !=(CreatureInventory? left, CreatureInventory? right)
        {
            return !(left == right);
        }
    }
}
