namespace BaseBuilderRPG.Content
{
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

            private Color GetSkinColor(float progress)
            {
                Color black = new Color(94, 54, 33);
                Color white = new Color(255, 220, 185);
                return Color.Lerp(black, white, progress);
            }

            public void Draw(SpriteBatch spriteBatch)
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
                            spriteBatch.DrawRectangleBorder(player.rectangleMelee, Color.Blue, 1f, 0.011f);
                            spriteBatch.DrawRectangleBorder(player.rectangleMeleeAI, Color.Cyan, 1f, 0.011f);
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
                Vector2 mousePosition = Input_Manager.Instance.mousePosition;
                Vector2 directionToMouse = mousePosition - player.position;

                float maxHeadRotation = MathHelper.ToRadians(20);
                float rotation = (float)Math.Atan2(directionToMouse.Y * player.direction, directionToMouse.X * player.direction);
                rotation = MathHelper.Clamp(rotation, -maxHeadRotation, maxHeadRotation);

                PreDraw(spriteBatch);
                PostDraw(spriteBatch, rotation);

                spriteBatch.Draw(player.textureBody, player.position, null, Color.Lerp(skinColor, Color.Red, player.hitEffectTimer), 0f, Vector2.Zero, 1f, eff, player.isControlled ? 0.851f : 0.751f);

                Vector2 headOrigin = new Vector2(player.textureHead.Width / 2, player.textureHead.Height);
                Vector2 eyesOrigin = new Vector2(player.textureEye.Width / 2, (player.textureEye.Height) / 2);

                Rectangle sourceRect = new Rectangle(0, 0, player.textureEye.Width, player.textureEye.Height / 2);
                spriteBatch.Draw(player.textureHead, player.position + new Vector2(player.textureHead.Width / 2 - 2 * player.direction, player.textureHead.Height), null, Color.Lerp(skinColor, Color.Red, player.hitEffectTimer), (player.isControlled) ? rotation : 0f, headOrigin, 1f, eff, (player.isControlled) ? 0.8511f : 0.7511f);
                spriteBatch.Draw(player.textureEye, player.position + new Vector2(player.textureEye.Width / 2 - 2 * player.direction, player.textureEye.Height / 2), sourceRect, Color.Lerp(Color.White, Color.Red, player.hitEffectTimer), (player.isControlled) ? rotation : 0f, eyesOrigin, 1f, eff, (player.isControlled) ? 0.8512f : 0.7512f);

                if (player.isControlled && player.inventoryVisible)
                {
                    player.inventory.Draw(spriteBatch, player);
                }
            }

            public void PreDraw(SpriteBatch spriteBatch)
            {
                if (player.equippedWeapon != null)
                {
                    if (player.equippedWeapon.weaponType == "One Handed Sword")
                    {
                        float end = (player.direction == 1) ? 110 * MathHelper.Pi / 180 : -290 * MathHelper.Pi / 180;
                        SpriteEffects eff = (player.direction == 1) ? SpriteEffects.None : SpriteEffects.FlipVertically;

                        Vector2 Pos = (player.direction == 1) ? new Vector2(player.width, player.height / 2)
                                                        : new Vector2(0, player.height / 2);
                        Vector2 weaponPosition = player.position + Pos;
                        Vector2 weaponOrigin = (player.direction == 1) ? new Vector2(0, player.equippedWeapon.texture.Height) : new Vector2(0, 0);

                        if (player.isSwinging)
                        {
                            spriteBatch.Draw(player.equippedWeapon.texture, weaponPosition, null, Color.White, player.rotationAngle, weaponOrigin, 0.8f, eff, player.isControlled ? 0.841f : 0.741f);
                        }
                        else
                        {
                            spriteBatch.Draw(player.equippedWeapon.texture, weaponPosition, null, Color.White, end, weaponOrigin, 0.8f, eff, player.isControlled ? 0.841f : 0.741f);
                        }
                    }
                }
            }

            public void PostDraw(SpriteBatch spriteBatch, float headRot) //0.8616f : 0.7616f MAX
            {
                if (player.health <= player.maxHealth)
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

                SpriteEffects eff = (player.direction == 1) ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
                if (player.inventory.equipmentSlots[2].equippedItem != null) //Offhand
                {
                    spriteBatch.Draw(player.inventory.equipmentSlots[2].equippedItem.texture, player.position + new Vector2(player.direction == 1 ? 0 : 22, player.height / 1.2f), null, Color.Lerp(Color.White, Color.DarkRed, player.immunityTime), 0f, player.origin, 0.8f, SpriteEffects.None, player.isControlled ? 0.8612f : 0.7612f);
                }
                if (player.inventory.equipmentSlots[1].equippedItem != null)//Body Armor
                {
                    spriteBatch.Draw(player.inventory.equipmentSlots[1].equippedItem.texture, player.position + new Vector2(0, 22), null, Color.Lerp(Color.White, Color.DarkRed, player.immunityTime), 0f, Vector2.Zero, 1f, eff, player.isControlled ? 0.8519f : 0.7519f);
                }
                if (player.inventory.equipmentSlots[4].equippedItem != null)//Head Armor
                {
                    Vector2 headOrigin = new Vector2(player.inventory.equipmentSlots[4].equippedItem.texture.Width / 2, player.inventory.equipmentSlots[4].equippedItem.texture.Height);
                    int headOffset;
                    switch (player.inventory.equipmentSlots[4].equippedItem.id)
                    {
                        case 6:
                            headOffset = 2;
                            break;

                        default:
                            headOffset = 0;
                            break;
                    }
                    spriteBatch.Draw(player.inventory.equipmentSlots[4].equippedItem.texture, player.position + new Vector2(player.textureHead.Width / 2 - headOffset * player.direction, player.textureHead.Height), null, Color.Lerp(Color.White, Color.DarkRed, player.immunityTime), player.isControlled ? headRot : 0f, headOrigin, 1f, eff, player.isControlled ? 0.8611f : 0.7611f);
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
                    float progress = player.useTimer / player.equippedWeapon.useTime;

                    float weaponStart = (player.direction == 1) ? -70 * MathHelper.Pi / 180 : -30 * MathHelper.Pi / 180;
                    float weaponEnd = (player.direction == 1) ? -35 * MathHelper.Pi / 180 : -240 * MathHelper.Pi / 180;

                    float particleStart = weaponStart - MathHelper.Pi / 4;
                    float particleEnd = weaponEnd + MathHelper.Pi / 4;

                    float angleRange = particleEnd - particleStart;

                    float radians;
                    float distance = player.equippedWeapon.texture.Height;

                    if (player.isSwinging)
                    {
                        radians = particleStart + (progress * angleRange);
                    }
                    else
                    {
                        radians = particleEnd;
                    }
                    Vector2 offset = new Vector2((float)Math.Cos(radians) * distance, (float)Math.Sin(radians) * distance);
                    Vector2 position;
                    if (player.equippedWeapon.id == 4)
                    {
                        Vector2 posCorrection = (player.direction == -1) ? Vector2.Zero : new Vector2(-player.equippedWeapon.texture.Width / 2, 0);
                        position = player.center + offset + posCorrection;

                        for (int i = 0; i < 16; i++)
                        {
                            if (Main.random.Next(25) == 0)
                            {
                                globalParticle.NewParticle(3, 0, position + new Vector2(Main.random.Next(-5, 5), Main.random.Next(-5, 5)), new Vector2(0, Main.random.Next(-30, -15)), Vector2.Zero, 0f, 0.6f, 0.2f + Main.random.NextFloat(0.5f, 2.5f), Color.Wheat, Color.OrangeRed, Color.White);
                                if (Main.random.Next(25) == 0)
                                {
                                    globalParticle.NewParticle(1, 1, position + new Vector2(Main.random.Next(-5, 5), Main.random.Next(-5, 5)), new Vector2(Main.random.Next(-20, 20), Main.random.Next(-110, -40)), Vector2.Zero, 0f, 1.2f, 2f + Main.random.NextFloat(0.5f, 3f), Color.Wheat, Color.OrangeRed, Color.White);
                                }
                            }
                        }
                    }
                    if (player.equippedWeapon.prefixName == "Magical")
                    {
                        position = player.center + offset + new Vector2(0, player.equippedWeapon.texture.Width / 8);

                        for (int i = 0; i < player.equippedWeapon.texture.Height / 2; i++)
                        {
                            if (Main.random.Next(150) == 0)
                            {
                                globalParticle.NewParticle(2, 0,
                                    (player.direction == 1) ? position + new Vector2(Main.random.Next(-player.equippedWeapon.texture.Height + 16, 0), Main.random.Next(-player.equippedWeapon.texture.Width / 2, player.equippedWeapon.texture.Width / 2)) : position + new Vector2(Main.random.Next(0, player.equippedWeapon.texture.Height - 16), Main.random.Next(-player.equippedWeapon.texture.Width / 2, player.equippedWeapon.texture.Width / 2)),
                                    new Vector2(Main.random.Next(-50, -10) * player.direction, Main.random.Next(-10, 10)), Vector2.Zero, 0f, 0.8f, 0.45f + Main.random.NextFloat(0.5f, 1f), Color.Wheat, Color.Lime, Color.Yellow);
                            }
                            if (Main.random.Next(150) == 0)
                            {
                                globalParticle.NewParticle(2, 0,
                                    (player.direction == 1) ? position + new Vector2(Main.random.Next(-player.equippedWeapon.texture.Height + 16, 0), Main.random.Next(-player.equippedWeapon.texture.Width / 2, player.equippedWeapon.texture.Width / 2)) : position + new Vector2(Main.random.Next(0, player.equippedWeapon.texture.Height - 16), Main.random.Next(-player.equippedWeapon.texture.Width / 2, player.equippedWeapon.texture.Width / 2)),
                                    new Vector2(Main.random.Next(-50, -10) * player.direction, Main.random.Next(-10, 10)), Vector2.Zero, 0f, 0.8f, 0.45f + Main.random.NextFloat(0.5f, 1f), Color.Wheat, Color.Aqua, Color.Magenta);
                            }
                            /* if (Main.random.Next(75) == 0)
                             {
                                 globalParticle.NewParticle(3, 1,
                                     (direction == 1) ? position + new Vector2(Main.random.Next(-equippedWeapon.texture.Height + 16, 0), Main.random.Next(-equippedWeapon.texture.Width / 2, equippedWeapon.texture.Width / 2)) : position + new Vector2(Main.random.Next(0, equippedWeapon.texture.Height - 16), Main.random.Next(-equippedWeapon.texture.Width / 2, equippedWeapon.texture.Width / 2)),
                                     new Vector2(Main.random.Next(-10, 10), Main.random.Next(-10, 10)), Vector2.Zero, 10f * -direction, 0.6f, 2f * Main.random.NextFloat(0.1f, 0.8f), Color.Wheat, Color.Aqua, Color.Magenta);
                             }*/
                        }
                    }
                }
            }
        }
    }
}