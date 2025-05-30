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
            set { item = value; }
        }

        public bool WasLoaded { get { return _wasLoaded; } set { _wasLoaded = value; } }

        public InventorySlot() { }
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
