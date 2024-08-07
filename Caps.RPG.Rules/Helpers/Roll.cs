
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
