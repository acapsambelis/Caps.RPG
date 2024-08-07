
namespace Caps.RPG.Rules.Attributes
{
    public class Constitution : Stat
    {
        public Constitution() : base() { }
        public Constitution(int constitution) : base(constitution) { }

        public int GetMaxHealth()
        {
            return this.value * 10;
        }
    }
}
