using Caps.RPG.Rules.Attributes;
using Caps.RPG.Rules.Creatures.Actions;
using Caps.RPG.Rules.Maps;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Caps.RPG.Rules.Creatures.Unclassed
{
    public class Monster : Creature, IComputerControlled
    {
        public MonsterBlueprint MonsterBlueprint;
        public Monster() { }
        public Monster(string name, AttributeSet attributes) : base(name, attributes) { }
        public Monster(string name) : base(name, new AttributeSet()) { }

        public CombatAction ChooseAction()
        {
            throw new NotImplementedException();
        }

        public TileBase[] ChooseTargets()
        {
            throw new NotImplementedException();
        }
    }

    public enum MonsterType
    {
        Beast,
        Undead,
        Humanoid,
        Aberration,
        Construct,
        Dragon,
        Elemental,
        Fey,
        Giant,
        Monstrosity,
        Ooze,
        Plant,
        Celestial,
        Fiend,
        Shadow
    }
}
