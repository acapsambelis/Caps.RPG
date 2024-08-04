using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caps.RPG.Rules.Creatures;
using Caps.RPG.Rules.Creatures.Actions;

namespace Caps.RPG.Rules
{
    public class MainLoop
    {
        public CombatState State;

        public MainLoop(List<(string, Creature)> combattants)
        {
            State = new CombatState();
            foreach ((string, Creature) c in combattants)
            {
                State.AddCombattant(c.Item1, c.Item2);
            }

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
