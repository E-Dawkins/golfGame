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
        public int holeNum = 1;
    }

    class Program
    {
        int windowWidth = 600;
        int windowHeight = 800;

        int curShot = 0;
        int bestShot = 0;
        List<String> bestScores = new List<string>();

        Ball ball;
        Hole hole;
        Texture2D arrow;
        Texture2D arrowHead;
        Texture2D box;

        static void Main(string[] args)
        {
            Program p = new Program();
            p.RunProgram();
        }

        void RunProgram()
        {
            Raylib.InitWindow(windowWidth, windowHeight, "Golf");
            Raylib.SetTargetFPS(60);

            LoadGame();

            while (!Raylib.WindowShouldClose())
            {
                Update();
                Draw();
            }

            Raylib.CloseWindow();
        }

        void LoadGame()
        {
            ball = new Ball();
            ball.pos = new Vector2(windowWidth / 2, windowHeight * 0.75f);
            ball.dir = new Vector2(0, -1);

            arrow = Raylib.LoadTexture("./Assets/arrow.png");
            arrowHead = Raylib.LoadTexture("./Assets/arrowHead.png");
            box = Raylib.LoadTexture("./Assets/box.png");

            hole = new Hole();
            hole.pos = new Vector2(windowWidth / 2, windowHeight * 0.25f);

            // load best score text file
            string file = Directory.GetParent(Directory.GetParent(Directory.GetParent(Environment.CurrentDirectory).ToString()).ToString()) + @"\Assets\bestScores.txt";

            if (File.Exists(file))
            {
                string[] lines = File.ReadAllLines(file);

                foreach (string line in lines)
                {
                    bestScores.Add(line);
                }
            }
        }

        void Update()
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

            try
            {
                bestShot = Convert.ToInt32(bestScores[hole.holeNum - 1]);
            }
            catch
            {
                bestShot = 0;
            }
            
        }

        void UpdateBall(Ball b)
        {
            b.pos += b.dir * b.speed;
            if (b.speed > 0) b.speed -= 0.25f; // slow down ball each frame
            b.storedSpeed = Math.Clamp(b.storedSpeed, 0, 25); // clamp stored speed, min and max

            // bounce off screen edges
            if (b.pos.X < 0 + b.radius) ball.dir.X *= -1;
            if (b.pos.X > windowWidth - b.radius) ball.dir.X *= -1;
            if (b.pos.Y < 0 + 75 + b.radius) ball.dir.Y *= -1;
            if (b.pos.Y > windowHeight - b.radius) ball.dir.Y *= -1;
        }

        void HoleCollision(Hole h, Ball b)
        {
            if (Vector2.Distance(b.pos, h.pos) < h.radius) // ball close enough to hole
            {
                if (curShot < bestShot)
                {
                    string file = Directory.GetParent(Directory.GetParent(Directory.GetParent(Environment.CurrentDirectory).ToString()).ToString()) + @"\Assets\bestScores.txt";

                    if (File.Exists(file)) // write to best scores file
                    {
                        bestScores[hole.holeNum - 1] = curShot.ToString();
                        File.WriteAllLines(file, bestScores);
                    }
                }
            }
        }

        void Draw()
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

            // stroke and best stroke text
            RaylibExt.centerText("Stroke", 25, new Vector2(windowWidth / 8, 15), Color.BLACK);
            RaylibExt.centerText(curShot.ToString(), 30, new Vector2(windowWidth / 8, 40), Color.BLACK);
            RaylibExt.centerText("Best", 25, new Vector2((windowWidth / 8) * 7, 15), Color.BLACK);
            RaylibExt.centerText(bestShot.ToString(), 30, new Vector2((windowWidth / 8) * 7, 40), Color.BLACK);

            Raylib.EndDrawing();
        }

        void LoadLevel(Hole h, Ball b)
        {
            // read level data from levels folder, using hole number
            string file = Directory.GetParent(Directory.GetParent(Directory.GetParent(Environment.CurrentDirectory).ToString()).ToString()) + @"\Assets\Levels\" + h.holeNum + ".txt";

            if (File.Exists(file))
            {
                string[] lines = File.ReadAllLines(file);

                foreach (string line in lines)
                {
                    if (line != null && line != "") // only does this for not blank lines
                    {
                        string[] parts = line.Split(",");
                        RaylibExt.DrawTexture(box, windowWidth*float.Parse(parts[0]), windowHeight * float.Parse(parts[1]), float.Parse(parts[2]), float.Parse(parts[3]), Color.SKYBLUE, 0, 0.5f, 0.5f);

                        float top = (windowHeight * float.Parse(parts[1])) - (float.Parse(parts[3]) / 2);
                        float bottom = (windowHeight * float.Parse(parts[1])) + (float.Parse(parts[3]) / 2);
                        float right = (windowWidth*float.Parse(parts[0])) + (float.Parse(parts[2]) / 2);
                        float left = (windowWidth * float.Parse(parts[0])) - (float.Parse(parts[2]) / 2);

                        if (b.pos.Y > top && b.pos.Y < bottom && b.pos.X > left && b.pos.X < right) // colliding with box
                        {
                            Vector2 dir = (new Vector2(windowWidth * float.Parse(parts[0]), windowHeight * float.Parse(parts[1])) - b.pos);

                            if (dir.Y > 0) b.dir.Y *= -1;
                            if (dir.Y < 0) b.dir.Y *= -1;
                            if (dir.X < 0) b.dir.X *= -1;
                            if (dir.X > 0) b.dir.X *= -1;
                        }
                    }
                }
            }
        }
    }
}
