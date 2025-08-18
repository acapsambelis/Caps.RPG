using System;

namespace Caps.RPG.DungeonCrawler
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            int version = args.Length > 0 && int.TryParse(args[0], out int v) ? v : 0;
            if (version == 1)
            {
                using var game = new DungeonCrawlerClient();
                game.Run();
            }
            else if (version == 2)
            {
                using var game = new GeonBitUI_Examples2();
                game.Run();
            }
            else
            {
                return;
            }
        }
    }
}
