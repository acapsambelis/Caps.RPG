
namespace Caps.RPG.Rules.Helpers
{
    public class Die
    {
        private int size;
        public Die(int size)
        {
            this.size = size;
        }

        public virtual int Roll()
        {
            return Rules.Helpers.Roll.RollDie(size);
        }


        public class DFlat : Die
        {
            private int x;
            public DFlat(int x) : base(x) { this.x = x; }

            public override int Roll() { return x; }
        }

        public class DFour : Die
        {
            public DFour() : base(4) { }
        }
        public class DSix : Die
        {
            public DSix() : base(6) { }
        }
        public class DEight : Die
        {
            public DEight() : base(8) { }
        }

        public class DTen : Die
        {
            public DTen() : base(10) { }
        }
        public class DTwelve : Die
        {
            public DTwelve() : base(12) { }
        }
        public class DTwenty : Die
        {
            public DTwenty() : base(20) { }
        }

    }
}
