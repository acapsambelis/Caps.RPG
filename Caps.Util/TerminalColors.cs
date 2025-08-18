namespace Caps.Util
{
    // used for terminal maps only
    public class TerminalColors
    {
        public static readonly TerminalColor Black = new(ConsoleColor.Black, ConsoleColor.Gray);
        public static readonly TerminalColor Blue = new(ConsoleColor.DarkBlue, ConsoleColor.Blue);
        public static readonly TerminalColor Green = new(ConsoleColor.DarkGreen, ConsoleColor.Green);
        public static readonly TerminalColor Cyan = new(ConsoleColor.DarkCyan, ConsoleColor.Cyan);
        public static readonly TerminalColor Red = new(ConsoleColor.DarkRed, ConsoleColor.Red);
        public static readonly TerminalColor Magenta = new(ConsoleColor.DarkMagenta, ConsoleColor.Magenta);
        public static readonly TerminalColor Yellow = new(ConsoleColor.DarkYellow, ConsoleColor.Yellow);
        public static readonly TerminalColor Gray = new(ConsoleColor.DarkGray, ConsoleColor.White);
    }

    public class TerminalColor
    {
        public ConsoleColor Color { get; set; } = ConsoleColor.Gray;
        public ConsoleColor Highlight { get; set; } = ConsoleColor.White;
        public TerminalColor(ConsoleColor color, ConsoleColor highlight)
        {
            Color = color;
            Highlight = highlight;
        }

        public override string ToString()
        {
            return Color.ToString();
        }
    }
}
