using Caps.RPG.Rules.Inventory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Caps.RPG.Content.Items
{
    public class Sword : Item, Weapon
    {
        public Sword(string name, string description, ItemType type) : base(name, description, type)
        {
        }
    }
}
