

namespace Caps.RPG.Rules.Helpers
{
    public enum Die
    {
        D4 = 4,
        D6 = 6,
        D8 = 8,
        D10 = 10,
        D12 = 12,
        D20 = 20,
    }

    public static class DieExtensions
    {
        public static int Size(this Die die)
        {
            return (int)die;
        }

        public static int Roll(this Die die)
        {
            return new DieInterior(die.Size()).Roll();
        }
    }

    internal class DieInterior
    {
        private readonly int size;
        public DieInterior(int size)
        {
            this.size = size;
        }

        public override bool Equals(object? obj)
        {
            return obj is DieInterior die &&
                   size == die.size;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(size);
        }

        public virtual int Roll()
        {
            return Helpers.Roll.RollDie(size);
        }
    }
}
