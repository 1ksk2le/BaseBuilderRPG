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
        public float Rotation, ImmunityTime, Health, MaxImmunityTime, AI_One, AI_Two, AI_Three;
        public int Width, Height;
        public bool IsImmune;

        private float AnimationTimer;
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
            MaxImmunityTime = 0.2f;
            ImmunityTime = MaxImmunityTime;
            IsImmune = true;
            AI_One = 0f;
            AI_Two = 0f;
            AI_Three = 0f;
        }

        public void Update(GameTime gameTime)
        {
            if (Health >= 0)
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

                AnimationTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;

                if (AnimationTimer >= 0.1f)
                {
                    CurrentFrame = (CurrentFrame + 1) % NumFrames;

                    AnimationTimer = 0f;
                }
                ProcessAI(gameTime);
                IsAlive = true;
            }
            else
            {
                IsAlive = false;
            }
        }

        public void ProcessAI(GameTime gameTime)
        {
            var tick = (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (AI == 1)
            {
                AI_One += tick;

                Vector2 direction = Target - Position;
                direction.Normalize();

                if (AI_One > 3f)
                {
                    AI_Two += tick;
                    if (AI_Two > 2f)
                    {
                        AI_Three += tick;
                        Position += direction * 1f;
                        if (AI_Two > 4f)
                        {
                            AI_One = 0f;
                            AI_Two = 0f;
                            AI_Three = 0f;
                        }
                    }
                }
                else
                {
                    Position += direction * 0.25f;
                }
            }

        }
        public void Kill()
        {

        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (AI == 1)
            {
                float levitationSpeed = 3f;
                float levitationAmplitude = 1f;

                float levitationOffset = (float)Math.Sin(AI_Three * levitationSpeed) * levitationAmplitude;

                float scale = 1.0f + 0.2f * levitationOffset;

                Rectangle sourceRect = new Rectangle(0, CurrentFrame * Height, Width, Height);
                Rectangle destinationRect = new Rectangle((int)Position.X, (int)Position.Y, Width, Height);
                //spriteBatch.Draw(Texture, destinationRect, sourceRect, Color.White);
                spriteBatch.Draw(Texture, Position, sourceRect, Color.White, Rotation, Vector2.Zero, scale, SpriteEffects.None, 0.69f);
            }
        }

        public void KillNPC(List<NPC> npcs)
        {
            npcs.Remove(this);
        }
    }
}