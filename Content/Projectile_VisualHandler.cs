using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

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
                    spriteBatch.Draw(projectile.texture, projectile.position + projectile.origin, null, Color.White, projectile.rotation, projectile.origin, scale, projectile.ai == 2 ? (projectile.owner.direction == 1 ? SpriteEffects.FlipVertically | SpriteEffects.FlipHorizontally : SpriteEffects.FlipVertically) : SpriteEffects.None, 0);
                }



            }
            if (Main.drawDebugRectangles)
            {
                spriteBatch.DrawCircle(projectile.center, 4f, Color.Lime * 1.5f, 64, 1f);
                spriteBatch.DrawRectangleBorder(projectile.rectangle, Color.Lime, 1f, 1F);
            }
        }

        public void SpawnProjectileParticles(Particle_Globals globalParticle, GameTime gameTime)
        {
            if (projectile.id == 1)
            {
                int numberOfParticles = 20;

                for (int i = 0; i < numberOfParticles; i++)
                {
                    Vector2 direction = projectile.velocity;
                    direction.Normalize();
                    Vector2 spawnPosition = projectile.center + direction * i * 1;
                    globalParticle.NewParticle(1, 3, spawnPosition, Vector2.Zero, Vector2.Zero, 0f, 0.1f, 1.2f, Color.Transparent, Color.Khaki, Color.Red);
                }
                if (projectile.lifeTime == (float)gameTime.ElapsedGameTime.TotalSeconds)
                {
                    Vector2 mouseDirection = Input_Manager.Instance.mousePosition - projectile.center;
                    mouseDirection.Normalize();

                    for (int i = 0; i < 12; i++)
                    {
                        Vector2 posAdjuster = new Vector2(Main.random.Next(-projectile.width / 2, projectile.width / 2), Main.random.Next(-projectile.height / 4, projectile.height / 4));
                        float angle;
                        if (projectile.owner.isControlled)
                        {
                            angle = (float)Math.Atan2(mouseDirection.Y, mouseDirection.X);
                        }
                        else
                        {
                            Vector2 projectileDirection = projectile.owner.target.center - projectile.center;
                            projectileDirection.Normalize();
                            angle = (float)Math.Atan2(projectileDirection.Y, projectileDirection.X);
                        }


                        Vector2 particleVelocity = new Vector2(Main.random.Next(25, 100), Main.random.Next(-25, 25));
                        particleVelocity = Vector2.Transform(particleVelocity, Matrix.CreateRotationZ(angle));

                        globalParticle.NewParticle(1, 2, projectile.center + posAdjuster, particleVelocity, Vector2.Zero, 0f, Main.random.NextFloat(0.1f, 0.6f), 2f * Main.random.NextFloat(0.2f, 2f), Color.Transparent, Color.DarkGray, Color.Black);
                        particleVelocity = new Vector2(Main.random.Next(30, 60), Main.random.Next(-30, 30));
                        particleVelocity = Vector2.Transform(particleVelocity, Matrix.CreateRotationZ(angle));

                        globalParticle.NewParticle(1, 2, projectile.center + posAdjuster, particleVelocity, Vector2.Zero, 0f, Main.random.NextFloat(0.1f, 0.6f), 2f * Main.random.NextFloat(0.2f, 2f), Color.Transparent, Color.Black, Color.Black);
                    }
                }
            }


        }

        public void SpawnProjectileKillParticles(Particle_Globals globalParticle)
        {
            if (projectile.id == 1)
            {
                for (int i = 0; i < 10; i++)
                {
                    Vector2 posAdjuster = new Vector2(Main.random.Next(-projectile.width / 2, projectile.width / 2), Main.random.Next(-projectile.height / 2, projectile.height / 2));
                    globalParticle.NewParticle(1, 2, projectile.center + posAdjuster, new Vector2(Main.random.Next(-60, 60), Main.random.Next(-60, 60)), Vector2.Zero, 0f, Main.random.NextFloat(0.1f, 0.6f), 5f * Main.random.NextFloat(0.2f, 1f), Color.Transparent, Color.Khaki, Color.Red);
                }
            }
        }
    }
}
