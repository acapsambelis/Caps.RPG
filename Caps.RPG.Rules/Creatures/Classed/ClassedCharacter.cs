﻿using System.Reflection;
using Caps.RPG.Rules.Attributes;
using Caps.RPG.Rules.Creatures.Actions;
using Caps.RPG.Rules.Creatures.Classed.Classes;
using Caps.RPG.Rules.Creatures.Types;

namespace Caps.RPG.Rules.Creatures.Classed
{
    public class ClassedCharacter : Creature, CreatureType
    {
        public ClassLevelMakeup ClassLevelMakeup { get; internal set; }
        public ClassedCharacter(string name, AttributeSet attributes) : base(name, attributes)
        {
            ClassLevelMakeup = new ClassLevelMakeup();
        }
        public ClassedCharacter(string name, AttributeSet attributes, Dictionary<Type, int> classes) : base(name, attributes)
        {
            ClassLevelMakeup = new ClassLevelMakeup(classes);
        }

        public int GetLevels(Type t)
        {
            return ClassLevelMakeup.GetLevels(t);
        }


        public override List<CombatAction> GetCombatActions()
        {
            List<CombatAction> generics = base.GetCombatActions();

            List<CombatAction> allActions = [.. generics];
            foreach (var c in ClassLevelMakeup.ClassLevels)
            {
                object? classInstance = Activator.CreateInstance(c.Key);
                MethodInfo? myMethod = c.Key.GetMethod("GetCombatActionsForClass");
                if (myMethod != null && classInstance != null)
                {
                    object? returnedActions = myMethod.Invoke(classInstance, new object[] { c.Value });
                    if (returnedActions != null)
                    {
                        allActions.AddRange((List<CombatAction>)returnedActions);
                    }
                }
            }

            return allActions;
        }


        public override string ToString()
        {
            return base.ToString() + ClassLevelMakeup.ToString();
        }
    }
}
