using Caps.RPG.Rules.Creatures;
using Caps.RPG.Rules.Creatures.Actions;
using Caps.RPG.Rules.Creatures.Unclassed;
using Caps.RPG.Rules.Maps;

namespace Caps.RPG.Rules
{
    public class MainLoop
    {
        public CombatState State;
        public Combattant CurrentCombattant { get; internal set; }

        public MainLoop(TileMap map, List<Combattant> combattants)
        {
            State = new CombatState(map);
            foreach (Combattant c in combattants)
            {
                State.AddCombattant(c);
            }
            State.BuildCombatOrder();
            CurrentCombattant = State.CombatOrder[0];
        }

        public void SynchronousLoop(
            Action<TileMap, Dictionary<TileBase, ConsoleColor?>?>? DrawMap,
            Action<Combattant[], Combattant, int, int>? TopDisplay,
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

                    int actionsAvailable = MAX_ACTIONS;
                    do
                    {
                        // display state
                        TopDisplay?.Invoke(State.CombatOrder, currentCreature, actionsAvailable, MAX_ACTIONS);
                        var highlights = new Dictionary<TileBase, ConsoleColor?>()
                        {
                            { currentCreature.Position, null }
                        };
                        DrawMap?.Invoke(State.Map, highlights);

                        // action phase
                        List<CombatAction> availableActions = currentCreature.Creature.GetCombatActions();
                        // get actions
                        CombatAction chosen = GetAction(availableActions);
                        TileBase[] targets = [];
                        // get targets if needed
                        if (chosen.Setup.NeedsTarget)
                        {
                            // prepare valid targets
                            TileBase[] validTargets = State.Map.GetTiles(
                                currentCreature.Position,
                                chosen.Setup.GetRange(currentCreature)
                            );
                            if (chosen.Setup.NeedsEmptyTile)
                                validTargets = [.. validTargets.Where(t => t.IsEmpty())];

                            // highlight valid targets
                            highlights = [];
                            foreach (TileBase tile in validTargets)
                            {
                                highlights.Add(tile, null);
                            }
                            DrawMap?.Invoke(State.Map, highlights);
                            // get targerts
                            targets = GetTargets(State.Map, currentCreature.Position, chosen.Setup);
                        }
                        // execute action
                        ActionResult result = chosen.Execution(currentCreature, targets);
                        actionsAvailable -= chosen.Cost;
                        // display result
                        DisplayActionResult(result);

                        // loop while action points remain
                    } while (actionsAvailable > 0 && currentCreature.Creature.Status == Creature.HealthStatus.Alive);
                }
            }
        }

        
        private int actionsAvailable;
        private readonly ManualResetEvent actionSetEvent = new(false);
        private CombatAction? chosenAction;
        private readonly ManualResetEvent targetsSetEvent = new(false);
        private TileBase[]? chosenTargets;

        private static readonly CombatAction PassAction = new("Pass", "End turn", int.MaxValue, (src, targets) => new ActionResult("Turn ended"));

        public event EventHandler<ActionCompletedEventArgs>? OnActionCompleted;

        public int ActionsAvailable
        {
            get => actionsAvailable;
        }

        public CombatAction ChosenAction
        {
            internal get
            {
                // Wait until chosenAction is set (not null)
                actionSetEvent.WaitOne();
                actionSetEvent.Reset();
                return chosenAction!;
            }
            set
            {
                chosenAction = value;
                actionSetEvent.Set();
            }
        }

        public TileBase[] ChosenTargets
        {
            internal get
            {
                targetsSetEvent.WaitOne();
                targetsSetEvent.Reset();
                return chosenTargets!;
            }
            set
            {
                chosenTargets = value;
                targetsSetEvent.Set();
            }
        }

        public void ResetChoices()
        {
            chosenAction = null;
            chosenTargets = null;
            actionSetEvent.Reset();
            targetsSetEvent.Reset();
        }

        public void StartAsyncLoop()
        {
            CombatAction chosen;
            TileBase[] targets;
            ActionResult result;
            while (State.HasNoVictor())
            {
                foreach (Combattant currentCreature in State.CombatOrder)
                {
                    CurrentCombattant = currentCreature;
                    if (currentCreature.Creature.Status != Creature.HealthStatus.Alive) continue;

                    actionsAvailable = currentCreature.ActionCounts;
                    do
                    {
                        if (!currentCreature.IsComputerControlled())
                        {
                            chosen = ChosenAction;
                            targets = chosen.Setup.NeedsTarget ? ChosenTargets : [];
                            result = chosen.Execution(currentCreature, targets);
                        }
                        else
                        {
                            (chosen, targets) = (currentCreature.Creature as IComputerControlled)!.ChooseAction(currentCreature.GetCombatActions());
                            result = chosen.Execution(currentCreature, targets);
                        }
                        actionsAvailable -= chosen.Cost;
                        OnActionCompleted?.Invoke(this, new ActionCompletedEventArgs(result));

                        // loop while action points remain
                    } while (actionsAvailable > 0 && currentCreature.Creature.Status == Creature.HealthStatus.Alive);
                }
            }
        }

        public void ForceEndTurn()
        {
            // keep this atomic to avoid races with the loop reading these fields
            lock (this)
            {
                // Inject a pass action and empty targets so any waiting getter will return quickly
                chosenAction = PassAction;
                chosenTargets = [];

                // Unblock any threads waiting for a choice
                actionSetEvent.Set();
                targetsSetEvent.Set();

                // Ensure the loop will exit the action loop for the current combattant
                actionsAvailable = 0;
            }
        }
    }

    public class ActionCompletedEventArgs : EventArgs
    {
        public ActionResult? Result { get; }

        public ActionCompletedEventArgs(ActionResult result)
        {
            this.Result = result;
        }
    }
}
