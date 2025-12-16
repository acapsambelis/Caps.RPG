-- Tier 1
Chestplates = {
    Debug = {
        Item = {
            Name = "Debug Chestplate",
            Description = "A chestplate",
            Type = ItemType.Chest,
            Tags = {},
            Modifiers = {
                {
                    Source = SourceType.Chest,
                    Target = ModifiedValue.DefenseClass,
                    Type = ActionType.Set,
                    Bonus = 30
                },
                {
                    Source = SourceType.Hands,
                    Target = ModifiedValue.MaxHealth,
                    Type = ActionType.Bonus,
                    Bonus = 150
                }
            },
            CombatActions = {
                {
                    Name = "InstaKill",
                    Description = "InstaKill debug action.",
                    Cost = 1,
                    Setup = {
                        NeedsTarget = true,
                        Target = TargetType.SingleCreature,
                        TargetRange = 1,
                        Tags = {
                            ActionTags.Attack
                        }
                    },
                    -- self: Combattant
                    -- tiles: TileBase[] (targets)
                    Execution = function(self, tiles)
                        local defeatedName = nil
                        for _, tile in ipairs(tiles) do
                            local targetCombattant = tile:GetFeatureByTypeName(Types.Combattant)
                            if targetCombattant and targetCombattant.Health then
                                targetCombattant.Health = 0
                                if targetCombattant.Name then
                                    defeatedName = targetCombattant.Name
                                end
                            end
                        end
                        return { Description = "This action defeated " .. defeatedName }
                    end
                }
            }
        }
    },
}
