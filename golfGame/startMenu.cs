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

        public static bool onStartMenu = true;
        bool inLevelsScreen = false;
        bool inInfoScreen = false;

        public void LoadMenu()
        {
            buttonSize = new Vector2(Program.game.windowWidth / 2, Program.game.windowHeight / 8);
            textOffset = new Vector2(Program.game.windowWidth / 4, Program.game.windowHeight / 32 + 5);

            newGame = new Button();
            newGame.pos = new Vector2(Program.game.windowWidth/4, Program.game.windowHeight * 0.2f);
            newGame.name = "New Game";

            levelSelect = new Button();
            levelSelect.pos = new Vector2(Program.game.windowWidth/4, Program.game.windowHeight * 0.4f);
            levelSelect.name = "Level Select";

            info = new Button();
            info.pos = new Vector2(Program.game.windowWidth / 4, Program.game.windowHeight * 0.6f);
            info.name = "How To Play";

            exit = new Button();
            exit.pos = new Vector2(Program.game.windowWidth / 4, Program.game.windowHeight * 0.8f);
            exit.name = "Exit";
        }

        public void Update()
        {
            if (curButton == "New Game")
            {
                setNewGame();
                curButton = "";
            }

            if (curButton == "Level Select")
            {
                selectLevel();
            }

            if (curButton == "How To Play")
            {
                infoScreen();
            }

            if (curButton == "Exit")
            {
                exitGame();
            }

            detectClick(newGame);
            detectClick(levelSelect);
            detectClick(info);
            detectClick(exit);

            if (inLevelsScreen || inInfoScreen) // moves buttons out of way
            {
                newGame.pos = new Vector2(10000,10000);
                levelSelect.pos = new Vector2(10000, 10000);
                info.pos = new Vector2(10000, 10000);
                exit.pos = new Vector2(10000, 10000);
            }
            else
            {
                LoadMenu();
            }
        }

        void setNewGame()
        {
            onStartMenu = false;
            curButton = "";

            // load best score text file
            string file = "./Assets/bestScores.txt";

            if (File.Exists(file))
            {
                string lines = "0\n0\n0\n0\n0\n0\n0\n0\n0";

                File.WriteAllText(file, lines);
            }

            // reset hole number
            Program.game.holeNum = 1;
        }

        void selectLevel()
        {
            inLevelsScreen = true;
            buttonSize = new Vector2(100, 100);
            textOffset = new Vector2(100/2, 100/3);

            // draw 9 boxes + title
            RaylibExt.centerText("Level Select", 50, new Vector2(Program.game.windowWidth / 2, Program.game.windowHeight * 0.075f), Color.WHITE);

            int thisName = 1;
            int offset = 50;

            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    Button b = new Button();

                    b.pos = new Vector2((Program.game.windowWidth / 4)*(j+1)-offset, 150*(i+1)+(offset*0.5f));
                    b.name = thisName.ToString();

                    DrawButton(b);
                    detectClick(b);

                    thisName++;

                    if (curButton == b.name)
                    {
                        onStartMenu = false;
                        curButton = "";
                        inLevelsScreen = false;

                        Program.game.holeNum = Convert.ToInt32(b.name);
                    }
                }
            }

            Button backButton = new Button();

            buttonSize = new Vector2(Program.game.windowWidth / 2, Program.game.windowHeight / 8);
            textOffset = new Vector2(Program.game.windowWidth / 4, Program.game.windowHeight / 32 + 5);

            backButton.pos = new Vector2((Program.game.windowWidth / 4), Program.game.windowHeight * 0.8f);
            backButton.name = "Back To Menu";

            DrawButton(backButton);
            detectClick(backButton);

            if (curButton == backButton.name)
            {
                inLevelsScreen = false;
            }
        }

        void infoScreen()
        {
            inInfoScreen = true;

            RaylibExt.centerText("How To Play", 50, new Vector2(Program.game.windowWidth / 2, Program.game.windowHeight * 0.075f), Color.WHITE);

            string file = "./Assets/info.txt";

            if (File.Exists(file))
            {
                string[] lines = File.ReadAllLines(file);

                for (int i = 0; i < lines.Length; i++)
                {
                    RaylibExt.centerText(lines[i], 35, new Vector2(Program.game.windowWidth / 2, (Program.game.windowHeight / 8)*(i+2)), Color.WHITE);
                }
            }

            Button backButton = new Button();

            buttonSize = new Vector2(Program.game.windowWidth / 2, Program.game.windowHeight / 8);
            textOffset = new Vector2(Program.game.windowWidth / 4, Program.game.windowHeight / 32 + 5);

            backButton.pos = new Vector2((Program.game.windowWidth / 4), Program.game.windowHeight * 0.7f);
            backButton.name = "Back To Menu";

            DrawButton(backButton);
            detectClick(backButton);

            if (curButton == backButton.name)
            {
                inInfoScreen = false;
            }
        }

        void exitGame()
        {
            Environment.Exit(0);
        }

        public void Draw()
        {
            Raylib.BeginDrawing();
            Raylib.ClearBackground(Color.GREEN);

            if (!inLevelsScreen && !inInfoScreen) // main menu
            {
                RaylibExt.centerText("Mini Golf", 50, new Vector2(Program.game.windowWidth / 2, Program.game.windowHeight * 0.075f), Color.WHITE);
                DrawButton(newGame);
                DrawButton(levelSelect);
                DrawButton(info);
                DrawButton(exit);
            }

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
                }
            }
        }
    }
}
