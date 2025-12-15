using Caps.RPG.Rules.Creatures.Actions;
using Caps.RPG.Rules.Modifiers;
using SNS.Data.DataSerializer;

namespace Caps.RPG.Rules.Inventory
{
    [DataClass("Equipment")]
    public class Equipment : IGenericDataObject<Equipment>
    {
        private InventorySlot crown;
        [SubDataObject("Crown")]
        public InventorySlot Crown
        {
            get => crown;
            set { crown = value; }
        }

        private InventorySlot face;
        [SubDataObject("Face")]
        public InventorySlot Face
        {
            get => face;
            set { face = value; }
        }

        private InventorySlot jewelry;
        [SubDataObject("Jewelry")]
        public InventorySlot Jewelry
        {
            get => jewelry;
            set { jewelry = value; }
        }

        private InventorySlot neck;
        [SubDataObject("Neck")]
        public InventorySlot Neck
        {
            get => neck;
            set { neck = value; }
        }

        private InventorySlot chest;
        [SubDataObject("Chest")]
        public InventorySlot Chest
        {
            get => chest;
            set { chest = value; }
        }

        private InventorySlot shoulders;
        [SubDataObject("Shoulders")]
        public InventorySlot Shoulders
        {
            get => shoulders;
            set { shoulders = value; }
        }

        private InventorySlot back;
        [SubDataObject("Back")]
        public InventorySlot Back
        {
            get => back;
            set { back = value; }
        }

        private InventorySlot arms;
        [SubDataObject("Arms")]
        public InventorySlot Arms
        {
            get => arms;
            set { arms = value; }
        }

        private InventorySlot gloves;
        [SubDataObject("Gloves")]
        public InventorySlot Gloves
        {
            get => gloves;
            set { gloves = value; }
        }

        private InventorySlot belt;
        [SubDataObject("Belt")]
        public InventorySlot Belt
        {
            get => belt;
            set { belt = value; }
        }

        private InventorySlot pants;
        [SubDataObject("Pants")]
        public InventorySlot Pants
        {
            get => pants;
            set { pants = value; }
        }

        private InventorySlot boots;
        [SubDataObject("Boots")]
        public InventorySlot Boots
        {
            get => boots;
            set { boots = value; }
        }

        private InventorySlot hands;
        [SubDataObject("Hands")]
        public InventorySlot Hands
        {
            get => hands;
            set { hands = value; }
        }

        private bool _wasLoaded = false;
        public bool WasLoaded { get { return _wasLoaded; } set { _wasLoaded = value; } }

        public Equipment()
        {
            crown = new InventorySlot(ItemType.Crown);
            face = new InventorySlot(ItemType.Face);
            jewelry = new InventorySlot(ItemType.Jewelry);
            neck = new InventorySlot(ItemType.Neck);
            chest = new InventorySlot(ItemType.Chest);
            shoulders = new InventorySlot(ItemType.Shoulders);
            back = new InventorySlot(ItemType.Back);
            arms = new InventorySlot(ItemType.Arms);
            gloves = new InventorySlot(ItemType.Gloves);
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
                ItemType.Jewelry => Jewelry,
                ItemType.Neck => Neck,
                ItemType.Chest => Chest,
                ItemType.Shoulders => Shoulders,
                ItemType.Back => Back,
                ItemType.Arms => Arms,
                ItemType.Gloves => Gloves,
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
            foreach (var slot in GetAllSlots())
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

        public List<CombatAction> GetCombatActions()
        {
            List<CombatAction> actions = [];
            foreach (var slot in GetAllSlots())
            {
                if (slot.Item != null)
                {
                    actions.AddRange(slot.Item.CombatActions);
                }
            }
            return actions;
        }

        private List<InventorySlot> GetAllSlots()
        {
            return [
                Crown, Face, Jewelry, Neck, Chest, Shoulders,
                Back, Arms, Gloves, Belt, Pants, Boots, Hands
            ];
        }

        public override bool Equals(object? obj)
        {
            return obj is Equipment equipment &&
                   EqualityComparer<InventorySlot>.Default.Equals(Crown, equipment.Crown) &&
                   EqualityComparer<InventorySlot>.Default.Equals(Face, equipment.Face) &&
                   EqualityComparer<InventorySlot>.Default.Equals(Jewelry, equipment.Jewelry) &&
                   EqualityComparer<InventorySlot>.Default.Equals(Neck, equipment.Neck) &&
                   EqualityComparer<InventorySlot>.Default.Equals(Chest, equipment.Chest) &&
                   EqualityComparer<InventorySlot>.Default.Equals(Shoulders, equipment.Shoulders) &&
                   EqualityComparer<InventorySlot>.Default.Equals(Back, equipment.Back) &&
                   EqualityComparer<InventorySlot>.Default.Equals(Arms, equipment.Arms) &&
                   EqualityComparer<InventorySlot>.Default.Equals(Gloves, equipment.Gloves) &&
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
            hash.Add(Jewelry);
            hash.Add(Neck);
            hash.Add(Chest);
            hash.Add(Shoulders);
            hash.Add(Back);
            hash.Add(Arms);
            hash.Add(Gloves);
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
