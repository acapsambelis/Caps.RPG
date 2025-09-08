using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Caps.RPG.Rules.Modifiers
{
    public interface IModifierBin
    {
        public List<IModifierBin> ChildSources { get; internal set;  }
        public Dictionary<TargetType, List<Modifier>> GetModifiers();
    }
}
