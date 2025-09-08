Landmarks = {
    mt_everest = {
        LandmarkInfo = {
            Name = "Mt. Everest",
            Position = { X = 86.925, Y = 27.9881 },
            Description = "Tallest mountain.",
            DockedShips = {
                Spaceships.Destroyer.SpaceshipInfo
            }
        }
    },
    grand_canyon = {
        LandmarkInfo = {
            Name = "Grand Canyon",
            Position = { X = -112.14, Y = 36.06 },
            Description = "Famous canyon.",
            --DockedShips = {
            --    Spaceships.Falcon.SpaceshipInfo
            --}
        }
    }
}

Planets = {
    Earth = {
        PlanetInfo = {
            Name = "Earth",
            Radius = 6371,
            Landmarks = {
                Landmarks.mt_everest.LandmarkInfo,
                Landmarks.grand_canyon.LandmarkInfo
            } 
        }
    }
}