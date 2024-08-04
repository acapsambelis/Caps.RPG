using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caps.RPG.CombatEngine.Attributes;
using Caps.RPG.CombatEngine.Creatures;
using Caps.RPG.CombatEngine.Creatures.Actions;
using Caps.RPG.CombatEngine.Creatures.Classed;
using Caps.RPG.CombatEngine.Creatures.Classed.Classes;

namespace Caps.RPG.CombatEngine
{
    public class MainLoop
    {
        public CombatState State;

        public MainLoop()
        {
            State = new CombatState();
            // blue team
            State.AddCombattant(
                "Blue",
                new ClassedCharacter(
                    "Dex F",
                    new AttributeSet(0, 4, 3, 0, 0, 1, 2, 0),
                    new Dictionary<Type, int> { { typeof(Fighter), 1 } }
                )
            );
            State.AddCombattant(
                "Blue",
                new ClassedCharacter(
                    "Str F",
                    new AttributeSet(4, 0, 3, 0, 0, 0, 1, 2),
                    new Dictionary<Type, int> { { typeof(Fighter), 1 } }
                )
            );
            State.AddCombattant(
                "Blue",
                new ClassedCharacter(
                    "Cleric",
                    new AttributeSet(0, 0, 2, 0, 3, 4, 1, 0),
                    new Dictionary<Type, int> { { typeof(Cleric), 1 } }
                )
            );

            // red team
            State.AddCombattant(
                "Red",
                new ClassedCharacter(
                    "Dex F",
                    new AttributeSet(0, 4, 3, 0, 0, 1, 2, 0),
                    new Dictionary<Type, int> { { typeof(Fighter), 1 } }
                )
            );
            State.AddCombattant(
                "Red",
                new ClassedCharacter(
                    "Str F",
                    new AttributeSet(4, 0, 3, 0, 0, 0, 1, 2),
                    new Dictionary<Type, int> { { typeof(Fighter), 1 } }
                )
            );
            State.AddCombattant(
                "Red",
                new ClassedCharacter(
                    "Cleric",
                    new AttributeSet(0, 0, 2, 0, 3, 4, 1, 0),
                    new Dictionary<Type, int> { { typeof(Cleric), 1 } }
                )
            );

            State.BuildCombatOrder();
        }

        public void Loop(Func<string, int> inputFunction)
        {
            string? input = string.Empty;
            int rounds = 0;
            while (State.HasNoVictor())
            {
                rounds++;

                Console.WriteLine("Start of turn state:");
                for (int i = 0; i < State.CombatOrder.Length; i++)
                {
                    CombatState.TeamedCreature possibleTarget = State.CombatOrder[i];
                    Console.WriteLine((i+1) + ": " + "(" + possibleTarget.Team + ")\t" + possibleTarget.Creature.Name + ":\t" + possibleTarget.Creature.Health + "/" + possibleTarget.Creature.MaxHealth);
                }
                Console.WriteLine("\n\n");
                foreach (CombatState.TeamedCreature currentCreature in State.CombatOrder)
                {
                    Creature creature = currentCreature.Creature;
                    Console.WriteLine("=== (" + currentCreature.Team + ") " + creature.Name + " ===");
                    Console.WriteLine("=== " + creature.Health + "/" + creature.MaxHealth + " ===");

                    List<CombatAction> availableActions = creature.GetCombatActions();
                    Console.WriteLine("Choose an action:");
                    for (int i = 0; i < availableActions.Count; i++)
                    {
                        Console.WriteLine(i + ": " + availableActions[i].ToString());
                    }

                    int chosenActionIndex = inputFunction("> ");

                    CombatAction chosen = availableActions[chosenActionIndex];
                    Creature? target = null;
                    if (chosen.NeedsTarget)
                    {
                        Console.WriteLine("Choose a target:");
                        for (int i = 0; i < State.CombatOrder.Length; i++)
                        {
                            CombatState.TeamedCreature possibleTarget = State.CombatOrder[i];
                            if (possibleTarget != currentCreature)
                            {
                                Console.WriteLine(i + ": " + "(" + possibleTarget.Team + ") " + possibleTarget.Creature.Name + ": " + possibleTarget.Creature.Health + "/" + possibleTarget.Creature.MaxHealth);
                            }
                        }
                        int chosenTargetIndex = inputFunction("> ");

                        target = State.CombatOrder[chosenTargetIndex].Creature;
                    }
                    chosen.Execution(creature, target);

                }
            }

            Console.WriteLine("Team " + State.GetVictorTeamName() + " has won in " + rounds + " rounds!");
        }
    }
}
