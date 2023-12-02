using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace BaseBuilderRPG.Content
{
    public class Player_VisualHandler
    {
        private Player player;
        private Color skinColor;

        public Player_VisualHandler(Player player)
        {
            this.player = player;
            this.skinColor = GetSkinColor(player.skinColorFloat);
        }

        private Color GetHitColor(bool armorPiece)
        {
            return armorPiece ? Color.Lerp(Color.White, Color.Red, player.hitEffectTimer) : Color.Lerp(skinColor, Color.Red, player.hitEffectTimer);
        }
        private Color GetSkinColor(float progress)
        {
            Color black = new Color(94, 54, 33);
            Color white = new Color(255, 220, 185);
            return Color.Lerp(black, white, progress);
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            DrawDebugInformation(spriteBatch);
            PreDraw(spriteBatch);
            PostDraw(gameTime, spriteBatch, HeadRotation());

            if (player.hasMovementOrder)
            {
                spriteBatch.DrawCircle(player.targetMovement, 8f, Color.Lime, 64, 0.012f);
            }

            Color nameColor;

            if (player.isPicked)
            {
                nameColor = Color.Aqua;
            }
            else if (player.rectangle.Contains(Input_Manager.Instance.mousePosition))
            {
                nameColor = Color.Lime;
            }
            else
            {
                nameColor = Color.White;
            }

            Vector2 textPosition = player.position + new Vector2(0, -14);
            textPosition.X = player.position.X + player.width / 2 - Main.testFont.MeasureString(player.name).X / 2;
            spriteBatch.DrawStringWithOutline(Main.testFont, player.name, textPosition, Color.Black, player.isControlled ? Color.Yellow : nameColor, 1f, player.isControlled ? 0.8616f : 0.7616f);

            SpriteEffects eff = (player.direction == 1) ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
            Vector2 headOrigin = new Vector2(player.textureHead.Width / 2, player.textureHead.Height);
            Vector2 eyesOrigin = new Vector2(player.textureEye.Width / 2, (player.textureEye.Height) / 2);
            Rectangle eyesSourceRectangle = new Rectangle(0, player.eyeTimer > 0f ? 24 : 0, player.textureEye.Width, player.textureEye.Height / 2);

            spriteBatch.Draw(player.textureBody, player.position, null, GetHitColor(false), 0f, Vector2.Zero, 1f, eff, player.isControlled ? 0.851f : 0.751f);
            spriteBatch.Draw(player.textureHead, player.position + headOrigin, null, GetHitColor(false), HeadRotation(), headOrigin, 1f, eff, (player.isControlled) ? 0.8512f : 0.7512f);
            spriteBatch.Draw(player.textureEye, player.position + eyesOrigin, eyesSourceRectangle, GetHitColor(true), HeadRotation(), eyesOrigin, 1f, eff, (player.isControlled) ? 0.8513f : 0.7513f);

            if (player.isControlled && player.inventoryVisible)
            {
                player.inventory.Draw(spriteBatch, player);
            }
        }

        public void PreDraw(SpriteBatch spriteBatch)
        {
            if (player.equippedWeapon != null)
            {
                if (player.equippedWeapon.weaponType == "One Handed Sword" && player.useTimer <= 0)
                {
                    float rotation = (player.direction == 1) ? -70 * MathHelper.Pi / 180 : 250 * MathHelper.Pi / 180; //110 -290
                    Vector2 origin = (player.direction == 1) ? new Vector2(0, player.equippedWeapon.texture.Height) : new Vector2(0, 0);
                    spriteBatch.Draw(player.equippedWeapon.texture, player.position + new Vector2(player.direction == 1 ? player.width : 0, player.height / 2 + player.equippedWeapon.texture.Height / 4), null, Color.White, rotation, origin, 1f, (player.direction == 1) ? SpriteEffects.None : SpriteEffects.FlipVertically, player.isControlled ? 0.841f : 0.741f);
                }
                if (player.equippedWeapon.weaponType == "One Handed Wand")
                {
                    float rotation = BowRotation();
                    Vector2 origin = new Vector2(player.equippedWeapon.texture.Width / 2, player.equippedWeapon.texture.Height);
                    spriteBatch.Draw(player.equippedWeapon.texture, player.center + new Vector2(player.equippedWeapon.texture.Width / 2 * player.direction, player.equippedWeapon.texture.Width / 2), null, Color.White, (float)(rotation + MathHelper.Pi / 2), origin, 1f, (player.direction == 1) ? SpriteEffects.None : SpriteEffects.FlipHorizontally, player.isControlled ? 0.8515f : 0.7515f);
                }
                if (player.equippedWeapon.weaponType == "Bow")
                {
                    Vector2 origin = new Vector2(player.equippedWeapon.texture.Width / 2, player.equippedWeapon.texture.Height / 2);
                    Vector2 drawPos = BowDrawPosition();
                    float drawRotation = BowRotation();

                    spriteBatch.Draw(player.equippedWeapon.texture, drawPos, null, Color.White, drawRotation, origin, 1f, (player.direction == 1) ? SpriteEffects.None : SpriteEffects.None, player.isControlled ? 0.8515f : 0.7515f);

                    Vector2 topOfBow = drawPos - new Vector2(player.equippedWeapon.texture.Width / 2 - 2, player.equippedWeapon.texture.Height / 2 - 2);
                    Vector2 bottomOfBow = drawPos + new Vector2(-player.equippedWeapon.texture.Width / 2 + 2, player.equippedWeapon.texture.Height / 2 - 2);

                    float progress = player.useTimer / player.equippedWeapon.useTime;

                    Vector2 middleOfBow = Vector2.Lerp(topOfBow, bottomOfBow, 0.5f);

                    Vector2 rotatedTopOfBow = Vector2.Transform(topOfBow - drawPos, Matrix.CreateRotationZ(drawRotation)) + drawPos;
                    Vector2 rotatedBottomOfBow = Vector2.Transform(bottomOfBow - drawPos, Matrix.CreateRotationZ(drawRotation)) + drawPos;
                    Vector2 rotatedMiddleOfBow = Vector2.Transform(middleOfBow - drawPos, Matrix.CreateRotationZ(drawRotation)) + drawPos;

                    rotatedMiddleOfBow = Vector2.Lerp(player.center, rotatedMiddleOfBow, progress);

                    Color startColor = player.equippedWeapon.stringSColor;
                    Color endColor = player.equippedWeapon.stringEColor;

                    spriteBatch.DrawGradientLine(rotatedTopOfBow, rotatedMiddleOfBow, startColor, endColor, player.isControlled ? 0.85149f : 0.75149f);
                    spriteBatch.DrawGradientLine(rotatedMiddleOfBow, rotatedBottomOfBow, endColor, startColor, player.isControlled ? 0.85149f : 0.75149f);
                }

            }
        }
        public void PostDraw(GameTime gameTime, SpriteBatch spriteBatch, float headRot)
        {
            if (player.health <= player.maxHealth)
            {
                DrawHealthBar(spriteBatch);
            }

            SpriteEffects eff = (player.direction == 1) ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
            if (player.equippedOffhand != null)
            {
                if (player.equippedOffhand.id == 13)
                {
                    float offsetY = 5.0f; // Adjust this value to control the amplitude of the motion
                    float oscillationSpeed = 2.0f; // Adjust this value to control the speed of the oscillation

                    float verticalOffset = offsetY * (float)Math.Sin(gameTime.TotalGameTime.TotalSeconds * oscillationSpeed);

                    Vector2 drawPosition = player.center + new Vector2(player.equippedOffhand.texture.Width * -player.direction, -player.equippedOffhand.texture.Height);
                    drawPosition.Y += verticalOffset;

                    spriteBatch.Draw(player.equippedOffhand.texture, drawPosition, null, GetHitColor(true), 0f, new Vector2(player.equippedOffhand.texture.Width / 2, player.equippedOffhand.texture.Height / 2), 1f, SpriteEffects.None, player.isControlled ? 0.8516f : 0.7516f);
                }

                else
                {
                    spriteBatch.Draw(player.equippedOffhand.texture, player.center + new Vector2(player.equippedOffhand.texture.Width / 2.5f * -player.direction, player.equippedOffhand.texture.Height / 2), null, GetHitColor(true), 0f, new Vector2(player.equippedOffhand.texture.Width / 2, player.equippedOffhand.texture.Height / 2), 1f, SpriteEffects.None, player.isControlled ? 0.8516f : 0.7516f);
                }
            }
            if (player.equippedBodyArmor != null)
            {
                spriteBatch.Draw(player.equippedBodyArmor.texture, player.position + new Vector2(0, 22), null, GetHitColor(true), 0f, Vector2.Zero, 1f, eff, player.isControlled ? 0.8511f : 0.7511f);
            }
            if (player.equippedHeadArmor != null)
            {
                Vector2 headOrigin = new Vector2(player.equippedHeadArmor.texture.Width / 2, player.equippedHeadArmor.texture.Height);
                spriteBatch.Draw(player.equippedHeadArmor.texture, player.position + new Vector2(player.textureHead.Width / 2, player.textureHead.Height), null, GetHitColor(true), headRot, headOrigin, 1f, eff, player.isControlled ? 0.8514f : 0.7514f);
            }
        }

        public void ParticleEffects(Particle_Globals globalParticle)
        {
            WeaponVisuals(globalParticle);
        }

        private void WeaponVisuals(Particle_Globals globalParticle)
        {
            if (player.equippedWeapon != null)
            {
                if (player.equippedWeapon.id == 4)
                {
                    for (int i = 0; i < 16; i++)
                    {
                        if (Main.random.Next(25) == 0)
                        {
                            float rotation = BowRotation();
                            float radius = player.equippedWeapon.texture.Height;
                            float circularMotionX = (float)Math.Cos(rotation) * radius;
                            float circularMotionY = (float)Math.Sin(rotation) * radius;

                            Vector2 pos = player.center + new Vector2((player.equippedWeapon.texture.Height / 2 + 10) * player.direction, player.equippedWeapon.texture.Width / 2 - 5);
                            globalParticle.NewParticle(3, 0, new Vector2(circularMotionX, circularMotionY) + player.center + new Vector2(Main.random.Next(-5, 5), Main.random.Next(-5, 5)), new Vector2(0, Main.random.Next(-30, -15)), Vector2.Zero, 0f, 0.6f, 0.2f + Main.random.NextFloat(0.5f, 2.5f), Color.Wheat, Color.OrangeRed, Color.White);
                            if (Main.random.Next(25) == 0)
                            {
                                globalParticle.NewParticle(1, 1, player.center + new Vector2(circularMotionX, circularMotionY) + new Vector2(Main.random.Next(-5, 5), Main.random.Next(-5, 20)), new Vector2(Main.random.Next(-5, 5), Main.random.Next(-110, -40)), Vector2.Zero, 0f, 1.2f, 2f + Main.random.NextFloat(0.5f, 3f), Color.Wheat, Color.OrangeRed, Color.White);
                            }
                        }
                    }
                }
                if (player.equippedWeapon.prefixName == "Magical")
                {
                    if (player.equippedWeapon.weaponType == "One Handed Sword" && player.useTimer <= 0)
                    {
                        Vector2 pos = player.position + new Vector2(player.direction == 1 ? player.width : 0, player.height / 2 - player.equippedWeapon.texture.Width / 2);

                        for (int i = 0; i < player.equippedWeapon.texture.Height / 2; i++)
                        {
                            if (Main.random.Next(150) == 0)
                            {
                                globalParticle.NewParticle(3, 0,
                                    pos + (player.direction == 1 ? new Vector2(Main.random.Next(-player.equippedWeapon.texture.Height, 0), Main.random.Next(-player.equippedWeapon.texture.Width / 2, player.equippedWeapon.texture.Width / 2)) : new Vector2(Main.random.Next(0, player.equippedWeapon.texture.Height), Main.random.Next(-player.equippedWeapon.texture.Width / 2, player.equippedWeapon.texture.Width / 2))),
                                    new Vector2(Main.random.Next(-50, -10) * player.direction, Main.random.Next(-20, 20)), Vector2.Zero, 0f, 0.8f, 0.45f + Main.random.NextFloat(0.5f, 1f), Color.Wheat, Color.Lime, Color.Yellow);
                            }
                            if (Main.random.Next(150) == 0)
                            {
                                globalParticle.NewParticle(3, 0,
                                    pos + (player.direction == 1 ? new Vector2(Main.random.Next(-player.equippedWeapon.texture.Height, 0), Main.random.Next(-player.equippedWeapon.texture.Width / 2, player.equippedWeapon.texture.Width / 2)) : new Vector2(Main.random.Next(0, player.equippedWeapon.texture.Height), Main.random.Next(-player.equippedWeapon.texture.Width / 2, player.equippedWeapon.texture.Width / 2))),
                                    new Vector2(Main.random.Next(-50, -10) * player.direction, Main.random.Next(-20, 20)), Vector2.Zero, 0f, 0.8f, 0.45f + Main.random.NextFloat(0.5f, 1f), Color.Wheat, Color.Aqua, Color.Magenta);
                            }
                            if (Main.random.Next(150) == 0)
                            {
                                globalParticle.NewParticle(3, 1,
                                    pos + (player.direction == 1 ? new Vector2(Main.random.Next(-player.equippedWeapon.texture.Height, 0), Main.random.Next(-player.equippedWeapon.texture.Width / 2, player.equippedWeapon.texture.Width / 2)) : new Vector2(Main.random.Next(0, player.equippedWeapon.texture.Height), Main.random.Next(-player.equippedWeapon.texture.Width / 2, player.equippedWeapon.texture.Width / 2))),
                                    new Vector2(Main.random.Next(-50, -30) * player.direction, Main.random.Next(-10, 10)), Vector2.Zero, 2f * player.direction * Main.random.Next(-2, 2), 0.8f, 2f * Main.random.NextFloat(0.2f, 0.8f), Color.Wheat, Color.Aqua, Color.Magenta);
                            }
                        }
                    }
                    if (player.equippedWeapon.weaponType == "Bow")
                    {
                        float rotation = BowRotation();
                        float radius = player.equippedWeapon.texture.Width * 1.5f;
                        float circularMotionX = (float)Math.Cos(rotation) * radius;
                        float circularMotionY = (float)Math.Sin(rotation) * radius;

                        Vector2 weaponDrawPosition = player.center + new Vector2(circularMotionX, circularMotionY);

                        for (int i = 0; i < 16; i++)
                        {
                            Vector2 particlePosition = weaponDrawPosition + new Vector2(Main.random.Next(-5, 5), Main.random.Next(-5, 5));

                            Vector2 particleVelocity = new Vector2(-player.equippedWeapon.texture.Height / 2, Main.random.Next(-50, 50));
                            particleVelocity = Vector2.Transform(particleVelocity, Matrix.CreateRotationZ(rotation));

                            if (Main.random.Next(75) == 0)
                            {
                                globalParticle.NewParticle(3, 0,
                                    particlePosition,
                                    particleVelocity, Vector2.Zero, 0f, 0.8f, 0.45f + Main.random.NextFloat(0.5f, 1f), Color.Wheat, Color.Lime, Color.Yellow);
                            }
                            if (Main.random.Next(75) == 0)
                            {
                                globalParticle.NewParticle(3, 0,
                                    particlePosition,
                                    particleVelocity, Vector2.Zero, 0f, 0.8f, 0.45f + Main.random.NextFloat(0.5f, 1f), Color.Wheat, Color.Aqua, Color.Magenta);
                            }
                            if (Main.random.Next(100) == 0)
                            {
                                globalParticle.NewParticle(3, 1,
                                    particlePosition,
                                    particleVelocity, Vector2.Zero, -1f * player.direction * Main.random.Next(-2, 2), 0.8f, 2f * Main.random.NextFloat(0.2f, 0.8f), Color.Wheat, Color.Aqua, Color.Magenta);
                            }
                        }
                    }
                    if (player.equippedWeapon.weaponType == "One Handed Wand")
                    {
                        float rotation = BowRotation() + MathHelper.Pi;
                        float radius = player.equippedWeapon.texture.Height;
                        float circularMotionX = (float)Math.Cos(rotation) * radius;
                        float circularMotionY = (float)Math.Sin(rotation) * radius;

                        Vector2 weaponDrawPosition = player.center + new Vector2(circularMotionX, circularMotionY);

                        for (int i = 0; i < 16; i++)
                        {
                            Vector2 particlePosition = weaponDrawPosition + new Vector2(Main.random.Next(player.equippedWeapon.texture.Height), Main.random.Next(-5, 5));
                            Vector2 particleVelocity = new Vector2(-player.equippedWeapon.texture.Height / 2, Main.random.Next(-50, 50));
                            particleVelocity = Vector2.Transform(particleVelocity, Matrix.CreateRotationZ(rotation));

                            if (Main.random.Next(1) == 0)
                            {
                                globalParticle.NewParticle(3, 0,
                                    particlePosition,
                                    particleVelocity, Vector2.Zero, 0f, 0.8f, 0.45f + Main.random.NextFloat(0.5f, 1f), Color.Wheat, Color.Lime, Color.Yellow);
                            }
                            if (Main.random.Next(75) == 0)
                            {
                                globalParticle.NewParticle(3, 0,
                                    particlePosition,
                                    particleVelocity, Vector2.Zero, 0f, 0.8f, 0.45f + Main.random.NextFloat(0.5f, 1f), Color.Wheat, Color.Aqua, Color.Magenta);
                            }
                            if (Main.random.Next(100) == 0)
                            {
                                globalParticle.NewParticle(3, 1,
                                    particlePosition,
                                    particleVelocity, Vector2.Zero, -1f * player.direction * Main.random.Next(-2, 2), 0.8f, 2f * Main.random.NextFloat(0.2f, 0.8f), Color.Wheat, Color.Aqua, Color.Magenta);
                            }
                        }
                    }
                }
            }
        }

        private void DrawDebugInformation(SpriteBatch spriteBatch)
        {
            if (Main.drawDebugRectangles)
            {
                Vector2 textPosition2 = player.position + new Vector2(0, player.height + 20);
                textPosition2.X = player.position.X + player.width / 2 - Main.testFont.MeasureString(player.aiState).X / 2;

                Vector2 textPosition3 = player.center + new Vector2(0, player.height);
                textPosition3.X = player.center.X + player.width / 2 - Main.testFont.MeasureString("Controlled by AI").X / 2;

                spriteBatch.DrawStringWithOutline(Main.testFont, player.aiState, textPosition2, Color.Black, Color.White, 1f, player.isControlled ? 0.8617f : 0.7617f);
                spriteBatch.DrawStringWithOutline(Main.testFont, (!player.isControlled) ? "Controlled by AI" : "", textPosition3 + new Vector2(0, -20), Color.Black, Color.White, 1f, player.isControlled ? 0.8617f : 0.7617f);

                if (player.equippedWeapon != null)
                {
                    if (player.equippedWeapon.damageType == "melee")
                    {
                        spriteBatch.DrawCircle(player.center, player.meleeRange, Color.Cyan, 64, 0.012f);
                        spriteBatch.DrawRectangleBorder(player.rectangleMelee, Color.Cyan, 1f, 0.011f);
                    }
                    else if (player.equippedWeapon.damageType == "ranged" || player.equippedWeapon.damageType == "magic")
                    {
                        spriteBatch.DrawCircle(player.center, player.rangedRange, Color.Cyan, 64, 0.012f);
                    }
                }
                if (player.hasMovementOrder)
                {
                    spriteBatch.DrawLine(player.center, player.targetMovement, Color.Indigo, 0.013f);
                }
                spriteBatch.DrawCircle(player.center, 4f, Color.Blue * 1.5f, 64, 1f);
                spriteBatch.DrawRectangleBorder(player.rectangle, Color.Blue, 1f, 0.012f);
                if (!player.isControlled && player.target != null)
                {
                    spriteBatch.DrawLine(player.center, player.target.center, Color.Blue, 0.013f);
                }
            }
        }
        private void DrawHealthBar(SpriteBatch spriteBatch)
        {
            float healthBarWidth = player.width * ((float)player.health / (float)player.maxHealth);
            int offSetY = 6;

            Rectangle healthBarRectangleBackground = new Rectangle((int)(player.position.X - 2), (int)(player.position.Y + player.height + offSetY - 1), player.width + 4, 4);
            Rectangle healthBarRectangleBackgroundRed = new Rectangle((int)(player.position.X), (int)(player.position.Y + player.height + offSetY), player.width, 2);
            Rectangle healthBarRectangle = new Rectangle((int)(player.position.X), (int)player.position.Y + player.height + offSetY, (int)healthBarWidth, 2);

            if (player.health < player.maxHealth)
            {
                spriteBatch.DrawRectangle(healthBarRectangleBackground, Color.Black, player.isControlled ? 0.8613f : 0.7613f);
                spriteBatch.DrawRectangle(healthBarRectangleBackgroundRed, Color.Red, player.isControlled ? 0.8614f : 0.7614f);
                spriteBatch.DrawRectangle(healthBarRectangle, Color.Lime, player.isControlled ? 0.8615f : 0.7615f);
            }
        }
        private float HeadRotation()
        {
            Vector2 directionToMouse = Input_Manager.Instance.mousePosition - player.position;

            float maxHeadRotation = MathHelper.ToRadians(20);
            float rotation;

            if (player.isControlled)
            {
                rotation = (float)Math.Atan2(directionToMouse.Y * player.direction, directionToMouse.X * player.direction);
                rotation = MathHelper.Clamp(rotation, -maxHeadRotation, maxHeadRotation);
            }
            else
            {
                if (player.hasMovementOrder)
                {
                    Vector2 targetDirection = player.targetMovement - player.center;
                    rotation = (float)Math.Atan2(targetDirection.Y, targetDirection.X) * player.direction;
                    rotation = MathHelper.Clamp(rotation, -maxHeadRotation, maxHeadRotation);
                }
                else
                {
                    if (player.target != null)
                    {
                        Vector2 targetDirection = player.target.center - player.center;
                        rotation = (float)Math.Atan2(targetDirection.Y, targetDirection.X) * player.direction;
                        rotation = MathHelper.Clamp(rotation, -maxHeadRotation, maxHeadRotation);
                    }
                    else
                    {
                        rotation = 0f;
                    }
                }
            }
            return rotation;
        }

        private float BowRotation()
        {
            Vector2 directionToMouse = Input_Manager.Instance.mousePosition - player.center;
            float rotation;
            if (player.isControlled)
            {
                rotation = (float)Math.Atan2(directionToMouse.Y, directionToMouse.X);
            }
            else
            {
                if (player.target != null)
                {
                    Vector2 targetDirection = player.target.center - player.center;
                    rotation = (float)Math.Atan2(targetDirection.Y, targetDirection.X);
                }
                else
                {
                    rotation = player.direction == 1 ? 0 : MathHelper.Pi;
                }
            }
            return rotation;
        }
        public Vector2 BowDrawPosition()
        {
            float rotation = BowRotation();
            float circularMotionX = (float)Math.Cos(rotation) * player.equippedWeapon.texture.Height / 2;
            float circularMotionY = (float)Math.Sin(rotation) * player.equippedWeapon.texture.Height / 2;

            Vector2 weaponDrawPosition = player.center + new Vector2(circularMotionX, circularMotionY);

            return weaponDrawPosition;
        }
    }
}
