using Caps.RPG.DungeonCrawler.GameObjects;
using Caps.RPG.DungeonCrawler.UI;
using Caps.RPG.MonoGame;
using Caps.RPG.MonoGame.Graphics;
using Caps.RPG.Rules;
using Caps.RPG.Rules.Creatures;
using Caps.RPG.Rules.Creatures.Actions;
using Caps.RPG.Rules.Creatures.Classed;
using Caps.RPG.Rules.Maps;
using Caps.Util;
using Caps.Util.Lua;
using GeonBit.UI;
using GeonBit.UI.Entities;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Caps.RPG.DungeonCrawler.Scenes
{
    public class GameScene(CommonConfig config) : BaseScene(config, CameraSceneMode.Panning)
    {
        private readonly TileMap hexMap = HexMap.GenerateRandomMap(0, 3);
        private readonly List<Combattant> combattants = [];
        private readonly Dictionary<string, Sprite> characterSprites = [];
        private Map _map;
        private MainLoop gameLoop;
        private readonly List<IUIEntity> uiUpdatingEntities = [];
        public override void Initialize()
        {
            var characterLoader = new LuaEntityLoader("Characters");
            List<ClassedCharacter> blueTeam = characterLoader.LoadComponentsFromCategory<ClassedCharacter>("BlueTeam");
            List<ClassedCharacter> redTeam = characterLoader.LoadComponentsFromCategory<ClassedCharacter>("RedTeam");

            combattants.AddRange(blueTeam.Select(c => new Combattant(c, TerminalColors.Blue, hexMap.RandomTile(true))));
            combattants.AddRange(redTeam.Select(c => new Combattant(c, TerminalColors.Red, hexMap.RandomTile(true))));

            foreach (Combattant combattant in combattants)
            {
                combattant.HealAll();
            }
            combattants[0].Health = 1;
            combattants[1].Health = 15;


            gameLoop = new(hexMap, combattants);

            base.Initialize();
            InitializeUI();

            worldSize = _map.GetBoundingBox();
            Core.Camera.ZoomToWorldSize(worldSize.Value);
            Core.Camera.FollowPosition = _map.GetTilePosition(combattants[0].Position);
            Core.Camera.Mode = CameraMoveMode.Follow;
            Task.Run(() => gameLoop.StartAsyncLoop());
        }

        public void InitializeUI()
        {
            // turn order
            // create top panel
            int topPanelHeight = 128;
            Panel topPanel = new(new Vector2(0, topPanelHeight + 2), PanelSkin.None, Anchor.TopCenter)
            {
                Padding = new Vector2(5)
            };

            int initiativeWidth = topPanelHeight * combattants.Count;
            Panel initiative = new(new Vector2(initiativeWidth + 10, topPanelHeight + 2 + 10), PanelSkin.Fancy, Anchor.TopCenter)
            {
                Padding = Vector2.Zero
            };
            topPanel.AddChild(initiative);

            foreach (Combattant combattant in gameLoop.State.CombatOrder)
            {
                Combattant currentCombattant = combattant;
                InitiativeTracker tracker = new(characterSprites[combattant.Name + " " + combattant.Team.ToString()], ref currentCombattant);
                initiative.AddChild(tracker.Panel);
            }
            UserInterface.Active.AddEntity(topPanel);

            Panel characterControlPanels = new(new Vector2(500, 120 * combattants.Count + 10), PanelSkin.Default, Anchor.BottomLeft)
            {
                Padding = new Vector2(5)
            };
            foreach (Combattant combattant in combattants)
            {
                CharacterControlsPanel characterControlPanel = new(combattant, hexMap);
                uiUpdatingEntities.Add(characterControlPanel);
                characterControlPanels.AddChild(characterControlPanel.Panel);
                characterControlPanel.Panel.Visible = combattant == gameLoop.CurrentCombattant;
            }
            UserInterface.Active.AddEntity(characterControlPanels);
        }

        public override void LoadContent()
        {
            base.LoadContent();

            TextureAtlas characterAtlas = TextureAtlas.FromFile(Core.Content, "images/characters-definition.xml");
            foreach (Combattant combattant in combattants)
            {
                TextureRegion region = characterAtlas.GetRegion(combattant.Name + " " + combattant.Team.ToString());
                characterSprites[combattant.Name + " " + combattant.Team.ToString()] = new Sprite(region, new Vector2(3));
            }

            TextureAtlas actionIconAtlas = TextureAtlas.FromFile(Core.Content, "images/action-icons-definition.xml");
            foreach (var kvp in actionIconAtlas.GetAllRegions())
            {
                CharacterControlsPanel.ActionIcons[kvp.Key.ToLower()] = new Sprite(kvp.Value, new Vector2(1.0f));
            }

            _map = new Map(hexMap, characterSprites);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            _map.Update();
            foreach (var entity in uiUpdatingEntities)
            {
                if (entity is InitiativeTracker tracker)
                {
                    tracker.Update();
                }
            }
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime, () =>
            {
                return _map.Draw();
            });
        }
    }
}
