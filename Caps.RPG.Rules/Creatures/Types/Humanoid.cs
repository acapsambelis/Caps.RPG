using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Caps.RPG.Rules.Creatures.Types
{
    internal interface Humanoid
    {
        public enum Species
        {
            None,
            Human,
            Elf,
            Dwarf,
            Dragonborn,
            Orc,
            Goblin
        }
    }
}
