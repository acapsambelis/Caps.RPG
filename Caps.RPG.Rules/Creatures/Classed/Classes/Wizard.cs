using Caps.RPG.Rules.Creatures.Actions;
using Caps.RPG.Rules.Helpers;
using Caps.RPG.Rules.Maps;

namespace Caps.RPG.Rules.Creatures.Classed.Classes
{
    public class Wizard : CharacterClass
    {
        public Wizard() { }

        #region GenericMethods
        public override string ToString()
        {
            return "Wizard";
        }
        #endregion

        public static List<CombatAction> GetCombatActionsForClass(int classCount)
        {
            return actionDictionary.Where(kvp => kvp.Key >= classCount).Select(kvp => kvp.Value).ToList();
        }

        public readonly static Dictionary<int, CombatAction> actionDictionary = new Dictionary<int, CombatAction>()
        {
            { 1, new CombatAction("Firebolt", "You deal damage to one target equal to 5 times your Wizard level.", 1, Firebolt, new ActionSetup(true, 10, null, ActionSetup.TargetType.SingleCreature)) },
            { 1, new CombatAction("Fireball", "You deal damage in a radius equal to 5 times your Wizard level.", 1, Fireball, new ActionSetup(true, 10, null, ActionSetup.TargetType.SingleCreature)) },
        };

        public static ActionResult Firebolt(Combattant source, TileBase[] targets)
        {
            if (targets.Length == 0)
                return new ActionResult();

            Combattant targetCreature = targets[0].GetFeature<Combattant>();
            ClassedCharacter? sourceClassed = source.Creature as ClassedCharacter;
            if (sourceClassed != null)
            {
                targetCreature.Health -= sourceClassed.GetLevels(typeof(Wizard)) * 5;
            }
            return new ActionResult();
        }

        public static ActionResult Fireball(Combattant source, TileBase[] targets)
        {
            if (targets.Length == 0)
                return new ActionResult();

            ClassedCharacter? sourceClassed = source.Creature as ClassedCharacter;
            if (sourceClassed != null)
            {
                List<Combattant> targetCreatures = [.. targets.Select(t => targets[0].GetFeature<Combattant>()).Where(tc => tc != null)];
                foreach (var creature in targetCreatures)
                {
                    creature.Health -= sourceClassed.GetLevels(typeof(Wizard)) * 5;
                }
            }
            return new ActionResult();
        }
    }
}
