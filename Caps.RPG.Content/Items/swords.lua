Swords = {
	HugeSword = {
        Sword = {
            Name = "Very Big Sword",
            Description = "Hits very hard.",
            Type = ItemType.Hands,
            Modifiers = {
                {
                    Source = SourceType.Hands,
                    Target = TargetType.AttackBonus,
                    Type = ActionType.Set,
                    Bonus = 25
                },
                {
                    Source = SourceType.Hands,
                    Target = TargetType.AttackDamage,
                    Type = ActionType.Set,
                    Bonus = 15
                }
            }
        }
    }
}
