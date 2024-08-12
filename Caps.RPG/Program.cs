using Caps.RPG.Rules;
using Caps.RPG.Rules.Attributes;
using Caps.RPG.Rules.Creatures.Classed.Classes;
using Caps.RPG.Rules.Creatures.Classed;
using Caps.RPG.Rules.Creatures;
using Caps.RPG.Rules.Helpers;
using Caps.RPG.Rules.Creatures.Actions;

namespace Caps.RPG
{
    public class Program
    {
        static void Main(string[] args)
        {
            List<(string, Creature, Vector2D)> combattants = new List<(string, Creature, Vector2D)>();
            // blue team

            ClassedCharacter blueDexFighter = new ClassedCharacter(
                "Dex F",
                new AttributeSet(0, 4, 3, 0, 0, 1, 2, 0),
                new Dictionary<Type, int> { { typeof(Fighter), 2 } }
            );
            blueDexFighter.Inventory.Equip(Content.Items.Hands.VeryLargeSword.Item);
            combattants.Add(("Blue", blueDexFighter, new Vector2D(1,3)));

            ClassedCharacter blueStrFighter = new ClassedCharacter(
                "Str F",
                new AttributeSet(4, 0, 3, 0, 0, 0, 1, 2),
                new Dictionary<Type, int> { { typeof(Fighter), 1 } }
            );
            blueStrFighter.Inventory.Equip(Content.Items.Hands.VeryLargeSword.Item);
            combattants.Add(("Blue", blueStrFighter, new Vector2D(1,4)));

            combattants.Add(
                ("Blue",
                new ClassedCharacter(
                    "Cleric",
                    new AttributeSet(0, 0, 2, 0, 3, 4, 1, 0),
                    new Dictionary<Type, int> { { typeof(Cleric), 1 } }
                ),
                new Vector2D(1,5)
            ));

            //test
            combattants.Add(
                ("Blue",
                new ClassedCharacter(
                    "Cleric2",
                    new AttributeSet(0, 0, 2, 0, 3, 4, 1, 0),
                    new Dictionary<Type, int> { { typeof(Cleric), 1 } }
                )
            ));

            // red team

            ClassedCharacter redDexFighter = new ClassedCharacter(
                "Dex F",
                new AttributeSet(0, 4, 3, 0, 0, 1, 2, 0),
                new Dictionary<Type, int> { { typeof(Fighter), 2 } }
            );
            redDexFighter.Inventory.Equip(Content.Items.Hands.VeryLargeSword.Item);
            combattants.Add(("Red", redDexFighter, new Vector2D(8,3) ));

            ClassedCharacter redStrFighter = new ClassedCharacter(
                "Str F",
                new AttributeSet(4, 0, 3, 0, 0, 0, 1, 2),
                new Dictionary<Type, int> { { typeof(Fighter), 1 } }
            );
            redStrFighter.Inventory.Equip(Content.Items.Hands.VeryLargeSword.Item);
            combattants.Add(("Red", redStrFighter, new Vector2D(8,4)));

            combattants.Add(
                ("Red",
                new ClassedCharacter(
                    "Cleric",
                    new AttributeSet(0, 0, 2, 0, 3, 4, 1, 0),
                    new Dictionary<Type, int> { { typeof(Cleric), 1 } }
                ),
                new Vector2D(8,5)
            ));

            MainLoop mainLoop = new MainLoop(combattants);
            mainLoop.BetterLoop(DisplayScoreboard, DrawMap, CreatureDisplay, GetDestination, GetAction, GetTarget);
        }

        public static int GetTextInput()
        {
            Console.Write("> ");
            string? input = Console.ReadLine();
            return Int32.Parse(input != null ? input : "");
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

        public static Combattant[,] DrawMap(Combattant[] combattants)
        {
            Console.WriteLine("Map");
            Combattant[,] map = new Combattant[11, 11];
            foreach (Combattant combattant in combattants)
            {
                map[combattant.Position.IntX, combattant.Position.IntY] = combattant;
            }

            Console.Write("    ");
            for (int x = 0; x < 10; x++)
            {
                Console.Write(x + " ");
            }
            Console.Write("\n");
            for (int y = 0; y < 10; y++)
            {
                Console.Write(y + ":  ");
                for (int x = 0; x < 10; x++)
                {
                    char slot = map[x, y] != null ? map[x, y].ShortName : '.';
                    if (map[x, y] != null)
                    {
                        Console.ForegroundColor = map[x, y].Team == "Blue" ? ConsoleColor.Blue : ConsoleColor.Red;
                        if (map[x, y].Creature.Status != Creature.HealthStatus.Alive)
                        {
                            Console.ForegroundColor = ConsoleColor.Gray;
                        }
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.White;
                    }
                    Console.Write(slot + " ");
                }
                Console.Write('\n');
            }
            Console.Write('\n');

            return map;
        }

        public static void CreatureDisplay(Combattant currentCreature)
        {
            Console.WriteLine("=== (" + currentCreature.Team + ") " + currentCreature.Creature.Name + " ===");
            Console.WriteLine("=== " + currentCreature.Creature.Health + "/" + currentCreature.Creature.MaxHealth + " ===");
        }

        public static Vector2D GetDestination(Combattant currentCreature)
        {
            Console.WriteLine("Your speed is " + currentCreature.Creature.MoveSpeed + ".");
            Console.WriteLine("Choose a valid location.");
            Console.WriteLine("X Destination:");
            int x = GetTextInput();
            Console.WriteLine("Y Destination:");
            int y = GetTextInput();
            return new Vector2D(x, y);
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

        public static Creature GetTarget(CombatState state, Combattant currentCreature, double distance)
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

            return nearbyCombattants[chosenTargetIndex].Creature;
        }
    }
}