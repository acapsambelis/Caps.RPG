using Caps.RPG.Rules.Creatures.Actions;


namespace Caps.RPG.Rules.Creatures.Classed.Classes
{
    public class Fighter : CharacterClass
    {

        public Fighter() { }

        #region GenericMethods
        public override string ToString()
        {
            return "Fighter";
        }
        #endregion

        public static List<CombatAction> GetCombatActionsForClass(int classCount)
        {
            return actionDictionary.Where(kvp => kvp.Key >= classCount).Select(kvp => kvp.Value).ToList();
        }

        public readonly static Dictionary<int, CombatAction> actionDictionary = new Dictionary<int, CombatAction>()
        {
            { 1, new CombatAction("Second Wind", "You regain Health equal to 5 times your Fighter level.", 1, SecondWind, false, 0) },
            { 2, new CombatAction("Attack Twice", "You regain attack twice.", 1, HitTwice, true, 1) },
        };

        public static ActionResult SecondWind(Creature source, Creature? target = null)
        {
            ClassedCharacter? sourceClassed = source as ClassedCharacter;
            if (sourceClassed != null)
            {
                sourceClassed.Health += sourceClassed.GetLevels(typeof(Fighter)) * 5;
            }
            return new ActionResult();
        }

        public static ActionResult HitTwice(Creature source, Creature? target)
        {
            CombatAction? combatAction = source.GetCombatActions().Where(c => c.Name == "Attack").FirstOrDefault();
            if (combatAction != null)
            {
                combatAction.Execution(source, target);
                combatAction.Execution(source, target);
            }
            return new ActionResult();
        }
    }
}
