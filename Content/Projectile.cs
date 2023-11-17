﻿using Microsoft.Xna.Framework;
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
        public float speed { get; set; }
        public string texturePath { get; set; }
        public string name { get; set; }
        public bool isAlive { get; set; }
        public Vector2 position { get; set; }

        private Vector2 spawnPosition;
        private Vector2 target;
        public Vector2 origin, center;
        public Rectangle rectangle;
        public int width, height;
        public float rotation;
        public bool didSpawn;


        public Projectile(Texture2D texture, string texturePath, int id, int ai, Vector2 position, Vector2 target, string name, int damage, int penetrate, float lifeTime, float knockBack, float speed, Player owner, bool isAlive, int width, int height)
        {
            this.texture = texture;
            this.texturePath = texturePath;
            this.id = id;
            this.ai = ai;
            this.name = name;
            this.damage = damage;
            this.lifeTimeMax = lifeTime;
            this.speed = speed;
            this.knockBack = knockBack;
            this.position = position;
            this.isAlive = isAlive;
            this.penetrate = penetrate;
            this.owner = owner;
            this.lifeTime = 0f;
            this.width = width;
            this.height = height;
            this.target = target;
            spawnPosition = this.position;
            didSpawn = false;
        }

        public void Update(GameTime gameTime, Projectile_Manager projManager)
        {
            if (!didSpawn)
            {
                origin = new Vector2(width / 2, height / 2);
                didSpawn = true;
            }
            center = position + origin;
            rectangle = new Rectangle((int)position.X, (int)position.Y, width, height);

            if (lifeTime >= lifeTimeMax)
            {
                Kill(projManager);
                isAlive = false;
            }
            else
            {
                lifeTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
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
                    spriteBatch.Draw(texture, position + origin, null, Color.White, rotation, origin, scale, SpriteEffects.None, 0);
                }



                if (Main.drawDebugRectangles)
                {
                    spriteBatch.DrawCircle(center, 4f, Color.Lime * 1.5f, 64, 1f);
                    spriteBatch.DrawRectangleWithBorder(rectangle, Color.Lime, 1f, 0.01f);
                }
            }
        }
    }
}