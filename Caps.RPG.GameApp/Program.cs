namespace Caps.RPG.GameApp
{
    public class Program
    {
        public static void Main()
        {
            using var game = new TutorialGame();
            game.Run();
        }
    }
}
