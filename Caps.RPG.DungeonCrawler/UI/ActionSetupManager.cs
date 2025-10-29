using Caps.RPG.Rules.Creatures;
using Caps.RPG.Rules.Creatures.Actions;
using Caps.RPG.Rules.Maps;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Caps.RPG.DungeonCrawler.UI
{
    public static class ActionSetupManager
    {
        public static Func<TileMap, Combattant, ActionSetup, TileBase[]> GetAction(ActionSetup setup)
        {
            return setup.TargetType switch
            {
                ActionSetup.SourceType.SingleTile => ChooseSingleTile,
                ActionSetup.SourceType.SingleCreature => ChooseSingleEntity,
                ActionSetup.SourceType.Area => ChooseArea,
                ActionSetup.SourceType.None => null,
                ActionSetup.SourceType.MultipleCreature => null,
                ActionSetup.SourceType.Custom => null,
                _ => throw new NotImplementedException($"TargetType {setup.TargetType} not implemented in ActionSetupManager.GetAction"),
            };
        }

        public static TileBase[] ChooseSingleTile(TileMap map, Combattant source, ActionSetup setup)
        {
            return [];
        }

        public static TileBase[] ChooseSingleEntity(TileMap map, Combattant source, ActionSetup setup)
        {
            return [];
        }

        public static TileBase[] ChooseArea(TileMap map, Combattant source, ActionSetup setup)
        {
            return [];
        }

        public static TileBase[] ValidTiles(TileMap map, Combattant source, ActionSetup setup)
        {
            // prepare valid targets
            TileBase[] validTargets = map.GetTiles(
                source.Position,
                setup.GetRange(source)
            );
            if (setup.NeedsEmptyTile)
                validTargets = [.. validTargets.Where(t => t.IsEmpty())];
            if (setup.TargetType == ActionSetup.SourceType.SingleCreature)
                validTargets = [.. validTargets.Where(t => t.GetFeature<Combattant>() != null)];

            return validTargets;
        }
    }
}
