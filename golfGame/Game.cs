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

    public class Game
    {
        public int windowWidth = 600;
        public int windowHeight = 800;

        int curShot = 0;
        int bestShot = 0;
        List<string> bestScores = new List<string>();

        Ball ball;
        Hole hole;
        Button resumeButton = new Button();
        Button quitButton = new Button();

        public int holeNum = 1;
        bool firstLoad = true;

        Texture2D arrow;
        Texture2D arrowHead;
        Texture2D box;

        bool gamePaused = false;
        string curButton = "";
        Vector2 buttonSize = new Vector2();
        Vector2 textOffset = new Vector2();

        List<Vector2> boxColPos = new List<Vector2>();
        List<Vector2> boxColSize = new List<Vector2>();
        List<Vector2> boxPos = new List<Vector2>();
        List<Vector2> boxSize = new List<Vector2>();

        List<int> movingBoxes = new List<int>();
        List<int> fanBoxes = new List<int>();
        List<int> popBoxes = new List<int>();

        List<Vector2> movingBoxPts = new List<Vector2>();
        List<Vector2> orgPts = new List<Vector2>();
        List<int> targetNums = new List<int>();

        List<Vector2> fanDirection = new List<Vector2>();
        List<Vector2> popSizes = new List<Vector2>();

        public void LoadGame()
        {
            buttonSize = new Vector2(windowWidth / 2, windowHeight / 8);
            textOffset = new Vector2(windowWidth / 4, windowHeight / 32 + 5);

            ball = new Ball();
            ball.dir = new Vector2(0, -1);

            arrow = Raylib.LoadTexture("./Assets/arrow.png");
            arrowHead = Raylib.LoadTexture("./Assets/arrowHead.png");
            box = Raylib.LoadTexture("./Assets/box.png");

            hole = new Hole();
        }

        public void Update()
        {
            string file = "./Assets/Levels/" + holeNum + ".txt";

            if (File.Exists(file) && firstLoad) // only runs this section once per hole, on load
            {
                string[] lines = File.ReadAllLines(file);

                string[] ballParts = lines[0].Split(",");
                string[] holeParts = lines[1].Split(",");

                ball.pos = new Vector2(windowWidth * float.Parse(ballParts[0]), windowHeight * float.Parse(ballParts[1]));
                hole.pos = new Vector2(windowWidth * float.Parse(holeParts[0]), windowHeight * float.Parse(holeParts[1]));

                loadBoxCols(file);
                loadBoxes(file);

                // load best score text file
                string file2 = "./Assets/bestScores.txt";

                bestScores.Clear();

                if (File.Exists(file2))
                {
                    try
                    {
                        string[] lines2 = File.ReadAllLines(file2);

                        foreach (string line in lines2)
                        {
                            bestScores.Add(line);
                        }
                    }
                    catch { }
                }

                firstLoad = false;
            }

            // check if game should be paused
            if (Raylib.IsKeyPressed(KeyboardKey.KEY_ESCAPE))
            {
                gamePaused = true;
            }

            if (gamePaused)
            {
                paused();
            }

            if (!gamePaused) // only while game not paused
            {
                UpdateBall(ball);
                HoleCollision(hole, ball);

                detectBoxCol(ball);
                drawBoxes();

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

            try // tries to set best shot text
            {
                bestShot = Convert.ToInt32(bestScores[holeNum - 1]);
            }
            catch
            {
                bestShot = 0;
            }

            // update moving boxes
            updateMovingBoxes();
        }

        void paused()
        {
            // sets button values
            int offset = 100;

            resumeButton.name = "Resume";
            resumeButton.pos = new Vector2(windowWidth/4, (windowHeight/2) - offset - 50);

            quitButton.name = "Exit To Menu";
            quitButton.pos = new Vector2(windowWidth / 4, (windowHeight/2) + offset - 50);

            detectClick(resumeButton);
            detectClick(quitButton);

            if (curButton == "Resume") // what to do when resume button pressed
            {
                gamePaused = false;
                curButton = "";
            }

            if (curButton == "Exit To Menu") // what to do when exit button pressed
            {
                startMenu.onStartMenu = true;
                gamePaused = false;
                curButton = "";
                firstLoad = true;

                ball.speed = 0;
                ball.dir = new Vector2(0, -1);
                curShot = 0;

                RaylibExt.Clear(boxColPos, boxColSize, boxPos, boxSize, movingBoxes, fanBoxes, popBoxes, movingBoxPts, orgPts, targetNums, fanDirection, popSizes);
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
                        if (holeNum <= 9) bestScores[holeNum - 1] = curShot.ToString();
                        File.WriteAllLines(file, bestScores);
                    }
                }

                // resets values for next level
                b.speed = 0;
                ball.dir = new Vector2(0, -1);
                if (holeNum < 9) holeNum += 1;
                else holeNum = 1;
                curShot = 0;

                RaylibExt.Clear(boxColPos, boxColSize, boxPos, boxSize, movingBoxes, fanBoxes, popBoxes, movingBoxPts, orgPts, targetNums, fanDirection, popSizes);

                firstLoad = true;
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

            if (gamePaused) // draws paused buttons
            {
                DrawButton(resumeButton);
                DrawButton(quitButton);
            }

            Raylib.EndDrawing();
        }

        void loadBoxCols(string filePath)
        {
            if (File.Exists(filePath))
            {
                string[] lines = File.ReadAllLines(filePath);
                int startLine = 0;

                for (int i = 0; i < lines.Length; i++) // searches for --col-- tag
                {
                    if (lines[i] == "--col--") // makes startline first line after --col-- tag
                    {
                        startLine = i+1;
                        break;
                    }
                }

                if (startLine != 0) // actually found --col-- tag
                {
                    int numMoveBoxes = 0;
                    int numFans = 0;
                    int numPopBoxes = 0;

                    for (int i = startLine; i < lines.Length; i++)
                    {
                        string[] parts = lines[i].Split(",");

                        // save box col pos / size
                        boxColPos.Add(new Vector2(windowWidth * float.Parse(parts[0]), windowHeight * float.Parse(parts[1])));
                        boxColSize.Add(new Vector2(float.Parse(parts[2]), float.Parse(parts[3])));

                        if (parts.Length == 5)
                        {
                            string[] parts2 = lines[i + 1].Split(",");

                            if (parts[4] == "moving")
                            {
                                movingBoxes.Add(numMoveBoxes);
                                numMoveBoxes++;
                                i++;

                                movingBoxPts.Add(new Vector2(windowWidth * float.Parse(parts2[0]), windowHeight * float.Parse(parts2[1])));
                                orgPts.Add(new Vector2(windowWidth * float.Parse(parts[0]), windowHeight * float.Parse(parts[1])));
                                targetNums.Add(0);
                            }

                            if (parts[4] == "fan")
                            {
                                fanBoxes.Add(numFans);
                                numFans++;
                                i++;

                                fanDirection.Add(new Vector2(float.Parse(parts2[0]), float.Parse(parts2[1])));
                            }

                            if (parts[4] == "pop")
                            {
                                popBoxes.Add(numPopBoxes);
                                numPopBoxes++;
                                i++;

                                popSizes.Add(new Vector2(float.Parse(parts2[0]), float.Parse(parts2[1])));
                            }
                        }
                    }
                }
            }
        }

        void detectBoxCol(Ball b)
        {
            for (int i = 0; i < boxColPos.Count; i++)
            {
                float top = boxColPos[i].Y - (boxColSize[i].Y / 2) - b.radius;
                float bottom = boxColPos[i].Y + (boxColSize[i].Y / 2) + b.radius;
                float right = boxColPos[i].X + (boxColSize[i].X / 2) + b.radius;
                float left = boxColPos[i].X - (boxColSize[i].X / 2) - b.radius;

                // checks which side is the closest to the ball
                float[] distances = new float[4];
                distances[0] = Vector2.Distance(b.pos, new Vector2(boxColPos[i].X, top)) / boxColSize[i].X;
                distances[1] = Vector2.Distance(b.pos, new Vector2(boxColPos[i].X, bottom)) / boxColSize[i].X;
                distances[2] = Vector2.Distance(b.pos, new Vector2(left, boxColPos[i].Y)) / boxColSize[i].Y;
                distances[3] = Vector2.Distance(b.pos, new Vector2(right, boxColPos[i].Y)) / boxColSize[i].Y;

                float smallestDistance = distances.Min();

                if (b.pos.Y < bottom && b.pos.Y > top && b.pos.X < right && b.pos.X > left) // if inside collision area
                {
                    if (smallestDistance == distances[0]) b.dir.Y = -MathF.Abs(b.dir.Y); // top
                    if (smallestDistance == distances[1]) b.dir.Y = MathF.Abs(b.dir.Y); // bottom
                    if (smallestDistance == distances[2]) b.dir.X = -MathF.Abs(b.dir.X); // left
                    if (smallestDistance == distances[3]) b.dir.X = MathF.Abs(b.dir.X); // right

                    if (movingBoxes.Contains(i)) // moving box, do extra collision step
                    {
                        float nudgeDistance = 5;

                        if (smallestDistance == distances[0]) b.pos.Y -= nudgeDistance; // top
                        if (smallestDistance == distances[1]) b.pos.Y += nudgeDistance; // bottom
                        if (smallestDistance == distances[2]) b.pos.X -= nudgeDistance; // left
                        if (smallestDistance == distances[3]) b.pos.X += nudgeDistance; // right
                    }
                }
            }
        }

        void loadBoxes(string filePath)
        {
            if (File.Exists(filePath))
            {
                string[] lines = File.ReadAllLines(filePath);
                int startLine = 0;

                for (int i = 0; i < lines.Length; i++) // searches for --pos-- tag
                {
                    if (lines[i] == "--pos--") // makes startline first line after --pos-- tag
                    {
                        startLine = i + 1;
                        break;
                    }
                }

                if (startLine != 0) // actually found --pos-- tag
                {
                    for (int i = startLine; i < lines.Length; i++)
                    {
                        if (lines[i] != "--col--")
                        {
                            string[] parts = lines[i].Split(",");

                            // save box pos / size
                            boxPos.Add(new Vector2(windowWidth*float.Parse(parts[0]), windowHeight*float.Parse(parts[1])));
                            boxSize.Add(new Vector2(float.Parse(parts[2]), float.Parse(parts[3])));
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }
        }

        void drawBoxes()
        {
            for (int i = 0; i < boxPos.Count; i++)
            {
                RaylibExt.DrawTexture(box, boxPos[i].X, boxPos[i].Y, boxSize[i].X, boxSize[i].Y, Color.SKYBLUE, 0, 0.5f, 0.5f);
            }
        }

        void updateMovingBoxes()
        {
            if (boxPos.Count() != 0)
            {
                for (int i = 0; i < movingBoxes.Count(); i++)
                {
                    Vector2[] targets = new Vector2[2];
                    targets[0] = movingBoxPts[i];
                    targets[1] = orgPts[i];

                    Vector2 startPos = targets[(targetNums[i] + 1) % 2];

                    Vector2 direction = targets[targetNums[i]] - startPos;
                    float moveSpeed = 0.01f;

                    if (Vector2.Distance(boxPos[movingBoxes[i]], targets[targetNums[i]]) > 10) // move box towards current target
                    {
                        boxPos[movingBoxes[i]] += direction * moveSpeed;
                        boxColPos[movingBoxes[i]] = boxPos[movingBoxes[i]];
                    }
                    else // change to next target point
                    {
                        targetNums[i] = (targetNums[i] + 1) % 2;
                    }
                }
            }
        }
    }
}
