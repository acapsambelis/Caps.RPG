using System;
using System.Text;
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

        public string ToString(int chars)
        {
            if (chars <= 0)
            {
                // Invalid chunk size; return original description unmodified.
                return description ?? string.Empty;
            }

            if (string.IsNullOrEmpty(description))
            {
                return string.Empty;
            }

            var input = description;
            int length = input.Length;

            var newline = Environment.NewLine;
            var indent = "  "; // Two spaces for indentation on subsequent lines

            // Approximate capacity: original length + number of inserted newlines * (newline length + indent length)
            int estimatedNewlines = Math.Max(0, (length - 1) / chars);
            var sb = new StringBuilder(length + estimatedNewlines * (newline.Length + indent.Length));

            int count = 0;
            for (int i = 0; i < length; i++)
            {
                sb.Append(input[i]);
                count++;

                if (count >= chars)
                {
                    // If not at the very end, insert newline, indentation and reset count
                    if (i != length - 1)
                    {
                        sb.Append(newline);
                        sb.Append(indent);
                    }
                    count = 0;
                }
            }

            return sb.ToString();
        }

        public ConsoleColor Color
        {
            get { return color.Color; }
        }
    }
}
