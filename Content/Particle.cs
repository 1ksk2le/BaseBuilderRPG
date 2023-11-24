using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

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
        public Vector2 velocity;
        public Vector2 origin { get; set; }
        public Vector2 acceleration { get; set; }
        public Color color { get; set; }
        public Color startColor { get; set; }
        public Color endColor { get; set; }

        public Action<Particle, GameTime> customUpdate { get; set; }

        private List<Vector2> previousPositions;


        public Particle(Texture2D texture, string texturePath, string name, int id, int ai, Vector2 position, Vector2 velocity, Vector2 origin, float lifeTime, float scale, Color color, Color startColor, Color endColor, bool isAlive)
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
            this.startColor = startColor;
            this.endColor = endColor;

            rotation = 0f;
            acceleration = Vector2.Zero;
            currentFrame = 0;
            totalFrames = 1;
            width = texture?.Width / 2 ?? 0;
            height = texture?.Height / 2 ?? 0;
            customUpdate = null;
            previousPositions = new List<Vector2>();
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

                    previousPositions.Insert(0, position);

                    if (previousPositions.Count > 10)
                    {
                        previousPositions.RemoveAt(previousPositions.Count - 1);
                    }
                    float lerpAmount = MathHelper.Clamp(lifeTime / lifeTimeMax, 0f, 1f);
                    color = Color.Lerp(startColor, endColor, lerpAmount);
                }

                if (ai == 1)
                {
                    velocity.Y += 3f;
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
                if (ai == 0)
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
                else if (ai == 1)
                {
                    float alpha = 1.0f - (lifeTime / lifeTimeMax);
                    alpha = MathHelper.Clamp(alpha, 0f, 1f);
                    int trailingParticlesCount = 8;
                    for (int i = 0; i < previousPositions.Count; i++)
                    {
                        float trailingScale = MathHelper.Lerp(scale, 0.1f, i / (float)trailingParticlesCount);

                        spriteBatch.Draw(texture, previousPositions[i], null, color * alpha, rotation, Vector2.Zero, trailingScale, SpriteEffects.None, 0);
                    }

                    spriteBatch.Draw(texture, position, null, color * alpha, rotation, Vector2.Zero, scale, SpriteEffects.None, 0);
                }
            }
        }
    }
}