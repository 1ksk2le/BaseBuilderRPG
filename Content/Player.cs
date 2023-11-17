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
        public bool isPicked { get; set; }
        public string name { get; set; }

        public Item equippedWeapon, mouseItem, hoveredItem;
        public Texture2D textureBody;
        public Texture2D textureHead;
        public Texture2D textureEye;
        private MouseState pMouse;
        private KeyboardState pKey;
        public Color skinColor;
        public Vector2 velocity, target, origin, center;
        public Rectangle rectangle, rectangleMelee;
        private Rectangle rectangleMeleeAI;
        public int direction = 1;
        public int width, height;
        public float immunityTime, immunityTimeMax, useTimer, meleeRange, rangedRange, rotationAngle;
        public bool inventoryVisible, isImmune, isSwinging, canHit, didSpawn;
        private bool aiAttackCheck = false;
        private string aiState;

        public Player(Texture2D texture, Texture2D headTexture, Texture2D eyeTexture, string name, Vector2 position, int healthMax, float skinColor, bool isActive)
        {
            this.skinColorFloat = skinColor;
            this.skinColor = GetSkinColor(this.skinColorFloat);
            this.position = position;
            this.name = name;

            textureBody = texture;
            textureHead = headTexture;
            textureEye = eyeTexture;
            health = 100;
            isPicked = isActive;
            maxHealth = healthMax;
            health = maxHealth;
            immunityTimeMax = 0.4f;
            immunityTime = 0f;
            width = textureBody.Width;
            height = textureBody.Height;
            origin = new Vector2(width / 2, height / 2);
            meleeRange = 2000f;
            inventory = new Inventory(5, 6);
            inventoryVisible = true;
            canHit = true;
            didSpawn = false;
            aiAttackCheck = false;
            aiState = "";
        }

        public void Update(GameTime gameTime, Dictionary<int, Item> itemDictionary, Item_Manager itemManager, Text_Manager textManager, Projectile_Manager projManager, List<NPC> npcs, List<Item> groundItems, List<Item> items)
        {
            rectangle = new Rectangle((int)position.X, (int)position.Y, width, height);
            center = position + origin;

            if (equippedWeapon != null)
            {
                if (equippedWeapon.damageType == "ranged")
                {
                    rangedRange = projManager.GetProjectile(equippedWeapon.shootID).lifeTimeMax * equippedWeapon.shootSpeed * 60;
                }
                Vector2 pos = (direction == 1) ? new Vector2(width + equippedWeapon.texture.Height * 0.2f, height / 2)
                                           : new Vector2(-equippedWeapon.texture.Height * 0.2f, height / 2);
                rectangleMelee = CalcMeleeRectangle(position + pos, (int)(equippedWeapon.texture.Width), (int)(equippedWeapon.texture.Height * 1.1f), rotationAngle);
                rectangleMeleeAI = new Rectangle((int)(center.X - equippedWeapon.texture.Height / 2), (int)(center.Y - equippedWeapon.texture.Height * 1.2f / 2), (int)(equippedWeapon.texture.Height), (int)(equippedWeapon.texture.Height * 1.2f));
            }

            KeyboardState keyboardState = Keyboard.GetState();

            if (immunityTime >= 0f)
            {
                isImmune = true;
                immunityTime -= (float)gameTime.ElapsedGameTime.TotalSeconds;
            }
            else
            {
                isImmune = false;
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
                if (!isPicked)
                {
                    AI(gameTime, npcs, projManager);

                }
                else
                {
                    Shoot(gameTime, projManager, new Vector2(pMouse.X, pMouse.Y));
                    aiState = "";
                    PlayerInventoryInteractions(Keys.I, groundItems);
                    Movement(Vector2.Zero, keyboardState);
                    Random rand = new Random();
                    AddItem(Keys.X, true, rand.Next(0, 11), itemDictionary, itemManager, groundItems, items);
                    inventory.SortItems(pMouse);
                    foreach (Item item in groundItems.ToList())
                    {
                        if (item.PlayerClose(this, 40f) && Keyboard.GetState().IsKeyDown(Keys.F) && !pKey.IsKeyDown(Keys.F) && item.onGround && !inventory.IsFull())
                        {
                            inventory.PickItem(textManager, this, item, groundItems);
                        }
                    }
                }

                if (equippedWeapon != null)
                {
                    if (equippedWeapon.damageType == "melee")
                    {
                        OneHandedSwing(gameTime);
                    }
                }
            }

            pKey = Keyboard.GetState();
            pMouse = Mouse.GetState();
        }

        private void AI(GameTime gameTime, List<NPC> npcs, Projectile_Manager projManager)
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

                            Vector2 targetDirection = target - center;
                            targetDirection.Normalize();
                            position += targetDirection * 1.5f;
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

                            Vector2 targetDirection = target - center;
                            targetDirection.Normalize();
                            position += targetDirection * 1.5f;
                            aiState = "Moving to target: [" + targetNPC.name + "]";
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

                                Vector2 targetDirection = target - center;
                                targetDirection.Normalize();
                                position -= targetDirection * 1.5f;
                                aiState = "Running away from: [" + targetNPC.name + "]";
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

        private void OneHandedSwing(GameTime gameTime)
        {
            if (equippedWeapon != null && equippedWeapon.weaponType == "One Handed Sword")
            {
                float start = (direction == 1) ? -90 * MathHelper.Pi / 180 : -90 * MathHelper.Pi / 180;
                float end = (direction == 1) ? 110 * MathHelper.Pi / 180 : -290 * MathHelper.Pi / 180;
                if (isPicked)
                {
                    if (Mouse.GetState().LeftButton == ButtonState.Pressed)
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
                        float progress = useTimer / equippedWeapon.useTime;
                        rotationAngle = MathHelper.Lerp(start, end, progress);
                    }
                }
            }
        }


        private void Movement(Vector2 movement, KeyboardState keyboardState, float Speed = 1.5f)
        {
            MouseState mouseState = Mouse.GetState();

            if (isPicked)
            {
                if (position.X > mouseState.X)
                {
                    direction = -1;
                }
                else
                {
                    direction = 1;
                }


                if (keyboardState.IsKeyDown(Keys.W))
                    movement.Y = -Speed;
                if (keyboardState.IsKeyDown(Keys.S))
                    movement.Y = Speed;
                if (keyboardState.IsKeyDown(Keys.A))
                    movement.X = -Speed;
                if (keyboardState.IsKeyDown(Keys.D))
                    movement.X = Speed;

                if (movement != Vector2.Zero)
                    movement.Normalize();

                velocity = movement * Speed;

                position += velocity;
            }
            else
            {
                velocity = Vector2.Zero;
            }
        }

        private Color GetSkinColor(float progress)
        {
            Color black = new Color(94, 54, 33);
            Color white = new Color(255, 220, 185);
            return Color.Lerp(black, white, progress);
        }

        public void GetDamaged(Text_Manager texMan, int damage)
        {
            health -= damage;
            texMan.AddFloatingText("-" + damage.ToString(), "", new Vector2(position.X + textureBody.Width / 2, position.Y), Color.Red, Color.Transparent, 1f, 1.1f);
            immunityTime = immunityTimeMax;
        }

        private void PlayerInventoryInteractions(Keys key, List<Item> groundItems)
        {
            bool isMouseOverItem = false;

            Rectangle closeInvSlotRectangle = new Rectangle((int)Main.inventoryPos.X + 170, (int)Main.inventoryPos.Y - 22, 20, 20);
            if (closeInvSlotRectangle.Contains(Mouse.GetState().X, Mouse.GetState().Y) && Mouse.GetState().LeftButton == ButtonState.Pressed && pMouse.LeftButton == ButtonState.Released)
            {
                inventoryVisible = false;
            }
            if (isPicked && Keyboard.GetState().IsKeyDown(key) && !pKey.IsKeyDown(key))
            {
                if (inventoryVisible)
                {
                    inventoryVisible = false;
                }
                else
                {
                    Main.inventoryPos = new Vector2(Mouse.GetState().X - Main.texInventory.Width / 2, Mouse.GetState().Y);
                    inventoryVisible = true;
                }
            }

            if (isPicked && inventoryVisible)
            {
                for (int i = 0; i < inventory.equipmentSlots.Count; i++)
                {
                    Vector2 position = Main.EquipmentSlotPositions(i);
                    if (inventory.equipmentSlots[i].equippedItem != null)
                    {
                        if (inventory.IsEquipmentSlotHovered((int)position.X, (int)position.Y, i))
                        {
                            isMouseOverItem = true;
                            hoveredItem = inventory.GetEquippedItem(i); // Adjust the slot parameter as needed
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

            if (Mouse.GetState().LeftButton == ButtonState.Pressed && pMouse.LeftButton == ButtonState.Released)
            {
                if (isPicked && inventoryVisible)
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

            if (Mouse.GetState().RightButton == ButtonState.Pressed && pMouse.RightButton == ButtonState.Released)
            {
                if (isPicked)
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

        private void AddItem(Keys key, bool addInventory, int itemID, Dictionary<int, Item> itemDictionary, Item_Manager itemManager, List<Item> groundItems, List<Item> items)
        {
            if (Keyboard.GetState().IsKeyDown(key) && !pKey.IsKeyDown(key))
            {
                Random rand = new Random();
                int prefixID;
                int suffixID;

                prefixID = rand.Next(0, 4);
                suffixID = rand.Next(0, 4);

                if (addInventory)
                {
                    if (isPicked)
                    {
                        if (itemDictionary.TryGetValue(itemID, out var itemData))
                        {
                            inventory.AddItem(itemManager.NewItem(itemData, Vector2.Zero, prefixID, suffixID, 1, false), groundItems);
                        }
                    }
                }
                else
                {
                    Vector2 mousePosition = new Vector2(Mouse.GetState().X, Mouse.GetState().Y);
                    itemManager.DropItem(rand.Next(items.Count), prefixID, suffixID, rand.Next(1, 4), mousePosition);
                }
            }
        }

        private void Shoot(GameTime gameTime, Projectile_Manager projManager, Vector2 target)
        {
            if (equippedWeapon != null && equippedWeapon.shootID > -1)
            {
                if (useTimer > 0)
                {
                    useTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds;
                }

                if (useTimer <= 0)
                {
                    if (isPicked)
                    {
                        if (Mouse.GetState().LeftButton == ButtonState.Pressed)
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
                if (equippedWeapon != null)
                {
                    if (equippedWeapon.damageType == "melee")
                    {
                        spriteBatch.DrawCircle(center, meleeRange, Color.Blue, 64, 0.012f);
                        spriteBatch.DrawRectangleWithBorder(rectangleMelee, Color.Blue, 1f, 0.011f);
                        spriteBatch.DrawRectangleWithBorder(rectangleMeleeAI, Color.Cyan, 1f, 0.011f);
                    }
                    else if (equippedWeapon.damageType == "ranged")
                    {
                        spriteBatch.DrawCircle(center, rangedRange, Color.Cyan, 64, 0.012f);

                    }

                }
                spriteBatch.DrawCircle(center, 4f, Color.Blue * 1.5f, 64, 1f);
                spriteBatch.DrawLine(center, target, Color.Blue, 0.013f);
                spriteBatch.DrawRectangleWithBorder(rectangle, Color.Blue, 1f, 0.012f);
            }

            PreDraw(spriteBatch);

            Color nameColor;
            if (rectangle.Contains(Mouse.GetState().X, Mouse.GetState().Y))
            {
                nameColor = Color.Lime;
            }
            else
            {
                nameColor = Color.White;
            }
            Vector2 textPosition = position + new Vector2(0, -14);
            textPosition.X = position.X + width / 2 - Main.testFont.MeasureString(name).X / 2;

            Vector2 textPosition2 = position + new Vector2(0, height + 10);
            textPosition2.X = position.X + width / 2 - Main.testFont.MeasureString(aiState).X / 2;

            spriteBatch.DrawStringWithOutline(Main.testFont, name, textPosition, Color.Black, isPicked ? Color.Yellow : nameColor, 1f, isPicked ? 0.8616f : 0.7616f);
            spriteBatch.DrawStringWithOutline(Main.testFont, aiState, textPosition2, Color.Black, isPicked ? Color.Yellow : nameColor, 1f, isPicked ? 0.8617f : 0.7617f);

            SpriteEffects eff = (direction == 1) ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
            MouseState mouseState = Mouse.GetState();
            Vector2 mousePosition = new Vector2(mouseState.X, mouseState.Y);
            Vector2 directionToMouse = mousePosition - position;

            float maxHeadRotation = MathHelper.ToRadians(20);
            float rotation = (float)Math.Atan2(directionToMouse.Y * direction, directionToMouse.X * direction);
            rotation = MathHelper.Clamp(rotation, -maxHeadRotation, maxHeadRotation);

            spriteBatch.Draw(textureBody, position, null, Color.Lerp(skinColor, Color.DarkRed, immunityTime), 0f, Vector2.Zero, 1f, eff, isPicked ? 0.851f : 0.751f);

            if (isPicked)
            {
                Vector2 headOrigin = new Vector2(textureHead.Width / 2, textureHead.Height);
                Vector2 eyesOrigin = new Vector2(textureEye.Width / 2, (textureEye.Height) / 2);

                spriteBatch.Draw(textureHead, position + new Vector2(textureHead.Width / 2 - 2 * direction, textureHead.Height), null, Color.Lerp(skinColor, Color.DarkRed, immunityTime), rotation, headOrigin, 1f, eff, 0.852f);

                Rectangle sourceRect = new Rectangle(0, 0, textureEye.Width, textureEye.Height / 2);
                spriteBatch.Draw(textureEye, position + new Vector2(textureEye.Width / 2 - 2 * direction, textureEye.Height / 2), sourceRect, Color.Lerp(Color.White, Color.DarkRed, immunityTime), rotation, eyesOrigin, 1f, eff, 0.853f);
            }
            else
            {
                spriteBatch.Draw(textureHead, position + new Vector2(-2 * direction, 0), null, Color.Lerp(skinColor, Color.DarkRed, immunityTime), 0f, Vector2.Zero, 1f, eff, 0.752f);
                Rectangle sourceRect = new Rectangle(0, 0, textureEye.Width, textureEye.Height / 2);
                spriteBatch.Draw(textureEye, position + new Vector2(-2 * direction, 0), sourceRect, Color.Lerp(Color.White, Color.DarkRed, immunityTime), 0f, Vector2.Zero, 1f, eff, 0.753f);
            }

            PostDraw(spriteBatch, rotation);

            if (isPicked && inventoryVisible)
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
                        spriteBatch.Draw(equippedWeapon.texture, weaponPosition, null, Color.White, rotationAngle, weaponOrigin, 0.8f, eff, isPicked ? 0.841f : 0.741f);
                    }
                    else
                    {
                        spriteBatch.Draw(equippedWeapon.texture, weaponPosition, null, Color.White, end, weaponOrigin, 0.8f, eff, isPicked ? 0.841f : 0.741f);
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
                    spriteBatch.DrawRectangle(healthBarRectangleBackground, Color.Black, isPicked ? 0.8613f : 0.7613f);
                    spriteBatch.DrawRectangle(healthBarRectangleBackgroundRed, Color.Red, isPicked ? 0.8614f : 0.7614f);
                    spriteBatch.DrawRectangle(healthBarRectangle, Color.Lime, isPicked ? 0.8615f : 0.7615f);
                }
            }


            SpriteEffects eff = (direction == 1) ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
            if (inventory.equipmentSlots[2].equippedItem != null) //Offhand
            {
                spriteBatch.Draw(inventory.equipmentSlots[2].equippedItem.texture, position + new Vector2(direction == 1 ? 0 : 22, height / 1.2f), null, Color.Lerp(Color.White, Color.DarkRed, immunityTime), 0f, origin, 0.8f, SpriteEffects.None, isPicked ? 0.8612f : 0.7612f);
            }
            if (inventory.equipmentSlots[1].equippedItem != null)//Body Armor
            {
                spriteBatch.Draw(inventory.equipmentSlots[1].equippedItem.texture, position + new Vector2(0, 22), null, Color.Lerp(Color.White, Color.DarkRed, immunityTime), 0f, Vector2.Zero, 1f, eff, isPicked ? 0.8519f : 0.7519f);
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
                spriteBatch.Draw(inventory.equipmentSlots[4].equippedItem.texture, position + new Vector2(textureHead.Width / 2 - headOffset * direction, textureHead.Height), null, Color.Lerp(Color.White, Color.DarkRed, immunityTime), isPicked ? headRot : 0f, headOrigin, 1f, eff, isPicked ? 0.8611f : 0.7611f);
            }
        }
    }
}