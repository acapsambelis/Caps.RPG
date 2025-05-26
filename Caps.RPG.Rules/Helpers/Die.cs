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
            return Helpers.Roll.RollDie(die.Size());
        }
    }
}
