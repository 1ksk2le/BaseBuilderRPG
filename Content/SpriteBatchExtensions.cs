using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace BaseBuilderRPG.Content
{
    public static class SpriteBatchExtensions
    {
        public static void DrawRectangle(this SpriteBatch spriteBatch, Rectangle rectangle, Color color, float layerDepth)
        {
            spriteBatch.Draw(Main.pixel, new Vector2(rectangle.X, rectangle.Y), null, color, 0, Vector2.Zero, new Vector2(rectangle.Width, rectangle.Height), SpriteEffects.None, layerDepth);
        }

        public static void DrawStringWithOutline(this SpriteBatch spriteBatch, SpriteFont font, string text, Vector2 position, Color outlineColor, Color innerColor, float scale, float layerDepth)
        {
            for (float i = 0; i < 360; i += 45)
            {
                float angle = MathHelper.ToRadians(i);
                Vector2 offset = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 1.5f * scale;

                spriteBatch.DrawString(font, text, position + offset, outlineColor, 0f, Vector2.Zero, scale, SpriteEffects.None, layerDepth);
                spriteBatch.DrawString(font, text, position, innerColor, 0f, Vector2.Zero, scale, SpriteEffects.None, layerDepth + 0.0001f);
            }
        }

        public static void DrawRectangleWithBorder(this SpriteBatch spriteBatch, Rectangle rectangle, Color borderColor, float borderWidth, float layerDepth)
        {
            spriteBatch.DrawRectangle(new Rectangle(rectangle.X, rectangle.Y, rectangle.Width, (int)borderWidth), borderColor, layerDepth);
            spriteBatch.DrawRectangle(new Rectangle(rectangle.X, rectangle.Bottom - (int)borderWidth, rectangle.Width, (int)borderWidth), borderColor, layerDepth);
            spriteBatch.DrawRectangle(new Rectangle(rectangle.X, rectangle.Y, (int)borderWidth, rectangle.Height), borderColor, layerDepth);
            spriteBatch.DrawRectangle(new Rectangle(rectangle.Right - (int)borderWidth, rectangle.Y, (int)borderWidth, rectangle.Height), borderColor, layerDepth);
        }

        public static void DrawCircle(this SpriteBatch spriteBatch, Vector2 center, float radius, Color color, int segments, float layerDepth)
        {
            float angleIncrement = MathHelper.TwoPi / segments;

            for (int i = 0; i < segments; i++)
            {
                float angle1 = i * angleIncrement;
                float angle2 = (i + 1) * angleIncrement;

                Vector2 point1 = center + new Vector2((float)Math.Cos(angle1) * radius, (float)Math.Sin(angle1) * radius);
                Vector2 point2 = center + new Vector2((float)Math.Cos(angle2) * radius, (float)Math.Sin(angle2) * radius);

                spriteBatch.DrawLine(point1, point2, color, layerDepth);
            }
        }

        public static void DrawLine(this SpriteBatch spriteBatch, Vector2 start, Vector2 end, Color color, float layerDepth)
        {
            Vector2 edge = end - start;
            float angle = (float)Math.Atan2(edge.Y, edge.X);

            spriteBatch.Draw(Main.pixel, start, null, color, angle, Vector2.Zero, new Vector2(edge.Length(), 1), SpriteEffects.None, layerDepth);
        }
    }
}
