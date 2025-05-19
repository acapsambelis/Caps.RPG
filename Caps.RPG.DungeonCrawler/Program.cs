namespace Caps.RPG.DungeonCrawler
{
    internal static class Program
    {
        private static void Main()
        {
            using var game = new DungeonCrawlerClient();
            game.Run();
        }
    }
}
