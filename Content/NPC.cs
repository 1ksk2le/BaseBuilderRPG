﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace BaseBuilderRPG.Content
{
    public class NPC
    {
        public Texture2D texture { get; set; }
        public int id { get; set; }
        public int ai { get; set; }
        public int damage { get; set; }
        public int numFrames { get; set; }
        public float healthMax { get; set; }
        public float knockBack { get; set; }
        public float knockBackRes { get; set; }
        public float targetRange { get; set; }
        public string texturePath { get; set; }
        public string name { get; set; }
        public bool isAlive { get; set; }
        public Vector2 position { get; set; }
        public Vector2 velocity { get; set; }

        public Vector2 target, origin, center;
        public Rectangle rectangle;
        public float rotation, animationSpeed, immunityTime, health, immunityTimeMax, aiX, aiY, aiZ;
        public int width, height;
        public bool isImmune;

        private bool didSpawn;
        private int currentFrame;
        private const float kbDuration = 0.25f;
        private float kbTimer = 0f;
        private float animationTimer, hitEffectTimer, hitEffectTimerMax;
        private Vector2 kbStartPos, kbEndPos;

        private Random random;
        public NPC(Texture2D texture, string texturePath, int id, int ai, Vector2 position, string name, int damage, float maxHealth, float knockBack, float knockBackRes, float targetRange, int numFrames, bool isAlive)
        {
            this.texture = texture;
            this.texturePath = texturePath;
            this.id = id;
            this.ai = ai;
            this.name = name;
            this.damage = damage;
            this.healthMax = maxHealth;
            this.knockBack = knockBack;
            this.knockBackRes = knockBackRes;
            this.targetRange = targetRange;
            this.isAlive = isAlive;
            this.numFrames = numFrames;
            this.position = position;

            health = this.healthMax;
            animationSpeed = 0.1f;
            immunityTimeMax = 0.1f;
            immunityTime = 0f;
            hitEffectTimerMax = 0.75f;
            hitEffectTimer = 0f;
            isImmune = true;
            didSpawn = false;
            aiX = 0f;
            aiY = 0f;
            aiZ = 0f;

            random = Main_Globals.GetRandomInstance();
        }

        public void Update(GameTime gameTime, List<Player> players, List<Projectile> projectiles, Text_Manager textManager, Global_Item globalItem, Global_Particle globalParticle)
        {
            if (!didSpawn)
            {
                width = texture.Width;
                height = texture.Height / numFrames;
                origin = new Vector2(width / 2, height / 2);
                didSpawn = true;
            }
            rectangle = new Rectangle((int)position.X, (int)position.Y, width, height);
            center = position + origin;

            if (health > 0)
            {
                if (immunityTime >= 0f)
                {
                    isImmune = true;
                    immunityTime -= (float)gameTime.ElapsedGameTime.TotalSeconds;

                }
                else
                {
                    isImmune = false;
                }

                if (hitEffectTimer >= 0f)
                {
                    hitEffectTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds;
                }

                animationTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;

                if (animationTimer >= animationSpeed)
                {
                    currentFrame = (currentFrame + 1) % numFrames;

                    animationTimer = 0f;
                }
                ProcessAI(gameTime, players, projectiles, textManager, globalParticle);
                isAlive = true;
            }
            else
            {
                Kill(globalItem, globalParticle);
                isAlive = false;
            }
        }

        public void ProcessAI(GameTime gameTime, List<Player> players, List<Projectile> projectiles, Text_Manager textManager, Global_Particle globalParticle)
        {
            Player targetPlayer = null;
            float distanceLimit = targetRange * targetRange;

            foreach (Player player in players)
            {
                float distance = Vector2.DistanceSquared(player.center, center);

                if (distance < distanceLimit)
                {
                    distanceLimit = distance;
                    targetPlayer = player;
                }
            }

            if (targetPlayer != null)
            {
                target = targetPlayer.center;
            }

            HitByProjectile(gameTime, projectiles, textManager, globalParticle);
            HitByPlayer(gameTime, players, textManager, globalParticle);
            HitPlayer(gameTime, players, textManager);

            var tick = (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (target != Vector2.Zero)
            {
                if (ai == 1)
                {
                    aiX += tick;

                    Vector2 direction = target - center;
                    direction.Normalize();

                    if (aiX > 3f)
                    {
                        aiY += tick;
                        if (aiY > 1f)
                        {
                            aiZ += tick;
                            position += direction * 1.5f;
                            if (aiY > 2f)
                            {
                                aiX = 0f;
                                aiY = 0f;
                                aiZ = 0f;
                            }
                        }
                    }
                    else
                    {
                        position += direction * 0.35f;
                    }
                }
            }
        }


        public void Draw(SpriteBatch spriteBatch)
        {
            if (Main.drawDebugRectangles)
            {
                spriteBatch.DrawCircle(center, 4f, Color.Red * 1.5f, 64, 1f);
                spriteBatch.DrawRectangleBorder(rectangle, Color.Red, 1f, 0.01f);
                spriteBatch.DrawCircle(center, targetRange, Color.Red, 64, 0.011f);

            }


            Color npcColor = Color.Lerp(Color.White, Color.Red, hitEffectTimer);
            if (ai == 0)
            {
                spriteBatch.Draw(texture, position, null, npcColor, rotation, Vector2.Zero, 1f, SpriteEffects.None, 0.69f);
            }
            if (ai == 1)
            {
                float levitationSpeed = 3.5f;
                float levitationAmplitude = 0.75f;

                float levitationOffset = (float)Math.Sin(aiZ * levitationSpeed) * levitationAmplitude;

                float scale = 1.0f + 0.4f * levitationOffset;

                Rectangle sourceRect = new Rectangle(0, currentFrame * height, width, height);
                spriteBatch.Draw(texture, position + origin, sourceRect, npcColor, rotation, origin, scale, SpriteEffects.None, 0.69f);
            }
        }

        private void HitByProjectile(GameTime gameTime, List<Projectile> projectiles, Text_Manager textManager, Global_Particle globalParticle)
        {
            foreach (Projectile proj in projectiles)
            {
                if (proj.isAlive && proj.damage > 0)
                {
                    if (proj.rectangle.Intersects(rectangle) && !isImmune)
                    {
                        if (kbTimer <= 0)
                        {
                            kbTimer = kbDuration;

                            Vector2 hitDirection = position - proj.position;
                            hitDirection.Normalize();

                            kbStartPos = position;
                            kbEndPos = position + hitDirection * (proj.knockBack * (1f - knockBackRes / 100));

                            proj.penetrate--;
                            target = proj.owner.center;
                            GetDamaged(textManager, proj.damage, globalParticle);
                        }
                    }
                }

                if (kbTimer > 0)
                {
                    ApplyKnockBack(gameTime);
                }
            }
        }

        private void HitByPlayer(GameTime gameTime, List<Player> players, Text_Manager textManager, Global_Particle globalParticle)
        {
            foreach (Player player in players)
            {
                if (player.equippedWeapon != null && player.equippedWeapon.damageType == "melee")
                {
                    if (player.rectangleMelee.Intersects(rectangle) && !isImmune && player.isSwinging && player.canHit)
                    {
                        if (kbTimer <= 0)
                        {
                            kbTimer = kbDuration;

                            Vector2 hitDirection = position - player.position;
                            hitDirection.Normalize();

                            kbStartPos = position;
                            kbEndPos = position + hitDirection * (player.equippedWeapon.knockBack * (1f - knockBackRes / 100));

                            target = player.center;
                            player.canHit = false;
                            GetDamaged(textManager, player.equippedWeapon.damage, globalParticle);
                        }
                    }
                }
            }

            if (kbTimer > 0)
            {
                ApplyKnockBack(gameTime);
            }
        }


        private void HitPlayer(GameTime gameTime, List<Player> players, Text_Manager textManager)
        {
            foreach (Player player in players)
            {
                if (rectangle.Intersects(player.rectangle) && !player.isImmune && damage > 0)
                {
                    player.GetDamaged(textManager, damage);
                }
            }
        }

        private void ApplyKnockBack(GameTime gameTime)
        {
            float progress = 1f - (kbTimer / kbDuration);
            position = Vector2.Lerp(kbStartPos, kbEndPos, progress);

            kbTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (kbTimer <= 0)
            {
                position = kbEndPos;
            }
        }

        private void GetDamaged(Text_Manager textManager, int damage, Global_Particle globalParticle)
        {
            int numberOfParticles = 10;
            float radius = 50f; // Adjust the radius of the circle
            float angleIncrement = MathHelper.TwoPi / numberOfParticles; // Calculate the angle between each particle

            for (int i = 0; i < numberOfParticles; i++)
            {
                float angle = i * angleIncrement;
                float xOffset = radius * (float)Math.Cos(angle);
                float yOffset = radius * (float)Math.Sin(angle);

                Vector2 particlePosition = center + new Vector2(xOffset, yOffset);
                Vector2 particleVelocity = new Vector2(random.Next(30, 50), random.Next(5, 5));

                globalParticle.NewParticle(0, 0, particlePosition, particleVelocity, origin, 2f, 1f * random.Next(1, 100) / 100, Color.Red);
            }


            health -= damage;

            textManager.AddFloatingText("-" + damage.ToString(), "", new Vector2(position.X + width / 2 + random.Next(-10, 10), position.Y), new Vector2(random.Next(-10, 10) * 1f, random.Next(1, 10) + 10f), Color.Red, Color.Transparent, 2f, 1.1f);

            hitEffectTimer = hitEffectTimerMax;
            immunityTime = immunityTimeMax;
        }

        public void Kill(Global_Item globalItem, Global_Particle globalParticle)
        {
            for (int i = 0; i < 30; i++)
            {
                globalParticle.NewParticle(0, 0, center + new Vector2(random.Next(width / 2), random.Next(height / 2)), new Vector2(random.Next(-10, 10), random.Next(-50, -30)), origin, 1f, 0.7f, Color.Red);
            }
            health = -1;


        }
    }
}