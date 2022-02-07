using System;
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

    class Program
    {
        int windowWidth = 600;
        int windowHeight = 800;

        int curShot = 0;
        int bestShot = 0;

        Ball ball;
        Texture2D arrow;
        Texture2D arrowHead;

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
        }

        void Update()
        {
            UpdateBall(ball);

            if (Raylib.IsKeyDown(KeyboardKey.KEY_LEFT)) // rotate ball direction anti-clockwise
            {
                ball.dir = Vec2.rotateDirection(ball.dir, -0.1f);
            }

            if (Raylib.IsKeyDown(KeyboardKey.KEY_RIGHT)) // rotate ball direction clockwise
            {
                ball.dir = Vec2.rotateDirection(ball.dir, 0.1f);
            }

            if (Raylib.IsKeyDown(KeyboardKey.KEY_SPACE) && ball.speed == 0) // increase power meter, when space held
            {
                ball.storedSpeed += 0.5f;
            }

            if (Raylib.IsKeyReleased(KeyboardKey.KEY_SPACE) && ball.speed == 0) // use the power when space released
            {
                ball.speed = ball.storedSpeed;
                ball.storedSpeed = 0;
                curShot += 1;
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

        void Draw()
        {
            Raylib.BeginDrawing();
            Raylib.ClearBackground(Color.GREEN);

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
    }
}
