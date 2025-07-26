using Caps.RPG.Rules.Creatures;
using Caps.RPG.Rules.Creatures.Actions;
using Caps.RPG.Rules.Helpers;

namespace Caps.RPG.Rules
{
    public class MainLoop
    {
        public CombatState State;

        public MainLoop(List<(string, Creature, Vector2D)> combattants)
        {
            State = new CombatState();
            foreach ((string, Creature, Vector2D) c in combattants)
            {
                State.AddCombattant(c.Item1, c.Item2, c.Item3);
            }

            State.BuildCombatOrder();
        }

        public MainLoop(List<Combattant> combattants)
        {
            State = new CombatState();
            foreach (Combattant c in combattants)
            {
                State.AddCombattant(c);
            }
            State.BuildCombatOrder();
        }

        public void BetterLoop(
            Action<Combattant[]> ScoreboardFunction,
            Func<Combattant[], Combattant[,]> MapFunction,
            Action<Combattant> CreatureDisplayFunction,
            Func<Combattant, double, Vector2D> GetDestination,
            Func<List<CombatAction>, CombatAction> GetAction,
            Func<CombatState, Combattant, double, Combattant> GetTarget
        )
        {
            int rounds = 0;
            while (State.HasNoVictor())
            {
                rounds++;

                foreach (Combattant currentCreature in State.CombatOrder)
                {
                    // display
                    if (currentCreature.Creature.Status != Creature.HealthStatus.Alive) continue;
                    ScoreboardFunction(State.CombatOrder);
                    Combattant[,] map = MapFunction(State.CombatOrder);
                    CreatureDisplayFunction(currentCreature);

                    // action
                    int actionsAvailable = 3;
                    while (actionsAvailable > 0 && currentCreature.Creature.Status == Creature.HealthStatus.Alive)
                    {
                        MapFunction(State.CombatOrder);

                        List<CombatAction> availableActions = currentCreature.Creature.GetCombatActions();
                        CombatAction chosen = GetAction(availableActions);
                        Combattant? target = null;
                        Vector2D? location = null;
                        if (chosen.NeedsTarget)
                        {
                            target = GetTarget(State, currentCreature, chosen.Distance);
                        }
                        if (chosen.NeedsLocation)
                        {
                            location = GetDestination(currentCreature, chosen.Distance);
                        }
                        chosen.Execution(currentCreature, target, location);
                        actionsAvailable -= chosen.Cost;
                    }
                }
            }
        }
    }
}
