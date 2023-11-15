using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace BaseBuilderRPG.Content
{
    public class NPC
    {
        public Texture2D Texture { get; set; }

        public int ID { get; set; }
        public int AI { get; set; }
        public int Damage { get; set; }
        public int NumFrames { get; set; }
        public float MaxHealth { get; set; }
        public float KnockBack { get; set; }
        public string TexturePath { get; set; }
        public string Name { get; set; }
        public bool IsAlive { get; set; }

        public Vector2 Position { get; set; }
        public Vector2 Velocity { get; set; }

        public Vector2 Target;
        public float Rotation, AnimationSpeed, ImmunityTime, Health, MaxImmunityTime, AI_One, AI_Two, AI_Three;
        public int Width, Height;
        public bool IsImmune;

        private float AnimationTimer, HitEffectTimer;
        private int CurrentFrame;
        public NPC(Texture2D texture, string texturePath, string name, int id, int ai, int damage, float maxHealth, float knockBack, Vector2 position, int numFrames, bool isAlive)
        {
            Texture = texture;
            TexturePath = texturePath;
            ID = id;
            AI = ai;
            Name = name;
            Damage = damage;
            MaxHealth = maxHealth;
            KnockBack = knockBack;
            Position = position;
            IsAlive = isAlive;
            Health = MaxHealth;
            NumFrames = numFrames;
            AnimationSpeed = 0.1f;
            MaxImmunityTime = 0.1f;
            ImmunityTime = 0f;
            HitEffectTimer = 0f;
            IsImmune = true;
            AI_One = 0f;
            AI_Two = 0f;
            AI_Three = 0f;
        }

        public void Update(GameTime gameTime, List<Player> players, List<Projectile> projectiles, Display_Text_Manager disTextManager, Item_Manager itemManager)
        {
            if (Health > 0)
            {
                Width = Texture.Width;
                Height = Texture.Height / NumFrames;
                if (ImmunityTime >= 0f)
                {
                    IsImmune = true;
                    ImmunityTime -= (float)gameTime.ElapsedGameTime.TotalSeconds;

                }
                else
                {
                    IsImmune = false;
                }

                if (HitEffectTimer >= 0f)
                {
                    HitEffectTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds;
                }

                AnimationTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;

                if (AnimationTimer >= AnimationSpeed)
                {
                    CurrentFrame = (CurrentFrame + 1) % NumFrames;

                    AnimationTimer = 0f;
                }
                ProcessAI(gameTime, players, projectiles, disTextManager);
                IsAlive = true;
            }
            else
            {
                Kill(itemManager);
                IsAlive = false;
            }
        }

        public void ProcessAI(GameTime gameTime, List<Player> players, List<Projectile> projectiles, Display_Text_Manager disTexManager)
        {
            Player closestPlayer = null;
            float closestDistance = float.MaxValue;

            foreach (Player player in players)
            {
                float distance = Vector2.DistanceSquared(player.Position, Position);

                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestPlayer = player;
                }
            }

            if (closestPlayer != null)
            {
                Target = closestPlayer.Position;
            }

            HitByProjectile(projectiles, disTexManager);
            HitByPlayer(players, disTexManager);
            HitPlayer(players, disTexManager);

            var tick = (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (AI == 1)
            {
                AI_One += tick;

                Vector2 direction = Target - Position;
                direction.Normalize();

                if (AI_One > 3f)
                {
                    AI_Two += tick;
                    if (AI_Two > 1f)
                    {
                        AI_Three += tick;
                        Position += direction * 1.5f;
                        if (AI_Two > 2f)
                        {
                            AI_One = 0f;
                            AI_Two = 0f;
                            AI_Three = 0f;
                        }
                    }
                }
                else
                {
                    Position += direction * 0.35f;
                }
            }

        }
        public void Kill(Item_Manager itemManager)
        {
            itemManager.DropItem(1, 0, 0, 1, Position);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            Color npcColor = Color.Lerp(Color.White, Color.DarkRed, HitEffectTimer);
            if (AI == 0)
            {
                spriteBatch.Draw(Texture, Position, null, npcColor, Rotation, Vector2.Zero, 1f, SpriteEffects.None, 0.69f);
            }
            if (AI == 1)
            {
                float levitationSpeed = 3.5f;
                float levitationAmplitude = 0.75f;

                float levitationOffset = (float)Math.Sin(AI_Three * levitationSpeed) * levitationAmplitude;

                float scale = 1.0f + 0.4f * levitationOffset;

                Rectangle sourceRect = new Rectangle(0, CurrentFrame * Height, Width, Height);
                spriteBatch.Draw(Texture, Position, sourceRect, npcColor, Rotation, Vector2.Zero, scale, SpriteEffects.None, 0.69f);
            }
        }

        private void GetDamaged(Display_Text_Manager texMan, int damage)
        {
            Health -= damage;
            texMan.AddFloatingText("-" + damage.ToString(), "", new Vector2(Position.X + Width / 2, Position.Y), Color.Red, Color.Transparent, 2f, 1.1f);
            HitEffectTimer = 0.5f;
            ImmunityTime = MaxImmunityTime;
        }

        private void HitByProjectile(List<Projectile> projectiles, Display_Text_Manager disTextManager)
        {
            Rectangle npcRectangle = new Rectangle((int)Position.X, (int)Position.Y, Texture.Width, Texture.Height / NumFrames);

            foreach (Projectile proj in projectiles)
            {
                if (proj.IsAlive && proj.Damage > 0)
                {
                    Rectangle projRectangle = new Rectangle((int)proj.Position.X, (int)proj.Position.Y, proj.Width, proj.Height);
                    if (projRectangle.Intersects(npcRectangle) && !IsImmune)
                    {
                        proj.Penetrate--;
                        GetDamaged(disTextManager, proj.Damage);
                    }
                }
            }
        }

        private void HitByPlayer(List<Player> players, Display_Text_Manager disTextManager)
        {
            Rectangle npcRectangle = new Rectangle((int)Position.X, (int)Position.Y, Texture.Width, Texture.Height / NumFrames);

            foreach (Player player in players)
            {

                if (player.IsSwinging && player.UseTimer + 0.1f >= player.EquippedWeapon.UseTime && player.EquippedWeapon != null)
                {
                    Rectangle playerRectangle = new Rectangle((int)(player.Position.X + player.Direction * player.EquippedWeapon.Texture.Height / 1.25f), (int)player.Position.Y, player.PlayerTexture.Width, player.PlayerTexture.Height);
                    if (playerRectangle.Intersects(npcRectangle) && !IsImmune && player.EquippedWeapon.DamageType == "melee")
                    {
                        GetDamaged(disTextManager, player.EquippedWeapon.Damage);
                    }
                }
            }
        }

        private void HitPlayer(List<Player> players, Display_Text_Manager disTextManager)
        {
            foreach (Player player in players)
            {
                Rectangle npcRectangle = new Rectangle((int)Position.X, (int)Position.Y, Texture.Width, Texture.Height / NumFrames);
                Rectangle playerRectangle = new Rectangle((int)player.Position.X, (int)player.Position.Y, player.PlayerTexture.Width, player.PlayerTexture.Height);
                if (npcRectangle.Intersects(playerRectangle) && !player.IsImmune)
                {
                    player.GetDamaged(disTextManager, Damage);
                }
            }
        }
    }
}