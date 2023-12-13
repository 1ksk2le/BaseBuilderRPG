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
        public float healthMax { get; set; }
        public float knockBack { get; set; }
        public float knockBackRes { get; set; }
        public float targetRange { get; set; }
        public string texturePath { get; set; }
        public string name { get; set; }
        public bool isAlive { get; set; }
        public Vector2 position { get; set; }
        public Vector2 velocity { get; set; }

        public Vector2 origin, center, kbStartPos, kbEndPos;
        public Player target;
        public Rectangle rectangle;
        public float rotation, animationSpeed, immunityTime, health, immunityTimeMax, animationTimer, hitEffectTimer, hitEffectTimerMax;
        public float[] aiTimer;
        public int width, height, currentFrame;
        public bool isImmune, didSpawn;

        public float kbTimerMax = 0.25f;
        public float kbTimer = 0f;

        private NPCAI_Handler aiHandler;

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
            immunityTimeMax = 0.01f;
            immunityTime = 0f;
            hitEffectTimerMax = 0.75f;
            hitEffectTimer = 0f;
            isImmune = true;
            didSpawn = false;
            aiTimer = new float[5];
            aiHandler = new NPCAI_Handler(this);
        }

        public void Update(GameTime gameTime, List<Player> players, List<Projectile> projectiles, Text_Manager textManager, Item_Globals globalItem, Particle_Globals globalParticle, Projectile_Globals globalProjectile)
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
                aiHandler.ProcessAI(gameTime, players, projectiles, textManager, globalParticle, globalProjectile);
                isAlive = true;
            }
            else
            {
                Kill(globalItem, globalParticle);
                isAlive = false;
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (Main.drawDebugRectangles)
            {
                spriteBatch.DrawCircle(center, 4f, Color.Red * 1.5f, 64, 1f);
                spriteBatch.DrawRectangleBorder(rectangle, Color.Red, 1f, 0.01f);
                spriteBatch.DrawCircle(center, targetRange, Color.Red, 64, 0.011f);

                if (aiTimer[0] != 0f)
                {
                    spriteBatch.DrawStringWithOutline(Main.testFont, "AI 0: " + aiTimer[0].ToString("F2"), center + new Vector2(0, 30), Color.Black, Color.Aqua, 1f, 1f);
                }
                if (aiTimer[1] != 0f)
                {
                    spriteBatch.DrawStringWithOutline(Main.testFont, "AI 1: " + aiTimer[1].ToString("F2"), center + new Vector2(0, 40), Color.Black, Color.Aqua, 1f, 1f);
                }
                if (aiTimer[2] != 0f)
                {
                    spriteBatch.DrawStringWithOutline(Main.testFont, "AI 2: " + aiTimer[2].ToString("F2"), center + new Vector2(0, 50), Color.Black, Color.Aqua, 1f, 1f);
                }
                if (aiTimer[3] != 0f)
                {
                    spriteBatch.DrawStringWithOutline(Main.testFont, "AI 3: " + aiTimer[3].ToString("F2"), center + new Vector2(0, 60), Color.Black, Color.Aqua, 1f, 1f);
                }
                if (aiTimer[4] != 0f)
                {
                    spriteBatch.DrawStringWithOutline(Main.testFont, "AI 4: " + aiTimer[4].ToString("F2"), center + new Vector2(0, 70), Color.Black, Color.Aqua, 1f, 1f);
                }
                if (target != null)
                {
                    spriteBatch.DrawStringWithOutline(Main.testFont, "Target Position: " + target.position.ToString(), center + new Vector2(0, 80), Color.Black, Color.Aqua, 1f, 1f);
                }
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

                float levitationOffset = (float)Math.Sin(aiTimer[1] * levitationSpeed) * levitationAmplitude;

                float scale = 1.0f + 0.4f * levitationOffset;

                Rectangle sourceRect = new Rectangle(0, currentFrame * height, width, height);
                spriteBatch.Draw(texture, position + origin, sourceRect, npcColor, rotation, origin, scale, SpriteEffects.None, 0.69f);
            }
        }

        public void Kill(Item_Globals globalItem, Particle_Globals globalParticle)
        {
            health = -1;
        }
    }
}