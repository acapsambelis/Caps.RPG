using Caps.RPG.Rules.Attributes;
using Caps.RPG.Rules.Creatures.Actions;
using Caps.RPG.Rules.Maps;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Caps.RPG.Rules.Creatures.Unclassed
{
    public class Monster : Creature, IComputerControlled
    {
        private MonsterBlueprint _monsterBlueprint;
        public MonsterBlueprint MonsterBlueprint
        {
            get { return _monsterBlueprint; }
            set
            {
                _monsterBlueprint = value;
                this.Attributes = value.Attributes;
            }
        }

        public Monster() { }
        public Monster(string name, MonsterBlueprint monsterBlueprint) : base(name, monsterBlueprint.Attributes) { }
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
