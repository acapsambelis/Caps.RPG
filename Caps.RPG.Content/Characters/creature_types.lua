CreatureTypes = {
	Humanoids = {
		Human = {
			CreatureType = {
				Name = "Human",
				Modifiers = {
					{ Source = SourceType.Base, Target = ModifiedValue.Strength,     Type = ActionType.Bonus, Bonus = 2 },
					{ Source = SourceType.Base, Target = ModifiedValue.Agility,      Type = ActionType.Bonus, Bonus = 2 },
				}
			}
		},
		Elf = {
			CreatureType = {
				Name = "Elf",
				Modifiers = {
					{ Source = SourceType.Base, Target = ModifiedValue.Strength,     Type = ActionType.Bonus, Bonus = 2 },
					{ Source = SourceType.Base, Target = ModifiedValue.Agility,      Type = ActionType.Bonus, Bonus = 2 },
				}
			}
		},
		Dwarf = {
			CreatureType = {
				Name = "Dwarf",
				Modifiers = {
					{ Source = SourceType.Base, Target = ModifiedValue.Strength,     Type = ActionType.Bonus, Bonus = 2 },
					{ Source = SourceType.Base, Target = ModifiedValue.Agility,      Type = ActionType.Bonus, Bonus = 2 },
				}
			}
		}
	},
	Undead = {
		Zombie = {
			CreatureType = {
				Name = "Zombie",
				Modifiers = {
					{ Source = SourceType.Base, Target = ModifiedValue.Strength,     Type = ActionType.Bonus, Bonus = 2 },
					{ Source = SourceType.Base, Target = ModifiedValue.Agility,      Type = ActionType.Bonus, Bonus = 2 },
				}
			}
		}
	}
}