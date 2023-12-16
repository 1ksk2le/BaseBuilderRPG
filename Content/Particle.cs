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

                if (ai == 4)
                {
                    float ovalWidth = 4;
                    float ovalHeight = 1f;

                    float angle = lifeTime * 2f * MathHelper.Pi;

                    // Calculate the position on the oval using parametric equations
                    float x = ovalWidth * 0.5f * (float)Math.Cos(angle);
                    float y = ovalHeight * 0.5f * (float)Math.Sin(angle);

                    // Apply the rotation offset
                    Vector2 rotationOffset = new Vector2(x, y);
                    position += rotationOffset;
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
                if (ai == 0 || ai == 4)
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
                else if (ai == 1 || ai == 2)
                {
                    float scaleMultiplier = lifeTime / lifeTimeMax;
                    float trailingScaleMultiplier = lifeTime / lifeTimeMax;

                    int trailingParticlesCount = 10;

                    for (int i = 0; i < previousPositions.Count; i++)
                    {
                        float trailingScale = MathHelper.Lerp(scale, 0.01f, i / (float)trailingParticlesCount) * trailingScaleMultiplier;
                        spriteBatch.Draw(texture, previousPositions[i], null, color * (1.0f - scaleMultiplier), rotation, Vector2.Zero, trailingScale, SpriteEffects.None, 0);
                    }

                    float currentScale = MathHelper.Lerp(scale, 0.01f, scaleMultiplier);

                    spriteBatch.Draw(texture, position, null, color * (1.0f - scaleMultiplier), rotation, Vector2.Zero, currentScale, SpriteEffects.None, 0);
                }
                else if (ai == 3)
                {
                    float alpha = 1.0f - (lifeTime / lifeTimeMax);
                    alpha = MathHelper.Clamp(alpha, 0f, 1f);

                    float scaleFactor = 1.0f + alpha;
                    float currentScale = scale * scaleFactor;

                    if (totalFrames > 1)
                    {
                        int sourceX = currentFrame * width;
                        Rectangle sourceRectangle = new Rectangle(sourceX, 0, width, height);

                        spriteBatch.Draw(texture, position, sourceRectangle, color * alpha, rotation, origin, currentScale, SpriteEffects.None, 0);
                    }
                    else
                    {
                        spriteBatch.Draw(texture, position, null, color * alpha, rotation, origin, currentScale, SpriteEffects.None, 0);
                    }
                }
            }
        }
    }
}