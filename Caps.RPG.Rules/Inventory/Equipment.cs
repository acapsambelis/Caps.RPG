using SNS.Data.DataSerializer;

namespace Caps.RPG.Rules.Inventory
{
    [DataClass("Equipment")]
    public class Equipment : IGenericDataObject<Equipment>
    {
        [SubDataObject("Crown")]
        public InventorySlot Crown { get; set; }
        [SubDataObject("Face")]
        public InventorySlot Face { get; set; }
        [SubDataObject("HeadJewelry")]
        public InventorySlot HeadJewelry { get; set; }
        [SubDataObject("Neck")]
        public InventorySlot Neck { get; set; }
        [SubDataObject("Chest")]
        public InventorySlot Chest { get; set; }
        [SubDataObject("Shoulders")]
        public InventorySlot Shoulders { get; set; }
        [SubDataObject("Back")]
        public InventorySlot Back { get; set; }
        [SubDataObject("Arms")]
        public InventorySlot Arms { get; set; }
        [SubDataObject("Gloves")]
        public InventorySlot Gloves { get; set; }
        [SubDataObject("HandJewelry")]
        public InventorySlot HandJewelry { get; set; }
        [SubDataObject("Belt")]
        public InventorySlot Belt { get; set; }
        [SubDataObject("Pants")]
        public InventorySlot Pants { get; set; }
        [SubDataObject("Boots")]
        public InventorySlot Boots { get; set; }
        [SubDataObject("Hands")]
        public InventorySlot Hands { get; set; }

        private bool _wasLoaded = false;
        public bool WasLoaded { get { return _wasLoaded; } set { _wasLoaded = value; } }

        public Equipment()
        {
            Crown = new InventorySlot(ItemType.Crown);
            Face = new InventorySlot(ItemType.Face);
            HeadJewelry = new InventorySlot(ItemType.HeadJewelry);
            Neck = new InventorySlot(ItemType.Neck);
            Chest = new InventorySlot(ItemType.Chest);
            Shoulders = new InventorySlot(ItemType.Shoulders);
            Back = new InventorySlot(ItemType.Back);
            Arms = new InventorySlot(ItemType.Arms);
            Gloves = new InventorySlot(ItemType.Gloves);
            HandJewelry = new InventorySlot(ItemType.HandJewelry);
            Belt = new InventorySlot(ItemType.Belt);
            Pants = new InventorySlot(ItemType.Pants);
            Boots = new InventorySlot(ItemType.Boots);
            Hands = new InventorySlot(ItemType.Hands);
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

        public override bool Equals(object? obj)
        {
            return obj is Equipment equipment &&
                   EqualityComparer<InventorySlot>.Default.Equals(Crown, equipment.Crown) &&
                   EqualityComparer<InventorySlot>.Default.Equals(Face, equipment.Face) &&
                   EqualityComparer<InventorySlot>.Default.Equals(HeadJewelry, equipment.HeadJewelry) &&
                   EqualityComparer<InventorySlot>.Default.Equals(Neck, equipment.Neck) &&
                   EqualityComparer<InventorySlot>.Default.Equals(Chest, equipment.Chest) &&
                   EqualityComparer<InventorySlot>.Default.Equals(Shoulders, equipment.Shoulders) &&
                   EqualityComparer<InventorySlot>.Default.Equals(Back, equipment.Back) &&
                   EqualityComparer<InventorySlot>.Default.Equals(Arms, equipment.Arms) &&
                   EqualityComparer<InventorySlot>.Default.Equals(Gloves, equipment.Gloves) &&
                   EqualityComparer<InventorySlot>.Default.Equals(HandJewelry, equipment.HandJewelry) &&
                   EqualityComparer<InventorySlot>.Default.Equals(Belt, equipment.Belt) &&
                   EqualityComparer<InventorySlot>.Default.Equals(Pants, equipment.Pants) &&
                   EqualityComparer<InventorySlot>.Default.Equals(Boots, equipment.Boots) &&
                   EqualityComparer<InventorySlot>.Default.Equals(Hands, equipment.Hands);
        }

        public override int GetHashCode()
        {
            HashCode hash = new HashCode();
            hash.Add(Crown);
            hash.Add(Face);
            hash.Add(HeadJewelry);
            hash.Add(Neck);
            hash.Add(Chest);
            hash.Add(Shoulders);
            hash.Add(Back);
            hash.Add(Arms);
            hash.Add(Gloves);
            hash.Add(HandJewelry);
            hash.Add(Belt);
            hash.Add(Pants);
            hash.Add(Boots);
            hash.Add(Hands);
            return hash.ToHashCode();
        }

        public static bool operator ==(Equipment? left, Equipment? right)
        {
            return EqualityComparer<Equipment>.Default.Equals(left, right);
        }

        public static bool operator !=(Equipment? left, Equipment? right)
        {
            return !(left == right);
        }
    }

}
