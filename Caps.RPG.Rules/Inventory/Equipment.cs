
namespace Caps.RPG.Rules.Inventory
{
    public class Equipment
    {
        public readonly InventorySlot Crown;
        public readonly InventorySlot Face;
        public readonly InventorySlot HeadJewelry;
        public readonly InventorySlot Neck;
        public readonly InventorySlot Chest;
        public readonly InventorySlot Shoulders;
        public readonly InventorySlot Back;
        public readonly InventorySlot Arms;
        public readonly InventorySlot Gloves;
        public readonly InventorySlot HandJewelry;
        public readonly InventorySlot Belt;
        public readonly InventorySlot Pants;
        public readonly InventorySlot Boots;
        public readonly InventorySlot Hands;

        public Equipment()
        {
            Crown        = new InventorySlot(ItemType.Crown);
            Face         = new InventorySlot(ItemType.Face);
            HeadJewelry  = new InventorySlot(ItemType.HeadJewelry);
            Neck         = new InventorySlot(ItemType.Neck);
            Chest        = new InventorySlot(ItemType.Chest);
            Shoulders    = new InventorySlot(ItemType.Shoulders);
            Back         = new InventorySlot(ItemType.Back);
            Arms         = new InventorySlot(ItemType.Arms);
            Gloves       = new InventorySlot(ItemType.Gloves);
            HandJewelry  = new InventorySlot(ItemType.HandJewelry);
            Belt         = new InventorySlot(ItemType.Belt);
            Pants        = new InventorySlot(ItemType.Pants);
            Boots        = new InventorySlot(ItemType.Boots);
            Hands        = new InventorySlot(ItemType.Hands);
        }

        public InventorySlot GetSlotForItem(Item item) 
        {
            return item.Type switch
            {
                ItemType.Crown => Crown,
                ItemType.Face => Face,
                ItemType.HeadJewelry => HeadJewelry,
                ItemType.Neck => Neck,
                ItemType.Chest => Chest,
                ItemType.Shoulders => Shoulders,
                ItemType.Back => Back,
                ItemType.Arms => Arms,
                ItemType.Gloves => Gloves,
                ItemType.HandJewelry => HandJewelry,
                ItemType.Belt => Belt,
                ItemType.Pants => Pants,
                ItemType.Boots => Boots,
                ItemType.Hands => Hands,
                _ => throw new ArgumentException("Invalid item type")
            };
        }
    }

}
