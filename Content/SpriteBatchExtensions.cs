using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace BaseBuilderRPG.Content
{
    public static class SpriteBatchExtensions
    {
        public static void DrawRectangle(this SpriteBatch spriteBatch, Rectangle rectangle, Color color, float layerDepth)
        {
            Texture2D pixel = new Texture2D(spriteBatch.GraphicsDevice, 1, 1);
            pixel.SetData(new[] { Color.White });

            spriteBatch.Draw(pixel, new Vector2(rectangle.X, rectangle.Y), null, color, 0, Vector2.Zero, new Vector2(rectangle.Width, rectangle.Height), SpriteEffects.None, layerDepth);
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
    }
}
