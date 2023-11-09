using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

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
    }
}
