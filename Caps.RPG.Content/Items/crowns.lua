Crowns = {
    RoyalCrown = {
        Item = {
            Name = "Royal Crown",
            Description = "Crown that boosts Charisma by 2",
            Type = ItemType.Crown,
            Modifiers = {
                { Source = SourceType.Base, Target = ModifiedValue.Charisma, Type = ActionType.Bonus, Bonus = 2 }
            }
        }
    }
}