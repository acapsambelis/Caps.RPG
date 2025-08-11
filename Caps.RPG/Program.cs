using Caps.RPG.Rules;
using Caps.RPG.Rules.Attributes;
using Caps.RPG.Rules.Creatures.Classed.Classes;
using Caps.RPG.Rules.Creatures.Classed;
using Caps.RPG.Rules.Creatures;
using Caps.RPG.Rules.Creatures.Actions;
using Caps.RPG.Rules.Maps;
using Caps.RPG.Rules.Helpers;

namespace Caps.RPG
{
    public class Program
    {
        static void Main()
        {
            TileMap hexMap = HexMap.GenerateRandomMap(0, 3);
            List<Combattant> combattants = [];

            // blue team

            ClassedCharacter blueDexFighter = new(
                "Dex F",
                new AttributeSet(0, 4, 3, 0, 0, 1, 2, 0),
                new Dictionary<Type, int> { { typeof(Fighter), 2 } }
            );
            blueDexFighter.Equip(Content.Items.Hands.VeryLargeSword.Item);
            combattants.Add(new Combattant(blueDexFighter, TerminalColors.Blue, hexMap.RandomEmptyTile()));

            ClassedCharacter blueStrFighter = new(
                "Str F",
                new AttributeSet(4, 0, 3, 0, 0, 0, 1, 2),
                new Dictionary<Type, int> { { typeof(Fighter), 1 } }
            );
            blueStrFighter.Equip(Content.Items.Hands.VeryLargeSword.Item);
            combattants.Add(new Combattant(blueStrFighter, TerminalColors.Blue, hexMap.RandomEmptyTile()));

            combattants.Add(new Combattant(
                new ClassedCharacter(
                    "Cleric",
                    new AttributeSet(0, 0, 2, 0, 3, 4, 1, 0),
                    new Dictionary<Type, int> { { typeof(Cleric), 1 } }
                ),
                TerminalColors.Blue,
                hexMap.RandomEmptyTile()
            ));

            //// red team

            ClassedCharacter redDexFighter = new(
                "Dex F",
                new AttributeSet(0, 4, 3, 0, 0, 1, 2, 0),
                new Dictionary<Type, int> { { typeof(Fighter), 2 } }
            );
            redDexFighter.Equip(Content.Items.Hands.VeryLargeSword.Item);
            combattants.Add(new Combattant(redDexFighter, TerminalColors.Red, hexMap.RandomEmptyTile()));

            ClassedCharacter redStrFighter = new(
                "Str F",
                new AttributeSet(4, 0, 3, 0, 0, 0, 1, 2),
                new Dictionary<Type, int> { { typeof(Fighter), 1 } }
            );
            redStrFighter.Equip(Content.Items.Hands.VeryLargeSword.Item);
            combattants.Add(new Combattant(redStrFighter, TerminalColors.Red, hexMap.RandomEmptyTile()));

            combattants.Add(new Combattant(
                new ClassedCharacter(
                    "Cleric",
                    new AttributeSet(0, 0, 2, 0, 3, 4, 1, 0),
                    new Dictionary<Type, int> { { typeof(Cleric), 1 } }
                ),
                TerminalColors.Red,
                hexMap.RandomEmptyTile()
            ));

            MainLoop mainLoop = new(hexMap, combattants);
            mainLoop.BetterLoop(DrawMap, TopDisplay, GetTargets, GetAction, DisplayActionResult);
        }

        private static int GetTextInput()
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

        public static void TopDisplay(Combattant[] combattants, Combattant currentCreature, int actionsLeft, int maxActions = 3)
        {
            Console.Write('\n');
            foreach (Combattant c in combattants)
            {
                if (c.Creature.Status == Creature.HealthStatus.Dead)
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                else
                    Console.ForegroundColor = c.Team.Color;
                if (c == currentCreature)
                {
                    Console.ForegroundColor = c.Team.Highlight;
                    Console.BackgroundColor = c.Team.Color;
                }
                Console.Write($"{c.Creature.Name}\t");
                Console.ResetColor();
            }
            Console.Write('\n');
            foreach (Combattant c in combattants)
            {
                if (c.Creature.Status == Creature.HealthStatus.Dead)
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                else
                    Console.ForegroundColor = c.Team.Color;
                if (c == currentCreature)
                {
                    Console.ForegroundColor = c.Team.Highlight;
                    Console.BackgroundColor = c.Team.Color;
                }
                Console.Write($"{c.Creature.Health}/{c.Creature.MaxHealth}\t");
                Console.ResetColor();
            }
            Console.Write('\n');

            Console.ForegroundColor = currentCreature.Team.Color;
            Console.WriteLine($"========= ({currentCreature.Team}) {currentCreature.Creature.Name} | HP: {currentCreature.Creature.Health} / {currentCreature.Creature.MaxHealth} =========");
            Console.WriteLine("STR\tAGI\tCON\tINT\tARC\tWIS\tPRE\tCHA");
            Console.WriteLine(
                currentCreature.Creature.Attributes.GetStatValue(Stat.Strength) + "\t" +
                currentCreature.Creature.Attributes.GetStatValue(Stat.Agility) + "\t" +
                currentCreature.Creature.Attributes.GetStatValue(Stat.Constitution) + "\t" +
                currentCreature.Creature.Attributes.GetStatValue(Stat.Intellect) + "\t" +
                currentCreature.Creature.Attributes.GetStatValue(Stat.Arcana) + "\t" +
                currentCreature.Creature.Attributes.GetStatValue(Stat.Wisdom) + "\t" +
                currentCreature.Creature.Attributes.GetStatValue(Stat.Presence) + "\t" +
                currentCreature.Creature.Attributes.GetStatValue(Stat.Charisma)
            );
            string actions = new string('O', actionsLeft) + new string('0', maxActions - actionsLeft);
            Console.WriteLine($"AC: {currentCreature.Creature.DefenseClass} | ATTACK: {currentCreature.Creature.AttackBonus} | MOVEMENT: {currentCreature.MoveSpeed} | ACTIONS: {actions}");
            Console.ForegroundColor = ConsoleColor.White;
        }

        public static TileBase[] GetTargets(TileMap map, TileBase source, ActionSetup metadata)
        {
            Combattant currentCreature = source.Features.Where(f => f.Value is Combattant).FirstOrDefault().Value as Combattant ?? throw new Exception("No valid Combattant found.");
            Console.WriteLine($"Select a {metadata}.");

            (int x, int y) = GetXYInput();
            (int x2, int y2) = (0, 0);
            if (metadata.Shape == MapShape.FreestandingLine)
            {
                Console.WriteLine("Select the end of the line.");
                (x2, y2) = GetXYInput();
            }

            return map.GetTiles(map[x, y], metadata.Shape, metadata.GetRange(currentCreature));
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

        public static void DisplayActionResult(ActionResult result)
        {
            Console.ForegroundColor = result.Color;
            Console.WriteLine(result.ToString());
            Console.ResetColor();
        }

        private static (int x, int y) GetXYInput()
        {
            Console.WriteLine("Enter coordinates as 'x, y':");
            string? input = Console.ReadLine() ?? throw new Exception("No input provided.");
            string[] parts = input.Split(',');
            if (parts.Length != 2) throw new Exception("Input must be in the format 'x, y'.");
            return (int.Parse(parts[0].Trim()), int.Parse(parts[1].Trim()));
        }
    }
}