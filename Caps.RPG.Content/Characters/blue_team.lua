BlueTeam = {
	DexFighter = {
		ClassedCharacter = {
			Name = "Dex F",
			Attributes = {
				Modifiers = {
					{ Source = SourceType.Base, Target = TargetType.Strength,     Type = ActionType.Set, Bonus = 0 },
					{ Source = SourceType.Base, Target = TargetType.Agility,      Type = ActionType.Set, Bonus = 4 },
					{ Source = SourceType.Base, Target = TargetType.Constitution, Type = ActionType.Set, Bonus = 3 },
					{ Source = SourceType.Base, Target = TargetType.Intellect,    Type = ActionType.Set, Bonus = 0 },
					{ Source = SourceType.Base, Target = TargetType.Arcana,       Type = ActionType.Set, Bonus = 0 },
					{ Source = SourceType.Base, Target = TargetType.Wisdom,       Type = ActionType.Set, Bonus = 1 },
					{ Source = SourceType.Base, Target = TargetType.Presence,     Type = ActionType.Set, Bonus = 2 },
					{ Source = SourceType.Base, Target = TargetType.Charisma,     Type = ActionType.Set, Bonus = 0 },
				}
			},
			ClassLevelMakeup = {
				ClassLevels = {
					Fighter = 1
				}
			},
			Inventory = {
				EquippedItems = {
					Crown = { Item = Crowns.RoyalCrown.Item },
					Hands = { Item = Swords.HugeSword.Sword }
				}
			}
		}
	},
	StrFighter = {
		ClassedCharacter = {
			Name = "Str F",
			Attributes = {
				Modifiers = {
					{ Source = SourceType.Base, Target = TargetType.Strength,     Type = ActionType.Set, Bonus = 4 },
					{ Source = SourceType.Base, Target = TargetType.Agility,      Type = ActionType.Set, Bonus = 0 },
					{ Source = SourceType.Base, Target = TargetType.Constitution, Type = ActionType.Set, Bonus = 3 },
					{ Source = SourceType.Base, Target = TargetType.Intellect,    Type = ActionType.Set, Bonus = 0 },
					{ Source = SourceType.Base, Target = TargetType.Arcana,       Type = ActionType.Set, Bonus = 0 },
					{ Source = SourceType.Base, Target = TargetType.Wisdom,       Type = ActionType.Set, Bonus = 0 },
					{ Source = SourceType.Base, Target = TargetType.Presence,     Type = ActionType.Set, Bonus = 1 },
					{ Source = SourceType.Base, Target = TargetType.Charisma,     Type = ActionType.Set, Bonus = 2 },
				}
			},
			ClassLevelMakeup = {
				ClassLevels = {
					Fighter = 1
				}
			},
			Inventory = {
				EquippedItems = {
					Hands = { Item = Swords.HugeSword.Sword }
				}
			}
		}
	},
	Cleric = {
		ClassedCharacter = {
			Name = "Cleric",
			Attributes = {
				Modifiers = {
					{ Source = SourceType.Base, Target = TargetType.Strength,     Type = ActionType.Set, Bonus = 0 },
					{ Source = SourceType.Base, Target = TargetType.Agility,      Type = ActionType.Set, Bonus = 0 },
					{ Source = SourceType.Base, Target = TargetType.Constitution, Type = ActionType.Set, Bonus = 2 },
					{ Source = SourceType.Base, Target = TargetType.Intellect,    Type = ActionType.Set, Bonus = 0 },
					{ Source = SourceType.Base, Target = TargetType.Arcana,       Type = ActionType.Set, Bonus = 3 },
					{ Source = SourceType.Base, Target = TargetType.Wisdom,       Type = ActionType.Set, Bonus = 4 },
					{ Source = SourceType.Base, Target = TargetType.Presence,     Type = ActionType.Set, Bonus = 1 },
					{ Source = SourceType.Base, Target = TargetType.Charisma,     Type = ActionType.Set, Bonus = 0 },
				}
			},
			ClassLevelMakeup = {
				ClassLevels = {
					Cleric = 1
				}
			},
			Inventory = {
				EquippedItems = {
					Crown = { Item = Crowns.RoyalCrown.Item },
				}
			}
		}
	},
}