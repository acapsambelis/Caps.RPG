﻿using Caps.RPG.CombatEngine.Creatures.Actions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Caps.RPG.CombatEngine.Creatures.Classed.Classes
{
    public class Paladin : CharacterClass
    {
        public Paladin() { }

        #region GenericMethods
        public override string ToString()
        {
            return "Paladin";
        }
        #endregion

        public static List<CombatAction> GetCombatActionsForClass(int classCount)
        {
            return actionDictionary.Where(kvp => kvp.Key >= classCount).Select(kvp => kvp.Value).ToList();
        }

        public readonly static Dictionary<int, CombatAction> actionDictionary = new Dictionary<int, CombatAction>()
        {
            { 1, new CombatAction("Smite", "You deal damage to one target equal to 5 times your Paladin level.", 1, Smite, true) },
        };

        public static void Smite(Creature source, Creature? target = null)
        {
            if (target == null)
                return;

            ClassedCharacter? sourceClassed = source as ClassedCharacter;
            if (sourceClassed != null)
            {
                target.Health -= sourceClassed.GetLevels(typeof(Paladin)) * 5;
            }
        }
    }
}
