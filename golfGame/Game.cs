using System;
using System.IO;
using System.Numerics;
using Raylib_cs;

namespace golfGame
{
    class Ball
    {
        public Vector2 pos = new Vector2();
        public Vector2 dir = new Vector2();
        public float speed = 0;
        public float storedSpeed = 0;
        public float radius = 12;
    }

    class Hole
    {
        public Vector2 pos = new Vector2();
        public float radius = 24;
    }

    class Game
    {
        public int windowWidth = 600;
        public int windowHeight = 800;

        int curShot = 0;
        int bestShot = 0;
        List<String> bestScores = new List<string>();

        Ball ball;
        Hole hole;
        Button resumeButton = new Button();
        Button quitButton = new Button();

        public int holeNum = 1;

        Texture2D arrow;
        Texture2D arrowHead;
        Texture2D box;

        bool gamePaused = false;
        string curButton = "";
        Vector2 buttonSize = new Vector2();
        Vector2 textOffset = new Vector2();

        public void LoadGame()
        {
            buttonSize = new Vector2(windowWidth / 2, windowHeight / 8);
            textOffset = new Vector2(windowWidth / 4, windowHeight / 32 + 5);

            ball = new Ball();
            ball.pos = new Vector2(windowWidth / 2, windowHeight * 0.75f);
            ball.dir = new Vector2(0, -1);

            arrow = Raylib.LoadTexture("./Assets/arrow.png");
            arrowHead = Raylib.LoadTexture("./Assets/arrowHead.png");
            box = Raylib.LoadTexture("./Assets/box.png");

            hole = new Hole();
            hole.pos = new Vector2(windowWidth / 2, windowHeight * 0.25f);
        }

        public void Update()
        {
            // check if game should be paused
            if (Raylib.IsKeyPressed(KeyboardKey.KEY_ESCAPE))
            {
                gamePaused = true;
            }

            if (gamePaused)
            {
                paused();
            }

            // load hole number
            string file = "./Assets/holeNum.txt";
            holeNum = Convert.ToInt32(File.ReadAllText(file));

            // load best score text file
            string file2 = "./Assets/bestScores.txt";

            bestScores.Clear();

            if (File.Exists(file2))
            {
                string[] lines = File.ReadAllLines(file2);

                foreach (string line in lines)
                {
                    bestScores.Add(line);
                }
            }

            if (!gamePaused)
            {
                UpdateBall(ball);
                HoleCollision(hole, ball);
                LoadLevel(hole, ball);

                if (ball.speed == 0)
                {
                    if (Raylib.IsKeyDown(KeyboardKey.KEY_LEFT)) // rotate ball direction anti-clockwise
                    {
                        ball.dir = Vec2.rotateDirection(ball.dir, -0.1f);
                    }

                    if (Raylib.IsKeyDown(KeyboardKey.KEY_RIGHT)) // rotate ball direction clockwise
                    {
                        ball.dir = Vec2.rotateDirection(ball.dir, 0.1f);
                    }

                    if (Raylib.IsKeyDown(KeyboardKey.KEY_SPACE)) // increase power meter, when space held
                    {
                        ball.storedSpeed += 0.5f;
                    }

                    if (Raylib.IsKeyReleased(KeyboardKey.KEY_SPACE)) // use the power when space released
                    {
                        ball.speed = ball.storedSpeed;
                        ball.storedSpeed = 0;
                        curShot += 1;
                    }
                }
            }

            try
            {
                bestShot = Convert.ToInt32(bestScores[holeNum - 1]);
            }
            catch
            {
                bestShot = 0;
            }

            // write hole number
            File.WriteAllText(file, holeNum.ToString());
        }

        void paused()
        {
            int offset = 100;

            resumeButton.name = "Resume";
            resumeButton.pos = new Vector2(windowWidth/4, (windowHeight/2) - offset - 50);

            quitButton.name = "Exit To Menu";
            quitButton.pos = new Vector2(windowWidth / 4, (windowHeight/2) + offset - 50);

            detectClick(resumeButton);
            detectClick(quitButton);

            if (curButton == "Resume")
            {
                gamePaused = false;
                curButton = "";
            }

            if (curButton == "Exit To Menu")
            {
                startMenu.onStartMenu = true;
                gamePaused = false;
                curButton = "";

                ball.speed = 0;
                ball.pos = new Vector2(windowWidth / 2, windowHeight * 0.75f);
                ball.dir = new Vector2(0, -1);
                curShot = 0;
            }
        }

        void DrawButton(Button b)
        {
            Raylib.DrawRectangleRounded(new Rectangle(b.pos.X, b.pos.Y, buttonSize.X, buttonSize.Y), 0.5f, 1, Color.BLACK);
            RaylibExt.centerText(b.name, 40, b.pos + textOffset, Color.WHITE);
        }

        bool canClick(Vector2 buttonPos, Vector2 buttonSize)
        {
            Vector2 mousePos = Raylib.GetMousePosition();

            if (mousePos.X >= buttonPos.X - buttonSize.X / 2 + textOffset.X && mousePos.X <= buttonPos.X + buttonSize.X / 2 + textOffset.X)
            {
                if (mousePos.Y >= buttonPos.Y - buttonSize.Y / 2 + textOffset.Y && mousePos.Y <= buttonPos.Y + buttonSize.Y / 2 + textOffset.Y)
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

        void UpdateBall(Ball b)
        {
            b.pos += b.dir * b.speed;
            if (b.speed > 0) b.speed -= 0.25f; // slow down ball each frame
            b.storedSpeed = Math.Clamp(b.storedSpeed, 0, 25); // clamp stored speed, min and max

            // bounce off screen edges
            if (b.pos.X < 0 + ball.radius) ball.dir.X = MathF.Abs(ball.dir.X);
            if (b.pos.X > windowWidth - ball.radius) ball.dir.X = -MathF.Abs(ball.dir.X);
            if (b.pos.Y < 0 + 75 + ball.radius) ball.dir.Y = MathF.Abs(ball.dir.Y);
            if (b.pos.Y > windowHeight - 75 - ball.radius) ball.dir.Y = -MathF.Abs(ball.dir.Y);
        }

        void HoleCollision(Hole h, Ball b)
        {
            if (Vector2.Distance(b.pos, h.pos) < h.radius) // ball close enough to hole
            {
                if (curShot < bestShot || bestShot == 0) // changes high score for that hole
                {
                    string file = "./Assets/bestScores.txt";

                    if (File.Exists(file)) // write to best scores file
                    {
                        if (holeNum < 9) bestScores[holeNum - 1] = curShot.ToString();
                        File.WriteAllLines(file, bestScores);
                    }
                }

                b.speed = 0;
                b.pos = new Vector2(windowWidth / 2, windowHeight * 0.75f);
                ball.dir = new Vector2(0, -1);
                if (holeNum < 9) holeNum += 1;
                else holeNum = 1;
                curShot = 0;
            }
        }

        public void Draw()
        {
            Raylib.BeginDrawing();
            Raylib.ClearBackground(Color.GREEN);

            // draw hole
            Raylib.DrawCircle((int)hole.pos.X, (int)hole.pos.Y, hole.radius, Color.DARKBROWN);

            // ball and aim arrow
            float rotation = (float)(Math.Atan2(ball.dir.Y, ball.dir.X) / (2 * Math.PI));
            RaylibExt.DrawTexture(arrow, ball.pos.X, ball.pos.Y, 50, 100, Color.WHITE, rotation * 360, 0, 0.5f);
            Raylib.DrawCircle((int)ball.pos.X, (int)ball.pos.Y, ball.radius, Color.WHITE);

            // power meter
            Raylib.DrawRectangle(0, 0, windowWidth, 75, Color.DARKBLUE);
            Raylib.DrawRectangle(0, 75, windowWidth, 5, Color.BLACK);
            Raylib.DrawRectangleGradientH((windowWidth / 2) - 150, 25, 300, 25, Color.YELLOW, Color.RED);
            RaylibExt.DrawTexture(arrowHead, ((windowWidth/2)-150) + (ball.storedSpeed * 11.15f), 45, 15, 15, Color.WHITE, 0, 0, 0);

            // level name
            Raylib.DrawRectangle(0, windowHeight - 75, windowWidth, 75, Color.DARKBLUE);
            Raylib.DrawRectangle(0, windowHeight - 75, windowWidth, 5, Color.BLACK);
            RaylibExt.centerText("Level - " + holeNum, 40, new Vector2(windowWidth / 2, windowHeight - 55), Color.WHITE);

            // stroke and best stroke text
            RaylibExt.centerText("Stroke", 20, new Vector2(windowWidth / 8, 15), Color.BLACK);
            RaylibExt.centerText(curShot.ToString(), 35, new Vector2(windowWidth / 8, 35), Color.BLACK);
            RaylibExt.centerText("Best", 20, new Vector2((windowWidth / 8) * 7, 15), Color.BLACK);
            RaylibExt.centerText(bestShot.ToString(), 35, new Vector2((windowWidth / 8) * 7, 35), Color.BLACK);

            if (gamePaused)
            {
                DrawButton(resumeButton);
                DrawButton(quitButton);
            }

            Raylib.EndDrawing();
        }

        void LoadLevel(Hole h, Ball b)
        {
            // read level data from levels folder, using hole number
            string file = "./Assets/Levels/" + holeNum + ".txt";

            if (File.Exists(file))
            {
                string[] lines = File.ReadAllLines(file);

                foreach (string line in lines)
                {
                    if (line != null && line != "") // only does this for not blank lines
                    {
                        string[] parts = line.Split(",");
                        RaylibExt.DrawTexture(box, windowWidth*float.Parse(parts[0]), windowHeight * float.Parse(parts[1]), float.Parse(parts[2]), float.Parse(parts[3]), Color.SKYBLUE, 0, 0.5f, 0.5f);

                        float top = (windowHeight * float.Parse(parts[1])) - (float.Parse(parts[3]) / 2) - b.radius;
                        float bottom = (windowHeight * float.Parse(parts[1])) + (float.Parse(parts[3]) / 2) + b.radius;
                        float right = (windowWidth*float.Parse(parts[0])) + (float.Parse(parts[2]) / 2) + b.radius;
                        float left = (windowWidth * float.Parse(parts[0])) - (float.Parse(parts[2]) / 2) - b.radius;

                        // checks which side is the closest to the ball
                        float[] distances = new float[4];
                        distances[0] = Vector2.Distance(b.pos, new Vector2(windowWidth * float.Parse(parts[0]), top));
                        distances[1] = Vector2.Distance(b.pos, new Vector2(windowWidth * float.Parse(parts[0]), bottom));
                        distances[2] = Vector2.Distance(b.pos, new Vector2(left, windowHeight * float.Parse(parts[1])));
                        distances[3] = Vector2.Distance(b.pos, new Vector2(right, windowHeight * float.Parse(parts[1])));

                        float smallestDistance = distances.Min();

                        if (b.pos.Y < bottom && b.pos.Y > top && b.pos.X < right && b.pos.X > left) // if inside collision area
                        {
                            if (smallestDistance == distances[0]) b.dir.Y = -MathF.Abs(b.dir.Y); // top
                            if (smallestDistance == distances[1]) b.dir.Y = MathF.Abs(b.dir.Y); // bottom
                            if (smallestDistance == distances[2]) b.dir.X = -MathF.Abs(b.dir.X); // left
                            if (smallestDistance == distances[3]) b.dir.X = MathF.Abs(b.dir.X); // right
                        }
                    }
                }
            }
        }
    }
}
