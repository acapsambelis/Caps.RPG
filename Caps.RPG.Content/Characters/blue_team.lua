BlueTeam = {
	DexFighter = {
		ClassedCharacter = {
			Name = "Dex Fighter",
			Attributes = {
				Modifiers = {
					{ Source = SourceType.Base, Target = TargetType.Strength,     Type = ActionType.Set, Bonus = 0 },
					{ Source = SourceType.Base, Target = TargetType.Agility,      Type = ActionType.Set, Bonus = 0 },
					{ Source = SourceType.Base, Target = TargetType.Constitution, Type = ActionType.Set, Bonus = 0 },
					{ Source = SourceType.Base, Target = TargetType.Intellect,    Type = ActionType.Set, Bonus = 0 },
					{ Source = SourceType.Base, Target = TargetType.Arcana,       Type = ActionType.Set, Bonus = 0 },
					{ Source = SourceType.Base, Target = TargetType.Wisdom,       Type = ActionType.Set, Bonus = 0 },
					{ Source = SourceType.Base, Target = TargetType.Presence,     Type = ActionType.Set, Bonus = 0 },
					{ Source = SourceType.Base, Target = TargetType.Charisma,     Type = ActionType.Set, Bonus = 0 },
				}
			},
			ClassLevelMakeup = {
				ClassLevels = {
					Fighter = 1
				}
			},
			Inventory = {
				EquipedItems = {
					Crown = { Item = Crowns.RoyalCrown.Item },
					Hands = { Item = Swords.HugeSword.Sword }
				}
			}
		}
	},
}