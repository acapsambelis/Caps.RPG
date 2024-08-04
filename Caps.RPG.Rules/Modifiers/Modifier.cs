using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Caps.RPG.Engine.Modifiers
{
    public class Modifier
    {

        public enum ModifierTarget
        {
            None,
            DefenseClass,
        }

        public enum ModifierActionType
        {
            None,
            Set,
            Bonus,
        }

        #region privateMembers
        private ModifierTarget target;
        private ModifierActionType actionType;
        private int val;
        private object source;
        #endregion

        #region PublicMembers
        public ModifierTarget Target
        {
            get { return target; }
            set { target = value; }
        }
        public ModifierActionType ActionType
        {
            get { return actionType; }
            set { actionType = value; }
        }
        public int Value
        {
            get { return val; }
            set { val = value; }
        }
        public object Source
        {
            get { return source; }
            set { source = value; }
        }
        #endregion

        #region Constructors
        public Modifier(ModifierTarget mTarget, ModifierActionType mActionType, int value, object source)
        {
            this.target = mTarget;
            this.actionType = mActionType;
            this.val = value;
            this.source = source;
        }
        #endregion

        #region StandardModifiers

        public static readonly Dictionary<ModifierTarget, List<Modifier>> CreatureModifiers = new Dictionary<ModifierTarget, List<Modifier>>()
        {
            { ModifierTarget.DefenseClass, new List<Modifier>() { new Modifier(ModifierTarget.DefenseClass, ModifierActionType.Bonus, 10, "Unarmored") } }
        };


        #endregion

    }
}
