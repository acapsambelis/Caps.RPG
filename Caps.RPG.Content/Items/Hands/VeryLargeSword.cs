using Caps.RPG.Rules.Modifiers;
using Caps.RPG.Rules.Inventory;


namespace Caps.RPG.Content.Items.Hands
{
    public class VeryLargeSword : Sword
    {
        public static readonly VeryLargeSword Item = new VeryLargeSword("Very Large Sword", "Hits very hard.", ItemType.Hands);

        public VeryLargeSword(string name, string description, ItemType type) : base(name, description, type)
        {
            this.Modifiers = [
                new Modifier(SourceType.Hands, TargetType.AttackBonus,  ActionType.Set, [BonusType.Flat], bonus: 25),
                new Modifier(SourceType.Hands, TargetType.AttackDamage, ActionType.Set, [BonusType.Flat], bonus: 15)
            ];
        }
    }
}
