using Caps.Util;

namespace Caps.RPG.Rules.Creatures.Actions
{
    public class ActionResult
    {
        private readonly string description;
        private readonly TerminalColor color;

        public ActionResult()
        {
            description = "";
            color = TerminalColors.Gray;
        }
        public ActionResult(string description)
        {
            this.description = description;
            color = TerminalColors.Gray;
        }
        public ActionResult(string description, TerminalColor color)
        {
            this.description = description;
            this.color = color;
        }

        public override string ToString()
        {
            return description;
        }
        public ConsoleColor Color
        {
            get { return color.Color; }
        }
    }
}
