using Caps.RPG.Rules.Attributes;
using Caps.RPG.Rules.Creatures.Actions;
using Caps.RPG.Rules.Maps;

namespace Caps.RPG.Rules.Creatures.Unclassed
{
    public class MonsterBlueprint
    {
        public string Name;
        public AttributeSet Attributes;

        private CombatAction _action;
        private TileBase[] _tileBase;

        public Action<Monster, CombatAction[], TileMap, TileBase> CombatThink;

        public MonsterBlueprint()
        {
            Attributes = new AttributeSet();
        }
        public MonsterBlueprint(string name, AttributeSet attributeSet)
        {
            Name = name;
            Attributes = attributeSet;
        }

        public (CombatAction, TileBase[]) ChooseAction(Monster monster, CombatAction[] availableActions, TileMap map, TileBase position)
        {
            if (CombatThink == null) throw new InvalidOperationException("CombatThink is not set for this MonsterBlueprint.");

            CombatThink(monster, availableActions, map, position);
            return (_action, _tileBase);
        }

        public void Decide(CombatAction action, TileBase[] tileBases)
        {
            _action = action;
            _tileBase = tileBases;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}