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

    public enum TargetType
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
    }

    public enum ActionType
    {
        None = 0,
        Base = 1,
        Set = 2,
        Bonus = 3,
    }

    public enum BonusType
    {
        None = 0,
        Flat = 1,
        Die = 2,
        Stat = 3,
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
        private TargetType target;
        private ActionType actionType;
        private BonusType[] typesUsed;
        private int bonus;
        private Dictionary<Die, int> dice;
        private Stat stat;
        private SourceType source;
        #endregion

        #region PublicMembers
        [DataProperty("Target")]
        public TargetType Target
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
        [DataProperty("Bonus")]
        public int Bonus
        {
            get { return bonus; }
            set { bonus = value; }
        }
        //[DataProperty("Dice")]
        public Dictionary<Die, int> Dice
        {
            get { return dice; }
            set { dice = value; }
        }
        [DataProperty("Stat")]
        public Stat Stat
        {
            get { return stat; }
            set { stat = value; }
        }
        [DataProperty("Source")]
        public SourceType Source
        {
            get { return source; }
            set { source = value; }
        }

        private bool _wasLoaded = false;
        public bool WasLoaded { get { return _wasLoaded; } set { _wasLoaded = value; } }
        #endregion

        #region Constructors
        public Modifier() { }

        public Modifier(
            SourceType source,
            TargetType target,
            ActionType actionType,
            BonusType[] typesUsed,
            Dictionary<Die, int>? dice = null,
            int? bonus = null,
            Stat? stat = null
        )
        {
            this.source = source;
            this.target = target;
            this.actionType = actionType;

            this.typesUsed = typesUsed;
            this.dice = dice ?? [];
            this.bonus = bonus != null ? (int)bonus : 0;
            this.stat = stat ?? new Stat();
        }
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
                if (!modifier.typesUsed.Contains(BonusType.Flat)) continue;

                if (modifier.Type == ActionType.Base)
                {
                    floor = Math.Max(floor, modifier.Bonus);
                }
                if (modifier.Type == ActionType.Bonus)
                {
                    bonus += modifier.Bonus;
                }
                if (modifier.Type == ActionType.Set)
                {
                    set = Math.Max(set, modifier.Bonus);
                }
            }
            return Math.Max(floor + bonus, set);
        }
        public static Dictionary<Die, int> SumModifierDie(List<Modifier> modifiers)
        {
            Dictionary<Die, int> mods = [];
            foreach (Modifier modifier in modifiers)
            {
                if (!modifier.typesUsed.Contains(BonusType.Die)) continue;

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
                if (!modifier.typesUsed.Contains(BonusType.Stat)) continue;

                if (modifier.Type == ActionType.Base)
                {
                    floor = Math.Max(floor, attributes.GetStatValue(modifier.Stat));
                }
                if (modifier.Type == ActionType.Bonus)
                {
                    bonus += attributes.GetStatValue(modifier.Stat);
                }
                if (modifier.Type == ActionType.Set)
                {
                    set = Math.Max(set, modifier.Bonus);
                }
            }
            return Math.Max(floor + bonus, set);
        }

        #region StandardModifiers

        public static Dictionary<TargetType, List<Modifier>> GetCreatureModifiers()
        {
            var clone = new Dictionary<TargetType, List<Modifier>>(CreatureModifiers.Count, CreatureModifiers.Comparer);
            foreach (var kvp in CreatureModifiers)
            {
                clone[kvp.Key] = new List<Modifier>(kvp.Value);
            }
            return clone;
        }

        private static readonly Dictionary<TargetType, List<Modifier>> CreatureModifiers = new()
        {
            { TargetType.DefenseClass, new List<Modifier>() { new(SourceType.Base, TargetType.DefenseClass, ActionType.Base,  [BonusType.Flat], bonus:10) } },
            { TargetType.AttackDamage, new List<Modifier>() { new(SourceType.Base, TargetType.AttackDamage, ActionType.Bonus, [BonusType.Die], dice: new Dictionary<Die, int> {{ Die.D4, 1 }}) } },
            { TargetType.AttackBonus,  new List<Modifier>() { new(SourceType.Base, TargetType.AttackBonus,  ActionType.Base,  [BonusType.Flat, BonusType.Stat], bonus:1, stat: Stat.Strength )} }
        };

        #endregion

        #region Overrides

        public override bool Equals(object? obj)
        {
            return obj is Modifier modifier &&
                   Target == modifier.Target &&
                   Type == modifier.Type &&
                   Bonus == modifier.Bonus &&
                   Dice.Count == modifier.Dice.Count && !Dice.Except(modifier.Dice).Any() &&
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
