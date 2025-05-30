using Caps.RPG.Rules.Modifiers;
using Caps.RPG.Rules.Inventory;
using SNS.Data.DataSerializer;

namespace Caps.RPG.Rules.Attributes
{
    public enum Stat
    {
        Strength = 0,
        Agility = 1,
        Constitution = 2,
        Intellect = 3,
        Arcana = 4,
        Wisdom = 5,
        Presence = 6,
        Charisma = 7
    }

    [DataClass("AttributeSet")]
    public class AttributeSet : IGenericDataObject<AttributeSet>
    {
        private Dictionary<TargetType, List<Modifier>> modifiers;
        private bool _wasLoaded = false;

        [DataProperty("ModifiersAttr")]
        public Dictionary<TargetType, List<Modifier>> Modifiers
        {
            get { return modifiers; }
            set { modifiers = value; }
        }

        public bool WasLoaded { get { return _wasLoaded; } set { _wasLoaded = value; } }

        public AttributeSet()
        {
            modifiers = [];

            modifiers.Add(TargetType.Strength,     [new Modifier(SourceType.Base, TargetType.Strength,     ActionType.Base, [BonusType.Flat], bonus: 0)]);
            modifiers.Add(TargetType.Agility,      [new Modifier(SourceType.Base, TargetType.Agility,      ActionType.Base, [BonusType.Flat], bonus: 0)]);
            modifiers.Add(TargetType.Constitution, [new Modifier(SourceType.Base, TargetType.Constitution, ActionType.Base, [BonusType.Flat], bonus: 0)]);
            modifiers.Add(TargetType.Intellect,    [new Modifier(SourceType.Base, TargetType.Intellect,    ActionType.Base, [BonusType.Flat], bonus: 0)]);
            modifiers.Add(TargetType.Arcana,       [new Modifier(SourceType.Base, TargetType.Arcana,       ActionType.Base, [BonusType.Flat], bonus: 0)]);
            modifiers.Add(TargetType.Wisdom,       [new Modifier(SourceType.Base, TargetType.Wisdom,       ActionType.Base, [BonusType.Flat], bonus: 0)]);
            modifiers.Add(TargetType.Presence,     [new Modifier(SourceType.Base, TargetType.Wisdom,       ActionType.Base, [BonusType.Flat], bonus: 0)]);
            modifiers.Add(TargetType.Charisma,     [new Modifier(SourceType.Base, TargetType.Charisma,     ActionType.Base, [BonusType.Flat], bonus: 0)]);
        }
        public AttributeSet(int str, int agi, int con, int itl, int arc, int wis, int pre, int cha)
        {
            modifiers = [];

            modifiers.Add(TargetType.Strength,     [new Modifier(SourceType.Base, TargetType.Strength,     ActionType.Base, [BonusType.Flat], bonus: str)]);
            modifiers.Add(TargetType.Agility,      [new Modifier(SourceType.Base, TargetType.Agility,      ActionType.Base, [BonusType.Flat], bonus: agi)]);
            modifiers.Add(TargetType.Constitution, [new Modifier(SourceType.Base, TargetType.Constitution, ActionType.Base, [BonusType.Flat], bonus: con)]);
            modifiers.Add(TargetType.Intellect,    [new Modifier(SourceType.Base, TargetType.Intellect,    ActionType.Base, [BonusType.Flat], bonus: itl)]);
            modifiers.Add(TargetType.Arcana,       [new Modifier(SourceType.Base, TargetType.Arcana,       ActionType.Base, [BonusType.Flat], bonus: arc)]);
            modifiers.Add(TargetType.Wisdom,       [new Modifier(SourceType.Base, TargetType.Wisdom,       ActionType.Base, [BonusType.Flat], bonus: wis)]);
            modifiers.Add(TargetType.Presence,     [new Modifier(SourceType.Base, TargetType.Wisdom,       ActionType.Base, [BonusType.Flat], bonus: pre)]);
            modifiers.Add(TargetType.Charisma,     [new Modifier(SourceType.Base, TargetType.Charisma,     ActionType.Base, [BonusType.Flat], bonus: cha)]);
        }

        public int GetStatValue(Stat stat)
        {
            return stat switch
            {
                Stat.Strength     => Modifier.SumAll(modifiers[TargetType.Strength]),
                Stat.Agility      => Modifier.SumAll(modifiers[TargetType.Agility]),
                Stat.Constitution => Modifier.SumAll(modifiers[TargetType.Constitution]),
                Stat.Intellect    => Modifier.SumAll(modifiers[TargetType.Intellect]),
                Stat.Arcana       => Modifier.SumAll(modifiers[TargetType.Arcana]),
                Stat.Wisdom       => Modifier.SumAll(modifiers[TargetType.Wisdom]),
                Stat.Presence     => Modifier.SumAll(modifiers[TargetType.Presence]),
                Stat.Charisma     => Modifier.SumAll(modifiers[TargetType.Charisma]),
                _ => throw new ArgumentException("Invalid Stat type")
            };
        }

        public void AddModifier(Modifier modifier, object? source)
        {
            List<Modifier> targetList = modifiers[modifier.Target];
            if (source is Item s)
            {
                foreach (var mod in targetList)
                {
                    if (mod.Source.IsItem() && mod.Source.ItemType() == s.Type)
                    {
                        targetList.Remove(mod);
                        break;
                    }
                }
            }
            modifiers[modifier.Target].Add(modifier);
        }

        public int GetMaxHealth()
        {
            return this.GetStatValue(Stat.Constitution) * 10;
        }

        public int InitiativeModifier()
        {
            return this.GetStatValue(Stat.Agility);
        }

        public int MoveSpeed()
        {
            return 5 + this.GetStatValue(Stat.Agility);
        }

        public override bool Equals(object? obj)
        {
            if (obj is AttributeSet set)
            {
                bool ret = true;

                // Check if the counts of the dictionaries are the same
                ret &= Modifiers.Count == set.Modifiers.Count;

                // Check if all keys and their corresponding values are equal
                foreach (var key in Modifiers.Keys)
                {
                    if (!set.Modifiers.ContainsKey(key))
                    {
                        ret = false;
                        break;
                    }

                    // Compare the lists of modifiers for each key
                    var thisModifiers = Modifiers[key];
                    var otherModifiers = set.Modifiers[key];

                    if (thisModifiers.Count != otherModifiers.Count ||
                        !thisModifiers.SequenceEqual(otherModifiers))
                    {
                        ret = false;
                        break;
                    }
                }

                return ret;
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
