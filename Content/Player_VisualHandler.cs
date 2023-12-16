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

        private Color GetHitColor(bool armorPiece, bool movementOrder, float alpha)
        {
            return armorPiece ? Color.Lerp(!movementOrder ? Color.White : new Color(255, 255, 255, alpha), !movementOrder ? Color.Red : new Color(255, 255, 255, alpha), player.hitEffectTimer)
                : Color.Lerp(!movementOrder ? skinColor : new Color(skinColor.R, skinColor.G, skinColor.B, (byte)alpha), !movementOrder ? Color.Red : new Color(skinColor.R, skinColor.G, skinColor.B, (byte)alpha), player.hitEffectTimer);
        }
        private Color GetSkinColor(float progress)
        {
            Color black = new Color(94, 54, 33);
            Color white = new Color(255, 220, 185);
            return Color.Lerp(black, white, progress);
        }
        float nameLayer = 0.8f;
        float headLayer = 0.71f;
        float bodyLayer = 0.7f;
        float eyeLayer = 0.72f;
        float offhandLayer = 0.725f;
        float headArmorLayer = 0.724f;
        float bodyArmorLayer = 0.723f;
        float weaponLayer = 0.73f;
        float healthBarLayer = 0.75f;
        float controlLayer = 0;
        public void Draw(SpriteBatch spriteBatch)
        {
            DrawDebugInformation(spriteBatch);
            DrawPlayerMisc(spriteBatch, 255);
            DrawPlayer(spriteBatch, HeadRot(), 255, player.targetMovement);


            Color nameColor;

            if (player.isSelected)
            {
                nameColor = player.hasMovementOrder ? new Color(Color.Aqua.R, Color.Aqua.G, Color.Aqua.B, (byte)125) : Color.Aqua;
            }
            else if (player.rectangle.Contains(Input_Manager.Instance.mousePosition))
            {
                nameColor = player.hasMovementOrder ? new Color(Color.Yellow.R, Color.Yellow.G, Color.Yellow.B, (byte)125) : Color.Yellow;
            }
            else
            {
                nameColor = player.hasMovementOrder ? new Color(Color.Lime.R, Color.Lime.G, Color.Lime.B, (byte)125) : Color.Lime;
            }

            Vector2 textPosition = player.position + new Vector2(0, -14);
            Vector2 textPositionMovement = player.targetMovement + new Vector2(0, -14 - player.origin.Y);
            textPosition.X = player.position.X + player.width / 2 - Main.testFont.MeasureString(player.name).X / 2;
            textPositionMovement.X = player.targetMovement.X + player.width / 2 - player.origin.X - Main.testFont.MeasureString(player.name).X / 2;
            spriteBatch.DrawStringWithOutline(Main.testFont, player.name, textPosition, Color.Black, player.isControlled ? Color.Yellow : nameColor, 1f, nameLayer + controlLayer);
            if (player.hasMovementOrder)
            {
                spriteBatch.DrawStringWithOutline(Main.testFont, player.name, textPositionMovement, Color.Gray, player.isControlled ? Color.Yellow : nameColor, 1f, nameLayer + controlLayer);
            }

        }
        public void DrawPlayerMisc(SpriteBatch spriteBatch, float alpha)
        {
            if (player.health <= player.maxHealth)
            {
                DrawHealthBar(spriteBatch);
            }

            #region DRAW WEAPONS
            if (player.equippedWeapon != null)
            {
                if (player.equippedWeapon.weaponType == "One Handed Sword" && player.useTimer <= 0)
                {
                    float rotation = (player.direction == 1) ? -70 * MathHelper.Pi / 180 : 250 * MathHelper.Pi / 180;
                    Vector2 origin = (player.direction == 1) ? new Vector2(0, player.equippedWeapon.texture.Height) : new Vector2(0, 0);
                    spriteBatch.Draw(player.equippedWeapon.texture, player.position + new Vector2(player.direction == 1 ? player.width : 0, player.height / 2 + player.equippedWeapon.texture.Height / 4), null, Color.White, rotation, origin, 1f, player.direction == -1 ? SpriteEffects.FlipVertically : SpriteEffects.None, weaponLayer + controlLayer);
                }
                if (player.equippedWeapon.weaponType == "Pistol")
                {
                    Vector2 origin = new Vector2(0, player.equippedWeapon.texture.Height / 2);

                    Vector2 targetPosition = player.center;

                    float lerpAmount = MathHelper.Clamp(player.animationTimer / player.equippedWeapon.useTime * 2, 0f, 1f);

                    Vector2 drawPos = Vector2.Lerp(player.center + new Vector2((float)Math.Cos(MouseRot()) * player.equippedWeapon.texture.Height * 1.2f / 2, (float)Math.Sin(MouseRot()) * player.equippedWeapon.texture.Height * 1.2f / 2), targetPosition, lerpAmount);
                    int recoil = 20;
                    float recoilRotation = MathHelper.Lerp(0f, MathHelper.ToRadians(recoil * -player.direction), lerpAmount);

                    float drawRotation = MouseRot() + recoilRotation;
                    spriteBatch.Draw(player.equippedWeapon.texture,
                        alpha == 125 ? player.targetMovement + new Vector2(player.equippedWeapon.texture.Width - player.textureBody.Width, player.textureBody.Height / 2 - player.equippedWeapon.texture.Height * 1.2f) : drawPos + new Vector2(0, player.equippedWeapon.texture.Height / 2f),
                        null,
                        alpha == 125 ? new Color(Color.White.R, Color.White.G, Color.White.B, (byte)alpha) : Color.White,
                        alpha == 125 ? 0f : drawRotation,
                        origin,
                        1f,
                        alpha == 125 ? SpriteEffects.None : player.direction == -1 ? SpriteEffects.FlipVertically : SpriteEffects.None,
                        weaponLayer + controlLayer);
                }
            }
            #endregion
        }
        public void DrawPlayer(SpriteBatch spriteBatch, float headRot, float alpha, Vector2 position)
        {
            if (player.isControlled)
            {
                controlLayer = 0.0001f;
            }
            else
            {
                controlLayer = 0f;
            }
            SpriteEffects eff = alpha == 125 ? SpriteEffects.None : (player.direction == 1) ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
            #region DRAW PLAYER BASE
            Vector2 headOrigin = new Vector2(player.textureHead.Width / 2, player.textureHead.Height);
            Vector2 eyesOrigin = new Vector2(player.textureEye.Width / 2, player.textureEye.Height / 2);
            Rectangle eyesSourceRectangle = new Rectangle(0, (player.eyeTimer > 0f && alpha == 255) ? 24 : 0, player.textureEye.Width, player.textureEye.Height / 2);

            spriteBatch.Draw(player.textureBody, alpha >= 255 ? player.position : position, null, GetHitColor(false, alpha != 255 ? true : false, alpha), 0f, Vector2.Zero, 1f, eff, bodyLayer + controlLayer);
            spriteBatch.Draw(player.textureHead, alpha >= 255 ? player.position + headOrigin : position + headOrigin, null, GetHitColor(false, alpha != 255 ? true : false, alpha), headRot, headOrigin, 1f, eff, headLayer + controlLayer);
            spriteBatch.Draw(player.textureEye, alpha >= 255 ? player.position + eyesOrigin : position + eyesOrigin, eyesSourceRectangle, GetHitColor(alpha != 255 ? false : true, alpha != 255 ? true : true, alpha), headRot, eyesOrigin, 1f, eff, eyeLayer + controlLayer);
            #endregion
            #region DRAW ARMORS

            if (player.equippedOffhand != null)
            {
                spriteBatch.Draw(player.equippedOffhand.texture, player.center + new Vector2(player.equippedOffhand.texture.Width / 2.5f * -player.direction, player.equippedOffhand.texture.Height / 2), null, GetHitColor(true, alpha != 255 ? true : false, alpha), 0f, new Vector2(player.equippedOffhand.texture.Width / 2, player.equippedOffhand.texture.Height / 2), 1f, (player.direction == 1) ? SpriteEffects.None : SpriteEffects.FlipHorizontally, offhandLayer + controlLayer);
            }
            if (player.equippedBodyArmor != null)
            {
                spriteBatch.Draw(player.equippedBodyArmor.texture, player.position + new Vector2(0, 22), null, GetHitColor(true, alpha != 255 ? true : false, alpha), 0f, Vector2.Zero, 1f, eff, bodyArmorLayer + controlLayer);
            }
            if (player.equippedHeadArmor != null)
            {
                Vector2 headArmorOrigin = new Vector2(player.equippedHeadArmor.texture.Width / 2, player.equippedHeadArmor.texture.Height);
                spriteBatch.Draw(player.equippedHeadArmor.texture, player.position + new Vector2(player.textureHead.Width / 2, player.textureHead.Height), null, GetHitColor(true, alpha != 255 ? true : false, alpha), headRot, headArmorOrigin, 1f, eff, headArmorLayer + controlLayer);
            }
            #endregion
        }

        public void ParticleEffects(Particle_Globals globalParticleBelow, Particle_Globals globalParticleAbove)
        {
            WeaponVisuals(globalParticleBelow, globalParticleAbove);
        }

        private void WeaponVisuals(Particle_Globals globalParticleBelow, Particle_Globals globalParticleAbove)
        {
            if (player.equippedWeapon != null)
            {
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

                spriteBatch.DrawStringWithOutline(Main.testFont, player.aiState, textPosition2, Color.Black, Color.White, 1f, 1f);
                spriteBatch.DrawStringWithOutline(Main.testFont, (!player.isControlled) ? "Controlled by AI" : "", textPosition3 + new Vector2(0, -20), Color.Black, Color.White, 1f, 1f);

                if (player.equippedWeapon != null)
                {
                    if (player.equippedWeapon.damageType == "melee")
                    {
                        spriteBatch.DrawCircle(player.center, player.meleeRange, Color.Cyan, 64, 1f);
                        spriteBatch.DrawRectangleBorder(player.rectangleMelee, Color.Cyan, 1f, 1f);
                    }
                    else if (player.equippedWeapon.damageType == "ranged")
                    {
                        spriteBatch.DrawCircle(player.center, player.rangedRange, Color.Cyan, 64, 1f);
                    }
                }
                if (player.hasMovementOrder)
                {
                    spriteBatch.DrawLine(player.center, player.targetMovement, Color.Indigo, 1f);
                }
                spriteBatch.DrawCircle(player.center, 4f, Color.Blue * 1.5f, 64, 1f);
                spriteBatch.DrawRectangleBorder(player.rectangle, Color.Blue, 1f, 1f);
                if (!player.isControlled && player.target != null)
                {
                    spriteBatch.DrawLine(player.center, player.target.center, Color.Blue, 1f);
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
                spriteBatch.DrawRectangle(healthBarRectangleBackground, Color.Black, healthBarLayer + controlLayer);
                spriteBatch.DrawRectangle(healthBarRectangleBackgroundRed, Color.Red, healthBarLayer + controlLayer + 0.01f);
                spriteBatch.DrawRectangle(healthBarRectangle, Color.Lime, healthBarLayer + controlLayer + 0.02f);
            }
        }
        private float HeadRot()
        {
            Vector2 directionToMouse = Input_Manager.Instance.mousePosition - player.position;

            float maxHeadRotation = MathHelper.ToRadians(30);
            float rotation;

            if (player.isControlled)
            {
                rotation = (float)Math.Atan2(directionToMouse.Y * player.direction, directionToMouse.X * player.direction);
                rotation = MathHelper.Clamp(rotation, -maxHeadRotation, maxHeadRotation);
            }
            else
            {
                if (player.target != null)
                {
                    Vector2 targetDirection = player.target.center - player.center;

                    rotation = (float)Math.Atan2(targetDirection.Y, targetDirection.X) + (player.direction == -1 ? MathHelper.Pi : 0);

                    rotation = NormalizeHeadRot(rotation);

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
                        rotation = 0f;
                    }
                }
            }
            return rotation;
        }
        float NormalizeHeadRot(float angle)
        {
            while (angle > MathHelper.Pi)
            {
                angle -= MathHelper.TwoPi;
            }

            while (angle <= -MathHelper.Pi)
            {
                angle += MathHelper.TwoPi;
            }

            return angle;
        }
        public float MouseRot()
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
    }
}
