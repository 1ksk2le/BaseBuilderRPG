using Microsoft.Xna.Framework;
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
        public float maxHealth { get; set; }
        public float knockBack { get; set; }
        public float knockBackRes { get; set; }
        public string texturePath { get; set; }
        public string name { get; set; }
        public bool isAlive { get; set; }
        public Vector2 position { get; set; }
        public Vector2 velocity { get; set; }

        public Vector2 target;
        public float rotation, animationSpeed, immunityTime, health, immunityTimeMax, aiX, aiY, aiZ;
        public int width, height;
        public bool isImmune;

        private int currentFrame;
        private const float kbDuration = 0.25f;
        private float kbTimer = 0f;
        private float animationTimer, hitEffectTimer, hitEffectTimerMax;
        private Vector2 kbStartPos, kbEndPos;
        public NPC(Texture2D texture, string texturePath, int id, int ai, Vector2 position, string name, int damage, float maxHealth, float knockBack, float knockBackRes, int numFrames, bool isAlive)
        {
            this.texture = texture;
            this.texturePath = texturePath;
            this.id = id;
            this.ai = ai;
            this.name = name;
            this.damage = damage;
            this.maxHealth = maxHealth;
            this.knockBack = knockBack;
            this.knockBackRes = knockBackRes;
            this.isAlive = isAlive;
            this.numFrames = numFrames;
            this.position = position;

            health = this.maxHealth;
            animationSpeed = 0.1f;
            immunityTimeMax = 0.1f;
            immunityTime = 0f;
            hitEffectTimerMax = 1f;
            hitEffectTimer = 0f;
            isImmune = true;
            aiX = 0f;
            aiY = 0f;
            aiZ = 0f;
        }

        public void Update(GameTime gameTime, List<Player> players, List<Projectile> projectiles, Display_Text_Manager disTextManager, Item_Manager itemManager)
        {
            if (health > 0)
            {
                width = texture.Width;
                height = texture.Height / numFrames;
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
                ProcessAI(gameTime, players, projectiles, disTextManager);
                isAlive = true;
            }
            else
            {
                Kill(itemManager);
                isAlive = false;
            }
        }

        public void ProcessAI(GameTime gameTime, List<Player> players, List<Projectile> projectiles, Display_Text_Manager disTexManager)
        {
            Player targetPlayer = null;
            float distanceLimit = 400f;

            foreach (Player player in players)
            {
                float distance = Vector2.DistanceSquared(player.Position, position);

                if (distance < distanceLimit && player.Name != "East")
                {
                    distanceLimit = distance;
                    targetPlayer = player;
                }
            }

            if (targetPlayer != null)
            {
                target = targetPlayer.Position + new Vector2(0, targetPlayer.PlayerTexture.Height / 2);
            }

            HitByProjectile(gameTime, projectiles, disTexManager);
            HitByPlayer(gameTime, players, disTexManager);
            HitPlayer(gameTime, players, disTexManager);

            var tick = (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (target != Vector2.Zero)
            {
                if (ai == 1)
                {
                    aiX += tick;

                    Vector2 direction = target - position;
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
            if (Main.DrawDebugRectangles)
            {
                Rectangle npcRectangle = new Rectangle((int)position.X, (int)position.Y, texture.Width, texture.Height / numFrames);
                spriteBatch.DrawRectangleWithBorder(npcRectangle, Color.Transparent, Color.Coral, 1f, 0.01f);

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
                spriteBatch.Draw(texture, position + new Vector2(width / 2, height / 2), sourceRect, npcColor, rotation, new Vector2(width / 2, height / 2), scale, SpriteEffects.None, 0.69f);
            }
        }

        private void HitByProjectile(GameTime gameTime, List<Projectile> projectiles, Display_Text_Manager disTextManager)
        {
            Rectangle npcRectangle = new Rectangle((int)position.X, (int)position.Y, texture.Width, texture.Height / numFrames);

            foreach (Projectile proj in projectiles)
            {
                if (proj.isAlive && proj.damage > 0)
                {
                    Rectangle projRectangle = new Rectangle((int)proj.position.X, (int)proj.position.Y, proj.width, proj.height);
                    if (projRectangle.Intersects(npcRectangle) && !isImmune)
                    {
                        if (kbTimer <= 0)
                        {
                            kbTimer = kbDuration;

                            Vector2 hitDirection = position - proj.position;
                            hitDirection.Normalize();

                            kbStartPos = position;
                            kbEndPos = position + hitDirection * (proj.knockBack * (1f - knockBackRes / 100));

                            proj.penetrate--;

                            proj.owner.TotalDamageDealt += proj.damage;
                            GetDamaged(disTextManager, proj.damage);
                        }
                    }
                }

                if (kbTimer > 0)
                {
                    ApplyKnockBack(gameTime);
                }
            }
        }

        private void HitByPlayer(GameTime gameTime, List<Player> players, Display_Text_Manager disTextManager)
        {
            Rectangle npcRectangle = new Rectangle((int)position.X, (int)position.Y, texture.Width, texture.Height / numFrames);

            foreach (Player player in players)
            {
                if (player.EquippedWeapon != null && player.EquippedWeapon.damageType == "melee")
                {
                    Vector2 Pos = (player.Direction == 1) ? new Vector2(player.PlayerTexture.Width + player.EquippedWeapon.texture.Height * 0.2f, player.PlayerTexture.Height / 2)
                                                    : new Vector2(-player.EquippedWeapon.texture.Height * 0.2f, player.PlayerTexture.Height / 2);
                    Rectangle playerWeaponRectangle = CalcRectangleForWeapons(player.Position + Pos, (int)(player.EquippedWeapon.texture.Width), (int)(player.EquippedWeapon.texture.Height * 0.9f), player.RotationAngle);

                    if (playerWeaponRectangle.Intersects(npcRectangle) && !isImmune && player.IsSwinging && player.CanHit)
                    {
                        if (kbTimer <= 0)
                        {
                            kbTimer = kbDuration;

                            Vector2 hitDirection = position - player.Position;
                            hitDirection.Normalize();

                            kbStartPos = position;
                            kbEndPos = position + hitDirection * (player.EquippedWeapon.knockBack * (1f - knockBackRes / 100));

                            player.TotalDamageDealt += player.EquippedWeapon.damage;
                            player.CanHit = false;
                            GetDamaged(disTextManager, player.EquippedWeapon.damage);
                        }
                    }
                }
            }

            if (kbTimer > 0)
            {
                ApplyKnockBack(gameTime);
            }
        }


        private void HitPlayer(GameTime gameTime, List<Player> players, Display_Text_Manager disTextManager)
        {
            foreach (Player player in players)
            {
                if (player.Name != "East")
                {
                    Rectangle npcRectangle = new Rectangle((int)position.X, (int)position.Y, texture.Width, texture.Height / numFrames);
                    Rectangle playerRectangle = new Rectangle((int)player.Position.X, (int)player.Position.Y, player.PlayerTexture.Width, player.PlayerTexture.Height);
                    if (npcRectangle.Intersects(playerRectangle) && !player.IsImmune && damage > 0)
                    {
                        player.GetDamaged(disTextManager, damage);
                    }
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

        private void GetDamaged(Display_Text_Manager texMan, int damage)
        {
            health -= damage;

            texMan.AddFloatingText("-" + damage.ToString(), "", new Vector2(position.X + width / 2, position.Y), Color.Red, Color.Transparent, 2f, 1.1f);

            hitEffectTimer = hitEffectTimerMax;
            immunityTime = immunityTimeMax;
        }

        public void Kill(Item_Manager itemManager)
        {
            health = -1;
            //itemManager.DropItem(1, 0, 0, 1, Position);
        }

        private Rectangle CalcRectangleForWeapons(Vector2 position, int width, int height, float rotation)
        {
            Matrix transform = Matrix.CreateRotationZ(rotation) * Matrix.CreateTranslation(position.X, position.Y, 0);

            Vector2 leftTop = Vector2.Transform(new Vector2(-width / 2, -height / 2), transform);
            Vector2 rightTop = Vector2.Transform(new Vector2(width / 2, -height / 2), transform);
            Vector2 leftBottom = Vector2.Transform(new Vector2(-width / 2, height / 2), transform);
            Vector2 rightBottom = Vector2.Transform(new Vector2(width / 2, height / 2), transform);

            Vector2 min = Vector2.Min(Vector2.Min(leftTop, rightTop), Vector2.Min(leftBottom, rightBottom));
            Vector2 max = Vector2.Max(Vector2.Max(leftTop, rightTop), Vector2.Max(leftBottom, rightBottom));

            return new Rectangle((int)min.X, (int)min.Y, (int)(max.X - min.X), (int)(max.Y - min.Y));
        }
    }
}