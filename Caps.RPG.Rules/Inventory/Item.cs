using Caps.RPG.Engine.Modifiers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Caps.RPG.Rules.Inventory
{
    public enum ItemType
    {
        None,
        Any,
        Crown,
        Face,
        HeadJewelry,
        Neck,
        Chest,
        Shoulders,
        Back,
        Arms,
        Gloves,
        HandJewelry,
        Belt,
        Pants,
        Boots,
        Hands,
    }
    public class Item
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public ItemType Type { get; set; }

        public Item(string name, string description, ItemType type)
        {
            this.Name = name;
            this.Description = description;
            this.Type = type;
        }

        public virtual Modifier[] GetModifiers()
        {
            return [];
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
