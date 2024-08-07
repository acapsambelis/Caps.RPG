using Caps.RPG.Engine.Modifiers;
using Caps.RPG.Rules.Inventory;


namespace Caps.RPG.Content.Items.Hands
{
    public class VeryLargeSword : Sword
    {
        public static readonly VeryLargeSword Item = new VeryLargeSword("Very Large Sword", "Hits very hard.", ItemType.Hands);

        public VeryLargeSword(string name, string description, ItemType type) : base(name, description, type)
        {
        }
        public override Modifier[] GetModifiers()
        {
            return [
                new Modifier(this, Modifier.TargetType.AttackBonus,  Modifier.ActionType.Set, [Modifier.BonusType.Flat], bonus: 25),
                new Modifier(this, Modifier.TargetType.AttackDamage, Modifier.ActionType.Set, [Modifier.BonusType.Flat], bonus: 25)
            ];
        }
    }
}
