using Caps.RPG.Rules.Creatures.Actions;
using Caps.RPG.Rules.Maps;

namespace Caps.RPG.Rules.Creatures.Classed.Classes
{
    public class Paladin : CharacterClass
    {
        public Paladin() { }

        #region GenericMethods
        public override string ToString()
        {
            return "Paladin";
        }
        #endregion

        public static List<CombatAction> GetCombatActionsForClass(int classCount)
        {
            return actionDictionary.Where(kvp => kvp.Key >= classCount).Select(kvp => kvp.Value).ToList();
        }

        public readonly static Dictionary<int, CombatAction> actionDictionary = new Dictionary<int, CombatAction>()
        {
            { 1, new CombatAction("Smite", "You deal damage to one target equal to 5 times your Paladin level.", 1, Smite, new ActionSetup(true, 1, null, ActionSetup.TargetType.SingleCreature)) },
        };

        public static ActionResult Smite(Combattant source, TileBase[] targets)
        {
            if (targets.Length != 1)
                return new ActionResult();

            Combattant targetCreature = targets[0].GetFeature<Combattant>();
            ClassedCharacter? sourceClassed = source.Creature as ClassedCharacter;
            if (sourceClassed != null)
            {
                targetCreature.Health -= sourceClassed.GetLevels(typeof(Paladin)) * 5;
            }
            return new ActionResult();
        }
    }
}
