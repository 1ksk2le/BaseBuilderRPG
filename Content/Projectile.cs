using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace BaseBuilderRPG.Content
{
    public class Projectile
    {
        public Player owner { get; set; }
        public Texture2D texture { get; set; }
        public int id { get; set; }
        public int width { get; set; }
        public int height { get; set; }
        public int penetrate { get; set; }
        public int ai { get; set; }
        public int damage { get; set; }
        public float lifeTime { get; set; }
        public float lifeTimeCurrent { get; set; }
        public float knockBack { get; set; }
        public float speed { get; set; }
        public string texturePath { get; set; }
        public string name { get; set; }
        public bool isAlive { get; set; }
        public Vector2 position { get; set; }

        private Vector2 spawnPosition;
        private Vector2 target;
        public float rotation;

        public Projectile(Texture2D texture, string texturePath, int id, int ai, Vector2 position, string name, int damage, int penetrate, float lifeTime, float knockBack, float speed, Player owner, bool isAlive)
        {
            this.texture = texture;
            this.texturePath = texturePath;
            this.id = id;
            this.ai = ai;
            this.name = name;
            this.damage = damage;
            this.lifeTime = lifeTime;
            this.speed = speed;
            this.knockBack = knockBack;
            this.position = position;
            this.isAlive = isAlive;
            this.penetrate = penetrate;
            this.owner = owner;

            MouseState mouseState = Mouse.GetState();
            lifeTimeCurrent = 0f;
            target = new Vector2(mouseState.X, mouseState.Y);
            spawnPosition = this.position;

        }

        public void Update(GameTime gameTime, Projectile_Manager projManager)
        {
            if (lifeTimeCurrent >= lifeTime)
            {
                Kill(projManager);
                isAlive = false;
            }
            else
            {
                lifeTimeCurrent += (float)gameTime.ElapsedGameTime.TotalSeconds;
            }

            if (penetrate < 0)
            {
                Kill(projManager);
                isAlive = false;
            }

            if (ai == 0 || ai == 1)
            {
                Vector2 direction = target - spawnPosition;
                direction.Normalize();

                position += direction * speed;
                rotation = (float)Math.Atan2(direction.Y, direction.X);
            }
        }

        public void Kill(Projectile_Manager projManager)
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
                    if (lifeTimeCurrent < lifeTime / 2)
                    {
                        scale = MathHelper.Lerp(0.3f, 1f, lifeTimeCurrent / (lifeTime / 2));
                    }
                    else
                    {
                        scale = MathHelper.Lerp(1f, 0.3f, (lifeTimeCurrent - lifeTime / 2) / (lifeTime / 2));
                    }
                }

                //spriteBatch.Draw(p.Texture, p.Position + new Vector2(0, 10), null, new Color(0, 0, 0, 200), p.Rotation, new Vector2(p.Texture.Width / 2, p.Texture.Height / 2), scale * 1.2f, SpriteEffects.None, 0);
                spriteBatch.Draw(texture, position, null, Color.White, rotation, new Vector2(texture.Width / 2, texture.Height / 2), scale, SpriteEffects.None, 0);
            }
        }
    }
}