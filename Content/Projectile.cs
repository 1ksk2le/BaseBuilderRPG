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
        public Projectile_VisualHandler visualHandler;
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
            visualHandler = new Projectile_VisualHandler(this);
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
                if (owner.equippedWeapon != null && owner.equippedWeapon.weaponType == "One Handed Sword")
                {
                    texture = owner.equippedWeapon.texture;
                    float progress = owner.useTimer / owner.equippedWeapon.useTime;
                    lifeTimeMax = owner.equippedWeapon.useTime;
                    origin = (owner.direction == -1) ? new Vector2(0, 0) : new Vector2(width, 0);

                    position = owner.position + (owner.direction == 1 ? new Vector2(width, owner.height / 2) : new Vector2(width / 2, owner.height / 2));

                    float angleToTarget = (float)Math.Atan2(target.Y - owner.center.Y, target.X - owner.center.X);
                    float midpointRotation = angleToTarget;


                    float startRotation = (owner.direction == 1) ? midpointRotation - (float)MathHelper.Pi + MathHelper.ToRadians(90) : midpointRotation - MathHelper.ToRadians(90);
                    float endRotation = (owner.direction == 1) ? midpointRotation - (float)MathHelper.Pi - MathHelper.ToRadians(90) : midpointRotation + MathHelper.ToRadians(90);

                    rotation = MathHelper.Lerp(startRotation, endRotation, progress);

                    rectangle = CalculateRotatedRectangle(texture.Bounds, position, origin, rotation);
                    SpawnMeleeParticles(globalParticle, texture.Bounds, position, origin, rotation);

                    if (owner.direction == 1)
                    {
                        rectangle = new Rectangle(rectangle.X + width, rectangle.Y, rectangle.Width, rectangle.Height);
                    }
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

        public Vector2 CalculateRotatedVector2Bottom(Rectangle bounds, Vector2 position, Vector2 origin, float rotation)
        {
            Matrix transform = Matrix.CreateTranslation(new Vector3(-origin, 0.0f)) *
                               Matrix.CreateRotationZ(rotation) *
                               Matrix.CreateTranslation(new Vector3(position, 0.0f));

            Vector2 bottom = Vector2.Transform(new Vector2(bounds.Left + bounds.Width / 2, bounds.Bottom), transform);

            return bottom;
        }

        public void SpawnMeleeParticles(Particle_Globals globalParticle, Rectangle bounds, Vector2 position, Vector2 origin, float rotation)
        {
            Vector2 basePos = CalculateRotatedVector2Bottom(bounds, position, origin, rotation);

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
                for (int i = 0; i < owner.equippedWeapon.texture.Height / 2; i++)
                {
                    if (Main.random.Next(150) == 0)
                    {
                        globalParticle.NewParticle(3, 0,
                            (owner.direction == 1) ? basePos + new Vector2(Main.random.Next(-owner.equippedWeapon.texture.Height + 16, 0), Main.random.Next(-owner.equippedWeapon.texture.Width / 2, owner.equippedWeapon.texture.Width / 2)) : basePos + new Vector2(Main.random.Next(0, owner.equippedWeapon.texture.Height - 16), Main.random.Next(-owner.equippedWeapon.texture.Width / 2, owner.equippedWeapon.texture.Width / 2)),
                            new Vector2(Main.random.Next(-50, -10) * owner.direction, Main.random.Next(-20, 20)), Vector2.Zero, 0f, 0.8f, 0.45f + Main.random.NextFloat(0.5f, 1f), Color.Wheat, Color.Lime, Color.Yellow);
                    }
                    if (Main.random.Next(150) == 0)
                    {
                        globalParticle.NewParticle(3, 0,
                            (owner.direction == 1) ? basePos + new Vector2(Main.random.Next(-owner.equippedWeapon.texture.Height + 16, 0), Main.random.Next(-owner.equippedWeapon.texture.Width / 2, owner.equippedWeapon.texture.Width / 2)) : basePos + new Vector2(Main.random.Next(0, owner.equippedWeapon.texture.Height - 16), Main.random.Next(-owner.equippedWeapon.texture.Width / 2, owner.equippedWeapon.texture.Width / 2)),
                            new Vector2(Main.random.Next(-50, -10) * owner.direction, Main.random.Next(-20, 20)), Vector2.Zero, 0f, 0.8f, 0.45f + Main.random.NextFloat(0.5f, 1f), Color.Wheat, Color.Aqua, Color.Magenta);
                    }
                    if (Main.random.Next(150) == 0)
                    {
                        globalParticle.NewParticle(3, 1,
                            (owner.direction == 1) ? basePos + new Vector2(Main.random.Next(-owner.equippedWeapon.texture.Height + 16, 0), Main.random.Next(-owner.equippedWeapon.texture.Width / 2, owner.equippedWeapon.texture.Width / 2)) : basePos + new Vector2(Main.random.Next(0, owner.equippedWeapon.texture.Height - 16), Main.random.Next(-owner.equippedWeapon.texture.Width / 2, owner.equippedWeapon.texture.Width / 2)),
                            new Vector2(Main.random.Next(-50, -30) * owner.direction, Main.random.Next(-10, 10)), Vector2.Zero, 2f * owner.direction * Main.random.Next(-2, 2), 0.8f, 2f * Main.random.NextFloat(0.2f, 0.8f), Color.Wheat, Color.Aqua, Color.Magenta);
                    }
                }
            }
        }

        public void Kill(Projectile_Globals projManager)
        {
            if (ai == 2)
            {
            }
        }
    }
}