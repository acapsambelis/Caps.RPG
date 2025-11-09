using Caps.RPG.Rules.Modifiers;
using SNS.Data.DataSerializer;

namespace Caps.RPG.Rules.Inventory
{
    [DataClass("Equipment")]
    public class Equipment : IGenericDataObject<Equipment>
    {
        private bool equipmentChanged = true;
        public bool EquipmentChanged {
            get { return equipmentChanged; }
            set { equipmentChanged = value; }
        }

        private InventorySlot crown;
        [SubDataObject("Crown")]
        public InventorySlot Crown
        {
            get => crown;
            set { crown = value; equipmentChanged = true; }
        }

        private InventorySlot face;
        [SubDataObject("Face")]
        public InventorySlot Face
        {
            get => face;
            set { face = value; equipmentChanged = true; }
        }

        private InventorySlot headJewelry;
        [SubDataObject("HeadJewelry")]
        public InventorySlot HeadJewelry
        {
            get => headJewelry;
            set { headJewelry = value; equipmentChanged = true; }
        }

        private InventorySlot neck;
        [SubDataObject("Neck")]
        public InventorySlot Neck
        {
            get => neck;
            set { neck = value; equipmentChanged = true; }
        }

        private InventorySlot chest;
        [SubDataObject("Chest")]
        public InventorySlot Chest
        {
            get => chest;
            set { chest = value; equipmentChanged = true; }
        }

        private InventorySlot shoulders;
        [SubDataObject("Shoulders")]
        public InventorySlot Shoulders
        {
            get => shoulders;
            set { shoulders = value; equipmentChanged = true; }
        }

        private InventorySlot back;
        [SubDataObject("Back")]
        public InventorySlot Back
        {
            get => back;
            set { back = value; equipmentChanged = true; }
        }

        private InventorySlot arms;
        [SubDataObject("Arms")]
        public InventorySlot Arms
        {
            get => arms;
            set { arms = value; equipmentChanged = true; }
        }

        private InventorySlot gloves;
        [SubDataObject("Gloves")]
        public InventorySlot Gloves
        {
            get => gloves;
            set { gloves = value; equipmentChanged = true; }
        }

        private InventorySlot handJewelry;
        [SubDataObject("HandJewelry")]
        public InventorySlot HandJewelry
        {
            get => handJewelry;
            set { handJewelry = value; equipmentChanged = true; }
        }

        private InventorySlot belt;
        [SubDataObject("Belt")]
        public InventorySlot Belt
        {
            get => belt;
            set { belt = value; equipmentChanged = true; }
        }

        private InventorySlot pants;
        [SubDataObject("Pants")]
        public InventorySlot Pants
        {
            get => pants;
            set { pants = value; equipmentChanged = true; }
        }

        private InventorySlot boots;
        [SubDataObject("Boots")]
        public InventorySlot Boots
        {
            get => boots;
            set { boots = value; equipmentChanged = true; }
        }

        private InventorySlot hands;
        [SubDataObject("Hands")]
        public InventorySlot Hands
        {
            get => hands;
            set { hands = value; equipmentChanged = true; }
        }

        private bool _wasLoaded = false;
        public bool WasLoaded { get { return _wasLoaded; } set { _wasLoaded = value; } }

        public Equipment()
        {
            crown = new InventorySlot(ItemType.Crown);
            face = new InventorySlot(ItemType.Face);
            headJewelry = new InventorySlot(ItemType.HeadJewelry);
            neck = new InventorySlot(ItemType.Neck);
            chest = new InventorySlot(ItemType.Chest);
            shoulders = new InventorySlot(ItemType.Shoulders);
            back = new InventorySlot(ItemType.Back);
            arms = new InventorySlot(ItemType.Arms);
            gloves = new InventorySlot(ItemType.Gloves);
            handJewelry = new InventorySlot(ItemType.HandJewelry);
            belt = new InventorySlot(ItemType.Belt);
            pants = new InventorySlot(ItemType.Pants);
            boots = new InventorySlot(ItemType.Boots);
            hands = new InventorySlot(ItemType.Hands);
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

        public Dictionary<ModifiedValue, List<Modifier>> GetAllModifiers()
        {
            Dictionary<ModifiedValue, List<Modifier>> allModifiers = [];
            InventorySlot[] slots = [
                Crown, Face, HeadJewelry, Neck, Chest, Shoulders,
                Back, Arms, Gloves, HandJewelry, Belt, Pants, Boots, Hands
            ];
            foreach (var slot in slots)
            {
                if (slot.Item != null)
                {
                    foreach (var modifier in slot.Item.Modifiers)
                    {
                        if (!allModifiers.TryGetValue(modifier.Target, out List<Modifier>? value))
                        {
                            value = [];
                            allModifiers[modifier.Target] = value;
                        }

                        value.Add(modifier);
                    }
                }
            }
            return allModifiers;
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
