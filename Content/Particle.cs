using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace BaseBuilderRPG.Content
{
    public class Particle
    {
        public Texture2D texture { get; set; }
        public int id { get; set; }
        public int ai { get; set; }
        public int currentFrame { get; set; }
        public int totalFrames { get; set; }
        public int width { get; set; }
        public int height { get; set; }
        public float lifeTimeMax { get; set; }
        public float lifeTime { get; set; }
        public float scale { get; set; }
        public float rotation { get; set; }
        public float rotationSpeed { get; set; }
        public string texturePath { get; set; }
        public string name { get; set; }
        public bool isAlive { get; set; }
        public Vector2 position { get; set; }
        public Vector2 velocity { get; set; }
        public Vector2 origin { get; set; }
        public Vector2 acceleration { get; set; }
        public Color color { get; set; }

        public Action<Particle, GameTime> customUpdate { get; set; }


        public Particle(Texture2D texture, string texturePath, string name, int id, int ai, Vector2 position, Vector2 velocity, Vector2 origin, float lifeTime, float scale, Color color, bool isAlive)
        {
            this.name = name;
            this.texture = texture;
            this.texturePath = texturePath;
            this.id = id;
            this.ai = ai;
            this.lifeTimeMax = lifeTime;
            this.velocity = velocity;
            this.position = position;
            this.scale = scale;
            this.origin = origin;
            this.isAlive = isAlive;
            this.lifeTime = 0f;
            this.color = color;

            rotation = 0f;
            acceleration = Vector2.Zero;
            currentFrame = 0;
            totalFrames = 1;
            width = texture?.Width ?? 0;
            height = texture?.Height ?? 0;
            customUpdate = null;
        }

        public void Update(GameTime gameTime)
        {
            if (isAlive)
            {
                if (lifeTime >= lifeTimeMax)
                {
                    Kill();
                }
                else
                {
                    lifeTime += (float)gameTime.ElapsedGameTime.TotalSeconds;

                    rotation += rotationSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;

                    position += Vector2.Transform(velocity, Matrix.CreateRotationZ(rotation))
                                * (float)gameTime.ElapsedGameTime.TotalSeconds;
                }
            }
        }


        public void Kill()
        {
            isAlive = false;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (texture != null && isAlive)
            {
                float alpha = 1.0f - (lifeTime / lifeTimeMax);
                alpha = MathHelper.Clamp(alpha, 0f, 1f);

                if (totalFrames > 1)
                {
                    int sourceX = currentFrame * width;
                    Rectangle sourceRectangle = new Rectangle(sourceX, 0, width, height);

                    spriteBatch.Draw(texture, position, sourceRectangle, color * alpha, rotation, origin, scale, SpriteEffects.None, 0);
                }
                else
                {
                    spriteBatch.Draw(texture, position, null, color * alpha, rotation, origin, scale, SpriteEffects.None, 0);
                }
            }
        }
    }
}