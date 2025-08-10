using Caps.RPG.Rules.Creatures;
using Caps.RPG.Rules.Creatures.Actions;
using Caps.RPG.Rules.Maps;

namespace Caps.RPG.Rules
{
    public class MainLoop
    {
        public CombatState State;

        public MainLoop(TileMap map, List<Combattant> combattants)
        {
            State = new CombatState(map);
            foreach (Combattant c in combattants)
            {
                State.AddCombattant(c);
            }
            State.BuildCombatOrder();
        }

        public void BetterLoop(
            Action<Combattant[]> ScoreboardFunction,
            Action<TileMap, Dictionary<TileBase, ConsoleColor?>?> DrawMap,
            Action<Combattant> CreatureDisplayFunction,
            Func<TileMap, TileBase, double, bool, TileBase> GetDestination,
            Func<List<CombatAction>, CombatAction> GetAction,
            Func<CombatState, Combattant, double, Combattant> GetTarget
        )
        {
            int rounds = 0;
            while (State.HasNoVictor())
            {
                rounds++;
                var highlights = new Dictionary<TileBase, ConsoleColor?>();
                foreach (Combattant currentCreature in State.CombatOrder)
                {
                    // display
                    if (currentCreature.Creature.Status != Creature.HealthStatus.Alive) continue;
                    ScoreboardFunction(State.CombatOrder);
                    CreatureDisplayFunction(currentCreature);

                    // action
                    int actionsAvailable = 3;
                    while (actionsAvailable > 0 && currentCreature.Creature.Status == Creature.HealthStatus.Alive)
                    {
                        highlights = new Dictionary<TileBase, ConsoleColor?>()
                        {
                            { currentCreature.Position, null }
                        };
                        DrawMap(State.Map, highlights);

                        List<CombatAction> availableActions = currentCreature.Creature.GetCombatActions();
                        CombatAction chosen = GetAction(availableActions);
                        TileBase? target = null;
                        if (chosen.NeedsLocation)
                        {
                            target = GetDestination(State.Map, currentCreature.Position, chosen.Distance, chosen.NeedsTarget);
                        }
                        chosen.Execution(currentCreature, target);
                        actionsAvailable -= chosen.Cost;
                    }
                }
            }
        }
    }
}
