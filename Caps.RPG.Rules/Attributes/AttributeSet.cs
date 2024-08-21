
using Caps.RPG.Engine.Modifiers;
using Caps.RPG.Rules.Inventory;

namespace Caps.RPG.Rules.Attributes
{
    public enum Stat
    {
        Strength,
        Agility,
        Constitution,
        Intellect,
        Arcana,
        Wisdom,
        Presence,
        Charisma
    }
    public class AttributeSet
    {
        private readonly Dictionary<Modifier.TargetType, List<Modifier>> modifiers;

        public AttributeSet()
        {
            modifiers = [];

            modifiers.Add(Modifier.TargetType.Strength, [new Modifier("Base", Modifier.TargetType.Strength, Modifier.ActionType.Base, [Modifier.BonusType.Flat], bonus: 0)]);
            modifiers.Add(Modifier.TargetType.Agility, [new Modifier("Base", Modifier.TargetType.Agility, Modifier.ActionType.Base, [Modifier.BonusType.Flat], bonus: 0)]);
            modifiers.Add(Modifier.TargetType.Constitution, [new Modifier("Base", Modifier.TargetType.Constitution, Modifier.ActionType.Base, [Modifier.BonusType.Flat], bonus: 0)]);
            modifiers.Add(Modifier.TargetType.Intellect, [new Modifier("Base", Modifier.TargetType.Intellect, Modifier.ActionType.Base, [Modifier.BonusType.Flat], bonus: 0)]);
            modifiers.Add(Modifier.TargetType.Arcana, [new Modifier("Base", Modifier.TargetType.Arcana, Modifier.ActionType.Base, [Modifier.BonusType.Flat], bonus: 0)]);
            modifiers.Add(Modifier.TargetType.Wisdom, [new Modifier("Base", Modifier.TargetType.Wisdom, Modifier.ActionType.Base, [Modifier.BonusType.Flat], bonus: 0)]);
            modifiers.Add(Modifier.TargetType.Presence, [new Modifier("Base", Modifier.TargetType.Wisdom, Modifier.ActionType.Base, [Modifier.BonusType.Flat], bonus: 0)]);
            modifiers.Add(Modifier.TargetType.Charisma, [new Modifier("Base", Modifier.TargetType.Charisma, Modifier.ActionType.Base, [Modifier.BonusType.Flat], bonus: 0)]);
        }
        public AttributeSet(int str, int agi, int con, int itl, int arc, int wis, int pre, int cha)
        {
            modifiers = [];

            modifiers.Add(Modifier.TargetType.Strength, [new Modifier("Base", Modifier.TargetType.Strength, Modifier.ActionType.Base, [Modifier.BonusType.Flat], bonus: str)]);
            modifiers.Add(Modifier.TargetType.Agility, [new Modifier("Base", Modifier.TargetType.Agility, Modifier.ActionType.Base, [Modifier.BonusType.Flat], bonus: agi)]);
            modifiers.Add(Modifier.TargetType.Constitution, [new Modifier("Base", Modifier.TargetType.Constitution, Modifier.ActionType.Base, [Modifier.BonusType.Flat], bonus: con)]);
            modifiers.Add(Modifier.TargetType.Intellect, [new Modifier("Base", Modifier.TargetType.Intellect, Modifier.ActionType.Base, [Modifier.BonusType.Flat], bonus: itl)]);
            modifiers.Add(Modifier.TargetType.Arcana, [new Modifier("Base", Modifier.TargetType.Arcana, Modifier.ActionType.Base, [Modifier.BonusType.Flat], bonus: arc)]);
            modifiers.Add(Modifier.TargetType.Wisdom, [new Modifier("Base", Modifier.TargetType.Wisdom, Modifier.ActionType.Base, [Modifier.BonusType.Flat], bonus: wis)]);
            modifiers.Add(Modifier.TargetType.Presence, [new Modifier("Base", Modifier.TargetType.Wisdom, Modifier.ActionType.Base, [Modifier.BonusType.Flat], bonus: pre)]);
            modifiers.Add(Modifier.TargetType.Charisma, [new Modifier("Base", Modifier.TargetType.Charisma, Modifier.ActionType.Base, [Modifier.BonusType.Flat], bonus: cha)]);
        }

        public int GetStatValue(Stat stat)
        {
            return stat switch
            {
                Stat.Strength => Modifier.SumAll(modifiers[Modifier.TargetType.Strength]),
                Stat.Agility => Modifier.SumAll(modifiers[Modifier.TargetType.Agility]),
                Stat.Constitution => Modifier.SumAll(modifiers[Modifier.TargetType.Constitution]),
                Stat.Intellect => Modifier.SumAll(modifiers[Modifier.TargetType.Intellect]),
                Stat.Arcana => Modifier.SumAll(modifiers[Modifier.TargetType.Arcana]),
                Stat.Wisdom => Modifier.SumAll(modifiers[Modifier.TargetType.Wisdom]),
                Stat.Presence => Modifier.SumAll(modifiers[Modifier.TargetType.Presence]),
                Stat.Charisma => Modifier.SumAll(modifiers[Modifier.TargetType.Charisma]),
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
                    if (mod.Source is Item item && item.Type == s.Type)
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

    }
}
