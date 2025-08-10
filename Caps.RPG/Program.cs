using Caps.RPG.Rules;
using Caps.RPG.Rules.Attributes;
using Caps.RPG.Rules.Creatures.Classed.Classes;
using Caps.RPG.Rules.Creatures.Classed;
using Caps.RPG.Rules.Creatures;
using Caps.RPG.Rules.Helpers;
using Caps.RPG.Rules.Creatures.Actions;
using Caps.RPG.Rules.Maps;

namespace Caps.RPG
{
    public class Program
    {
        static void Main()
        {
            TileMap hexMap = HexMap.GenerateRandom(0, 0);
            List<Combattant> combattants = [];

            // blue team

            ClassedCharacter blueDexFighter = new(
                "Dex F",
                new AttributeSet(0, 4, 3, 0, 0, 1, 2, 0),
                new Dictionary<Type, int> { { typeof(Fighter), 2 } },
                sightRange: 5
            );
            blueDexFighter.Equip(Content.Items.Hands.VeryLargeSword.Item);
            combattants.Add(new Combattant(blueDexFighter, MapColors.Blue, hexMap[1, 0]));

            ClassedCharacter blueStrFighter = new(
                "Str F",
                new AttributeSet(4, 0, 3, 0, 0, 0, 1, 2),
                new Dictionary<Type, int> { { typeof(Fighter), 1 } },
                sightRange: 5
            );
            blueStrFighter.Equip(Content.Items.Hands.VeryLargeSword.Item);
            combattants.Add(new Combattant(blueStrFighter, MapColors.Blue, hexMap[6, 3]));

            combattants.Add(new Combattant(
                new ClassedCharacter(
                    "Cleric",
                    new AttributeSet(0, 0, 2, 0, 3, 4, 1, 0),
                    new Dictionary<Type, int> { { typeof(Cleric), 1 } },
                    sightRange: 5
                ),
                MapColors.Blue,
                hexMap[1, 3]
            ));

            //// red team

            ClassedCharacter redDexFighter = new(
                "Dex F",
                new AttributeSet(0, 4, 3, 0, 0, 1, 2, 0),
                new Dictionary<Type, int> { { typeof(Fighter), 2 } },
                    sightRange: 5
            );
            redDexFighter.Equip(Content.Items.Hands.VeryLargeSword.Item);
            combattants.Add(new Combattant(redDexFighter, MapColors.Red, hexMap[5, 3]));

            ClassedCharacter redStrFighter = new(
                "Str F",
                new AttributeSet(4, 0, 3, 0, 0, 0, 1, 2),
                new Dictionary<Type, int> { { typeof(Fighter), 1 } },
                    sightRange: 5
            );
            redStrFighter.Equip(Content.Items.Hands.VeryLargeSword.Item);
            combattants.Add(new Combattant(redStrFighter, MapColors.Red, hexMap[4, 2]));

            combattants.Add(new Combattant(
                new ClassedCharacter(
                    "Cleric",
                    new AttributeSet(0, 0, 2, 0, 3, 4, 1, 0),
                    new Dictionary<Type, int> { { typeof(Cleric), 1 } },
                    sightRange: 5
                ),
                MapColors.Red,
                hexMap[5, 4]
            ));

            MainLoop mainLoop = new MainLoop(hexMap, combattants);
            mainLoop.BetterLoop(DisplayScoreboard, DrawMap, CreatureDisplay, GetDestination, GetAction, GetTarget);
        }

        public static int GetTextInput()
        {
            Console.Write("> ");
            string? input = Console.ReadLine();
            return Int32.Parse(input ?? "");
        }

        public static void DisplayScoreboard(Combattant[] combattants)
        {
            Console.WriteLine("Start of turn state:");
            for (int i = 0; i < combattants.Length; i++)
            {
                Combattant possibleTarget = combattants[i];
                Console.WriteLine((i + 1) + ": " + "(" + possibleTarget.Team + ")\t" + possibleTarget.Creature.Name + ":\t" + possibleTarget.Creature.Health + "/" + possibleTarget.Creature.MaxHealth);
            }
        }

        public static void DrawMap(TileMap map, Dictionary<TileBase, ConsoleColor?>? highlights)
        {
            map.PrintToConsole(highlights);
        }

        public static void CreatureDisplay(Combattant currentCreature)
        {
            Console.ForegroundColor = currentCreature.Team.Color;
            Console.WriteLine("=== (" + currentCreature.Team + ") " + currentCreature.Creature.Name + " ===");
            Console.WriteLine("=== " + currentCreature.Creature.Health + "/" + currentCreature.Creature.MaxHealth + " ===");
            Console.ForegroundColor = ConsoleColor.White;
        }

        public static TileBase GetDestination(TileMap map, TileBase source, double range = double.MaxValue, bool needsCharacter = false)
        {
            TileBase destination;
            Combattant currentCreature = source.Features.Where(f => f.Value is Combattant).FirstOrDefault().Value as Combattant ?? throw new Exception("No valid Combattant found.");
            do
            {
                Console.WriteLine("Your range is " + currentCreature.Creature.MoveSpeed + ".");
                Console.WriteLine("Choose a valid location.");
                Console.WriteLine("X Destination:");
                int x = GetTextInput();
                Console.WriteLine("Y Destination:");
                int y = GetTextInput();

                destination = map[new Vector2D(x, y)];
            } while (source.GetDistance(destination) > range && range != -1);
            return destination;
        }

        public static CombatAction GetAction(List<CombatAction> combatActions)
        {
            Console.WriteLine("Choose an action:");
            for (int i = 0; i < combatActions.Count; i++)
            {
                Console.WriteLine(i + ": " + combatActions[i].ToString());
            }

            int chosenActionIndex = GetTextInput();
            return combatActions[chosenActionIndex];
        }

        public static Combattant GetTarget(CombatState state, Combattant currentCreature, double distance)
        {
            Console.WriteLine("Choose a target:");
            Combattant[] nearbyCombattants = state.GetNeighbors(currentCreature.Position, distance);
            for (int i = 0; i < nearbyCombattants.Length; i++)
            {
                Combattant possibleTarget = nearbyCombattants[i];
                if (possibleTarget != currentCreature)
                {
                    Console.WriteLine(i + ": " + "(" + possibleTarget.Team + ") " + possibleTarget.Creature.Name + ": " + possibleTarget.Creature.Health + "/" + possibleTarget.Creature.MaxHealth);
                }
            }
            int chosenTargetIndex = GetTextInput();

            return nearbyCombattants[chosenTargetIndex];
        }
    }
}