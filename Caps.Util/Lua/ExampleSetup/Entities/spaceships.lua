NPCs = {
    MerchantBob = {
        NPCInfo = {
            Name = "Merchant Bob",
            Dialogue = "Welcome to my starport!"
        }
    },
    GuardAlice = {
        NPCInfo = {
            Name = "Guard Alice",
            Dialogue = "You're not on the list."
        }
    }
}

Spaceships = {
    Falcon = {
        SpaceshipInfo = {
            Name = "Falcon",
            Firepower = 40,
            Captain = NPCs.MerchantBob.NPCInfo,
            FirstMate = NPCs.GuardAlice.NPCInfo,
            Crew = {
                { Name = "Alice", Dialogue = "Hello!" },
                { Name = "Bob", Dialogue = "Ready!" }
            }
        }
    },
    Destroyer = {
        SpaceshipInfo = {
            Name = "Imperial Destroyer",
            Firepower = 100,
            Captain = NPCs.GuardAlice.NPCInfo
        }
    }
}