using Caps.RPG.Rules.Creatures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GeonBit.UI.Entities;

namespace Caps.RPG.DungeonCrawler.UI
{
    public class InitiativePanel
    {
        public Dictionary<Combattant, InitiativeTracker> combattants;
        public readonly Panel panel;
        
        private Combattant currentCombattant;

        public InitiativePanel(Panel panel)
        {
            this.panel = panel;
            combattants = [];
        }

        public void AddCombattant(Combattant combattant, InitiativeTracker combattantPanel)
        {
            combattants[combattant] = combattantPanel;
        }

        public void Update(Combattant[] order, Combattant current)
        {
            if (currentCombattant == current) return;

            currentCombattant = current;
            panel.ClearChildren();
            int startIndex = Array.IndexOf(order, current);
            for (int i = 0; i < order.Length; i++)
            {
                int index = (startIndex + i) % order.Length;
                if (combattants.TryGetValue(order[index], out var combattantPanel))
                {
                    panel.AddChild(combattantPanel.Panel);
                }
            }
        }
    }
}
