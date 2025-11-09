BlueTeam = {
	DexFighter = {
		ClassedCharacter = {
			Name = "Dex F",
			Attributes = {
				Modifiers = {
					{ Source = SourceType.Base, Target = ModifiedValue.Strength,     Type = ActionType.Set, Bonus = 0 },
					{ Source = SourceType.Base, Target = ModifiedValue.Agility,      Type = ActionType.Set, Bonus = 4 },
					{ Source = SourceType.Base, Target = ModifiedValue.Constitution, Type = ActionType.Set, Bonus = 3 },
					{ Source = SourceType.Base, Target = ModifiedValue.Intellect,    Type = ActionType.Set, Bonus = 0 },
					{ Source = SourceType.Base, Target = ModifiedValue.Arcana,       Type = ActionType.Set, Bonus = 0 },
					{ Source = SourceType.Base, Target = ModifiedValue.Wisdom,       Type = ActionType.Set, Bonus = 1 },
					{ Source = SourceType.Base, Target = ModifiedValue.Presence,     Type = ActionType.Set, Bonus = 2 },
					{ Source = SourceType.Base, Target = ModifiedValue.Charisma,     Type = ActionType.Set, Bonus = 0 },
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
					{ Source = SourceType.Base, Target = ModifiedValue.Strength,     Type = ActionType.Set, Bonus = 4 },
					{ Source = SourceType.Base, Target = ModifiedValue.Agility,      Type = ActionType.Set, Bonus = 0 },
					{ Source = SourceType.Base, Target = ModifiedValue.Constitution, Type = ActionType.Set, Bonus = 3 },
					{ Source = SourceType.Base, Target = ModifiedValue.Intellect,    Type = ActionType.Set, Bonus = 0 },
					{ Source = SourceType.Base, Target = ModifiedValue.Arcana,       Type = ActionType.Set, Bonus = 0 },
					{ Source = SourceType.Base, Target = ModifiedValue.Wisdom,       Type = ActionType.Set, Bonus = 0 },
					{ Source = SourceType.Base, Target = ModifiedValue.Presence,     Type = ActionType.Set, Bonus = 1 },
					{ Source = SourceType.Base, Target = ModifiedValue.Charisma,     Type = ActionType.Set, Bonus = 2 },
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
					{ Source = SourceType.Base, Target = ModifiedValue.Strength,     Type = ActionType.Set, Bonus = 0 },
					{ Source = SourceType.Base, Target = ModifiedValue.Agility,      Type = ActionType.Set, Bonus = 0 },
					{ Source = SourceType.Base, Target = ModifiedValue.Constitution, Type = ActionType.Set, Bonus = 2 },
					{ Source = SourceType.Base, Target = ModifiedValue.Intellect,    Type = ActionType.Set, Bonus = 0 },
					{ Source = SourceType.Base, Target = ModifiedValue.Arcana,       Type = ActionType.Set, Bonus = 3 },
					{ Source = SourceType.Base, Target = ModifiedValue.Wisdom,       Type = ActionType.Set, Bonus = 4 },
					{ Source = SourceType.Base, Target = ModifiedValue.Presence,     Type = ActionType.Set, Bonus = 1 },
					{ Source = SourceType.Base, Target = ModifiedValue.Charisma,     Type = ActionType.Set, Bonus = 0 },
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