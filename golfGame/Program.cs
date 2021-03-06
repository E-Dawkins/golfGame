using Raylib_cs;

namespace golfGame
{
    internal class Program
    {
        public static Game game;

        static void Main(string[] args)
        {
            game = new Game();
            startMenu sMenu = new startMenu();

            Raylib.InitWindow(game.windowWidth, game.windowHeight, "Golf");
            Raylib.SetTargetFPS(60);
            Raylib.SetExitKey(KeyboardKey.KEY_NULL);

            game.LoadGame();
            sMenu.LoadMenu();

            while (!Raylib.WindowShouldClose())
            {
                if (startMenu.onStartMenu)
                {
                    sMenu.Update();
                    sMenu.Draw();
                }
                else
                {
                    game.Update();
                    game.Draw();
                }
            }

            Raylib.CloseWindow();
        }
    }
}