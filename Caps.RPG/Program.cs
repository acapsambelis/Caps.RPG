using Caps.RPG.CombatEngine;

namespace Caps.RP
{
    public class Program
    {
        static void Main(string[] args)
        {
            MainLoop mainLoop = new MainLoop();
            mainLoop.Loop(GetTextInput);
        }

        public static int GetTextInput(string prompt)
        {
            Console.Write(prompt);
            string? input = Console.ReadLine();
            return Int32.Parse(input != null ? input : "");
        }
    }
}