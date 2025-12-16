using Caps.RPG.Rules.Inventory;
using Caps.RPG.Rules.Modifiers;
using SNS.Data.DataSerializer;

namespace Caps.RPG.Rules.Attributes
{
    public enum Stat
    {
        None = 0,
        Strength = 1,
        Agility = 2,
        Constitution = 3,
        Intellect = 4,
        Arcana = 5,
        Wisdom = 6,
        Presence = 7,
        Charisma = 8
    }

    //public static class StatExtensions
    //{
    //    public static string ToString(this Stat stat)
    //    {
    //        return stat.ToString();
    //    }
    //}

    [DataClass("AttributeSet")]
    public class AttributeSet : IGenericDataObject<AttributeSet>
    {
        private List<Modifier> modifiers = [];
        private bool _wasLoaded = false;

        //[DataProperty("ModifiersAttr")]
        public List<Modifier> Modifiers
        {
            get { return modifiers; }
            set { modifiers = value; }
        }

        public bool WasLoaded { get { return _wasLoaded; } set { _wasLoaded = value; } }

        public AttributeSet() { }
        public AttributeSet(int str, int agi, int con, int itl, int arc, int wis, int pre, int cha)
        {
            this[Stat.Strength]     = [new(Stat.Strength, str)];
            this[Stat.Agility]      = [new(Stat.Agility, agi)];
            this[Stat.Constitution] = [new(Stat.Constitution, con)];
            this[Stat.Intellect]    = [new(Stat.Intellect, itl)];
            this[Stat.Arcana]       = [new(Stat.Arcana, arc)];
            this[Stat.Wisdom]       = [new(Stat.Wisdom, wis)];
            this[Stat.Presence]     = [new(Stat.Presence, pre)];
            this[Stat.Charisma]     = [new(Stat.Charisma, cha)];
        }

        public List<Modifier> this[Stat stat]
        {
            get {
                return [.. modifiers.Where(m => m.Target.ToStat() == stat)];
            }
            set { modifiers.AddRange(value); }
        }

        public int SumModifiers(Stat stat)
        {
            return Modifier.SumAll(this[stat]);
        }

        public void AddModifier(Modifier modifier, object? source)
        {
            List<Modifier> targetList = this[modifier.Target.ToStat()];
            if (source is Item s)
            {
                foreach (var mod in targetList.ToList())
                {
                    if (mod.Source.IsItem() && mod.Source.ItemType() == s.Type)
                    {
                        targetList.Remove(mod);
                        break;
                    }
                }
            }
            this[modifier.Target.ToStat()].Add(modifier);
        }

        public override bool Equals(object? obj)
        {
            if (obj is AttributeSet set)
            {
                // Compare counts first
                if (Modifiers.Count != set.Modifiers.Count)
                    return false;

                // Compare each modifier in order
                for (int i = 0; i < Modifiers.Count; i++)
                {
                    if (!Modifiers[i].Equals(set.Modifiers[i]))
                        return false;
                }

                return true;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Modifiers);
        }

        public static bool operator ==(AttributeSet? left, AttributeSet? right)
        {
            return EqualityComparer<AttributeSet>.Default.Equals(left, right);
        }

        public static bool operator !=(AttributeSet? left, AttributeSet? right)
        {
            return !(left == right);
        }
    }
}
