using Caps.RPG.Rules.Creatures.Actions;
using Caps.RPG.Rules.Maps;
using System;
using System.Collections.Generic;
using System.Linq;

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

        private TileMap _map;
        private readonly HashSet<Creature> _hostileList = [];

        public TileMap FullMap { get => _map; set => _map = value; }


        public Monster() { }

        public (CombatAction, TileBase[]) ChooseAction()
        {
            return _monsterBlueprint.ChooseAction(this, [..CombatActions], _map, _map[this]);
        }

        public bool IsHostileTo(Creature other)
        {
            return _hostileList.Contains(other) && other.Status == HealthStatus.Alive;
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
