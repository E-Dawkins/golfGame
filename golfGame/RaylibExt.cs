using Raylib_cs;
using System.Numerics;

namespace Raylib_cs
{
    internal class RaylibExt
    {
        public static void DrawTexture(Texture2D texture, float xPos, float yPos, float width, float height, Color color,
            float rotation = 0.0f, float xOrigin = 0.0f, float yOrigin = 0.0f)
        {
            var dst = new Rectangle(xPos, yPos, width, height);
            var src = new Rectangle(0, 0, texture.width, texture.height);
            var origin = new Vector2(xOrigin * width, yOrigin * height);
            Raylib.DrawTexturePro(texture, src, dst, origin, rotation, color);
        }

        public static void centerText(string inText, int fontSize, Vector2 position, Color color) // centers text using text width
        {
            int centerAmount = Raylib.MeasureText(inText, fontSize) / 2;
            Raylib.DrawText(inText, (int)position.X - centerAmount, (int)position.Y, fontSize, color);
        }
    }

    internal class Vec2
    {
        public static Vector2 rotateDirection(Vector2 orgDir, float angle)
        {
            Vector2 result = Vector2.Zero;

            result.X = orgDir.X * MathF.Cos(angle) - orgDir.Y * MathF.Sin(angle);
            result.Y = orgDir.X * MathF.Sin(angle) + orgDir.Y * MathF.Cos(angle);

            return result;
        }
    }
}
