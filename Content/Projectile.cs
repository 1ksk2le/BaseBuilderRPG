using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

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

        public void Update(GameTime gameTime, Projectile_Globals globalProjectile, Particle_Globals globalParticle, List<NPC> npcs)
        {
            visualHandler.SpawnProjectileParticles(globalParticle, gameTime);
            if (!didSpawn)
            {
                didSpawn = true;
            }
            center = position + origin;
            rectangle = new Rectangle((int)position.X, (int)position.Y, width, height);

            if (lifeTime >= lifeTimeMax)
            {
                Kill(globalProjectile, globalParticle);
            }
            else
            {
                lifeTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
            }

            if (penetrate < 0 && ai != 2)
            {
                Kill(globalProjectile, globalParticle);
            }

            if (ai == 0 || ai == 1)
            {
                rotation = (float)Math.Atan2(velocity.Y, velocity.X);

                position += velocity * speed * (float)gameTime.ElapsedGameTime.TotalSeconds;
            }

            if (ai == 3)
            {
                float arcTimer = 1.0f - (lifeTime / lifeTimeMax);
                arcTimer = MathHelper.Clamp(arcTimer, 0f, 1f);

                if (lifeTime < lifeTimeMax / 16)
                {
                    position += velocity * speed * (float)gameTime.ElapsedGameTime.TotalSeconds;
                }
                else
                {
                    NPC targetNPC = null;
                    float closestDistance = float.MaxValue;
                    float maxSearchRange = 200f;

                    foreach (NPC npc in npcs)
                    {
                        float distance = Vector2.DistanceSquared(center, npc.center);

                        if (npc.isAlive && distance < closestDistance && distance < maxSearchRange * maxSearchRange)
                        {
                            closestDistance = distance;
                            targetNPC = npc;
                        }
                    }

                    if (targetNPC != null)
                    {
                        Vector2 targetDirection = targetNPC.center - center;
                        targetDirection.Normalize();

                        float interpolationFactor = 1.0f - arcTimer;

                        Vector2 interpolatedDirection = Vector2.Lerp(velocity, targetDirection, interpolationFactor);
                        interpolatedDirection.Normalize();

                        position += interpolatedDirection * speed * (float)gameTime.ElapsedGameTime.TotalSeconds;
                    }
                    else
                    {
                        isAlive = false;
                    }
                }
            }

            if (ai == 2)
            {
                if (owner.equippedWeapon != null && (owner.equippedWeapon.weaponType == "One Handed Sword" || owner.equippedWeapon.weaponType == "One Handed Wand"))
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

        public void Kill(Projectile_Globals projManager, Particle_Globals globalParticle)
        {
            visualHandler.SpawnProjectileKillParticles(globalParticle);
            isAlive = false;
        }
    }
}