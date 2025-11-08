using Caps.RPG.Rules.Creatures.Actions;
using Caps.RPG.Rules.Maps;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Caps.RPG.Rules.Creatures.Unclassed
{
    public interface IComputerControlled
    {
        public TileMap FullMap { get; set; }

        public (CombatAction, TileBase[]) ChooseAction(List<CombatAction> combatActions);
    }
}
