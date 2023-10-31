using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace BaseBuilderRPG
{
    public class Projectile
    {
        public Texture2D Texture { get; set; }

        public int ID { get; set; }
        public int AI { get; set; }
        public int Damage { get; set; }
        public float LifeTime { get; set; }
        public float CurrentLifeTime { get; set; }
        public float KnockBack { get; set; }
        public float Speed { get; set; }
        public string TexturePath { get; set; }
        public string Name { get; set; }
        public bool IsAlive { get; set; }

        public Vector2 Position { get; set; }
        private Vector2 SpawnPosition;
        private Vector2 Target;
        private float Rotation;
        public Player Owner { get; set; }


        public Projectile(Texture2D texture, string texturePath, string name, int id, int ai, int damage, float lifeTime, float knockBack, Vector2 position, Player owner, bool isAlive)
        {
            CurrentLifeTime = 0f;
            Texture = texture;
            TexturePath = texturePath;
            ID = id;
            AI = ai;
            Name = name;
            Damage = damage;
            LifeTime = lifeTime;
            KnockBack = knockBack;
            Position = position;
            Owner = owner;
            MouseState mouseState = Mouse.GetState();
            Target = new Vector2(mouseState.X, mouseState.Y);
            SpawnPosition = Position;
            IsAlive = isAlive;
            SetDefaults();
        }
        private void SetDefaults()
        {

        }

        public void Update(GameTime gameTime)
        {
            if (CurrentLifeTime >= LifeTime)
            {
                IsAlive = false;
            }
            else
            {
                CurrentLifeTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
            }

            if (AI == 0)
            {
                Vector2 direction = Target - SpawnPosition;
                direction.Normalize();

                Position += direction * Speed;
                Rotation = (float)Math.Atan2(direction.Y, direction.X);
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (Texture != null && IsAlive)
            {
                spriteBatch.Begin();

                float scale = 1.0f;

                if (AI == 0)
                {
                    if (CurrentLifeTime < LifeTime / 2)
                    {
                        scale = MathHelper.Lerp(0.3f, 1f, CurrentLifeTime / (LifeTime / 2));
                    }
                    else
                    {
                        scale = MathHelper.Lerp(1f, 0.3f, (CurrentLifeTime - (LifeTime / 2)) / (LifeTime / 2));
                    }
                }

                spriteBatch.Draw(Texture, Position + new Vector2(0, 10), null, new Color(0, 0, 0, 150), Rotation, new Vector2(Texture.Width / 2, Texture.Height / 2), scale * 1.2f, SpriteEffects.None, 0);
                spriteBatch.Draw(Texture, Position, null, Color.White, Rotation, new Vector2(Texture.Width / 2, Texture.Height / 2), scale, SpriteEffects.None, 0);
                spriteBatch.End();
            }
        }

        public void RemoveProjectile(List<Projectile> projectiles)
        {
            projectiles.Remove(this);
        }

        public Projectile Clone(Player owner, bool isAlive)
        {
            return new Projectile(Texture, TexturePath, Name, ID, AI, Damage, LifeTime, KnockBack, Position, owner, isAlive);
        }
        public Projectile Clone(int id, int ai, int damage, Vector2 pos, Player owner, bool isAlive)
        {
            return new Projectile(Texture, TexturePath, Name, id, ai, damage, LifeTime, KnockBack, pos, owner, isAlive);
        }
        public Projectile Clone(int id, int damage, Vector2 pos, Player owner, bool isAlive)
        {
            return new Projectile(Texture, TexturePath, Name, id, AI, damage, LifeTime, KnockBack, pos, owner, isAlive);
        }
    }
}