-- Tier 1
Swords = {
    Debug = {
        Item = {
            Name = "Debug Sword",
            Description = "A sword",
            Type = ItemType.Hands,
            Tags = {},
            Modifiers = {},
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
	Broadsword = {
        Item = {
            Name = "Broadsword",
            Description = "Agility, Melee, One-Handed, Reliable",
            Type = ItemType.Hands,
            Tags = {
                ItemTags.Agility.ItemTag
            },
            Modifiers = {
                {
                    Source = SourceType.Hands,
                    Target = ModifiedValue.AttackBonus,
                    Type = ActionType.Bonus,
                    Bonus = 1
                },
                {
                    Source = SourceType.Hands,
                    Target = ModifiedValue.AttackDamage,
                    Type = ActionType.Bonus,
                    Dice = { [Die.D8] = 1 }
                }
            }
        }
    },
    Longsword = {
        Item = {
            Name = "Longsword",
            Description = "Agility, Melee, Two-Handed",
            Type = ItemType.Hands,
            Tags = {
                ItemTags.Agility.ItemTag
            },
            Modifiers = {
                {
                    Source = SourceType.Hands,
                    Target = ModifiedValue.AttackDamage,
                    Type = ActionType.Bonus,
                    Dice = { [Die.D8] = 1 },
                    Bonus = 3
                }
            }
        }
    },
    Greatsword = {
        Item = {
            Name = "Greatsword",
            Description = "Strength, Melee, Two-Handed, Massive",
            Type = ItemType.Hands,
            Tags = {
                ItemTags.Strength.ItemTag
            },
            Modifiers = {
                {
                    Source = SourceType.Hands,
                    Target = ModifiedValue.AttackDamage,
                    Type = ActionType.Bonus,
                    Dice = { [Die.D10] = 1 },
                    Bonus = 3
                }
            }
        }
    }
}
