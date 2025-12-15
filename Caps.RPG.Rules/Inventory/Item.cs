using Caps.RPG.Rules.Creatures.Actions;
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
        Jewelry = 4,
        Neck = 5,
        Chest = 6,
        Shoulders = 7,
        Back = 8,
        Arms = 9,
        Gloves = 10,
        Belt = 11,
        Pants = 12,
        Boots = 13,
        Hands = 14,
    }

    public static class ItemTypeExtensions
    {
        public static SourceType SourceType(this ItemType type)
        {
            return type switch
            {
                ItemType.Crown => Modifiers.SourceType.Crown,
                ItemType.Face => Modifiers.SourceType.Face,
                ItemType.Jewelry => Modifiers.SourceType.Jewelry,
                ItemType.Neck => Modifiers.SourceType.Neck,
                ItemType.Chest => Modifiers.SourceType.Chest,
                ItemType.Shoulders => Modifiers.SourceType.Shoulders,
                ItemType.Back => Modifiers.SourceType.Back,
                ItemType.Arms => Modifiers.SourceType.Arms,
                ItemType.Gloves => Modifiers.SourceType.Gloves,
                ItemType.Belt => Modifiers.SourceType.Belt,
                ItemType.Pants => Modifiers.SourceType.Pants,
                ItemType.Boots => Modifiers.SourceType.Boots,
                ItemType.Hands => Modifiers.SourceType.Hands,
                _ => Modifiers.SourceType.None
            };
        }
    }

    [DataClass("Items")]
    public class Item : IGenericDataObject<Item>
    {
        private string name;
        private string description;
        private ItemType type;
        private ItemTag[] itemTags;
        private Modifier[] modifiers;
        private CombatAction[] combatActions;

        private bool modifiersChanged = true;

        [DataProperty("Name")]
        public string Name
        {
            get => name;
            set => name = value;
        }
        [DataProperty("Description")]
        public string Description
        {
            get => description;
            set => description = value;
        }
        [DataProperty("Type")]
        public ItemType Type
        {
            get => type;
            set => type = value;
        }
        [SubDataObject("Tags")]
        public ItemTag[] Tags
        {
            get => itemTags;
            set
            {
                itemTags = value; 
                modifiersChanged = true;
                foreach (var tag in itemTags)
                {
                    tag.Modifiers.ForEach(mod => mod.Source = Type.SourceType());
                }
            }
        }
        [DataProperty("Modifiers")]
        public Modifier[] Modifiers
        {
            get
            {
                if (!modifiersChanged)
                    return modifiers;
                List<Modifier> allModifiers = [];
                foreach (var tag in Tags)
                {
                    allModifiers.AddRange(tag.Modifiers);
                }
                allModifiers.AddRange(modifiers);
                modifiers = [.. allModifiers];
                modifiersChanged = false;
                return modifiers;
            }
            set { modifiers = value; modifiersChanged = true; }
        }
        [DataProperty("CombatActions")]
        public CombatAction[] CombatActions
        {
            get => combatActions;
            set => combatActions = value;
        }

        private bool _wasLoaded = false;
        public bool WasLoaded { get { return _wasLoaded; } set { _wasLoaded = value; } }


        public Item()
        {
            Name = "";
            Description = "";
            Type = ItemType.None;
            Modifiers = [];
            CombatActions = [];
            Tags = [];
        }
        public Item(string name, string description, ItemType type)
        {
            Name = name;
            Description = description;
            Type = type;
            Modifiers = [];
            Tags = [];
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
