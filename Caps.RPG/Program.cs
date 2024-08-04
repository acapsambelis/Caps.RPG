using Caps.RPG.Rules;
using Caps.RPG.Rules.Attributes;
using Caps.RPG.Rules.Creatures.Classed.Classes;
using Caps.RPG.Rules.Creatures.Classed;
using Caps.RPG.Rules.Creatures;

namespace Caps.RP
{
    public class Program
    {
        static void Main(string[] args)
        {
            List<(string, Creature)> combattants = new List<(string, Creature)>();
            // blue team
            combattants.Add(
                ("Blue",
                new ClassedCharacter(
                    "Dex F",
                    new AttributeSet(0, 4, 3, 0, 0, 1, 2, 0),
                    new Dictionary<Type, int> { { typeof(Fighter), 1 } }
                )
            ));

            ClassedCharacter blueStrFighter = new ClassedCharacter(
                "Str F",
                new AttributeSet(4, 0, 3, 0, 0, 0, 1, 2),
                new Dictionary<Type, int> { { typeof(Fighter), 1 } }
            );
            blueStrFighter.Inventory.Equip(RPG.Content.Items.Hands.VeryLargeSword.Item);
            combattants.Add(("Blue", blueStrFighter));

            combattants.Add(
                ("Blue",
                new ClassedCharacter(
                    "Cleric",
                    new AttributeSet(0, 0, 2, 0, 3, 4, 1, 0),
                    new Dictionary<Type, int> { { typeof(Cleric), 1 } }
                )
            ));

            // red team
            combattants.Add(
                ("Red",
                new ClassedCharacter(
                    "Dex F",
                    new AttributeSet(0, 4, 3, 0, 0, 1, 2, 0),
                    new Dictionary<Type, int> { { typeof(Fighter), 1 } }
                )
            ));

            ClassedCharacter redStrFighter = new ClassedCharacter(
                "Str F",
                new AttributeSet(4, 0, 3, 0, 0, 0, 1, 2),
                new Dictionary<Type, int> { { typeof(Fighter), 1 } }
            );
            redStrFighter.Inventory.Equip(RPG.Content.Items.Hands.VeryLargeSword.Item);
            combattants.Add(("Red", redStrFighter));

            combattants.Add(
                ("Red",
                new ClassedCharacter(
                    "Cleric",
                    new AttributeSet(0, 0, 2, 0, 3, 4, 1, 0),
                    new Dictionary<Type, int> { { typeof(Cleric), 1 } }
                )
            ));

            MainLoop mainLoop = new MainLoop(combattants);
            mainLoop.Loop(GetTextInput);
        }

        public static int GetTextInput(string prompt)
        {
            Console.Write(prompt);
            string? input = Console.ReadLine();
            return Int32.Parse(input != null ? input : "");
        }
    }
}