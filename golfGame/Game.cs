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

    class Game
    {
        public int windowWidth = 600;
        public int windowHeight = 800;

        public bool onStartMenu = true;

        int curShot = 0;
        int bestShot = 0;
        List<String> bestScores = new List<string>();

        Ball ball;
        Hole hole;
        Texture2D arrow;
        Texture2D arrowHead;
        Texture2D box;

        public void LoadGame()
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

        public void Update()
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
                    string file = Directory.GetParent(Directory.GetParent(Directory.GetParent(Environment.CurrentDirectory).ToString()).ToString()) + @"\Assets\bestScores.txt";

                    if (File.Exists(file)) // write to best scores file
                    {
                        if (h.holeNum < 9) bestScores[h.holeNum - 1] = curShot.ToString();
                        File.WriteAllLines(file, bestScores);
                    }
                }

                b.speed = 0;
                b.pos = new Vector2(windowWidth / 2, windowHeight * 0.75f);
                ball.dir = new Vector2(0, -1);
                if (h.holeNum < 9) h.holeNum += 1;
                else h.holeNum = 1;
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
            RaylibExt.centerText("Level - " + hole.holeNum, 40, new Vector2(windowWidth / 2, windowHeight - 55), Color.WHITE);

            // stroke and best stroke text
            RaylibExt.centerText("Stroke", 20, new Vector2(windowWidth / 8, 15), Color.BLACK);
            RaylibExt.centerText(curShot.ToString(), 35, new Vector2(windowWidth / 8, 35), Color.BLACK);
            RaylibExt.centerText("Best", 20, new Vector2((windowWidth / 8) * 7, 15), Color.BLACK);
            RaylibExt.centerText(bestShot.ToString(), 35, new Vector2((windowWidth / 8) * 7, 35), Color.BLACK);

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
