using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace BaseBuilderRPG.Content
{
    public class Inventory
    {
        private Item[,] items;
        public List<EquipmentSlot> equipmentSlots;
        public int width { get; }
        public int height { get; }

        public Inventory(int width, int height)
        {
            this.width = width;
            this.height = height;
            items = new Item[width, height];

            equipmentSlots = new List<EquipmentSlot>
            {
                new EquipmentSlot("Weapon"),
                new EquipmentSlot("Body Armor"),
                new EquipmentSlot("Offhand"),
                new EquipmentSlot("Boots"),
                new EquipmentSlot("Head Armor"),
                new EquipmentSlot("Accessory")
            };
        }

        public void Draw(SpriteBatch spriteBatch, Player player)
        {
            var mousePosition = Input_Manager.Instance.mousePosition;
            DrawInventory(spriteBatch, player.mouseItem, player.hoveredItem);

            int slotSize = Main.inventorySlotSize;

            Rectangle sourceRect = new Rectangle(0, 0, Main.texInventoryExtras.Width, Main.texInventoryExtras.Height / 2);
            Rectangle sourceRect2 = new Rectangle(0, Main.texInventoryExtras.Height / 2, Main.texInventoryExtras.Width, Main.texInventoryExtras.Height / 2);
            spriteBatch.Draw(Main.texInventoryExtras, Main.inventoryPos + new Vector2(0, -22), sourceRect, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.9502f);
            spriteBatch.Draw(Main.texInventoryExtras, Main.inventoryPos + new Vector2(0, 374), sourceRect2, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.9501f);
            spriteBatch.Draw(Main.texInventory, Main.inventoryPos, null, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.95f);

            Rectangle sortSlotRectangle = new Rectangle((int)Main.inventoryPos.X + 84, (int)Main.inventoryPos.Y + 374, 20, 20);
            if (sortSlotRectangle.Contains(mousePosition))
            {
                string text = "Sort Inventory";
                Vector2 textSize = Main.testFont.MeasureString(text);
                spriteBatch.DrawRectangle(new Rectangle((int)mousePosition.X + 14, (int)mousePosition.Y - 2, (int)textSize.X + 8, (int)textSize.Y + 4), Color.Black, 0.9503f);
                spriteBatch.DrawStringWithOutline(Main.testFont, text, new Vector2((int)mousePosition.X + 18, mousePosition.Y), Color.Black, Color.White, 1f, 0.95031f);
            }

            Rectangle closeInvSlotRectangle = new Rectangle((int)Main.inventoryPos.X + 170, (int)Main.inventoryPos.Y - 22, 20, 20);
            if (closeInvSlotRectangle.Contains(Input_Manager.Instance.mousePosition))
            {
                string text = "Close Inventory";
                Vector2 textSize = Main.testFont.MeasureString(text);
                spriteBatch.DrawRectangle(new Rectangle((int)mousePosition.X + 14, (int)mousePosition.Y - 2, (int)textSize.X + 8, (int)textSize.Y + 4), Color.Black, 0.9503f);
                spriteBatch.DrawStringWithOutline(Main.testFont, text, new Vector2((int)mousePosition.X + 18, mousePosition.Y), Color.Black, Color.White, 1f, 0.95031f);
            }

            for (int width = 0; width < player.inventory.width; width++)
            {
                for (int height = 0; height < player.inventory.height; height++)
                {
                    int x = (int)Main.inventoryPos.X + width * slotSize;
                    int y = (int)Main.inventoryPos.Y + height * slotSize + 148;

                    Item item = player.inventory.GetItem(width, height);
                    if (item != null)
                    {
                        spriteBatch.Draw(Main.texInventorySlotBackground, new Vector2(x, y), null, new Color((int)item.rarityColor.R, item.rarityColor.G, item.rarityColor.B, 255), 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.96f);

                        float scale = 1f;
                        int insideSlotSize = 38;
                        if (item.texture.Width > insideSlotSize || item.texture.Height > insideSlotSize)
                        {
                            scale = Math.Min((float)insideSlotSize / item.texture.Width, (float)slotSize / item.texture.Height);
                        }

                        Vector2 itemPosition = new Vector2(x + insideSlotSize / 2, y + insideSlotSize / 2);

                        spriteBatch.Draw(item.texture, itemPosition + new Vector2(0, 4), null, new Color(0, 0, 0, 150), 0f, new Vector2(item.texture.Width / 2, item.texture.Height / 2), scale * 1.05f, SpriteEffects.None, 0.97f);
                        spriteBatch.Draw(item.texture, itemPosition + new Vector2(0, -2), null, Color.White, 0f, new Vector2(item.texture.Width / 2, item.texture.Height / 2), scale, SpriteEffects.None, 0.98f);

                        if (item.stackLimit > 1)
                        {
                            spriteBatch.DrawStringWithOutline(Main.testFont, item.stackSize.ToString(), new Vector2(x + 20, y + 24), Color.Black, Color.White, 1f, 0.9820f);
                        }
                    }
                }
            }

            for (int i = 0; i < equipmentSlots.Count; i++)
            {
                if (equipmentSlots[i].equippedItem != null)
                {
                    Vector2 position = Main.EquipmentSlotPositions(i);
                    if (equipmentSlots[i].equippedItem.type == "Accessory")
                    {
                        // Handle accessory-specific logic if needed
                    }
                    else
                    {
                        int equipmentSlotSize = 44;
                        float scale = 1f;
                        if (equipmentSlots[i].equippedItem.texture.Width > equipmentSlotSize || equipmentSlots[i].equippedItem.texture.Height > equipmentSlotSize)
                        {
                            scale = Math.Min((float)equipmentSlotSize / equipmentSlots[i].equippedItem.texture.Width, (float)equipmentSlotSize / equipmentSlots[i].equippedItem.texture.Height);
                        }

                        Vector2 itemPosition = new(position.X + equipmentSlotSize / 2, position.Y + equipmentSlotSize / 2);

                        spriteBatch.Draw(Main.texMainSlotBackground, position, null, equipmentSlots[i].equippedItem.rarityColor, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.96f);

                        spriteBatch.Draw(equipmentSlots[i].equippedItem.texture, itemPosition + new Vector2(0, 4), null, new Color(0, 0, 0, 150), 0f, new Vector2(equipmentSlots[i].equippedItem.texture.Width / 2, equipmentSlots[i].equippedItem.texture.Height / 2), scale * 1.2f, SpriteEffects.None, 0.97f);
                        spriteBatch.Draw(equipmentSlots[i].equippedItem.texture, itemPosition + new Vector2(0, -2), null, Color.White, 0f, new Vector2(equipmentSlots[i].equippedItem.texture.Width / 2, equipmentSlots[i].equippedItem.texture.Height / 2), scale, SpriteEffects.None, 0.98f);
                    }
                }
            }
        }

        public void EquipItem(Item item, int x, int y)
        {
            foreach (var slot in equipmentSlots)
            {
                if (slot.SlotType == item.type)
                {
                    if (slot.equippedItem == null)
                    {
                        slot.equippedItem = item;
                        items[x, y] = null;
                    }
                    else
                    {
                        var existingItem = slot.equippedItem;
                        items[x, y] = existingItem;
                        slot.equippedItem = item;
                    }
                    return;
                }
            }
        }

        public void AddItem(Item item, List<Item> droppedItems)
        {
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Item existingStack = items[x, y];
                    if (existingStack != null && existingStack.id == item.id && existingStack.stackSize < existingStack.stackLimit)
                    {
                        int spaceAvailable = existingStack.stackLimit - existingStack.stackSize;

                        if (spaceAvailable >= item.stackSize)
                        {
                            existingStack.stackSize += item.stackSize;
                            item.stackSize = 0;
                            return;
                        }
                        else
                        {
                            existingStack.stackSize = existingStack.stackLimit;
                            item.stackSize -= spaceAvailable;
                        }
                    }
                }
            }

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (items[x, y] == null)
                    {
                        items[x, y] = item;
                        return;
                    }
                }
            }

            if (item.stackSize > 0)
            {
                droppedItems.Add(item);
            }
        }

        public void PickItem(Player player, Item item, List<Item> droppedItems)
        {
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Item existingStack = items[x, y];
                    if (existingStack != null && existingStack.id == item.id && existingStack.stackSize < existingStack.stackLimit)
                    {
                        int spaceAvailable = existingStack.stackLimit - existingStack.stackSize;

                        if (spaceAvailable >= item.stackSize)
                        {
                            existingStack.stackSize += item.stackSize;
                            item.stackSize = 0;

                            if (item.onGround)
                            {
                                droppedItems.Remove(item);
                            }

                            return;
                        }
                        else
                        {
                            existingStack.stackSize = existingStack.stackLimit;
                            item.stackSize -= spaceAvailable;
                        }
                    }
                }
            }

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (items[x, y] == null)
                    {
                        items[x, y] = item;

                        if (item.onGround)
                        {
                            droppedItems.Remove(item);
                        }

                        return;
                    }
                }
            }
        }

        public void ClearInventory()
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    items[x, y] = null;
                }
            }
        }

        public void ClearEquippedItems()
        {
            for (int x = 0; x < equipmentSlots.Count; x++)
            {
                equipmentSlots[x].equippedItem = null;
            }
        }

        public bool RemoveItem(int x, int y)
        {
            if (x >= 0 && x < width && y >= 0 && y < height && items[x, y] != null)
            {
                items[x, y] = null;
                return true;
            }
            return false;
        }

        public Item GetItem(int x, int y)
        {
            if (x >= 0 && x < width && y >= 0 && y < height)
            {
                return items[x, y];
            }
            return null;
        }

        public Item GetEquippedItem(int slot)
        {
            if (equipmentSlots[slot].equippedItem != null)
            {
                return equipmentSlots[slot].equippedItem;
            }
            else
            {
                return null;
            }
        }

        public List<Item> GetAllItems()
        {
            List<Item> itemList = new List<Item>();

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Item item = items[x, y];
                    if (item != null)
                    {
                        itemList.Add(item);
                    }
                }
            }

            return itemList;
        }

        public void SortItems()
        {
            Rectangle slotRect = new Rectangle((int)Main.inventoryPos.X + 84, (int)Main.inventoryPos.Y + 374, 20, 20);
            if (slotRect.Contains(Input_Manager.Instance.mousePosition))
            {
                if (Input_Manager.Instance.IsButtonSingleClick(true))
                {
                    List<Item> itemList = GetAllItems();

                    itemList.Sort((item1, item2) =>
                    {
                        if (item1.type == "Weapon" && item2.type != "Weapon")
                        {
                            return -1;
                        }
                        else if (item1.type != "Weapon" && item2.type == "Weapon")
                        {
                            return 1;
                        }
                        else
                        {
                            int rarityComparison = item2.rarity.CompareTo(item1.rarity);
                            if (rarityComparison == 0)
                            {
                                return item2.stackSize.CompareTo(item1.stackSize);
                            }
                            return rarityComparison;
                        }
                    });

                    int index = 0;
                    for (int y = 0; y < height; y++)
                    {
                        for (int x = 0; x < width; x++)
                        {
                            if (index < itemList.Count)
                            {
                                items[x, y] = itemList[index];
                                index++;
                            }
                            else
                            {
                                items[x, y] = null;
                            }
                        }
                    }
                }
            }
        }

        public void SetItem(int x, int y, Item item)
        {
            if (x >= 0 && x < width && y >= 0 && y < height)
            {
                items[x, y] = item;
            }
        }

        public bool IsSlotHovered(int slotX, int slotY)
        {
            Rectangle slotRect = new Rectangle(slotX, slotY, Main.inventorySlotSize, Main.inventorySlotSize);
            return slotRect.Contains(Input_Manager.Instance.mousePosition);
        }

        public bool IsEquipmentSlotHovered(int x, int y, int slot)
        {
            if (slot == 5)
            {
                Rectangle slotRect = new(x, y, 44, 44);
                if (slotRect.Contains(Input_Manager.Instance.mousePosition))
                {
                    return true;
                }
            }
            else
            {
                Rectangle slotRect = new Rectangle(x, y, 44, 44);
                if (slotRect.Contains(Input_Manager.Instance.mousePosition))
                {
                    return true;
                }
            }
            return false;
        }

        public bool IsInventoryHovered()
        {
            Rectangle slotRect = new((int)Main.inventoryPos.X, (int)Main.inventoryPos.Y - Main.texInventoryExtras.Height / 2, Main.texInventory.Width, Main.texInventory.Height + Main.texInventoryExtras.Height);
            if (slotRect.Contains(Input_Manager.Instance.mousePosition))
            {
                return true;
            }
            return false;
        }

        public bool IsFull()
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (items[x, y] == null)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        private void DrawInventory(SpriteBatch spriteBatch, Item mouseItem, Item hoveredItem)
        {
            if (hoveredItem != null && mouseItem == null)
            {
                float maxTextWidth = 0;
                foreach (string tooltip in hoveredItem.toolTips)
                {
                    Vector2 textSize = Main.testFont.MeasureString(tooltip);
                    maxTextWidth = Math.Max(maxTextWidth, textSize.X);
                }

                int initialX = (int)Input_Manager.Instance.mousePosition.X + 18;
                int initialY = (int)Input_Manager.Instance.mousePosition.Y;

                for (int i = 0; i < hoveredItem.toolTips.Count; i++)
                {
                    Color toolTipColor, bgColor;
                    switch (i)
                    {
                        case 0:
                            toolTipColor = Color.White;
                            bgColor = hoveredItem.rarityColor;
                            break;

                        case 1:
                            toolTipColor = Color.Yellow;
                            bgColor = Color.Black;
                            break;

                        default:
                            toolTipColor = Color.White;
                            bgColor = Color.Black;
                            break;
                    }

                    if (hoveredItem.toolTips[i].StartsWith("'"))
                    {
                        toolTipColor = Color.Aquamarine;
                    }

                    Vector2 textSize = Main.testFont.MeasureString(hoveredItem.toolTips[i]);
                    Vector2 backgroundSize = new Vector2(maxTextWidth, textSize.Y);

                    int tooltipX = initialX;
                    int tooltipY = initialY + i * ((int)textSize.Y);

                    if (tooltipX + (int)backgroundSize.X + 8 > /*GraphicsDevice.Viewport.Width*/1500)
                    {
                        tooltipX = (int)Input_Manager.Instance.mousePosition.X - (int)backgroundSize.X - 18;
                    }

                    spriteBatch.DrawRectangle(new Rectangle(tooltipX - 8, tooltipY + 8, (int)backgroundSize.X + 6, (int)backgroundSize.Y + 6), Color.Black, 0.98210f);
                    spriteBatch.DrawRectangle(new Rectangle(tooltipX - 4, tooltipY + 4, (int)backgroundSize.X + 8, (int)backgroundSize.Y + 4), hoveredItem.rarityColor, 0.98211f);
                    spriteBatch.DrawRectangle(new Rectangle(tooltipX - 2, tooltipY + 6, (int)backgroundSize.X + 4, (int)backgroundSize.Y), bgColor, 0.98212f);

                    if (i == 0 || i == 1)
                    {
                        spriteBatch.DrawStringWithOutline(Main.testFont, hoveredItem.toolTips[i], new Vector2(tooltipX + (maxTextWidth - textSize.X) / 2, tooltipY + 5), Color.Black, toolTipColor, 1f, 0.9822f);
                    }
                    else
                    {
                        spriteBatch.DrawStringWithOutline(Main.testFont, hoveredItem.toolTips[i], new Vector2(tooltipX, tooltipY + 5), Color.Black, toolTipColor, 1f, 0.9822f);
                    }
                }
            }

            if (mouseItem != null)
            {
                spriteBatch.Draw(mouseItem.texture, Input_Manager.Instance.mousePosition, null, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.9822f);
            }
        }
    }

    public class EquipmentSlot
    {
        public string SlotType { get; }
        public Item equippedItem { get; set; }

        public EquipmentSlot(string slotType)
        {
            SlotType = slotType;
        }
    }
}