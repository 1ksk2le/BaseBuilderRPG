using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BaseBuilderRPG.Content
{
    public class Player_ControlHandler
    {
        private Player player;

        public Player_ControlHandler(Player player)
        {
            this.player = player;
        }
        public void Movement(Vector2 movement, KeyboardState keyboardState)
        {
            if (player.isControlled)
            {
                if (player.isControlled)
                {
                    player.direction = player.position.X > (int)Input_Manager.Instance.mousePosition.X ? -1 : 1;
                }

                if (keyboardState.IsKeyDown(Keys.W))
                    movement.Y = -player.speed;
                if (keyboardState.IsKeyDown(Keys.S))
                    movement.Y = player.speed;
                if (keyboardState.IsKeyDown(Keys.A))
                    movement.X = -player.speed;
                if (keyboardState.IsKeyDown(Keys.D))
                    movement.X = player.speed;

                if (movement != Vector2.Zero)
                    movement.Normalize();

                player.velocity = movement * player.speed;

                player.position += player.velocity;
            }
        }

        public void UseItem(GameTime gameTime, Projectile_Globals projManager, Vector2 target)
        {
            if (player.equippedWeapon != null && !player.inventory.IsInventoryHovered())
            {
                if (player.equippedWeapon.shootID > -1)
                {
                    if (player.useTimer <= 0)
                    {
                        if (player.isControlled)
                        {
                            if (Input_Manager.Instance.IsButtonPressed(true))
                            {
                                projManager.NewProjectile(player.equippedWeapon.shootID, player.center, target, player.equippedWeapon.damage, player.equippedWeapon.shootSpeed, player.equippedWeapon.knockBack, player, true);
                                player.useTimer = player.equippedWeapon.useTime;
                            }
                        }
                    }
                }
                if (player.equippedWeapon.weaponType == "One Handed Sword")
                {
                    if (player.useTimer <= 0)
                    {
                        if (player.isControlled)
                        {
                            if (Input_Manager.Instance.IsButtonPressed(true))
                            {
                                projManager.NewProjectile(0, player.center, target, player.equippedWeapon.damage, player.equippedWeapon.shootSpeed, player.equippedWeapon.knockBack, player, true);
                                player.useTimer = player.equippedWeapon.useTime;
                            }
                        }
                    }
                }
            }
        }

        public void PlayerInventoryInteractions(Keys key, List<Item> groundItems, Text_Manager textManager, Dictionary<int, Item> itemDictionary, Item_Globals globalItem, List<Item> items)
        {
            var inputManager = Input_Manager.Instance;
            bool isMouseOverItem = false;

            player.inventory.SortItems();
            AddItem(Keys.X, true, Main.random.Next(0, 12), itemDictionary, globalItem, groundItems, items);
            PickItem(groundItems, inputManager, textManager);


            Rectangle closeInvSlotRectangle = new Rectangle((int)Main.inventoryPos.X + 170, (int)Main.inventoryPos.Y - 22, 20, 20);
            if (closeInvSlotRectangle.Contains(inputManager.mousePosition) && inputManager.IsButtonSingleClick(true))
            {
                player.inventoryVisible = false;
            }
            if (player.isControlled && inputManager.IsKeySinglePress(key))
            {
                if (player.inventoryVisible)
                {
                    player.inventoryVisible = false;
                }
                else
                {
                    Main.inventoryPos = new Vector2(inputManager.mousePosition.X - Main.texInventory.Width / 2, inputManager.mousePosition.Y);
                    player.inventoryVisible = true;
                }
            }

            if (player.isControlled && player.inventoryVisible)
            {
                for (int i = 0; i < player.inventory.equipmentSlots.Count; i++)
                {
                    Vector2 position = Main.EquipmentSlotPositions(i);
                    if (player.inventory.equipmentSlots[i].equippedItem != null)
                    {
                        if (player.inventory.IsEquipmentSlotHovered((int)position.X, (int)position.Y, i))
                        {
                            isMouseOverItem = true;
                            player.hoveredItem = player.inventory.GetEquippedItem(i);
                        }
                    }
                }
                for (int y = 0; y < player.inventory.height; y++)
                {
                    for (int x = 0; x < player.inventory.width; x++)
                    {
                        int slotSize = Main.inventorySlotSize;
                        int slotX = (int)Main.inventoryPos.X + x * slotSize;
                        int slotY = (int)Main.inventoryPos.Y + y * slotSize + Main.inventorySlotStartPos;

                        if (player.inventory.IsSlotHovered(slotX, slotY))
                        {
                            player.hoveredItem = player.inventory.GetItem(x, y);
                            isMouseOverItem = true;
                        }
                    }
                }
            }

            foreach (Item item in groundItems)
            {
                if (item.InteractsWithMouse())
                {
                    player.hoveredItem = item;
                    isMouseOverItem = true;
                }
            }

            if (!isMouseOverItem)
            {
                player.hoveredItem = null;
            }

            if (inputManager.IsButtonSingleClick(true))
            {
                if (player.isControlled && player.inventoryVisible)
                {
                    for (int y = 0; y < player.inventory.height; y++)
                    {
                        for (int x = 0; x < player.inventory.width; x++)
                        {
                            int slotSize = Main.inventorySlotSize;
                            int slotX = (int)Main.inventoryPos.X + x * slotSize;
                            int slotY = (int)Main.inventoryPos.Y + y * slotSize + Main.inventorySlotStartPos;

                            if (player.inventory.IsSlotHovered(slotX, slotY))
                            {
                                if (player.mouseItem == null)
                                {
                                    player.mouseItem = player.inventory.GetItem(x, y);
                                    player.inventory.RemoveItem(x, y);
                                }
                                else
                                {
                                    Item temp = player.mouseItem;
                                    player.mouseItem = player.inventory.GetItem(x, y);
                                    player.inventory.SetItem(x, y, temp);
                                }
                            }
                        }
                    }
                    for (int i = 0; i < player.inventory.equipmentSlots.Count; i++)
                    {
                        Vector2 position = Main.EquipmentSlotPositions(i);
                        if (player.inventory.IsEquipmentSlotHovered((int)position.X, (int)position.Y, i))
                        {
                            var equipSlot = player.inventory.equipmentSlots[i];
                            if (player.mouseItem == null)
                            {
                                player.mouseItem = player.inventory.GetEquippedItem(i);
                                equipSlot.equippedItem = null;
                            }
                            else
                            {
                                if (player.mouseItem.type == equipSlot.SlotType)
                                {
                                    Item temp = player.mouseItem;
                                    player.mouseItem = player.inventory.GetEquippedItem(i);
                                    equipSlot.equippedItem = temp;
                                }
                            }
                        }
                    }
                }
            }

            if (inputManager.IsButtonSingleClick(false))
            {
                if (player.isControlled)
                {
                    if (player.mouseItem != null)
                    {
                        player.mouseItem.position = player.position;
                        player.mouseItem.onGround = true;
                        groundItems.Add(player.mouseItem);
                        player.mouseItem = null;
                    }

                    if (player.inventoryVisible)
                    {
                        for (int y = 0; y < player.inventory.height; y++)
                        {
                            for (int x = 0; x < player.inventory.width; x++)
                            {
                                int slotSize = Main.inventorySlotSize;
                                int slotX = (int)Main.inventoryPos.X + x * slotSize;
                                int slotY = (int)Main.inventoryPos.Y + y * slotSize + Main.inventorySlotStartPos;
                                if (player.inventory.IsSlotHovered(slotX, slotY))
                                {
                                    player.hoveredItem = player.inventory.GetItem(x, y);
                                    if (player.hoveredItem != null)
                                    {
                                        player.inventory.EquipItem(player.hoveredItem, x, y);
                                    }
                                }
                            }
                        }

                        for (int i = 0; i < player.inventory.equipmentSlots.Count; i++)
                        {
                            Vector2 position = Main.EquipmentSlotPositions(i);
                            if (player.inventory.IsEquipmentSlotHovered((int)position.X, (int)position.Y, i))
                            {
                                if (!player.inventory.IsFull() && player.inventory.equipmentSlots[i].equippedItem != null)
                                {
                                    player.inventory.AddItem(player.inventory.equipmentSlots[i].equippedItem, groundItems);
                                    player.inventory.equipmentSlots[i].equippedItem = null;
                                }
                            }
                        }
                    }
                }
            }
        }

        public void AddItem(Keys key, bool addInventory, int itemID, Dictionary<int, Item> itemDictionary, Item_Globals itemManager, List<Item> groundItems, List<Item> items)
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
                    if (player.isControlled)
                    {
                        if (itemDictionary.TryGetValue(itemID, out var itemData))
                        {
                            player.inventory.AddItem(itemManager.NewItem(itemData, Vector2.Zero, prefixID, suffixID, 1, false), groundItems);
                        }
                    }
                }
                else
                {
                    itemManager.DropItem(rand.Next(items.Count), prefixID, suffixID, rand.Next(1, 4), inputManager.mousePosition);
                }
            }
        }

        public void PickItem(List<Item> groundItems, Input_Manager inputManager, Text_Manager textManager)
        {
            Item targetItem = null;
            float closestDistance = 30f * 30f;

            foreach (Item item in groundItems.ToList())
            {
                float distance = Vector2.DistanceSquared(player.center, item.center);

                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    targetItem = item;
                }
            }
            if (targetItem != null && targetItem.PlayerClose(player, 40f) && inputManager.IsKeySinglePress(Keys.F) && targetItem.onGround && !player.inventory.IsFull())
            {
                string text = "Picked: " + targetItem.prefixName + " " + targetItem.name + " " + targetItem.suffixName;
                Vector2 textSize = Main.testFont.MeasureString(text);

                Vector2 textPos = player.position + new Vector2(-textSize.X / 5f, -10);
                textManager.AddFloatingText("Picked: ", (targetItem.prefixName + " " + targetItem.name + " " + targetItem.suffixName), textPos, new Vector2(0, 10), Color.White, targetItem.rarityColor, 0.75f, 1f);
                player.inventory.PickItem(player, targetItem, groundItems);
            }
        }
    }
}