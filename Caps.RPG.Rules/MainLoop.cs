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
            Action<TileMap, Dictionary<TileBase, ConsoleColor?>?> DrawMap,
            Action<Combattant[], Combattant, int, int> TopDisplay,
            Func<TileMap, TileBase, ActionSetup, TileBase[]> GetTargets,
            Func<List<CombatAction>, CombatAction> GetAction,
            Action<ActionResult> DisplayActionResult
        )
        {
            int MAX_ACTIONS = 3;
            int rounds = 0;
            while (State.HasNoVictor())
            {
                rounds++;
                foreach (Combattant currentCreature in State.CombatOrder)
                {
                    if (currentCreature.Creature.Status != Creature.HealthStatus.Alive) continue;

                    // action
                    int actionsAvailable = MAX_ACTIONS;
                    do
                    {
                        TopDisplay(State.CombatOrder, currentCreature, actionsAvailable, MAX_ACTIONS);
                        var highlights = new Dictionary<TileBase, ConsoleColor?>()
                        {
                            { currentCreature.Position, null }
                        };
                        DrawMap(State.Map, highlights);

                        List<CombatAction> availableActions = currentCreature.Creature.GetCombatActions();
                        CombatAction chosen = GetAction(availableActions);
                        TileBase[] targets = [];
                        if (chosen.Setup.NeedsTarget)
                        {
                            TileBase[] validTargets = State.Map.GetTiles(
                                currentCreature.Position,
                                chosen.Setup.GetRange(currentCreature)
                            );
                            if (chosen.Setup.NeedsEmptyTile)
                                validTargets = [.. validTargets.Where(t => t.IsEmpty())];

                            highlights = [];
                            foreach (TileBase tile in validTargets)
                            {
                                highlights.Add(tile, null);
                            }
                            DrawMap(State.Map, highlights);
                            targets = GetTargets(State.Map, currentCreature.Position, chosen.Setup);
                        }
                        ActionResult result = chosen.Execution(currentCreature, targets);
                        actionsAvailable -= chosen.Cost;
                        DisplayActionResult(result);
                    } while (actionsAvailable > 0 && currentCreature.Creature.Status == Creature.HealthStatus.Alive);
                }
            }
        }
    }
}
