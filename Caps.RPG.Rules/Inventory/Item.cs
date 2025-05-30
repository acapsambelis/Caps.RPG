using Caps.RPG.Rules.Modifiers;
using SNS.Data.DataSerializer;

namespace Caps.RPG.Rules.Inventory
{
    public enum ItemType
    {
        None = 0,
        Any = 1,
        Crown = 2,
        Face = 3,
        HeadJewelry = 4,
        Neck = 5,
        Chest = 6,
        Shoulders = 7,
        Back = 8,
        Arms = 9,
        Gloves = 10,
        HandJewelry = 11,
        Belt = 12,
        Pants = 13,
        Boots = 14,
        Hands = 15,
    }

    [DataClass("Items")]
    public class Item : IGenericDataObject<Item>
    {
        [DataProperty("Name")]
        public string Name { get; set; }
        [DataProperty("Description")]
        public string Description { get; set; }
        [DataProperty("Type")]
        public ItemType Type { get; set; }
        [DataProperty("Modifiers")]
        public Modifier[] Modifiers { get; set; }

        private bool _wasLoaded = false;
        public bool WasLoaded { get { return _wasLoaded; } set { _wasLoaded = value; } }

        public Item()
        {
            Name = "";
            Description = "";
            Type = ItemType.None;
            Modifiers = [];
        }
        public Item(string name, string description, ItemType type)
        {
            Name = name;
            Description = description;
            Type = type;
            Modifiers = [];
        }

        public override string ToString()
        {
            return Name;
        }

        public override bool Equals(object? obj)
        {
            if (obj is not Item item)
                return false;

            bool equals = true;
            equals &= Name == item.Name;
            equals &= Description == item.Description;
            equals &= Type == item.Type;
            equals &= EqualityComparer<Modifier[]>.Default.Equals(Modifiers, item.Modifiers);

            return equals;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Name, Description, Type, Modifiers);
        }

        public static bool operator ==(Item? left, Item? right)
        {
            return EqualityComparer<Item>.Default.Equals(left, right);
        }

        public static bool operator !=(Item? left, Item? right)
        {
            return !(left == right);
        }
    }
}
