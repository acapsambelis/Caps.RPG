Monsters = {
	Zombie = {
		MonsterBlueprint = {
			Name = "Zombie",
			Attributes = {
				Modifiers = {
					{ Source = SourceType.Base, Target = TargetType.Strength,     Type = ActionType.Set, Bonus = 2 },
					{ Source = SourceType.Base, Target = TargetType.Agility,      Type = ActionType.Set, Bonus = 0 },
					{ Source = SourceType.Base, Target = TargetType.Constitution, Type = ActionType.Set, Bonus = 2 },
					{ Source = SourceType.Base, Target = TargetType.Intellect,    Type = ActionType.Set, Bonus = -4 },
					{ Source = SourceType.Base, Target = TargetType.Arcana,       Type = ActionType.Set, Bonus = -4 },
					{ Source = SourceType.Base, Target = TargetType.Wisdom,       Type = ActionType.Set, Bonus = -2 },
					{ Source = SourceType.Base, Target = TargetType.Presence,     Type = ActionType.Set, Bonus = -2 },
					{ Source = SourceType.Base, Target = TargetType.Charisma,     Type = ActionType.Set, Bonus = -2 },
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
					self.MonsterBlueprint.Decide(availableActions[1], nearestEnemy)
				end
				
				--for _, action in ipairs(availableActions) do
				--	if action.Tags then
				--		for _, tag in ipairs(action.Tags) do
				--			if tag == "Attack" then
								
								-- This action has the Attack tag
								-- Your logic here
								
				--				break -- Stop checking tags for this action
				--			end
				--		end
				--	end
				--end
			end,
		}
	},
}