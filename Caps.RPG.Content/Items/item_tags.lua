ItemTags = {
    Strength = {
        ItemTag = {
            Name = "Strength",
            Description = "This item uses Strength for attack rolls.",
            Modifiers = {
                {
                    Source = SourceType.ItemTag,
                    Target = ModifiedValue.AttackBonus,
                    Type = ActionType.Bonus,
                    Stat = Stat.Strength
                }
            }
        }
    },
    Agility = {
        ItemTag = {
            Name = "Agility",
            Description = "This item uses Agility for attack rolls.",
            Modifiers = {
                {
                    Source = SourceType.ItemTag,
                    Target = ModifiedValue.AttackBonus,
                    Type = ActionType.Bonus,
                    Stat = Stat.Agility
                }
            }
        }
    },
    Reliable = {
        ItemTag = {
            Name = "Reliable",
            Description = "This item grants a +1 bonus to attack rolls.",
            Modifiers = {
                {
                    Source = SourceType.ItemTag,
                    Target = ModifiedValue.AttackBonus,
                    Type = ActionType.Bonus,
                    Bonus = 1
                }
            }
        }
    },
    Massive = {
        ItemTag = {
            Name = "Massive",
            Description = "This item grants a -1 penalty to agility",
            Modifiers = {
                {
                    Source = SourceType.ItemTag,
                    Target = ModifiedValue.Agility,
                    Type = ActionType.Bonus,
                    Bonus = -1
                }
            }
        }
    }
}