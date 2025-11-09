using Caps.RPG.MonoGame;
using Caps.RPG.Rules.Creatures.Classed;
using Caps.Util.Lua;
using GeonBit.UI;
using GeonBit.UI.Entities;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Caps.RPG.DungeonCrawler.Scenes
{
    public class PartyCreationScene(CommonConfig config) : BaseScene(config, CameraSceneMode.FullScreen)
    {
        ClassedCharacter[] playerCharacters = [];

        public override void Initialize()
        {
            var characterLoader = new LuaEntityLoader("Characters");
            playerCharacters = [.. characterLoader.LoadComponentsFromCategory<ClassedCharacter>("BlueTeam")];

            base.Initialize();
            InitializeUI();
        }

        public void InitializeUI()
        {
            Button start = new("Start", ButtonSkin.Default, Anchor.Center, new Vector2(150, 50));
            start.OnClick += (btn) => StartGame();
            UserInterface.Active.AddEntity(start);
        }

        private void StartGame()
        {
            Core.ChangeScene(new GameScene(config, playerCharacters));
        }
    }
}
