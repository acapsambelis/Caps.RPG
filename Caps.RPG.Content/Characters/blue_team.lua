BlueTeam = {
	DexFighter = {
		ClassedCharacter = {
			Name = "Dex F",
			Attributes = {
				Modifiers = {
					{ Source = SourceType.Base, Target = ModifiedValue.Strength,     Type = ActionType.Base, Bonus = 0 },
					{ Source = SourceType.Base, Target = ModifiedValue.Agility,      Type = ActionType.Base, Bonus = 4 },
					{ Source = SourceType.Base, Target = ModifiedValue.Constitution, Type = ActionType.Base, Bonus = 3 },
					{ Source = SourceType.Base, Target = ModifiedValue.Intellect,    Type = ActionType.Base, Bonus = 0 },
					{ Source = SourceType.Base, Target = ModifiedValue.Arcana,       Type = ActionType.Base, Bonus = 0 },
					{ Source = SourceType.Base, Target = ModifiedValue.Wisdom,       Type = ActionType.Base, Bonus = 1 },
					{ Source = SourceType.Base, Target = ModifiedValue.Presence,     Type = ActionType.Base, Bonus = 2 },
					{ Source = SourceType.Base, Target = ModifiedValue.Charisma,     Type = ActionType.Base, Bonus = 0 },
				}
			},
			CreatureType = CreatureTypes.Humanoids.Human.CreatureType,
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
					{ Source = SourceType.Base, Target = ModifiedValue.Strength,     Type = ActionType.Base, Bonus = 4 },
					{ Source = SourceType.Base, Target = ModifiedValue.Agility,      Type = ActionType.Base, Bonus = 0 },
					{ Source = SourceType.Base, Target = ModifiedValue.Constitution, Type = ActionType.Base, Bonus = 3 },
					{ Source = SourceType.Base, Target = ModifiedValue.Intellect,    Type = ActionType.Base, Bonus = 0 },
					{ Source = SourceType.Base, Target = ModifiedValue.Arcana,       Type = ActionType.Base, Bonus = 0 },
					{ Source = SourceType.Base, Target = ModifiedValue.Wisdom,       Type = ActionType.Base, Bonus = 0 },
					{ Source = SourceType.Base, Target = ModifiedValue.Presence,     Type = ActionType.Base, Bonus = 1 },
					{ Source = SourceType.Base, Target = ModifiedValue.Charisma,     Type = ActionType.Base, Bonus = 2 },
				}
			},
			CreatureType = CreatureTypes.Humanoids.Elf.CreatureType,
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
					{ Source = SourceType.Base, Target = ModifiedValue.Strength,     Type = ActionType.Base, Bonus = 0 },
					{ Source = SourceType.Base, Target = ModifiedValue.Agility,      Type = ActionType.Base, Bonus = 0 },
					{ Source = SourceType.Base, Target = ModifiedValue.Constitution, Type = ActionType.Base, Bonus = 2 },
					{ Source = SourceType.Base, Target = ModifiedValue.Intellect,    Type = ActionType.Base, Bonus = 0 },
					{ Source = SourceType.Base, Target = ModifiedValue.Arcana,       Type = ActionType.Base, Bonus = 3 },
					{ Source = SourceType.Base, Target = ModifiedValue.Wisdom,       Type = ActionType.Base, Bonus = 4 },
					{ Source = SourceType.Base, Target = ModifiedValue.Presence,     Type = ActionType.Base, Bonus = 1 },
					{ Source = SourceType.Base, Target = ModifiedValue.Charisma,     Type = ActionType.Base, Bonus = 0 },
				}
			},
			CreatureType = CreatureTypes.Humanoids.Dwarf.CreatureType,
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