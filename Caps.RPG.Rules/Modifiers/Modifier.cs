using Caps.RPG.Rules.Attributes;
using Caps.RPG.Rules.Helpers;
using Caps.RPG.Rules.Inventory;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Caps.RPG.Engine.Modifiers
{
    public class Modifier
    {

        public enum TargetType
        {
            None,
            DefenseClass,
            AttackDamage,
            AttackBonus,
        }

        public enum ActionType
        {
            None,
            Base,
            Set,
            Bonus,
        }

        public enum BonusType
        {
            None,
            Flat,
            Die,
            Stat,
        }

        #region privateMembers
        private TargetType target;
        private ActionType actionType;
        private BonusType[] typesUsed;
        private int bonus;
        private Dictionary<Die, int> dice;
        private Stat stat;
        private object source;
        #endregion

        #region PublicMembers
        public TargetType Target
        {
            get { return target; }
        }
        public ActionType Type
        {
            get { return actionType; }
        }
        public int Bonus
        {
            get { return bonus; }
        }
        public Dictionary<Die, int> Dice
        {
            get { return dice; }
        }
        public Stat Stat
        {
            get { return stat; }
        }
        public object Source
        {
            get { return source; }
            set { source = value; }
        }
        #endregion

        #region Constructors
        public Modifier(
            object source,
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
            this.dice = dice != null ? dice : new Dictionary<Die, int>();
            this.bonus = bonus != null ? (int)bonus : 0;
            this.stat = stat != null ? stat : new Stat();
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
            Dictionary<Die, int> mods = new Dictionary<Die, int>();
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

        private static readonly Dictionary<TargetType, List<Modifier>> CreatureModifiers = new Dictionary<TargetType, List<Modifier>>()
        {
            { TargetType.DefenseClass, new List<Modifier>() { new Modifier(new Item("Unarmored", "You are wearing no armor.",  ItemType.Chest), TargetType.DefenseClass, ActionType.Base,  [BonusType.Flat], bonus:10) } },
            { TargetType.AttackDamage, new List<Modifier>() { new Modifier(new Item("Unarmed", "You are wielding no weapons.", ItemType.Hands), TargetType.AttackDamage, ActionType.Bonus, [BonusType.Die], dice: new Dictionary<Die, int> {{ new Die.DFour(), 1 }}) } },
            { TargetType.AttackBonus,  new List<Modifier>() { new Modifier(new Item("Unarmed", "You are wielding no weapons.", ItemType.Hands), TargetType.AttackBonus,  ActionType.Base,  [BonusType.Flat, BonusType.Stat], bonus:1, stat:new Strength() )} }
        };

        #endregion

    }
}
