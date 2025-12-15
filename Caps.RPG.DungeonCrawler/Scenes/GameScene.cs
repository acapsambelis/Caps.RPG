using Caps.RPG.DungeonCrawler.GameObjects;
using Caps.RPG.DungeonCrawler.UI;
using Caps.RPG.MonoGame;
using Caps.RPG.MonoGame.Graphics;
using Caps.RPG.Rules;
using Caps.RPG.Rules.Creatures;
using Caps.RPG.Rules.Creatures.Actions;
using Caps.RPG.Rules.Creatures.Classed;
using Caps.RPG.Rules.Creatures.Unclassed;
using Caps.RPG.Rules.Maps;
using GeonBit.UI;
using GeonBit.UI.Entities;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Caps.RPG.DungeonCrawler.Scenes
{
    public class GameScene(CommonConfig config, ClassedCharacter[] playerCharacters, Dictionary<string, Sprite> characterSprites) : BaseScene(config, CameraSceneMode.Panning)
    {
        private readonly TileMap hexMap = HexMap.GenerateRandomMap(0, 3);
        private readonly ClassedCharacter[] playerCharacters = playerCharacters;
        private readonly List<Combattant> combattants = [];
        private readonly Dictionary<string, Sprite> characterSprites = characterSprites;
        private Map _map;
        private MainLoop gameLoop;
        private readonly List<IUIEntity> uiUpdatingEntities = [];
        private Panel characterControlPanels;
        private CharacterControlsPanel currentCharacterPanel;
        private InitiativePanel initiativePanel;
        private SelectList actionLog;
        private int actionsAvailable;
        private CombatAction tentativeAction;

        public override void Initialize()
        {
            combattants.AddRange(playerCharacters.Select(c => new Combattant(c, PartyCreationScene.PLAYER_TEAM, hexMap.RandomTile(true))));

            var monsterLoader = Program.LuaEnvironment.LoadFromFolder("Monsters");
            List<Monster> redTeam = monsterLoader.LoadComponentsFromCategory<Monster>("MonsterInstances");
            combattants.AddRange(redTeam.Select(c => new Combattant(c, "MONSTERS", hexMap.RandomTile(true))));


            var itemLoader = Program.LuaEnvironment.LoadFromFolder("Items");
            var swords = itemLoader.LoadComponentsFromCategory<Rules.Inventory.Item>("Swords");
            var crowns = itemLoader.LoadComponentsFromCategory<Rules.Inventory.Item>("Crowns");
            var debugSword = swords.FirstOrDefault(i => i.Name == "Debug Sword");
            var crown = crowns.FirstOrDefault(i => i.Name == "Royal Crown");
            playerCharacters[0].Inventory.Slots[0].Item = debugSword;
            playerCharacters[0].Inventory.Slots[3].Item = crown;
            playerCharacters[1].Inventory.Slots[1].Item = debugSword;
            playerCharacters[1].Inventory.Slots[3].Item = crown;

            foreach (Combattant combattant in combattants)
            {
                combattant.HealMax();
                if (combattant.IsComputerControlled()) ((IComputerControlled)combattant.Creature).FullMap = hexMap;
            }

            gameLoop = new(hexMap, combattants);
            gameLoop.OnActionCompleted += LogAction;

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
            UserInterface.Active.AddEntity(topPanel);
            initiativePanel = new(initiative);

            // create character panels
            characterControlPanels = new(new Vector2(510, 730), PanelSkin.Default, Anchor.BottomLeft)
            {
                Padding = new Vector2(5)
            };
            var bottomLeftPanels = GeonBit.UI.Utils.PanelsGrid.GenerateColums([new(510, 0), new(75, 0)], characterControlPanels);
            var characterPanelsContainer = bottomLeftPanels[0];
            var characterButtonsContainer = bottomLeftPanels[1];

            foreach (Combattant combattant in combattants)
            {
                Combattant currentCombattant = combattant;
                Sprite characterSprite;
                characterSprite = characterSprites[currentCombattant.Name + " " + currentCombattant.Team.ToString()];
                CombattantEntity entity = new(ref currentCombattant, characterSprite, ref _map, ActionClicked, EndTurn);
                characterPanelsContainer.AddChild(entity.CharacterControlsPanel.Panel);
                initiative.AddChild(entity.InitiativeTracker.Panel);
                initiativePanel.AddCombattant(currentCombattant, entity.InitiativeTracker);
                entity.CharacterControlsPanel.Panel.Visible = currentCombattant == gameLoop.CurrentCombattant;
                uiUpdatingEntities.Add(entity);

                if (combattant.IsComputerControlled()) continue;

                characterButtonsContainer.AddChild(entity.SidebarButtons);
                UserInterface.Active.AddEntity(entity.InventoryManager.Panel);
            }
            UserInterface.Active.AddEntity(characterControlPanels);

            // create action log (for debug/the informed player)
            Panel actionLogPanel = new(new Vector2(1000, 160), PanelSkin.Simple, Anchor.BottomCenter, new Vector2(-10, 0))
            {
                Visible = true
            };
            actionLog = new(size: new Vector2(-1, 120))
            {
                ExtraSpaceBetweenLines = -8,
                ItemsScale = 0.75f,
                Locked = true,
            };
            actionLogPanel.AddChild(actionLog);
            UserInterface.Active.AddEntity(actionLogPanel);
        }

        public override void LoadContent()
        {
            base.LoadContent();

            TextureAtlas monsterAtlas = TextureAtlas.FromFile(Core.Content, "images/monsters-definition.xml");
            foreach (Combattant combattant in combattants.Where(c => c.Creature is Monster))
            {
                TextureRegion region = monsterAtlas.GetRegion((combattant.Creature as Monster).MonsterBlueprint.ToString());
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

        private void ActionClicked(GeonBit.UI.Entities.Entity entity)
        {
            string actionName = ((Button)entity).Tag;
            if (string.IsNullOrEmpty(actionName)) return;

            var actions = gameLoop.CurrentCombattant.GetCombatActions();
            tentativeAction = actions.FirstOrDefault(a => a.Name == actionName);
        }

        private void EndTurn(GeonBit.UI.Entities.Entity entity)
        {
            gameLoop.ForceEndTurn();
        }

        private void LogAction(object sender, ActionCompletedEventArgs e)
        {
            List<string> tokens = [.. e.Result.ToString(130).Split('\n')];
            tokens.ForEach(actionLog.AddItem);
            actionLog.scrollToEnd();
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
                {
                    if (tentativeAction != null)
                    {
                        gameLoop.ChosenAction = tentativeAction;
                        tentativeAction = null;
                        gameLoop.ChosenTargets = [tile.TileBase];
                    }
                }
                foreach (Tile t in _map.Tiles)
                    t.TileBase.Highlighted = false;

                currentCharacterPanel.ToggleAbilityButtons();
            }
            if (gameLoop.ActionsAvailable != actionsAvailable)
            {
                actionsAvailable = gameLoop.ActionsAvailable;
                currentCharacterPanel.SetActionNumber(actionsAvailable);
            }

            initiativePanel.Update(gameLoop.State.CombatOrder, gameLoop.CurrentCombattant);
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
                {
                    currentCharacterPanel = combattantEntity.CharacterControlsPanel;
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
