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

        public void Draw(SpriteBatch spriteBatch)
        {
            DrawDebugInformation(spriteBatch);
            PreDraw(spriteBatch);
            PostDraw(spriteBatch, HeadRotation());

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
                    float rotation = (player.direction == 1) ? 110 * MathHelper.Pi / 180 : -290 * MathHelper.Pi / 180;
                    Vector2 origin = (player.direction == 1) ? new Vector2(0, player.equippedWeapon.texture.Height) : new Vector2(0, 0);
                    spriteBatch.Draw(player.equippedWeapon.texture, player.position + new Vector2(player.direction == 1 ? player.width : 0, player.height / 2), null, Color.White, rotation, origin, 1f, (player.direction == 1) ? SpriteEffects.None : SpriteEffects.FlipVertically, player.isControlled ? 0.841f : 0.741f);
                }
            }
        }

        public void PostDraw(SpriteBatch spriteBatch, float headRot)
        {
            if (player.health <= player.maxHealth)
            {
                DrawHealthBar(spriteBatch);
            }

            SpriteEffects eff = (player.direction == 1) ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
            if (player.equippedOffhand != null)
            {
                spriteBatch.Draw(player.equippedOffhand.texture, player.position + new Vector2(player.direction == 1 ? 0 : 22, player.height / 1.2f), null, GetHitColor(true), 0f, player.origin, 1f, SpriteEffects.None, player.isControlled ? 0.8515f : 0.7515f);
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
            if (player.equippedWeapon != null && player.useTimer <= 0)
            {
                if (player.equippedWeapon.id == 4)
                {
                    for (int i = 0; i < 16; i++)
                    {
                        if (Main.random.Next(25) == 0)
                        {
                            Vector2 posAdjuster = new Vector2((player.equippedWeapon.texture.Height + 5) * player.direction, (player.equippedWeapon.texture.Width + 5) / 2);
                            globalParticle.NewParticle(3, 0, player.center + posAdjuster + new Vector2(Main.random.Next(-5, 5), Main.random.Next(-5, 5)), new Vector2(0, Main.random.Next(-30, -15)), Vector2.Zero, 0f, 0.6f, 0.2f + Main.random.NextFloat(0.5f, 2.5f), Color.Wheat, Color.OrangeRed, Color.White);
                            if (Main.random.Next(25) == 0)
                            {
                                globalParticle.NewParticle(1, 1, player.center + posAdjuster + new Vector2(Main.random.Next(-5, 5), Main.random.Next(-5, 5)), new Vector2(Main.random.Next(-20, 20), Main.random.Next(-110, -40)), Vector2.Zero, 0f, 1.2f, 2f + Main.random.NextFloat(0.5f, 3f), Color.Wheat, Color.OrangeRed, Color.White);
                            }
                        }
                    }
                }
                if (player.equippedWeapon.prefixName == "Magical")
                {
                    Vector2 posAdjuster = new Vector2((player.equippedWeapon.texture.Height + 5) * player.direction, (player.equippedWeapon.texture.Width));

                    for (int i = 0; i < player.equippedWeapon.texture.Height / 2; i++)
                    {
                        if (Main.random.Next(150) == 0)
                        {
                            globalParticle.NewParticle(3, 0,
                                (player.direction == 1) ? player.center + posAdjuster + new Vector2(Main.random.Next(-player.equippedWeapon.texture.Height + 16, 0), Main.random.Next(-player.equippedWeapon.texture.Width / 2, player.equippedWeapon.texture.Width / 2)) : player.center + posAdjuster + new Vector2(Main.random.Next(0, player.equippedWeapon.texture.Height - 16), Main.random.Next(-player.equippedWeapon.texture.Width / 2, player.equippedWeapon.texture.Width / 2)),
                                new Vector2(Main.random.Next(-50, -10) * player.direction, Main.random.Next(-20, 20)), Vector2.Zero, 0f, 0.8f, 0.45f + Main.random.NextFloat(0.5f, 1f), Color.Wheat, Color.Lime, Color.Yellow);
                        }
                        if (Main.random.Next(150) == 0)
                        {
                            globalParticle.NewParticle(3, 0,
                                (player.direction == 1) ? player.center + posAdjuster + new Vector2(Main.random.Next(-player.equippedWeapon.texture.Height + 16, 0), Main.random.Next(-player.equippedWeapon.texture.Width / 2, player.equippedWeapon.texture.Width / 2)) : player.center + posAdjuster + new Vector2(Main.random.Next(0, player.equippedWeapon.texture.Height - 16), Main.random.Next(-player.equippedWeapon.texture.Width / 2, player.equippedWeapon.texture.Width / 2)),
                                new Vector2(Main.random.Next(-50, -10) * player.direction, Main.random.Next(-20, 20)), Vector2.Zero, 0f, 0.8f, 0.45f + Main.random.NextFloat(0.5f, 1f), Color.Wheat, Color.Aqua, Color.Magenta);
                        }
                        if (Main.random.Next(150) == 0)
                        {
                            globalParticle.NewParticle(3, 1,
                                (player.direction == 1) ? player.center + posAdjuster + new Vector2(Main.random.Next(-player.equippedWeapon.texture.Height + 16, 0), Main.random.Next(-player.equippedWeapon.texture.Width / 2, player.equippedWeapon.texture.Width / 2)) : player.center + posAdjuster + new Vector2(Main.random.Next(0, player.equippedWeapon.texture.Height - 16), Main.random.Next(-player.equippedWeapon.texture.Width / 2, player.equippedWeapon.texture.Width / 2)),
                                new Vector2(Main.random.Next(-50, -30) * player.direction, Main.random.Next(-10, 10)), Vector2.Zero, 2f * player.direction * Main.random.Next(-2, 2), 0.8f, 2f * Main.random.NextFloat(0.2f, 0.8f), Color.Wheat, Color.Aqua, Color.Magenta);
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
                    else if (player.equippedWeapon.damageType == "ranged")
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

                    if (player.direction == 1)
                    {
                        rotation = (float)Math.Atan2(targetDirection.Y, targetDirection.X);
                    }
                    else
                    {
                        rotation = (float)Math.Atan2(-targetDirection.Y, -targetDirection.X);
                    }

                    rotation = MathHelper.Clamp(rotation, -maxHeadRotation, maxHeadRotation);
                }
                else
                {
                    if (player.target != null)
                    {
                        Vector2 targetDirection = player.target.center - player.center;

                        if (player.direction == 1)
                        {
                            rotation = (float)Math.Atan2(targetDirection.Y, targetDirection.X);
                        }
                        else
                        {
                            rotation = (float)Math.Atan2(-targetDirection.Y, -targetDirection.X);
                        }

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
    }
}
