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
                    spriteBatch.Draw(projectile.texture, projectile.position + projectile.origin, null, Color.White, projectile.rotation, projectile.origin, scale, projectile.ai == 2 ? (projectile.owner.direction == 1 ? SpriteEffects.FlipVertically | SpriteEffects.FlipHorizontally : SpriteEffects.FlipVertically) : SpriteEffects.None, 0);
                }



            }
            if (Main.drawDebugRectangles)
            {
                spriteBatch.DrawCircle(projectile.center, 4f, Color.Lime * 1.5f, 64, 1f);
                spriteBatch.DrawRectangleBorder(projectile.rectangle, Color.Lime, 1f, 0.01f);
            }
        }

        public Vector2 CalculateRotatedVector2Bottom(Rectangle bounds, Vector2 position, Vector2 origin, float rotation)
        {
            Matrix transform = Matrix.CreateTranslation(new Vector3(-origin, 0.0f)) *
                               Matrix.CreateRotationZ(rotation) *
                               Matrix.CreateTranslation(new Vector3(position, 0.0f));

            Vector2 bottom = Vector2.Transform(new Vector2(bounds.Left + bounds.Width / 2, bounds.Bottom), transform);

            return bottom;
        }
        public void SpawnProjectileParticles(Particle_Globals globalParticle)
        {
            if (projectile.id == 3)
            {
                for (int i = 0; i < projectile.width; i++)
                {
                    Vector2 posAdjuster = new Vector2(Main.random.Next(-projectile.width / 2, projectile.width / 2), Main.random.Next(-projectile.height / 2, projectile.height / 2));
                    globalParticle.NewParticle(3, 0, projectile.center + posAdjuster, Vector2.Zero, Vector2.Zero, 0f, Main.random.NextFloat(0.01f, 0.4f), 3f * Main.random.NextFloat(0.2f, 1f), Color.Transparent, Color.OrangeRed, Color.LightYellow);
                    globalParticle.NewParticle(3, 0, projectile.center, Vector2.Zero, Vector2.Zero, 0f, Main.random.NextFloat(0.3f, 0.6f), 2f * Main.random.NextFloat(0.2f, 0.7f), Color.Transparent, Color.OrangeRed, Color.LightYellow);
                    if (Main.random.Next(40) == 0)
                    {
                        globalParticle.NewParticle(1, 2, projectile.center + posAdjuster, new Vector2(Main.random.Next(-30, 30), Main.random.Next(-30, 30)), Vector2.Zero, 0f, 0.6f, 1.5f * Main.random.NextFloat(0.1f, 3f), Color.Wheat, Color.Red, Color.LightYellow);
                    }
                }
            }
        }

        public void SpawnProjectileKillParticles(Particle_Globals globalParticle)
        {
            if (projectile.id == 3)
            {
                for (int i = 0; i < projectile.width * 2; i++)
                {
                    Vector2 posAdjuster = new Vector2(Main.random.Next(-projectile.width / 2, projectile.width / 2), Main.random.Next(-projectile.height / 2, projectile.height / 2));
                    globalParticle.NewParticle(1, 2, projectile.center + posAdjuster, new Vector2(Main.random.Next(-100, 100), Main.random.Next(-100, 100)), Vector2.Zero, 0f, Main.random.NextFloat(0.2f, 0.8f), 3f * Main.random.NextFloat(0.2f, 2f), Color.Transparent, Color.OrangeRed, Color.LightYellow);
                    globalParticle.NewParticle(1, 2, projectile.center + posAdjuster, new Vector2(Main.random.Next(-60, 60), Main.random.Next(-60, 60)), Vector2.Zero, 0f, Main.random.NextFloat(0.2f, 0.8f), 3f * Main.random.NextFloat(0.2f, 2f), Color.Transparent, Color.Red, Color.Orange);
                }
            }
        }

        public void SpawnMeleeParticles(Particle_Globals globalParticle, Rectangle bounds, Vector2 position, Vector2 origin, float rotation)
        {
            Vector2 basePos = CalculateRotatedVector2Bottom(bounds, position, origin, rotation);
            var owner = projectile.owner;

            if (owner.equippedWeapon.id == 4)
            {
                for (int i = 0; i < 16; i++)
                {
                    if (Main.random.Next(25) == 0)
                    {
                        globalParticle.NewParticle(3, 0, basePos + new Vector2(Main.random.Next(-5, 5), Main.random.Next(-5, 5)), new Vector2(0, Main.random.Next(-30, -15)), Vector2.Zero, 0f, 0.6f, 0.2f + Main.random.NextFloat(0.5f, 2.5f), Color.Wheat, Color.OrangeRed, Color.White);
                        if (Main.random.Next(25) == 0)
                        {
                            globalParticle.NewParticle(1, 1, basePos + new Vector2(Main.random.Next(-5, 5), Main.random.Next(-5, 5)), new Vector2(Main.random.Next(-20, 20), Main.random.Next(-110, -40)), Vector2.Zero, 0f, 1.2f, 2f + Main.random.NextFloat(0.5f, 3f), Color.Wheat, Color.OrangeRed, Color.White);
                        }
                    }
                }
            }

            if (owner.equippedWeapon.prefixName == "Magical")
            {
                if (owner.equippedWeapon.weaponType == "One Handed Sword")
                {
                    for (int i = 0; i < owner.equippedWeapon.texture.Height / 2; i++)
                    {
                        Vector2 particleOffset = new Vector2(Main.random.Next(owner.equippedWeapon.texture.Width / 2, owner.equippedWeapon.texture.Width), Main.random.Next(6, owner.equippedWeapon.texture.Height));

                        Vector2 particlePosition = projectile.center + Vector2Extensions.RotateVector(particleOffset, rotation, origin);

                        if (Main.random.Next(150) == 0)
                        {

                            globalParticle.NewParticle(3, 0,
                                particlePosition,
                                Vector2.Transform(new Vector2(0, Main.random.Next(-50, -10)), Matrix.CreateRotationZ(rotation)), Vector2.Zero, 0f, 0.8f, 0.45f + Main.random.NextFloat(0.5f, 1f), Color.Wheat, Color.Lime, Color.Yellow);
                        }
                        if (Main.random.Next(150) == 0)
                        {
                            globalParticle.NewParticle(3, 0,
                                particlePosition,
                                Vector2.Transform(new Vector2(Main.random.Next(-50, -10), Main.random.Next(-20, 20)), Matrix.CreateRotationZ(rotation)), Vector2.Zero, 0f, 0.8f, 0.45f + Main.random.NextFloat(0.5f, 1f), Color.Wheat, Color.Aqua, Color.Magenta);
                        }
                        if (Main.random.Next(150) == 0)
                        {
                            globalParticle.NewParticle(3, 1,
                                particlePosition,
                                Vector2.Transform(new Vector2(Main.random.Next(-50, -30), Main.random.Next(-10, 10)), Matrix.CreateRotationZ(rotation)), Vector2.Zero, 2f * owner.direction * Main.random.Next(-2, 2), 0.8f, 2f * Main.random.NextFloat(0.2f, 0.8f), Color.Wheat, Color.Aqua, Color.Magenta);
                        }
                    }
                }
            }
        }
    }
}
