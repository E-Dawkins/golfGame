using System;
using System.IO;
using System.Numerics;
using Raylib_cs;

namespace golfGame
{
    class Ball
    {
        public Vector2 pos = new Vector2();
        public Vector2 dir = new Vector2(0, -1);
        public float rotation = -90;
        public float rotationSpeed = 4;
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
        Texture2D fanBox;

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
        List<Vector2> orgPopSizes = new List<Vector2>();
        List<float> popCoolDowns = new List<float>();
        List<float> orgCoolDowns = new List<float>();

        public void LoadGame()
        {
            buttonSize = new Vector2(windowWidth / 2, windowHeight / 8);
            textOffset = new Vector2(windowWidth / 4, windowHeight / 32 + 5);

            ball = new Ball();

            arrow = Raylib.LoadTexture("./Assets/arrow.png");
            arrowHead = Raylib.LoadTexture("./Assets/arrowHead.png");
            box = Raylib.LoadTexture("./Assets/box.png");
            fanBox = Raylib.LoadTexture("./Assets/fanBox.png");

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
                        ball.rotation -= ball.rotationSpeed;
                    }

                    if (Raylib.IsKeyDown(KeyboardKey.KEY_RIGHT)) // rotate ball direction clockwise
                    {
                        ball.rotation += ball.rotationSpeed;
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

            // update special boxes (moving, fan, pop)
            updateFanBoxes(ball);
            updatePopBoxes();
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
                ball.rotation = -90;
                curShot = 0;

                RaylibExt.Clear(boxColPos, boxColSize, boxPos, boxSize, movingBoxes, fanBoxes, popBoxes, movingBoxPts, orgPts, targetNums, fanDirection, popSizes, orgPopSizes, popCoolDowns, orgCoolDowns);
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
            if (b.rotation > 0) b.rotation -= 360;
            if (b.rotation < -360) b.rotation += 360;

            b.storedSpeed = Math.Clamp(b.storedSpeed, 0, 25);
            b.speed = Math.Clamp(b.speed, 0, 25);
            b.dir = GetFacingDirection(b);

            b.pos += b.dir * b.speed;
            if (b.speed > 0) b.speed -= 0.25f; // slow down ball each frame

            float inAngle;

            // bounce off screen edges
            if (b.pos.X < 0 + ball.radius) // left wall
            {
                if (b.dir.X < 0 && b.dir.Y < 0) // moving up
                {
                    inAngle = MathF.Abs(b.rotation) - 90;
                    b.rotation = -90 + inAngle;
                }
                    
                if (b.dir.X < 0 && b.dir.Y > 0) // moving down
                {
                    inAngle = MathF.Abs(b.rotation) - 180;
                    b.rotation = inAngle;
                }
            }
            if (b.pos.X > windowWidth - ball.radius) // right wall
            {
                if (b.dir.X > 0 && b.dir.Y > 0) // moving down
                {
                    inAngle = MathF.Abs(b.rotation) - 270;
                    b.rotation = -270 + inAngle;
                }
                if (b.dir.X > 0 && b.dir.Y < 0) // moving up
                {
                    inAngle = 90 - MathF.Abs(b.rotation);
                    b.rotation = -90 - inAngle;
                }
            }
            if (b.pos.Y < 75 + ball.radius) // top wall
            {
                if (b.dir.X < 0 && b.dir.Y < 0) // moving left
                {
                    inAngle = 180 - MathF.Abs(b.rotation);
                    b.rotation = -180 - inAngle;
                }
                if (b.dir.X > 0 && b.dir.Y < 0) // moving right
                {
                    inAngle = 90 - MathF.Abs(b.rotation);
                    b.rotation = -270 - inAngle;
                }
            }
            if (b.pos.Y > windowHeight - 75 - ball.radius) // bottom wall
            {
                if (b.dir.X < 0 && b.dir.Y > 0) // moving left
                {
                    inAngle = MathF.Abs(b.rotation) - 180;
                    b.rotation = -180 + inAngle;
                }
                if (b.dir.X > 0 && b.dir.Y > 0) // moving right
                {
                    inAngle = MathF.Abs(b.rotation) - 270;
                    b.rotation = -90 + inAngle;
                }
            }
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
                b.rotation = -90;
                if (holeNum < 9) holeNum += 1;
                else holeNum = 1;
                curShot = 0;

                RaylibExt.Clear(boxColPos, boxColSize, boxPos, boxSize, movingBoxes, fanBoxes, popBoxes, movingBoxPts, orgPts, targetNums, fanDirection, popSizes, orgPopSizes, popCoolDowns, orgCoolDowns);

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
                                orgPopSizes.Add(new Vector2(float.Parse(parts[2]), float.Parse(parts[3])));
                                popCoolDowns.Add(float.Parse(parts2[2]));
                                orgCoolDowns.Add(float.Parse(parts2[2]));
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
                    float inAngle;

                    if (smallestDistance == distances[0]) // top
                    {
                        if (b.dir.X < 0 && b.dir.Y > 0) // moving left
                        {
                            inAngle = MathF.Abs(b.rotation) - 180;
                            b.rotation = -180 + inAngle;
                        }
                        if (b.dir.X > 0 && b.dir.Y > 0) // moving right
                        {
                            inAngle = MathF.Abs(b.rotation) - 270;
                            b.rotation = -90 + inAngle;
                        }
                    }

                    if (smallestDistance == distances[1]) // bottom
                    {
                        if (b.dir.X < 0 && b.dir.Y < 0) // moving left
                        {
                            inAngle = 180 - MathF.Abs(b.rotation);
                            b.rotation = -180 - inAngle;
                        }
                        if (b.dir.X > 0 && b.dir.Y < 0) // moving right
                        {
                            inAngle = 90 - MathF.Abs(b.rotation);
                            b.rotation = -270 - inAngle;
                        }
                    }

                    if (smallestDistance == distances[2]) // left
                    {
                        if (b.dir.X > 0 && b.dir.Y > 0) // moving down
                        {
                            inAngle = MathF.Abs(b.rotation) - 270;
                            b.rotation = -270 + inAngle;
                        }
                        if (b.dir.X > 0 && b.dir.Y < 0) // moving up
                        {
                            inAngle = 90 - MathF.Abs(b.rotation);
                            b.rotation = -90 - inAngle;
                        }
                    }

                    if (smallestDistance == distances[3]) // right
                    {
                        if (b.dir.X < 0 && b.dir.Y < 0) // moving up
                        {
                            inAngle = MathF.Abs(b.rotation) - 90;
                            b.rotation = -90 + inAngle;
                        }

                        if (b.dir.X < 0 && b.dir.Y > 0) // moving down
                        {
                            inAngle = MathF.Abs(b.rotation) - 180;
                            b.rotation = inAngle;
                        }
                    }

                    if (movingBoxes.Contains(i)) // moving box, do extra collision step
                    {
                        float nudgeDistance = 5;

                        if (smallestDistance == distances[0]) b.pos.Y -= nudgeDistance; // top
                        if (smallestDistance == distances[1]) b.pos.Y += nudgeDistance; // bottom
                        if (smallestDistance == distances[2]) b.pos.X -= nudgeDistance; // left
                        if (smallestDistance == distances[3]) b.pos.X += nudgeDistance; // right
                    }

                    if (popBoxes.Contains(i))
                    {
                        if (smallestDistance == distances[0]) b.pos.Y = top; // top
                        if (smallestDistance == distances[1]) b.pos.Y = bottom; // bottom
                        if (smallestDistance == distances[2]) b.pos.X = left; // left
                        if (smallestDistance == distances[3]) b.pos.X = right; // right
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
                if (fanBoxes.Contains(i)) // draw fan box
                {
                    RaylibExt.DrawTexture(fanBox, boxPos[i].X, boxPos[i].Y, boxSize[i].X, boxSize[i].Y, Color.GOLD, DirectionToRotation(fanDirection[i]), 0.5f, 0.5f);

                    Vector2 rectPos = new Vector2(boxPos[i].X + (75 * fanDirection[i].X), boxPos[i].Y + (75 * fanDirection[i].Y));
                    Vector2 rectSize = new Vector2(boxSize[i].X * MathF.Abs(fanDirection[i].X) * 5, boxSize[i].Y * MathF.Abs(fanDirection[i].Y) * 5);

                    if (rectSize.X == 0) rectSize.X = 75;
                    if (rectSize.Y == 0) rectSize.Y = 75;

                    if (fanDirection[i].X == -1) rectPos.X -= 337.5f;
                    if (fanDirection[i].X == 1 || fanDirection[i].X == 0) rectPos.X -= 37.5f;
                    if (fanDirection[i].Y == -1) rectPos.Y -= 337.5f;
                    if (fanDirection[i].Y == 1 || fanDirection[i].Y == 0) rectPos.Y -= 37.5f;

                    Color rectCol = Raylib.ColorAlpha(Color.SKYBLUE, 0.5f);

                    Raylib.DrawRectangleRounded(new Rectangle(rectPos.X, rectPos.Y, rectSize.X, rectSize.Y), 0.5f, 1, rectCol);
                }

                if (!fanBoxes.Contains(i)) // draw normal box
                {
                    RaylibExt.DrawTexture(box, boxPos[i].X, boxPos[i].Y, boxSize[i].X, boxSize[i].Y, Color.SKYBLUE, 0, 0.5f, 0.5f);
                }
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

        void updateFanBoxes(Ball b)
        {
            float fanRotAmount = 5f;
            float fanRange = 375;

            if (boxPos.Count() != 0)
            {
                for (int i = 0; i < fanBoxes.Count(); i++) // for each fan box
                {
                    float top = boxColPos[fanBoxes[i]].Y - (boxColSize[fanBoxes[i]].Y / 2);
                    float bottom = boxColPos[i].Y + (boxColSize[i].Y / 2);
                    float left = boxColPos[i].X - (boxColSize[fanBoxes[i]]).X / 2;
                    float right = boxColPos[i].X + (boxColSize[fanBoxes[i]]).X / 2;

                    if (fanDirection[i] == new Vector2(0, -1)) // up
                    {
                        if (b.pos.X < right && b.pos.X > left && b.pos.Y < top && b.pos.Y > top - fanRange && b.pos.Y > 75 + b.radius)
                        {
                            if (b.speed != 0)
                            {
                                if (b.dir.X < 0) b.rotation += fanRotAmount;
                                if (b.dir.X > 0) b.rotation -= fanRotAmount;
                            }

                            b.pos.Y -= 5;
                        }
                    }

                    if (fanDirection[i] == new Vector2(0, 1)) // down
                    {
                        if (b.pos.X < right && b.pos.X > left && b.pos.Y > bottom && b.pos.Y < bottom + fanRange && b.pos.Y < windowHeight - 75 - b.radius)
                        {
                            if (b.speed != 0)
                            {
                                if (b.dir.X < 0) b.rotation -= fanRotAmount;
                                if (b.dir.X > 0) b.rotation += fanRotAmount;
                            }

                            b.pos.Y += 5;
                        }
                    }

                    if (fanDirection[i] == new Vector2(-1, 0)) // left
                    {
                        if (b.pos.Y < bottom && b.pos.Y > top && b.pos.X < left && b.pos.X > left - fanRange && b.pos.X > b.radius)
                        {
                            if (b.speed != 0)
                            {
                                if (b.dir.Y < 0) b.rotation -= fanRotAmount;
                                if (b.dir.Y > 0) b.rotation += fanRotAmount;
                            }

                            b.pos.X -= 5;
                        }
                    }

                    if (fanDirection[i] == new Vector2(1, 0)) // right
                    {
                        if (b.pos.Y < bottom && b.pos.Y > top && b.pos.X > right && b.pos.X < right + fanRange && b.pos.X < windowWidth - b.radius)
                        {
                            if (b.speed != 0)
                            {
                                if (b.dir.Y < 0) b.rotation += fanRotAmount;
                                if (b.dir.Y > 0) b.rotation -= fanRotAmount;
                            }

                            b.pos.X += 5;
                        }
                    }
                }
            }
        }

        void updatePopBoxes()
        {
            if (boxPos.Count != 0)
            {
                for (int i = 0; i < popBoxes.Count; i++)
                {
                    float curCooldown = popCoolDowns[i];

                    if (curCooldown > 0)
                    {
                        curCooldown -= Raylib.GetFrameTime();
                        popCoolDowns[i] = curCooldown;

                        float xDiff = 0;
                        float yDiff = 0;

                        if (boxSize[popBoxes[i]].X > orgPopSizes[popBoxes[i]].X)
                        {
                            xDiff -= (popSizes[i].X - orgPopSizes[popBoxes[i]].X) / (orgCoolDowns[i] * 60);

                            if (boxSize[popBoxes[i]].Y > orgPopSizes[popBoxes[i]].Y)
                            {
                                yDiff -= (popSizes[i].Y - orgPopSizes[popBoxes[i]].Y) / (orgCoolDowns[i] * 60);
                            }
                        }

                        boxSize[popBoxes[i]] = new Vector2(boxSize[popBoxes[i]].X + xDiff, boxSize[popBoxes[i]].Y + yDiff);
                    }

                    if (curCooldown <= 0) // do the pop
                    {
                        boxSize[popBoxes[i]] = popSizes[i];

                        popCoolDowns[i] = orgCoolDowns[i];
                    }

                    boxColSize[popBoxes[i]] = boxSize[popBoxes[i]];
                }
            }
        }

        Vector2 GetFacingDirection(Ball b)
        {
            return new Vector2(
                MathF.Cos((MathF.PI / 180) * b.rotation),
                MathF.Sin((MathF.PI / 180) * b.rotation)
                );
        }

        float DirectionToRotation(Vector2 inDir)
        {
            float rot = (float)(Math.Atan2(inDir.Y, inDir.X) / (2 * Math.PI));
            return (rot * 360) + 180;
        }
    }
}
