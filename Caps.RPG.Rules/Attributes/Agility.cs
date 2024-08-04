using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Caps.RPG.Rules.Attributes
{
    public class Agility : Stat
    {
        public Agility() : base() { }
        public Agility(int agility) : base(agility) { }

        public int InitiativeModifier()
        {
            return base.value;
        }
    }
}
