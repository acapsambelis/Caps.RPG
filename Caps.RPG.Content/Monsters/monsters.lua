Monsters = {
	Zombie = {
		MonsterBlueprint = {
			Name = "Zombie",
			Attributes = {
				Modifiers = {
					{ Source = SourceType.Base, Target = ModifiedValue.Strength,     Type = ActionType.Base, Bonus = 2 },
					{ Source = SourceType.Base, Target = ModifiedValue.Agility,      Type = ActionType.Base, Bonus = 0 },
					{ Source = SourceType.Base, Target = ModifiedValue.Constitution, Type = ActionType.Base, Bonus = 2 },
					{ Source = SourceType.Base, Target = ModifiedValue.Intellect,    Type = ActionType.Base, Bonus = -4 },
					{ Source = SourceType.Base, Target = ModifiedValue.Arcana,       Type = ActionType.Base, Bonus = -4 },
					{ Source = SourceType.Base, Target = ModifiedValue.Wisdom,       Type = ActionType.Base, Bonus = -2 },
					{ Source = SourceType.Base, Target = ModifiedValue.Presence,     Type = ActionType.Base, Bonus = -2 },
					{ Source = SourceType.Base, Target = ModifiedValue.Charisma,     Type = ActionType.Base, Bonus = -2 },
				}
			},

			-- CombatThink receives (self, availableActions, map, position)
			--               self : Monster
			--   availableActions : CombatAction[]
			--                map : TileMap
			--           position : TileBase
			-- This basic combat think expects only two availabe actions: attack, and move.
			--   It will move toward the nearest enemy or attack within range.
			-- Returns one of the combat actions and optional targets in a SelectedAction object
			CombatThink = function(self, availableActions, map, position)
				local nearestEnemy = map:GetNearestEnemy(position)
				if (nearestEnemy) then
					local path = position.FindPath(nearestEnemy)
					local minAttackDistance = 9999;
					for _, action in ipairs(availableActions) do
						if action.Setup.Tags then
							if (contains(action.Setup.Tags, ActionTags.Attack)) then
								local attackRange = action.Setup:GetRange(self)

								if (attackRange < minAttackDistance) then
									minAttackDistance = attackRange
								end

								-- if it can attack, then attack
								if (nearestEnemy:GetDistance(position) <= attackRange) then
									print("Within range")
									self.MonsterBlueprint:Decide(action, { nearestEnemy })
									return
								end
							end
						end
					end
					for _, action in ipairs(availableActions) do
						if action.Setup.Tags then
							if (contains(action.Setup.Tags, ActionTags.Movement)) then
								local movementRange = action.Setup:GetRange(self)
								if (#path > minAttackDistance) then -- outside range
									print("moving closer")
									local farthestAble = path[math.min(movementRange, #path)]
									self.MonsterBlueprint:Decide(action, { farthestAble })
									return
								end
							end
						end
					end
				end
			end,
		}
	},
}