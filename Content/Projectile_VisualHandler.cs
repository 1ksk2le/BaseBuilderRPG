using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BaseBuilderRPG.Content
{
    public class Projectile_VisualHandler
    {
        private Projectile projectile;

        public Projectile_VisualHandler(Projectile projectile)
        {
            this.projectile = projectile;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (projectile.texture != null && projectile.isAlive)
            {
                float scale = 1.0f;

                if (projectile.ai == 0)
                {
                    if (projectile.lifeTime < projectile.lifeTimeMax / 2)
                    {
                        scale = MathHelper.Lerp(0.3f, 1f, projectile.lifeTime / (projectile.lifeTimeMax / 2));
                    }
                    else
                    {
                        scale = MathHelper.Lerp(1f, 0.3f, (projectile.lifeTime - projectile.lifeTimeMax / 2) / (projectile.lifeTimeMax / 2));
                    }
                    spriteBatch.Draw(projectile.texture, projectile.position + projectile.origin, null, Color.White, projectile.rotation, new Vector2(projectile.texture.Width, projectile.origin.Y / 2), scale, SpriteEffects.None, 0);
                }
                else
                {
                    spriteBatch.Draw(projectile.texture, projectile.position + projectile.origin, null, Color.White, projectile.rotation, projectile.origin, scale, projectile.ai == 2 ? SpriteEffects.FlipVertically : SpriteEffects.None, 0);
                }


                if (Main.drawDebugRectangles)
                {
                    spriteBatch.DrawCircle(projectile.center, 4f, Color.Lime * 1.5f, 64, 1f);
                    spriteBatch.DrawRectangleBorder(projectile.rectangle, Color.Lime, 1f, 0.01f);
                }
            }
        }

    }
}
