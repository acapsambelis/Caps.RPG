using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Caps.RPG.Rules.Helpers
{
    public class Roll
    {
        private static readonly Random random = new Random();
        public static int RollDie(int size)
        {
            return random.Next(size) + 1;
        }
    }
}
