Monsters = {
	Zombie = {
		MonsterBlueprint = {
			Name = "Zombie",
			Attributes = {
				Modifiers = {
					{ Source = SourceType.Base, Target = ModifiedValue.Strength,     Type = ActionType.Set, Bonus = 2 },
					{ Source = SourceType.Base, Target = ModifiedValue.Agility,      Type = ActionType.Set, Bonus = 0 },
					{ Source = SourceType.Base, Target = ModifiedValue.Constitution, Type = ActionType.Set, Bonus = 2 },
					{ Source = SourceType.Base, Target = ModifiedValue.Intellect,    Type = ActionType.Set, Bonus = -4 },
					{ Source = SourceType.Base, Target = ModifiedValue.Arcana,       Type = ActionType.Set, Bonus = -4 },
					{ Source = SourceType.Base, Target = ModifiedValue.Wisdom,       Type = ActionType.Set, Bonus = -2 },
					{ Source = SourceType.Base, Target = ModifiedValue.Presence,     Type = ActionType.Set, Bonus = -2 },
					{ Source = SourceType.Base, Target = ModifiedValue.Charisma,     Type = ActionType.Set, Bonus = -2 },
				}
			},

			-- CombatThink receives (self, availableActions, map, position)
			--               self : Monster
			--   availableActions : CombatAction[]
			--                map : TileMap
			--           position : TileBase
			-- Returns one of the combat actions and optional targets in a SelectedAction object
			CombatThink = function(self, availableActions, map, position)
				-- Loop through available actions to find an enemy within range
				local nearestEnemy = map:GetNearestEnemy(position)
				if (nearestEnemy) then
					--	self.MonsterBlueprint.Decide(availableActions[1], nearestEnemy)

					for _, action in ipairs(availableActions) do
						if action.Setup.Tags then
							if (contains(action.Setup.Tags, ActionTags.Attack)) then
								print("Attack tag found using contains function")
							end
							if (contains(action.Setup.Tags, ActionTags.Attack)) then
								print("Movement tag found using contains function")
							end
						end
					end
				end

			end,
		}
	},
}