using Caps.RPG.Rules.Modifiers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Caps.RPG.Rules.Inventory
{
    public class ItemTag
    {
        private string _name;
        private string _description;
        private List<Modifier> _modifiers;

        public string Name
        {
            get => _name;
            set => _name = value;
        }
        public string Description
        {
            get => _description;
            set => _description = value;
        }
        public List<Modifier> Modifiers {
            get => _modifiers;
            set => _modifiers = value;
        }

        public ItemTag()
        {
            _name = string.Empty;
            _description = string.Empty;
            Modifiers = [];
        }
    }
}
