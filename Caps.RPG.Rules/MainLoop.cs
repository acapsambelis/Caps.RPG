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

        public void BetterLoop(
            Action<Combattant[]> ScoreboardFunction,
            Func<Combattant[], Combattant[,]> MapFunction,
            Action<Combattant> CreatureDisplayFunction,
            Func<Combattant, Vector2D> GetDestination,
            Func<List<CombatAction>, CombatAction> GetAction,
            Func<CombatState, Combattant, double, Creature> GetTarget
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

                    // movement
                    Vector2D destination = new Vector2D(-Double.MaxValue, -Double.MaxValue);
                    while (currentCreature.Position.Distance(destination) > currentCreature.Creature.MoveSpeed)
                    {
                        Vector2D tmp = GetDestination(currentCreature);

                        if (map[tmp.IntX, tmp.IntY] != null && map[tmp.IntX, tmp.IntY] != currentCreature) continue;

                        destination = tmp;
                    }
                    currentCreature.Move(destination);
                    MapFunction(State.CombatOrder);

                    // action
                    List<CombatAction> availableActions = currentCreature.Creature.GetCombatActions();
                    CombatAction chosen = GetAction(availableActions);
                    Creature? target = null;
                    if (chosen.NeedsTarget)
                    {
                        target = GetTarget(State, currentCreature, chosen.Distance);
                    }
                    chosen.Execution(currentCreature.Creature, target);
                }
            }
        }
    }
}
