using Caps.RPG.Rules.Modifiers;
using SNS.Data.DataSerializer;

namespace Caps.RPG.Rules.Inventory
{
    [DataClass("InventorySlots")]
    public class InventorySlot : IGenericDataObject<InventorySlot>
    {
        private Item? item;
        private bool _wasLoaded = false;

        [DataProperty("ItemType")]
        public ItemType ItemType { get; set; }
        [DataProperty("Item")]
        public Item? Item
        {
            get { return item; }
            set { if (value != null) SetItem(value); }
        }

        public bool WasLoaded { get { return _wasLoaded; } set { _wasLoaded = value; } }

        public InventorySlot() { }
        public InventorySlot(ItemType type)
        {
            this.item = null;
            this.ItemType = type;
        }

        public bool SetItem(Item? item)
        {
            if (item == null)
            {
                this.item = null;
                return true;
            }

            if (ItemType == ItemType.None) ItemType = item.Type;
            if (ItemType == ItemType.Any || item.Type == ItemType)
            {
                this.item = item;
                return true;
            }
            throw new InvalidCastException("Invalid equip.");
        }

        public override bool Equals(object? obj)
        {
            if (obj is not InventorySlot slot)
                return false;

            bool isEqual = true;
            isEqual &= ItemType == slot?.ItemType;
            isEqual &= EqualityComparer<Item?>.Default.Equals(Item, slot?.Item);
            return isEqual;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(ItemType, Item);
        }

        public static bool operator ==(InventorySlot? left, InventorySlot? right)
        {
            return EqualityComparer<InventorySlot>.Default.Equals(left, right);
        }

        public static bool operator !=(InventorySlot? left, InventorySlot? right)
        {
            return !(left == right);
        }
    }
}
