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

        public Item() { }
        public Item(string name, string description, ItemType type)
        {
            this.Name = name;
            this.Description = description;
            this.Type = type;
            this.Modifiers = [];
        }

        public virtual Modifier[] GetModifiers()
        {
            return Modifiers;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
