using Caps.RPG.Rules.Attributes;
using Caps.RPG.Rules.Helpers;
using Caps.RPG.Rules.Inventory;
using SNS.Data.DataSerializer;

namespace Caps.RPG.Rules.Modifiers
{
    public enum SourceType
    {
        None = 0,
        Base = 1,
        Crown = 2,
        Face = 3,
        HeadJewelry = 4,
        Neck = 5,
        Chest = 6,
        Shoulders = 7,
        Back = 8,
        Arms = 9,
        Gloves = 10,
        HandJewelry = 11,
        Belt = 12,
        Pants = 13,
        Boots = 14,
        Hands = 15,
    }

    public enum ModifiedValue
    {
        None = 0,
        Strength = 1,
        Agility = 2,
        Constitution = 3,
        Intellect = 4,
        Arcana = 5,
        Wisdom = 6,
        Presence = 7,
        Charisma = 8,
        DefenseClass = 9,
        AttackDamage = 10,
        AttackBonus = 11,
        Initiative = 12,
        MovementSpeed = 13,
        MaxHealth = 14,
    }

    public static class TargetTypeExtensions
    {
        public static bool IsStat(this ModifiedValue type)
        {
            return (int)type >= 1 && (int)type <= 8;
        }
        public static Stat ToStat(this ModifiedValue type)
        {
            return type switch
            {
                ModifiedValue.Strength => Stat.Strength,
                ModifiedValue.Agility => Stat.Agility,
                ModifiedValue.Constitution => Stat.Constitution,
                ModifiedValue.Intellect => Stat.Intellect,
                ModifiedValue.Arcana => Stat.Arcana,
                ModifiedValue.Wisdom => Stat.Wisdom,
                ModifiedValue.Presence => Stat.Presence,
                ModifiedValue.Charisma => Stat.Charisma,
                _ => Stat.None
            };
        }

        public static ModifiedValue ToTargetType(this Stat stat)
        {
            return stat switch
            {
                Stat.Strength => ModifiedValue.Strength,
                Stat.Agility => ModifiedValue.Agility,
                Stat.Constitution => ModifiedValue.Constitution,
                Stat.Intellect => ModifiedValue.Intellect,
                Stat.Arcana => ModifiedValue.Arcana,
                Stat.Wisdom => ModifiedValue.Wisdom,
                Stat.Presence => ModifiedValue.Presence,
                Stat.Charisma => ModifiedValue.Charisma,
                _ => ModifiedValue.None
            };
        }
    }

    public enum ActionType
    {
        None = 0,
        Base = 1,
        Set = 2,
        Bonus = 3,
    }

    public static class SourceTypeExtensions
    {
        public static bool IsItem(this SourceType type)
        {
            return (int)type >= 2 && (int)type <= 15;
        }

        public static ItemType ItemType(this SourceType type)
        {
            return type switch
            {
                SourceType.Crown       => Inventory.ItemType.Crown,
                SourceType.Face        => Inventory.ItemType.Face,
                SourceType.HeadJewelry => Inventory.ItemType.HeadJewelry,
                SourceType.Neck        => Inventory.ItemType.Neck,
                SourceType.Chest       => Inventory.ItemType.Chest,
                SourceType.Shoulders   => Inventory.ItemType.Shoulders,
                SourceType.Back        => Inventory.ItemType.Back,
                SourceType.Arms        => Inventory.ItemType.Arms,
                SourceType.Gloves      => Inventory.ItemType.Gloves,
                SourceType.HandJewelry => Inventory.ItemType.HandJewelry,
                SourceType.Belt        => Inventory.ItemType.Belt,
                SourceType.Pants       => Inventory.ItemType.Pants,
                SourceType.Boots       => Inventory.ItemType.Boots,
                SourceType.Hands       => Inventory.ItemType.Hands,
                _ => Inventory.ItemType.None
            };
        }
    }

    [DataClass("Modifiers")]
    public class Modifier : IGenericDataObject<Modifier>
    {
        #region privateMembers
        private SourceType source = SourceType.Base;
        private ModifiedValue target;
        private ActionType actionType = ActionType.Base;
        private int? bonus = null;
        private Dictionary<Die, int>? dice = null;
        private Stat? stat = null;
        #endregion

        #region PublicMembers
        [DataProperty("Source")]
        public SourceType Source
        {
            get { return source; }
            set { source = value; }
        }
        [DataProperty("Target")]
        public ModifiedValue Target
        {
            get { return target; }
            set { target = value; }
        }
        [DataProperty("ActionType")]
        public ActionType Type
        {
            get { return actionType; }
            set { actionType = value; }
        }
        //[DataProperty("Bonus")]
        //public int? Bonus
        //{
        //    get { return bonus; }
        //    set { bonus = value; }
        //}
        public int Bonus { get; set; }
        [DataProperty("Dice")]
        public Dictionary<Die, int>? Dice
        {
            get { return dice; }
            set { dice = value; }
        }
        [DataProperty("Stat")]
        public Stat? Stat
        {
            get { return stat; }
            set { stat = value; }
        }

        private bool _wasLoaded = false;
        public bool WasLoaded { get { return _wasLoaded; } set { _wasLoaded = value; } }
        #endregion

        #region Constructors
        public Modifier() { }

        public Modifier(
            SourceType source,
            ModifiedValue target,
            ActionType actionType,
            Dictionary<Die, int>? dice = null,
            int? bonus = null,
            Stat? stat = null
        )
        {
            this.source = source;
            this.target = target;
            this.actionType = actionType;

            this.dice = dice;
            this.bonus = bonus;
            this.stat = stat;
        }

        public Modifier(Stat statTarget, int bonus) : this(SourceType.Base, statTarget.ToTargetType(), ActionType.Base, bonus: bonus) { }
        #endregion

        public static int SumAll(List<Modifier> modifiers, AttributeSet attributes)
        {
            int sum = 0;
            sum += SumModifierFlat(modifiers);
            Dictionary<Die, int> diceResult = SumModifierDie(modifiers);
            foreach (Die d in diceResult.Keys)
            {
                for (int i = 0; i < diceResult[d]; i++)
                {
                    sum += d.Roll();
                }
            }
            sum += SumModifierStat(modifiers, attributes);
            return sum;
        }
        public static int SumAll(List<Modifier> modifiers)
        {
            int sum = 0;
            sum += SumModifierFlat(modifiers);
            Dictionary<Die, int> diceResult = SumModifierDie(modifiers);
            foreach (Die d in diceResult.Keys)
            {
                for (int i = 0; i < diceResult[d]; i++)
                {
                    sum += d.Roll();
                }
            }
            return sum;
        }
        public static int SumModifierFlat(List<Modifier> modifiers)
        {
            int floor = 0; int bonus = 0; int set = 0;
            foreach (Modifier modifier in modifiers)
            {
                if (modifier.Bonus == null) continue;

                if (modifier.Type == ActionType.Base)
                {
                    floor = Math.Max(floor, (int)modifier.Bonus);
                }
                if (modifier.Type == ActionType.Bonus)
                {
                    bonus += (int)modifier.Bonus;
                }
                if (modifier.Type == ActionType.Set)
                {
                    set = Math.Max(set, (int)modifier.Bonus);
                }
            }
            return Math.Max(floor + bonus, set);
        }
        public static Dictionary<Die, int> SumModifierDie(List<Modifier> modifiers)
        {
            Dictionary<Die, int> mods = [];
            foreach (Modifier modifier in modifiers)
            {
                if (modifier.Dice == null) continue;

                if (modifier.Type == ActionType.Bonus)
                {
                    foreach (Die d in modifier.Dice.Keys)
                    {
                        if (!mods.ContainsKey(d)) mods[d] = 0;
                        mods[d] += modifier.Dice[d];
                    }
                }
            }
            return mods;
        }
        public static int SumModifierStat(List<Modifier> modifiers, AttributeSet attributes)
        {
            int floor = 0; int bonus = 0; int set = 0;
            foreach (Modifier modifier in modifiers)
            {
                if (modifier.Stat == null) continue;

                if (modifier.Type == ActionType.Base)
                {
                    floor = Math.Max(floor, attributes.SumModifiers((Stat)modifier.Stat));
                }
                if (modifier.Type == ActionType.Bonus)
                {
                    bonus += attributes.SumModifiers((Stat)modifier.Stat);
                }
                if (modifier.Type == ActionType.Set)
                {
                    set = Math.Max(set, attributes.SumModifiers((Stat)modifier.Stat));
                }
            }
            return Math.Max(floor + bonus, set);
        }

        #region StandardModifiers

        public static Dictionary<ModifiedValue, List<Modifier>> GetCreatureModifiers()
        {
            var clone = new Dictionary<ModifiedValue, List<Modifier>>(CreatureModifiers.Count, CreatureModifiers.Comparer);
            foreach (var kvp in CreatureModifiers)
            {
                clone[kvp.Key] = [.. kvp.Value];
            }
            return clone;
        }

        private static readonly Dictionary<ModifiedValue, List<Modifier>> CreatureModifiers = new()
        {
            { ModifiedValue.DefenseClass, new List<Modifier>() { new(SourceType.Base, ModifiedValue.DefenseClass, ActionType.Base,  bonus:10) } },
            { ModifiedValue.AttackDamage, new List<Modifier>() { new(SourceType.Base, ModifiedValue.AttackDamage, ActionType.Bonus, dice: new Dictionary<Die, int> {{ Die.D4, 1 }}) } },
            { ModifiedValue.AttackBonus,  new List<Modifier>() { new(SourceType.Base, ModifiedValue.AttackBonus,  ActionType.Base,  bonus:1, stat: Attributes.Stat.Strength )} }
        };

        #endregion

        #region Overrides

        public override bool Equals(object? obj)
        {
            return obj is Modifier modifier &&
                   Target == modifier.Target &&
                   Type == modifier.Type &&
                   Bonus == modifier.Bonus &&
                   ((Dice == null && modifier.Dice == null) ||
                    (Dice != null && modifier.Dice != null &&
                     Dice.Count == modifier.Dice.Count && !Dice.Except(modifier.Dice).Any())) &&
                   Stat == modifier.Stat &&
                   Source == modifier.Source;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Target, Type, Bonus, Dice, Stat, Source);
        }

        public static bool operator ==(Modifier? left, Modifier? right)
        {
            return EqualityComparer<Modifier>.Default.Equals(left, right);
        }

        public static bool operator !=(Modifier? left, Modifier? right)
        {
            return !(left == right);
        }

        #endregion
    }
}
