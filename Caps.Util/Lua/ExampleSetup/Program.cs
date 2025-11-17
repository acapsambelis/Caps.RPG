using Caps.Util.Lua.ExampleSetup.Componenets;

namespace Caps.Util.Lua.ExampleSetup
{
    internal class Program
    {
        static void Main()
        {
            var env = new LuaEnvironment("Caps.Util.Lua.ExampleSetup");
            var loader = env.LoadFromFolder("Entities");

            var planets = loader.LoadEntitiesFromCategory("Planets");
            foreach (var planetEntity in planets)
            {
                var planet = planetEntity.GetComponent<PlanetInfo>();
                if (planet != null)
                {
                    Console.WriteLine(planet.ToString());
                }
            }

            var spaceships = loader.LoadEntitiesFromCategory("Spaceships");
            foreach (var spaceshipEntity in spaceships)
            {
                var spaceship = spaceshipEntity.GetComponent<SpaceshipInfo>();
                if (spaceship != null)
                {
                    Console.WriteLine(spaceship.ToString());
                }
            }
        }
    }
}
