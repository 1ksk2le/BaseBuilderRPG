using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace BaseBuilderRPG.Content
{
    public class Projectile
    {
        public Player owner { get; set; }
        public Texture2D texture { get; set; }
        public int id { get; set; }
        public int penetrate { get; set; }
        public int ai { get; set; }
        public int damage { get; set; }
        public float lifeTimeMax { get; set; }
        public float lifeTime { get; set; }
        public float knockBack { get; set; }
        public string texturePath { get; set; }
        public string name { get; set; }
        public bool isAlive { get; set; }
        public Vector2 position { get; set; }
        public Vector2 target { get; set; }

        public Vector2 velocity;
        public Vector2 origin, center;
        public Rectangle rectangle;
        public int width, height;
        public float rotation;
        public float rotationSpeed;
        public float speed;
        public bool didSpawn;
        private float midpointrot, startrot, endrot;

        public Projectile(Texture2D texture, string texturePath, int id, int ai, Vector2 position, Vector2 target, float speed, string name, int damage, int penetrate, float lifeTime, float knockBack, Player owner, bool isAlive, int width, int height)
        {
            this.texture = texture;
            this.texturePath = texturePath;
            this.id = id;
            this.ai = ai;
            this.name = name;
            this.damage = damage;
            this.lifeTimeMax = lifeTime;
            this.knockBack = knockBack;
            this.position = position;
            this.isAlive = isAlive;
            this.penetrate = penetrate;
            this.speed = speed;
            this.owner = owner;
            this.lifeTime = 0f;
            this.width = width;
            this.height = height;
            this.target = target;
            didSpawn = false;

            velocity = (target - position);
            velocity.Normalize();

            origin = new Vector2(width / 2, height / 2);
        }

        public void Update(GameTime gameTime, Projectile_Globals globalProjectile, Particle_Globals globalParticle)
        {
            if (!didSpawn)
            {
                didSpawn = true;
            }
            center = position + origin;
            rectangle = new Rectangle((int)position.X, (int)position.Y, width, height);

            if (lifeTime >= lifeTimeMax)
            {
                Kill(globalProjectile);
                isAlive = false;
            }
            else
            {
                lifeTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
            }

            if (penetrate < 0 && ai != 2)
            {
                Kill(globalProjectile);
                isAlive = false;
            }

            if (ai == 0 || ai == 1)
            {
                rotation = (float)Math.Atan2(velocity.Y, velocity.X);

                position += velocity * speed * (float)gameTime.ElapsedGameTime.TotalSeconds;
            }

            if (ai == 2)
            {
                if (owner.equippedWeapon != null && owner.equippedWeapon.weaponType == "One Handed")
                {
                    texture = owner.equippedWeapon.texture;
                    float progress = owner.useTimer / owner.equippedWeapon.useTime;
                    lifeTimeMax = owner.equippedWeapon.useTime;
                    origin = (owner.direction == -1) ? new Vector2(0, 0) : new Vector2(width / 2 + height / 2, 0);

                    position = owner.position + (owner.direction == 1 ? new Vector2(owner.width / 2 - width * 0.2f, owner.height / 2) : new Vector2(width / 2, owner.height / 2));

                    float angleToTarget = (float)Math.Atan2(target.Y - owner.center.Y, target.X - owner.center.X);
                    float midpointRotation = angleToTarget;


                    float startRotation = (owner.direction == 1) ? midpointRotation - (float)MathHelper.Pi + MathHelper.ToRadians(90) : midpointRotation - MathHelper.ToRadians(90);
                    float endRotation = (owner.direction == 1) ? midpointRotation - (float)MathHelper.Pi - MathHelper.ToRadians(90) : midpointRotation + MathHelper.ToRadians(90);

                    rotation = MathHelper.Lerp(startRotation, endRotation, progress);

                    rectangle = CalculateRotatedRectangle(texture.Bounds, position, origin, rotation);
                    midpointrot = midpointRotation;
                    startrot = startRotation;
                    endrot = endRotation;

                }
            }
        }

        public Rectangle CalculateRotatedRectangle(Rectangle bounds, Vector2 position, Vector2 origin, float rotation)
        {
            Matrix transform = Matrix.CreateTranslation(new Vector3(-origin, 0.0f)) *
                               Matrix.CreateRotationZ(rotation) *
                               Matrix.CreateTranslation(new Vector3(position, 0.0f));

            Vector2 topLeft = Vector2.Transform(new Vector2(bounds.Left, bounds.Top), transform);
            Vector2 topRight = Vector2.Transform(new Vector2(bounds.Right, bounds.Top), transform);
            Vector2 bottomLeft = Vector2.Transform(new Vector2(bounds.Left, bounds.Bottom), transform);
            Vector2 bottomRight = Vector2.Transform(new Vector2(bounds.Right, bounds.Bottom), transform);

            Vector2 min = Vector2.Min(Vector2.Min(topLeft, topRight), Vector2.Min(bottomLeft, bottomRight));
            Vector2 max = Vector2.Max(Vector2.Max(topLeft, topRight), Vector2.Max(bottomLeft, bottomRight));

            return new Rectangle((int)min.X, (int)min.Y, (int)(max.X - min.X), (int)(max.Y - min.Y));
        }

        public void Kill(Projectile_Globals projManager)
        {
            if (ai == 0)
            {
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (texture != null && isAlive)
            {
                float scale = 1.0f;

                if (ai == 0)
                {
                    if (lifeTime < lifeTimeMax / 2)
                    {
                        scale = MathHelper.Lerp(0.3f, 1f, lifeTime / (lifeTimeMax / 2));
                    }
                    else
                    {
                        scale = MathHelper.Lerp(1f, 0.3f, (lifeTime - lifeTimeMax / 2) / (lifeTimeMax / 2));
                    }
                    spriteBatch.Draw(texture, position + origin, null, Color.White, rotation, new Vector2(texture.Width, origin.Y / 2), scale, SpriteEffects.None, 0);
                }
                else
                {
                    spriteBatch.Draw(texture, position + origin, null, Color.White, rotation, origin, scale, ai == 2 ? SpriteEffects.FlipVertically : SpriteEffects.None, 0);
                }


                //if (Main.drawDebugRectangles)
                {
                    spriteBatch.DrawCircle(center, 4f, Color.Lime * 1.5f, 64, 1f);
                    spriteBatch.DrawRectangleBorder(rectangle, Color.Lime, 1f, 0.01f);
                    spriteBatch.DrawString(Main.testFont, "midpoint " + midpointrot.ToString("F2"), owner.center + new Vector2(0, 40), Color.Black);
                    spriteBatch.DrawString(Main.testFont, "start " + startrot.ToString("F2"), owner.center + new Vector2(0, 60), Color.Black);
                    spriteBatch.DrawString(Main.testFont, "end " + endrot.ToString("F2"), owner.center + new Vector2(0, 70), Color.Black);
                    spriteBatch.DrawString(Main.testFont, "midpoint test " + ((endrot + startrot) / 2).ToString("f2"), owner.center + new Vector2(0, 80), Color.Black);
                    spriteBatch.DrawString(Main.testFont, "midpoint test substract" + (endrot - startrot).ToString("f2"), owner.center + new Vector2(0, 90), Color.Black);
                }
            }

        }
    }
}