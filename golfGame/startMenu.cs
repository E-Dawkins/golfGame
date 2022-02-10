using Raylib_cs;
using System.Numerics;

namespace golfGame
{
    class Button
    {
        public Vector2 pos = new Vector2();
        public string name = "Button";
    }

    class startMenu
    {
        string curButton = "";
        Vector2 buttonSize = new Vector2();
        Vector2 textOffset = new Vector2();

        Button newGame;
        Button levelSelect;
        Button info;
        Button exit;
        Game game = new Game();

        public void LoadMenu()
        {
            buttonSize = new Vector2(game.windowWidth / 2, game.windowHeight / 8);
            textOffset = new Vector2(game.windowWidth / 4, game.windowHeight / 32 + 5);

            newGame = new Button();
            newGame.pos = new Vector2(game.windowWidth/4, game.windowHeight * 0.2f);
            newGame.name = "New Game";

            levelSelect = new Button();
            levelSelect.pos = new Vector2(game.windowWidth/4, game.windowHeight * 0.4f);
            levelSelect.name = "Level Select";

            info = new Button();
            info.pos = new Vector2(game.windowWidth / 4, game.windowHeight * 0.6f);
            info.name = "How To Play";

            exit = new Button();
            exit.pos = new Vector2(game.windowWidth / 4, game.windowHeight * 0.8f);
            exit.name = "Exit";
        }

        public void Update()
        {
            detectClick(newGame);
            detectClick(levelSelect);
            detectClick(info);
            detectClick(exit);
        }

        public void Draw()
        {
            Raylib.BeginDrawing();
            Raylib.ClearBackground(Color.GREEN);

            DrawButton(newGame);
            DrawButton(levelSelect);
            DrawButton(info);
            DrawButton(exit);

            Raylib.EndDrawing();
        }

        void DrawButton(Button b)
        {
            Raylib.DrawRectangleRounded(new Rectangle(b.pos.X, b.pos.Y, buttonSize.X, buttonSize.Y), 0.5f, 1, Color.BLACK);
            RaylibExt.centerText(b.name, 40, b.pos + textOffset, Color.WHITE);
        }

        bool canClick(Vector2 buttonPos, Vector2 buttonSize)
        {
            Vector2 mousePos = Raylib.GetMousePosition();

            if (mousePos.X >= buttonPos.X - buttonSize.X/2 + textOffset.X && mousePos.X <= buttonPos.X + buttonSize.X/2 + textOffset.X)
            {
                if (mousePos.Y >= buttonPos.Y - buttonSize.Y/2 + textOffset.Y && mousePos.Y <= buttonPos.Y + buttonSize.Y/2 + textOffset.Y)
                {
                    return true;
                }
            }

            return false;
        }

        void detectClick(Button b)
        {
            if (Raylib.IsMouseButtonPressed(MouseButton.MOUSE_LEFT_BUTTON))
            {
                if (canClick(b.pos, buttonSize))
                {
                    curButton = b.name;
                    Console.WriteLine(curButton);
                }
            }
        }
    }
}
