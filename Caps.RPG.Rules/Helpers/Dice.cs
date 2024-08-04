using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Caps.RPG.Rules.Helpers
{
    public class Dice
    {
        private int size;
        public Dice(int size)
        {
            this.size = size;
        }

        public int Roll()
        {
            return CombatEngine.Helpers.Roll.RollDie(size);
        }


        public class DFour : Dice
        {
            public DFour() : base(4)
            {
            }
        }
        public class DSix : Dice
        {
            public DSix() : base(6)
            {
            }
        }
        public class DEight : Dice
        {
            public DEight() : base(8)
            {
            }
        }

        public class DTen : Dice
        {
            public DTen() : base(10)
            {
            }
        }
        public class DTwelve : Dice
        {
            public DTwelve() : base(12)
            {
            }
        }

    }
}
