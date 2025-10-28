using Caps.RPG.DungeonCrawler.GameObjects;
using Caps.RPG.DungeonCrawler.UI;
using Caps.RPG.MonoGame;
using Caps.RPG.MonoGame.Graphics;
using Caps.RPG.Rules;
using Caps.RPG.Rules.Creatures;
using Caps.RPG.Rules.Creatures.Classed;
using Caps.RPG.Rules.Maps;
using Caps.Util;
using Caps.Util.Lua;
using GeonBit.UI;
using GeonBit.UI.Entities;
using Microsoft.Xna.Framework;
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
        private Panel characterControlPanels;
        private CharacterControlsPanel currentCharacterPanel;
        private int actionsAvailable;

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
            characterControlPanels = new(new Vector2(500, 120 * combattants.Count + 10), PanelSkin.Default, Anchor.BottomLeft)
            {
                Padding = new Vector2(5)
            };

            foreach (Combattant combattant in combattants)
            {
                Combattant currentCombattant = combattant;
                Sprite characterSprite = characterSprites[currentCombattant.Name + " " + currentCombattant.Team.ToString()];
                CombattantEntity entity = new(ref currentCombattant, characterSprite, ref _map, ActionClicked);
                characterControlPanels.AddChild(entity.CharacterControlsPanel.Panel);
                initiative.AddChild(entity.InitiativeTracker.Panel);
                entity.CharacterControlsPanel.Panel.Visible = currentCombattant == gameLoop.CurrentCombattant;
                uiUpdatingEntities.Add(entity);
            }
            // end add
            UserInterface.Active.AddEntity(topPanel);
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
            foreach (Tile t in _map.Tiles)
                RegisterClickable(t);
        }

        public void ActionClicked(GeonBit.UI.Entities.Entity entity)
        {
            string actionName = ((Button)entity).Tag;
            if (string.IsNullOrEmpty(actionName)) return;

            var actions = gameLoop.CurrentCombattant.Creature.GetCombatActions();
            gameLoop.ChosenAction = actions.FirstOrDefault(a => a.Name == actionName);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            _map.Update();
            SetCurrentCharacterPanel(gameLoop.CurrentCombattant);
            var tile = _map.ClickedTile;
            if (tile != null)
            {
                if (tile.TileBase.Highlighted)
                    gameLoop.ChosenTargets = [tile.TileBase];
                foreach (Tile t in _map.Tiles)
                    t.TileBase.Highlighted = false;

                currentCharacterPanel.ToggleAbilityButtons();
            }
            if (gameLoop.ActionsAvailable != actionsAvailable)
            {
                actionsAvailable = gameLoop.ActionsAvailable;
                currentCharacterPanel.SetActionNumber(actionsAvailable);
            }
            foreach (var entity in uiUpdatingEntities)
            {
                entity.Update();
            }
        }

        private void SetCurrentCharacterPanel(Combattant combattant)
        {
            foreach (var combattantEntity in uiUpdatingEntities.OfType<CombattantEntity>())
            {
                if (combattantEntity.SetVisibility(combattant))
                    currentCharacterPanel = combattantEntity.CharacterControlsPanel;
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
