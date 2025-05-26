namespace Caps.RPG.DungeonCrawler.Server
{
    internal class Program
    {
        static void Main(string[] args)
        {
            try
            {
                DungeonListTCPServer server = new DungeonListTCPServer();
                server.Start();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fatal error starting server: {ex}");
            }
        }
    }
}
