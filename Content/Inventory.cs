using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace BaseBuilderRPG.Content
{
    public class Inventory
    {
        private Item[,] items;
        public List<EquipmentSlot> equipmentSlots;
        public int Width { get; }
        public int Height { get; }

        public Inventory(int width, int height)
        {
            Width = width;
            Height = height;
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
            int slotSize = Main.inventorySlotSize;

            Rectangle sourceRect = new Rectangle(0, 0, Main.texInventoryExtras.Width, Main.texInventoryExtras.Height / 2);
            Rectangle sourceRect2 = new Rectangle(0, Main.texInventoryExtras.Height / 2, Main.texInventoryExtras.Width, Main.texInventoryExtras.Height / 2);
            spriteBatch.Draw(Main.texInventoryExtras, Main.inventoryPos + new Vector2(0, -22), sourceRect, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.9502f);
            spriteBatch.Draw(Main.texInventoryExtras, Main.inventoryPos + new Vector2(0, 374), sourceRect2, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.9501f);
            spriteBatch.Draw(Main.texInventory, Main.inventoryPos, null, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.95f);

            Rectangle sortSlotRectangle = new Rectangle((int)Main.inventoryPos.X + 84, (int)Main.inventoryPos.Y + 374, 20, 20);
            if (sortSlotRectangle.Contains(Mouse.GetState().X, Mouse.GetState().Y))
            {
                string text = "Sort Inventory";
                Vector2 textSize = Main.testFont.MeasureString(text);
                spriteBatch.DrawRectangle(new Rectangle(Mouse.GetState().X + 14, Mouse.GetState().Y - 2, (int)textSize.X + 8, (int)textSize.Y + 4), Color.Black, 0.9503f);
                spriteBatch.DrawString(Main.testFont, text, new Vector2(Mouse.GetState().X + 18, Mouse.GetState().Y), Color.White, 0, Vector2.Zero, 1f, SpriteEffects.None, 0.95031f);
            }

            Rectangle closeInvSlotRectangle = new Rectangle((int)Main.inventoryPos.X + 170, (int)Main.inventoryPos.Y - 22, 20, 20);
            if (closeInvSlotRectangle.Contains(Mouse.GetState().X, Mouse.GetState().Y))
            {
                string text = "Close Inventory";
                Vector2 textSize = Main.testFont.MeasureString(text);
                spriteBatch.DrawRectangle(new Rectangle(Mouse.GetState().X + 14, Mouse.GetState().Y - 2, (int)textSize.X + 8, (int)textSize.Y + 4), Color.Black, 0.9503f);
                spriteBatch.DrawString(Main.testFont, text, new Vector2(Mouse.GetState().X + 18, Mouse.GetState().Y), Color.White, 0, Vector2.Zero, 1f, SpriteEffects.None, 0.95031f);
            }

            for (int width = 0; width < player.Inventory.Width; width++)
            {
                for (int height = 0; height < player.Inventory.Height; height++)
                {
                    int x = (int)Main.inventoryPos.X + width * slotSize;
                    int y = (int)Main.inventoryPos.Y + height * slotSize + 148;

                    Item item = player.Inventory.GetItem(width, height);
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
                if (equipmentSlots[i].EquippedItem != null)
                {
                    Vector2 position = Main.EquipmentSlotPositions(i);
                    if (equipmentSlots[i].EquippedItem.type == "Accessory")
                    {
                        // Handle accessory-specific logic if needed
                    }
                    else
                    {
                        int equipmentSlotSize = 44;
                        float scale = 1f;
                        if (equipmentSlots[i].EquippedItem.texture.Width > equipmentSlotSize || equipmentSlots[i].EquippedItem.texture.Height > equipmentSlotSize)
                        {
                            scale = Math.Min((float)equipmentSlotSize / equipmentSlots[i].EquippedItem.texture.Width, (float)equipmentSlotSize / equipmentSlots[i].EquippedItem.texture.Height);
                        }

                        Vector2 itemPosition = new(position.X + equipmentSlotSize / 2, position.Y + equipmentSlotSize / 2);

                        spriteBatch.Draw(Main.texMainSlotBackground, position, null, equipmentSlots[i].EquippedItem.rarityColor, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.96f);

                        spriteBatch.Draw(equipmentSlots[i].EquippedItem.texture, itemPosition + new Vector2(0, 4), null, new Color(0, 0, 0, 150), 0f, new Vector2(equipmentSlots[i].EquippedItem.texture.Width / 2, equipmentSlots[i].EquippedItem.texture.Height / 2), scale * 1.2f, SpriteEffects.None, 0.97f);
                        spriteBatch.Draw(equipmentSlots[i].EquippedItem.texture, itemPosition + new Vector2(0, -2), null, Color.White, 0f, new Vector2(equipmentSlots[i].EquippedItem.texture.Width / 2, equipmentSlots[i].EquippedItem.texture.Height / 2), scale, SpriteEffects.None, 0.98f);
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
                    if (slot.EquippedItem == null)
                    {
                        slot.EquippedItem = item;
                        items[x, y] = null;
                    }
                    else
                    {
                        var existingItem = slot.EquippedItem;
                        items[x, y] = existingItem;
                        slot.EquippedItem = item;
                    }
                    return;
                }
            }
        }

        public void AddItem(Item item, List<Item> droppedItems)
        {
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
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

            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
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

        public void PickItem(Item item, List<Item> droppedItems)
        {
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
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

            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
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
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    items[x, y] = null;
                }
            }
        }

        public void ClearEquippedItems()
        {
            for (int x = 0; x < equipmentSlots.Count; x++)
            {
                equipmentSlots[x].EquippedItem = null;
            }
        }

        public bool RemoveItem(int x, int y)
        {
            if (x >= 0 && x < Width && y >= 0 && y < Height && items[x, y] != null)
            {
                items[x, y] = null;
                return true;
            }
            return false;
        }

        public Item GetItem(int x, int y)
        {
            if (x >= 0 && x < Width && y >= 0 && y < Height)
            {
                return items[x, y];
            }
            return null;
        }

        public Item GetEquippedItem(int slot)
        {
            if (equipmentSlots[slot].EquippedItem != null)
            {
                return equipmentSlots[slot].EquippedItem;
            }
            else
            {
                return null;
            }
        }

        public List<Item> GetAllItems()
        {
            List<Item> itemList = new List<Item>();

            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
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
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
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

        public bool IsSlotHovered(int slotX, int slotY)
        {
            Rectangle slotRect = new Rectangle(slotX, slotY, Main.inventorySlotSize, Main.inventorySlotSize);
            return slotRect.Contains(Mouse.GetState().X, Mouse.GetState().Y);
        }

        public bool IsEquipmentSlotHovered(int x, int y, int slot)
        {
            if (slot == 5)
            {
                Rectangle slotRect = new(x, y, 44, 44);
                if (slotRect.Contains(Mouse.GetState().X, Mouse.GetState().Y))
                {
                    return true;
                }
            }
            else
            {
                Rectangle slotRect = new Rectangle(x, y, 44, 44);
                if (slotRect.Contains(Mouse.GetState().X, Mouse.GetState().Y))
                {
                    return true;
                }
            }
            return false;
        }

        public bool IsFull()
        {
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    if (items[x, y] == null)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public void SetItem(int x, int y, Item item)
        {
            if (x >= 0 && x < Width && y >= 0 && y < Height)
            {
                items[x, y] = item;
            }
        }
    }

    public class EquipmentSlot
    {
        public string SlotType { get; }
        public Item EquippedItem { get; set; }

        public EquipmentSlot(string slotType)
        {
            SlotType = slotType;
        }
    }
}