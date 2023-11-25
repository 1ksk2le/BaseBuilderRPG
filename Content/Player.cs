using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BaseBuilderRPG.Content
{
    public class Player
    {
        public Inventory inventory { get; private set; }
        public Vector2 position { get; set; }
        public int health { get; set; }
        public int maxHealth { get; set; }
        public float skinColorFloat { get; set; }
        public bool isControlled { get; set; }
        public string name { get; set; }

        public List<Projectile> ownedProjectiles;
        public Item equippedWeapon, mouseItem, hoveredItem;
        public Texture2D textureBody;
        public Texture2D textureHead;
        public Texture2D textureEye;
        public Color skinColor;
        public Vector2 velocity, target, origin, center, targetMovement;
        public Rectangle rectangle, rectangleMelee;
        private Rectangle rectangleMeleeAI;
        public int direction = 1;
        public int width, height;
        private float immunityTime, immunityTimeMax, useTimer, meleeRange, rangedRange, rotationAngle, speed, hitEffectTimer, hitEffectTimerMax;
        public bool inventoryVisible, isImmune, isSwinging, isPicked, canHit, didSpawn, hasMovementOrder;
        private bool aiAttackCheck;
        private string aiState;

        private Random random;

        public Player(Texture2D texture, Texture2D headTexture, Texture2D eyeTexture, string name, Vector2 position, int healthMax, float skinColor, bool isActive)
        {
            this.skinColorFloat = skinColor;
            this.skinColor = GetSkinColor(this.skinColorFloat);
            this.position = position;
            this.name = name;

            textureBody = texture;
            textureHead = headTexture;
            textureEye = eyeTexture;
            isControlled = isActive;
            maxHealth = healthMax;
            health = maxHealth;
            immunityTimeMax = 0.4f;
            speed = 1.5f;
            immunityTime = 0f;
            hitEffectTimer = 0f;
            hitEffectTimerMax = 0.75f;
            targetMovement = center;
            width = textureBody.Width;
            height = textureBody.Height;
            origin = new Vector2(width / 2, height / 2);
            inventory = new Inventory(5, 6);
            inventoryVisible = true;
            canHit = true;
            didSpawn = false;
            aiAttackCheck = false;
            hasMovementOrder = false;
            isPicked = false;
            aiState = "";

            ownedProjectiles = new List<Projectile>();

            random = Main_Globals.GetRandomInstance();
        }

        public void Update(GameTime gameTime, Dictionary<int, Item> itemDictionary, Global_Item globalItem, Text_Manager textManager, Global_Projectile globalProjectile, Global_Particle globalParticle, List<NPC> npcs, List<Item> groundItems, List<Item> items)
        {
            var inputManager = Input_Manager.Instance;
            rectangle = new Rectangle((int)position.X, (int)position.Y, width, height);
            center = position + origin;

            if (equippedWeapon != null)
            {
                meleeRange = (equippedWeapon.damageType == "melee") ? 200f : 0;
                if (equippedWeapon.damageType == "ranged")
                {
                    rangedRange = globalProjectile.GetProjectile(equippedWeapon.shootID).lifeTimeMax * equippedWeapon.shootSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds * 60;
                }
                else if (equippedWeapon.damageType == "melee")
                {
                    Vector2 pos = (direction == 1) ? new Vector2(width + equippedWeapon.texture.Height * 0.2f, height / 2)
                                               : new Vector2(-equippedWeapon.texture.Height * 0.2f, height / 2);
                    rectangleMelee = CalcMeleeRectangle(position + pos, (int)(equippedWeapon.texture.Width), (int)(equippedWeapon.texture.Height * 1.1f), rotationAngle);
                    rectangleMeleeAI = new Rectangle((int)(center.X - equippedWeapon.texture.Height / 2), (int)(center.Y - equippedWeapon.texture.Height * 1.2f / 2), (int)(equippedWeapon.texture.Height), (int)(equippedWeapon.texture.Height * 1.2f));
                }
            }

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

            if (inventory.equipmentSlots[0].equippedItem != null)
            {
                equippedWeapon = inventory.equipmentSlots[0].equippedItem;
            }
            else
            {
                equippedWeapon = null;
            }

            if (health > 0f)
            {
                if (!isControlled)
                {
                    AI(gameTime, npcs, globalProjectile);

                }
                else
                {
                    Shoot(gameTime, globalProjectile, new Vector2(inputManager.previousMouseState.X, inputManager.previousMouseState.Y));
                    aiState = "";
                    PlayerInventoryInteractions(Keys.I, groundItems);
                    AddItem(Keys.X, true, random.Next(0, 11), itemDictionary, globalItem, groundItems, items);
                    inventory.SortItems(inputManager.previousMouseState);
                    foreach (Item item in groundItems.ToList())
                    {
                        if (item.PlayerClose(this, 40f) && inputManager.IsKeySinglePress(Keys.F) && item.onGround && !inventory.IsFull())
                        {
                            string text = "Picked: " + item.prefixName + " " + item.name + " " + item.suffixName;
                            Vector2 textSize = Main.testFont.MeasureString(text);

                            Vector2 textPos = position + new Vector2(-textSize.X / 5f, -10);
                            textManager.AddFloatingText("Picked: ", (item.prefixName + " " + item.name + " " + item.suffixName), textPos, new Vector2(0, 10), Color.White, item.rarityColor, 0.75f, 1f);
                            inventory.PickItem(textManager, this, item, groundItems);
                        }
                    }
                }

                if (equippedWeapon != null)
                {
                    if (equippedWeapon.weaponType == "One Handed Sword")
                    {
                        OneHandedSwing(gameTime, globalParticle);
                    }
                }
                Movement(Vector2.Zero, Input_Manager.Instance.currentKeyboardState);
            }
        }

        private void AI(GameTime gameTime, List<NPC> npcs, Global_Projectile projManager)
        {
            if (equippedWeapon != null)
            {
                if (equippedWeapon.damageType == "melee")
                {
                    NPC targetNPC = null;
                    float closestDistance = meleeRange * meleeRange;

                    foreach (NPC npc in npcs)
                    {
                        float distance = Vector2.DistanceSquared(center, npc.center);

                        if (distance < closestDistance)
                        {
                            closestDistance = distance;
                            targetNPC = npc;
                        }
                    }

                    if (targetNPC != null)
                    {
                        target = targetNPC.center;
                        if (!rectangleMeleeAI.Intersects(targetNPC.rectangle))
                        {
                            if (position.X > target.X)
                            {
                                direction = -1;
                            }
                            else
                            {
                                direction = 1;
                            }

                            if (!hasMovementOrder)
                            {
                                Vector2 targetDirection = target - center;
                                targetDirection.Normalize();
                                position += targetDirection * speed;
                            }

                            aiState = "Moving to target: [" + targetNPC.name + "]";
                        }
                        else
                        {
                            aiState = "Attacking target: [" + targetNPC.name + "]";
                            isSwinging = true;
                        }
                    }
                    else
                    {
                        aiState = "Waiting for orders";
                    }
                }
                if (equippedWeapon.damageType == "ranged")
                {
                    NPC targetNPC = null;
                    float closestDistance = rangedRange * rangedRange * 2;

                    foreach (NPC npc in npcs)
                    {
                        float distance = Vector2.DistanceSquared(center, npc.center);

                        if (distance < closestDistance)
                        {
                            closestDistance = distance;
                            targetNPC = npc;
                        }
                    }

                    if (targetNPC != null)
                    {
                        target = targetNPC.center;
                        if (Vector2.Distance(target, center) > rangedRange * 0.8f)
                        {
                            if (position.X > target.X)
                            {
                                direction = -1;
                            }
                            else
                            {
                                direction = 1;
                            }

                            if (!hasMovementOrder)
                            {
                                Vector2 targetDirection = target - center;
                                targetDirection.Normalize();
                                position += targetDirection * speed;
                                aiState = "Moving to target: [" + targetNPC.name + "]";
                            }

                        }
                        else
                        {
                            if (Vector2.Distance(target, center) < 100f)
                            {
                                if (position.X > target.X)
                                {
                                    direction = -1;
                                }
                                else
                                {
                                    direction = 1;
                                }
                                if (!hasMovementOrder)
                                {
                                    Vector2 targetDirection = target - center;
                                    targetDirection.Normalize();
                                    position -= targetDirection * speed;
                                    aiState = "Running away from: [" + targetNPC.name + "]";
                                }
                            }
                            else
                            {
                                Shoot(gameTime, projManager, target);
                                aiState = "Shooting at target: [" + targetNPC.name + "]";
                            }

                        }
                    }
                    else
                    {
                        aiState = "Waiting for orders";
                    }
                }
            }
        }

        private void OneHandedSwing(GameTime gameTime, Global_Particle globalParticle)
        {
            if (equippedWeapon != null && equippedWeapon.weaponType == "One Handed Sword")
            {
                float start = (direction == 1) ? -90 * MathHelper.Pi / 180 : -90 * MathHelper.Pi / 180;
                float end = (direction == 1) ? 110 * MathHelper.Pi / 180 : -290 * MathHelper.Pi / 180;
                float progress = useTimer / equippedWeapon.useTime;
                if (isControlled)
                {
                    if (Input_Manager.Instance.IsButtonPressed(true) && !inventory.IsInventoryHovered())
                    {
                        if (!isSwinging)
                        {
                            isSwinging = true;
                            useTimer = 0;
                            rotationAngle = start;
                            canHit = true;
                        }
                    }

                }
                else
                {
                    if (isSwinging && !aiAttackCheck)
                    {
                        canHit = true;
                        aiAttackCheck = true;
                    }
                }



                if (isSwinging)
                {
                    useTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;

                    if (useTimer >= equippedWeapon.useTime)
                    {
                        aiAttackCheck = false;
                        isSwinging = false;
                        useTimer = 0;
                        rotationAngle = end;

                    }
                    else
                    {
                        rotationAngle = MathHelper.Lerp(start, end, progress);
                    }
                }

                float weaponStart = (direction == 1) ? -70 * MathHelper.Pi / 180 : -30 * MathHelper.Pi / 180;
                float weaponEnd = (direction == 1) ? -35 * MathHelper.Pi / 180 : -240 * MathHelper.Pi / 180;

                float particleStart = weaponStart - MathHelper.Pi / 4;
                float particleEnd = weaponEnd + MathHelper.Pi / 4;

                float angleRange = particleEnd - particleStart;


                float radians;
                float distance = equippedWeapon.texture.Height;

                if (isSwinging)
                {
                    radians = particleStart + (progress * angleRange);
                }
                else
                {
                    radians = particleEnd;
                }
                Vector2 offset = new Vector2((float)Math.Cos(radians) * distance, (float)Math.Sin(radians) * distance);
                Vector2 position;
                if (equippedWeapon.id == 4)
                {
                    position = center + offset;

                    for (int i = 0; i < 12; i++)
                    {
                        if (random.Next(25) == 0)
                        {
                            globalParticle.NewParticle(1, 0, position + new Vector2(random.Next(-5, 5), random.Next(-5, 5)), new Vector2(0, random.Next(-30, -15)), Vector2.Zero, 0f, 0.6f, 0.5f + random.NextFloat(0.5f, 3f), Color.Wheat, Color.OrangeRed, Color.White);
                            if (random.Next(25) == 0)
                            {
                                globalParticle.NewParticle(1, 1, position + new Vector2(random.Next(-5, 5), random.Next(-5, 5)), new Vector2(random.Next(-20, 20), random.Next(-110, -40)), Vector2.Zero, 0f, 1.2f, 2f + random.NextFloat(0.5f, 3f), Color.Wheat, Color.OrangeRed, Color.White);
                            }
                        }
                    }
                }
                if (equippedWeapon.prefixName == "Magical")
                {
                    position = center + offset + new Vector2(0, equippedWeapon.texture.Width / 8);

                    for (int i = 0; i < equippedWeapon.texture.Height / 2; i++)
                    {
                        if (random.Next(150) == 0)
                        {
                            globalParticle.NewParticle(1, 0,
                                (direction == 1) ? position + new Vector2(random.Next(-equippedWeapon.texture.Height + 16, 0), random.Next(-equippedWeapon.texture.Width / 2, equippedWeapon.texture.Width / 2)) : position + new Vector2(random.Next(0, equippedWeapon.texture.Height - 16), random.Next(-equippedWeapon.texture.Width / 2, equippedWeapon.texture.Width / 2)),
                                new Vector2(0, random.Next(-30, 30)), Vector2.Zero, 0f, 0.8f, 0.5f + random.NextFloat(0.5f, 3f), Color.Wheat, Color.Lime, Color.Yellow);
                        }
                        if (random.Next(150) == 0)
                        {
                            globalParticle.NewParticle(1, 1,
                                (direction == 1) ? position + new Vector2(random.Next(-equippedWeapon.texture.Height + 16, 0), random.Next(-equippedWeapon.texture.Width / 2, equippedWeapon.texture.Width / 2)) : position + new Vector2(random.Next(0, equippedWeapon.texture.Height - 16), random.Next(-equippedWeapon.texture.Width / 2, equippedWeapon.texture.Width / 2)),
                                new Vector2(random.Next(-10, 10), random.Next(-10, 10)), Vector2.Zero, 10f * -direction, 0.6f, 4f * random.NextFloat(0.1f, 0.8f), Color.Wheat, Color.Aqua, Color.Magenta);
                        }
                    }
                }
            }
        }

        private void DrawParticleLine(Global_Particle globalParticle, Vector2 pointA, Vector2 pointB, float thickness, Color color, Color startColor, Color endColor, int particleCount)
        {
            Vector2 direction = pointB - pointA;
            float length = direction.Length();

            direction.Normalize();

            float angle = (float)Math.Atan2(direction.Y, direction.X);

            float spacing = length / particleCount;

            for (int i = 0; i < particleCount; i++)
            {
                Vector2 particlePosition = pointA + direction * (spacing * i);

                globalParticle.NewParticle(
                    1,
                    0,
                    particlePosition,
                    Vector2.Zero,
                    Vector2.Zero,
                    0f,
                    thickness / 10f,
                    thickness / 10f,
                    color,
                    startColor,
                    endColor
                );
            }
        }


        private void Movement(Vector2 movement, KeyboardState keyboardState)
        {
            if (isControlled)
            {
                if (position.X > (int)Input_Manager.Instance.mousePosition.X)
                {
                    direction = -1;
                }
                else
                {
                    direction = 1;
                }


                if (keyboardState.IsKeyDown(Keys.W))
                    movement.Y = -speed;
                if (keyboardState.IsKeyDown(Keys.S))
                    movement.Y = speed;
                if (keyboardState.IsKeyDown(Keys.A))
                    movement.X = -speed;
                if (keyboardState.IsKeyDown(Keys.D))
                    movement.X = speed;

                if (movement != Vector2.Zero)
                    movement.Normalize();

                velocity = movement * speed;

                position += velocity;
            }

            if (hasMovementOrder && !isControlled)
            {

                direction = (position.X > targetMovement.X) ? -1 : 1;

                float distanceThreshold = 1f;
                float deltaX = targetMovement.X - center.X;
                float deltaY = targetMovement.Y - center.Y;
                float distance = (float)Math.Sqrt(deltaX * deltaX + deltaY * deltaY);

                if (distance < distanceThreshold)
                {
                    aiState = "";
                    hasMovementOrder = false;
                }
                else
                {
                    float directionX = deltaX / distance;
                    float directionY = deltaY / distance;
                    float newPositionX = position.X + directionX * speed;
                    float newPositionY = position.Y + directionY * speed;
                    aiState = "Moving to: [" + targetMovement.ToString() + "]";
                    position = new Vector2(newPositionX, newPositionY);
                }
            }
            else
            {
                if (isControlled)
                {
                    isPicked = false;
                }
                targetMovement = center;
                hasMovementOrder = false;
            }

        }

        private Color GetSkinColor(float progress)
        {
            Color black = new Color(94, 54, 33);
            Color white = new Color(255, 220, 185);
            return Color.Lerp(black, white, progress);
        }

        public void GetDamaged(Text_Manager texMan, int damage, Global_Particle globalParticle, NPC npc)
        {
            health -= damage;
            texMan.AddFloatingText("-" + damage.ToString(), "", new Vector2(position.X + textureBody.Width / 2 + random.Next(-10, 10), position.Y), new Vector2(random.Next(-10, 10), random.Next(1, 10) + 10f), Color.Red, Color.Transparent, 2f, 1.1f);
            immunityTime = immunityTimeMax;
            hitEffectTimer = hitEffectTimerMax;

            for (int i = 0; i < damage; i++)
            {
                if (npc != null)
                {
                    globalParticle.NewParticle(1, 1, position + new Vector2(random.Next(width), random.Next(height)),
                   (npc.position.X > position.X) ? -1 * new Vector2(random.Next(10, 50), random.Next(70, 90)) : new Vector2(random.Next(10, 50), random.Next(-90, -70)), origin, 0f, 1f, random.NextFloat(1.5f, 4f), Color.DarkRed, Color.Red, Color.DarkRed);
                }

            }
        }



        private void Shoot(GameTime gameTime, Global_Projectile projManager, Vector2 target)
        {
            if (equippedWeapon != null && equippedWeapon.shootID > -1)
            {
                if (useTimer > 0)
                {
                    useTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds;
                }

                if (useTimer <= 0)
                {
                    if (isControlled)
                    {
                        if (Input_Manager.Instance.IsButtonPressed(true))
                        {
                            projManager.NewProjectile(equippedWeapon.shootID, center, target, equippedWeapon.damage, equippedWeapon.shootSpeed, this, true);
                            useTimer = equippedWeapon.useTime;
                        }
                    }
                    else
                    {
                        projManager.NewProjectile(equippedWeapon.shootID, center, target, equippedWeapon.damage, equippedWeapon.shootSpeed, this, true);
                        useTimer = equippedWeapon.useTime;
                    }
                }
            }
        }

        private Rectangle CalcMeleeRectangle(Vector2 position, int width, int height, float rotation)
        {
            Matrix transform = Matrix.CreateRotationZ(rotation) * Matrix.CreateTranslation(position.X, position.Y, 0);

            Vector2 leftTop = Vector2.Transform(new Vector2(-width / 2, -height / 2), transform);
            Vector2 rightTop = Vector2.Transform(new Vector2(width / 2, -height / 2), transform);
            Vector2 leftBottom = Vector2.Transform(new Vector2(-width / 2, height / 2), transform);
            Vector2 rightBottom = Vector2.Transform(new Vector2(width / 2, height / 2), transform);

            Vector2 min = Vector2.Min(Vector2.Min(leftTop, rightTop), Vector2.Min(leftBottom, rightBottom));
            Vector2 max = Vector2.Max(Vector2.Max(leftTop, rightTop), Vector2.Max(leftBottom, rightBottom));

            return new Rectangle((int)min.X, (int)min.Y, (int)(max.X - min.X), (int)(max.Y - min.Y));
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (Main.drawDebugRectangles)
            {
                Vector2 textPosition2 = position + new Vector2(0, height + 20);
                textPosition2.X = position.X + width / 2 - Main.testFont.MeasureString(aiState).X / 2;

                Vector2 textPosition3 = center + new Vector2(0, height);
                textPosition3.X = center.X + width / 2 - Main.testFont.MeasureString("Controlled by AI").X / 2;

                spriteBatch.DrawStringWithOutline(Main.testFont, aiState, textPosition2, Color.Black, Color.White, 1f, isControlled ? 0.8617f : 0.7617f);
                spriteBatch.DrawStringWithOutline(Main.testFont, (!isControlled) ? "Controlled by AI" : "", textPosition3 + new Vector2(0, -20), Color.Black, Color.White, 1f, isControlled ? 0.8617f : 0.7617f);

                if (equippedWeapon != null)
                {
                    if (equippedWeapon.damageType == "melee")
                    {
                        spriteBatch.DrawCircle(center, meleeRange, Color.Cyan, 64, 0.012f);
                        spriteBatch.DrawRectangleBorder(rectangleMelee, Color.Blue, 1f, 0.011f);
                        spriteBatch.DrawRectangleBorder(rectangleMeleeAI, Color.Cyan, 1f, 0.011f);
                    }
                    else if (equippedWeapon.damageType == "ranged")
                    {
                        spriteBatch.DrawCircle(center, rangedRange, Color.Cyan, 64, 0.012f);

                    }

                }
                if (hasMovementOrder)
                {
                    spriteBatch.DrawLine(center, targetMovement, Color.Indigo, 0.013f);

                }
                spriteBatch.DrawCircle(center, 4f, Color.Blue * 1.5f, 64, 1f);
                spriteBatch.DrawRectangleBorder(rectangle, Color.Blue, 1f, 0.012f);
                if (!isControlled && target != Vector2.Zero)
                {
                    spriteBatch.DrawLine(center, target, Color.Blue, 0.013f);
                }
            }

            if (hasMovementOrder)
            {
                spriteBatch.DrawCircle(targetMovement, 8f, Color.Lime, 64, 0.012f);

            }

            PreDraw(spriteBatch);

            Color nameColor;

            if (isPicked)
            {
                nameColor = Color.Aqua;
            }
            else if (rectangle.Contains(Input_Manager.Instance.mousePosition))
            {
                nameColor = Color.Lime;
            }
            else
            {
                nameColor = Color.White;
            }

            Vector2 textPosition = position + new Vector2(0, -14);
            textPosition.X = position.X + width / 2 - Main.testFont.MeasureString(name).X / 2;

            spriteBatch.DrawStringWithOutline(Main.testFont, name, textPosition, Color.Black, isControlled ? Color.Yellow : nameColor, 1f, isControlled ? 0.8616f : 0.7616f);


            SpriteEffects eff = (direction == 1) ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
            Vector2 mousePosition = Input_Manager.Instance.mousePosition;
            Vector2 directionToMouse = mousePosition - position;

            float maxHeadRotation = MathHelper.ToRadians(20);
            float rotation = (float)Math.Atan2(directionToMouse.Y * direction, directionToMouse.X * direction);
            rotation = MathHelper.Clamp(rotation, -maxHeadRotation, maxHeadRotation);

            spriteBatch.Draw(textureBody, position, null, Color.Lerp(skinColor, Color.Red, hitEffectTimer), 0f, Vector2.Zero, 1f, eff, isControlled ? 0.851f : 0.751f);

            if (isControlled)
            {
                Vector2 headOrigin = new Vector2(textureHead.Width / 2, textureHead.Height);
                Vector2 eyesOrigin = new Vector2(textureEye.Width / 2, (textureEye.Height) / 2);

                spriteBatch.Draw(textureHead, position + new Vector2(textureHead.Width / 2 - 2 * direction, textureHead.Height), null, Color.Lerp(skinColor, Color.Red, hitEffectTimer), rotation, headOrigin, 1f, eff, 0.852f);

                Rectangle sourceRect = new Rectangle(0, 0, textureEye.Width, textureEye.Height / 2);
                spriteBatch.Draw(textureEye, position + new Vector2(textureEye.Width / 2 - 2 * direction, textureEye.Height / 2), sourceRect, Color.Lerp(Color.White, Color.Red, hitEffectTimer), rotation, eyesOrigin, 1f, eff, 0.853f);
            }
            else
            {
                spriteBatch.Draw(textureHead, position + new Vector2(-2 * direction, 0), null, Color.Lerp(skinColor, Color.Red, hitEffectTimer), 0f, Vector2.Zero, 1f, eff, 0.752f);
                Rectangle sourceRect = new Rectangle(0, 0, textureEye.Width, textureEye.Height / 2);
                spriteBatch.Draw(textureEye, position + new Vector2(-2 * direction, 0), sourceRect, Color.Lerp(Color.White, Color.Red, hitEffectTimer), 0f, Vector2.Zero, 1f, eff, 0.753f);
            }

            PostDraw(spriteBatch, rotation);

            if (isControlled && inventoryVisible)
            {
                inventory.Draw(spriteBatch, this);
            }
        }


        public void PreDraw(SpriteBatch spriteBatch)
        {
            if (equippedWeapon != null)
            {
                if (equippedWeapon.weaponType == "One Handed Sword")
                {
                    float end = (direction == 1) ? 110 * MathHelper.Pi / 180 : -290 * MathHelper.Pi / 180;
                    SpriteEffects eff = (direction == 1) ? SpriteEffects.None : SpriteEffects.FlipVertically;

                    Vector2 Pos = (direction == 1) ? new Vector2(width, height / 2)
                                                    : new Vector2(0, height / 2);
                    Vector2 weaponPosition = position + Pos;
                    Vector2 weaponOrigin = (direction == 1) ? new Vector2(0, equippedWeapon.texture.Height) : new Vector2(0, 0);

                    if (isSwinging)
                    {
                        spriteBatch.Draw(equippedWeapon.texture, weaponPosition, null, Color.White, rotationAngle, weaponOrigin, 0.8f, eff, isControlled ? 0.841f : 0.741f);
                    }
                    else
                    {
                        spriteBatch.Draw(equippedWeapon.texture, weaponPosition, null, Color.White, end, weaponOrigin, 0.8f, eff, isControlled ? 0.841f : 0.741f);
                    }
                }
            }
        }
        public void PostDraw(SpriteBatch spriteBatch, float headRot) //0.8616f : 0.7616f MAX
        {
            if (health <= maxHealth)
            {
                float healthBarWidth = width * ((float)health / (float)maxHealth);
                int offSetY = 6;

                Rectangle healthBarRectangleBackground = new Rectangle((int)(position.X - 2), (int)(position.Y + height + offSetY - 1), width + 4, 4);
                Rectangle healthBarRectangleBackgroundRed = new Rectangle((int)(position.X), (int)(position.Y + height + offSetY), width, 2);
                Rectangle healthBarRectangle = new Rectangle((int)(position.X), (int)position.Y + height + offSetY, (int)healthBarWidth, 2);


                if (health < maxHealth)
                {
                    spriteBatch.DrawRectangle(healthBarRectangleBackground, Color.Black, isControlled ? 0.8613f : 0.7613f);
                    spriteBatch.DrawRectangle(healthBarRectangleBackgroundRed, Color.Red, isControlled ? 0.8614f : 0.7614f);
                    spriteBatch.DrawRectangle(healthBarRectangle, Color.Lime, isControlled ? 0.8615f : 0.7615f);
                }
            }


            SpriteEffects eff = (direction == 1) ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
            if (inventory.equipmentSlots[2].equippedItem != null) //Offhand
            {
                spriteBatch.Draw(inventory.equipmentSlots[2].equippedItem.texture, position + new Vector2(direction == 1 ? 0 : 22, height / 1.2f), null, Color.Lerp(Color.White, Color.DarkRed, immunityTime), 0f, origin, 0.8f, SpriteEffects.None, isControlled ? 0.8612f : 0.7612f);
            }
            if (inventory.equipmentSlots[1].equippedItem != null)//Body Armor
            {
                spriteBatch.Draw(inventory.equipmentSlots[1].equippedItem.texture, position + new Vector2(0, 22), null, Color.Lerp(Color.White, Color.DarkRed, immunityTime), 0f, Vector2.Zero, 1f, eff, isControlled ? 0.8519f : 0.7519f);
            }
            if (inventory.equipmentSlots[4].equippedItem != null)//Head Armor
            {
                Vector2 headOrigin = new Vector2(inventory.equipmentSlots[4].equippedItem.texture.Width / 2, inventory.equipmentSlots[4].equippedItem.texture.Height);
                int headOffset;
                switch (inventory.equipmentSlots[4].equippedItem.id)
                {
                    case 6:
                        headOffset = 2;
                        break;

                    default:
                        headOffset = 0;
                        break;
                }
                spriteBatch.Draw(inventory.equipmentSlots[4].equippedItem.texture, position + new Vector2(textureHead.Width / 2 - headOffset * direction, textureHead.Height), null, Color.Lerp(Color.White, Color.DarkRed, immunityTime), isControlled ? headRot : 0f, headOrigin, 1f, eff, isControlled ? 0.8611f : 0.7611f);
            }
        }

        private void PlayerInventoryInteractions(Keys key, List<Item> groundItems)
        {
            var inputManager = Input_Manager.Instance;
            bool isMouseOverItem = false;

            Rectangle closeInvSlotRectangle = new Rectangle((int)Main.inventoryPos.X + 170, (int)Main.inventoryPos.Y - 22, 20, 20);
            if (closeInvSlotRectangle.Contains(inputManager.mousePosition) && inputManager.IsButtonSingleClick(true))
            {
                inventoryVisible = false;
            }
            if (isControlled && inputManager.IsKeySinglePress(key))
            {
                if (inventoryVisible)
                {
                    inventoryVisible = false;
                }
                else
                {
                    Main.inventoryPos = new Vector2(inputManager.mousePosition.X - Main.texInventory.Width / 2, inputManager.mousePosition.Y);
                    inventoryVisible = true;
                }
            }

            if (isControlled && inventoryVisible)
            {
                for (int i = 0; i < inventory.equipmentSlots.Count; i++)
                {
                    Vector2 position = Main.EquipmentSlotPositions(i);
                    if (inventory.equipmentSlots[i].equippedItem != null)
                    {
                        if (inventory.IsEquipmentSlotHovered((int)position.X, (int)position.Y, i))
                        {
                            isMouseOverItem = true;
                            hoveredItem = inventory.GetEquippedItem(i);
                        }
                    }
                }
                for (int y = 0; y < inventory.height; y++)
                {
                    for (int x = 0; x < inventory.width; x++)
                    {
                        int slotSize = Main.inventorySlotSize;
                        int slotX = (int)Main.inventoryPos.X + x * slotSize;
                        int slotY = (int)Main.inventoryPos.Y + y * slotSize + Main.inventorySlotStartPos;


                        if (inventory.IsSlotHovered(slotX, slotY))
                        {
                            hoveredItem = inventory.GetItem(x, y);
                            isMouseOverItem = true;
                        }
                    }
                }
            }

            foreach (Item item in groundItems)
            {
                if (item.InteractsWithMouse())
                {
                    hoveredItem = item;
                    isMouseOverItem = true;
                }
            }

            if (!isMouseOverItem)
            {
                hoveredItem = null;
            }

            if (inputManager.IsButtonSingleClick(true))
            {
                if (isControlled && inventoryVisible)
                {
                    for (int y = 0; y < inventory.height; y++)
                    {
                        for (int x = 0; x < inventory.width; x++)
                        {
                            int slotSize = Main.inventorySlotSize;
                            int slotX = (int)Main.inventoryPos.X + x * slotSize;
                            int slotY = (int)Main.inventoryPos.Y + y * slotSize + Main.inventorySlotStartPos;

                            if (inventory.IsSlotHovered(slotX, slotY))
                            {
                                if (mouseItem == null)
                                {
                                    mouseItem = inventory.GetItem(x, y);
                                    inventory.RemoveItem(x, y);
                                }
                                else
                                {
                                    Item temp = mouseItem;
                                    mouseItem = inventory.GetItem(x, y);
                                    inventory.SetItem(x, y, temp);
                                }
                            }
                        }
                    }
                    for (int i = 0; i < inventory.equipmentSlots.Count; i++)
                    {
                        Vector2 position = Main.EquipmentSlotPositions(i);
                        if (inventory.IsEquipmentSlotHovered((int)position.X, (int)position.Y, i))
                        {
                            var equipSlot = inventory.equipmentSlots[i];
                            if (mouseItem == null)
                            {
                                mouseItem = inventory.GetEquippedItem(i);
                                equipSlot.equippedItem = null;
                            }
                            else
                            {
                                if (mouseItem.type == equipSlot.SlotType)
                                {
                                    Item temp = mouseItem;
                                    mouseItem = inventory.GetEquippedItem(i);
                                    equipSlot.equippedItem = temp;
                                }
                            }
                        }
                    }
                }
            }

            if (inputManager.IsButtonSingleClick(false))
            {
                if (isControlled)
                {
                    if (mouseItem != null)
                    {
                        mouseItem.position = position;
                        mouseItem.onGround = true;
                        groundItems.Add(mouseItem);
                        mouseItem = null;
                    }

                    if (inventoryVisible)
                    {
                        for (int y = 0; y < inventory.height; y++)
                        {
                            for (int x = 0; x < inventory.width; x++)
                            {
                                int slotSize = Main.inventorySlotSize;
                                int slotX = (int)Main.inventoryPos.X + x * slotSize;
                                int slotY = (int)Main.inventoryPos.Y + y * slotSize + Main.inventorySlotStartPos;
                                if (inventory.IsSlotHovered(slotX, slotY))
                                {
                                    hoveredItem = inventory.GetItem(x, y);
                                    if (hoveredItem != null)
                                    {
                                        inventory.EquipItem(hoveredItem, x, y);
                                    }
                                }
                            }
                        }

                        for (int i = 0; i < inventory.equipmentSlots.Count; i++)
                        {
                            Vector2 position = Main.EquipmentSlotPositions(i);
                            if (inventory.IsEquipmentSlotHovered((int)position.X, (int)position.Y, i))
                            {
                                if (!inventory.IsFull() && inventory.equipmentSlots[i].equippedItem != null)
                                {
                                    inventory.AddItem(inventory.equipmentSlots[i].equippedItem, groundItems);
                                    inventory.equipmentSlots[i].equippedItem = null;
                                }
                            }
                        }
                    }
                }
            }
        }

        private void AddItem(Keys key, bool addInventory, int itemID, Dictionary<int, Item> itemDictionary, Global_Item itemManager, List<Item> groundItems, List<Item> items)
        {
            var inputManager = Input_Manager.Instance;
            if (inputManager.IsKeySinglePress(key))
            {
                Random rand = new Random();
                int prefixID;
                int suffixID;

                prefixID = rand.Next(0, 4);
                suffixID = rand.Next(0, 4);

                if (addInventory)
                {
                    if (isControlled)
                    {
                        if (itemDictionary.TryGetValue(itemID, out var itemData))
                        {
                            inventory.AddItem(itemManager.NewItem(itemData, Vector2.Zero, prefixID, suffixID, 1, false), groundItems);
                        }
                    }
                }
                else
                {
                    itemManager.DropItem(rand.Next(items.Count), prefixID, suffixID, rand.Next(1, 4), inputManager.mousePosition);
                }
            }
        }
    }
}