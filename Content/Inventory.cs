using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace BaseBuilderRPG
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
                new EquipmentSlot("Armor"),
                new EquipmentSlot("Offhand"),
                new EquipmentSlot("Boots"),
                new EquipmentSlot("Helmet"),
                new EquipmentSlot("Accessory")
            };
        }

        public void Draw(SpriteBatch spriteBatch, Player player)
        {
            int slotSize = 44;
            int slotSpacing = 0;
            int xStart = 10;
            int yStart = 50;

            spriteBatch.Draw(Game1.texInventory, new Vector2(10, 42), Color.White);
            for (int width = 0; width < player.Inventory.Width; width++)
            {
                for (int height = 0; height < player.Inventory.Height; height++)
                {
                    int x = xStart + width * (slotSize + slotSpacing);
                    int y = yStart + height * (slotSize + slotSpacing);

                    Item item = player.Inventory.GetItem(width, height);
                    if (item != null)
                    {
                        spriteBatch.Draw(Game1.texInventorySlotBackground, new Vector2(x, y), item.RarityColor);

                        float scale = 1f;
                        if (item.Texture.Width > slotSize || item.Texture.Height > slotSize)
                        {
                            scale = Math.Min((float)slotSize / item.Texture.Width, (float)slotSize / item.Texture.Height);
                        }

                        Vector2 itemPosition = new Vector2(x + slotSize / 2, y + slotSize / 2);

                        spriteBatch.Draw(item.Texture, itemPosition, null, Color.White, 0f, new Vector2(item.Texture.Width / 2, item.Texture.Height / 2), scale, SpriteEffects.None, 0f);

                        if (item.StackSize > 1 && item.Type != "Offhand" && item.Type != "Weapon")
                        {
                            spriteBatch.DrawString(Game1.TestFont, item.StackSize.ToString(), new Vector2(x + 28, y + 28), Color.Black, 0, Vector2.Zero, 1.2f, SpriteEffects.None, 1f);
                        }
                    }
                }
            }



            for (int i = 0; i < equipmentSlots.Count; i++)
            {
                if (equipmentSlots[i].EquippedItem != null)
                {
                    Vector2 position = Game1.EquipmentSlotPositions(i);
                    if (equipmentSlots[i].EquippedItem.Type == "Accessory")
                    {
                        // Handle accessory-specific logic if needed
                    }
                    else
                    {
                        int equipmentSlotSize = 52;
                        float scale = 1f;
                        if (equipmentSlots[i].EquippedItem.Texture.Width > equipmentSlotSize || equipmentSlots[i].EquippedItem.Texture.Height > equipmentSlotSize)
                        {
                            scale = Math.Min((float)equipmentSlotSize / equipmentSlots[i].EquippedItem.Texture.Width, (float)equipmentSlotSize / equipmentSlots[i].EquippedItem.Texture.Height);
                        }

                        Vector2 itemPosition = new(position.X + equipmentSlotSize / 2, position.Y + equipmentSlotSize / 2);

                        spriteBatch.Draw(Game1.texMainSlotBackground, position, equipmentSlots[i].EquippedItem.RarityColor);
                        spriteBatch.Draw(equipmentSlots[i].EquippedItem.Texture, itemPosition + new Vector2(-2, -2), null, Color.White, 0f, new Vector2(equipmentSlots[i].EquippedItem.Texture.Width / 2, equipmentSlots[i].EquippedItem.Texture.Height / 2), scale, SpriteEffects.None, 0f);
                    }
                }
            }

        }

        public void EquipItem(Item item, List<Item> itemsToRemove, int x, int y)
        {
            foreach (var slot in equipmentSlots)
            {
                if (slot.SlotType == item.Type)
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
                    if (existingStack != null && existingStack.ID == item.ID && existingStack.StackSize < existingStack.StackLimit)
                    {
                        int spaceAvailable = existingStack.StackLimit - existingStack.StackSize;

                        if (spaceAvailable >= item.StackSize)
                        {
                            existingStack.StackSize += item.StackSize;
                            item.StackSize = 0;
                            return;
                        }
                        else
                        {
                            existingStack.StackSize = existingStack.StackLimit;
                            item.StackSize -= spaceAvailable;
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

            if (item.StackSize > 0)
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
                    if (existingStack != null && existingStack.ID == item.ID && existingStack.StackSize < existingStack.StackLimit)
                    {
                        int spaceAvailable = existingStack.StackLimit - existingStack.StackSize;

                        if (spaceAvailable >= item.StackSize)
                        {
                            existingStack.StackSize += item.StackSize;
                            item.StackSize = 0;

                            if (item.OnGround)
                            {
                                droppedItems.Remove(item);
                            }

                            return;
                        }
                        else
                        {
                            existingStack.StackSize = existingStack.StackLimit;
                            item.StackSize -= spaceAvailable;
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

                        if (item.OnGround)
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
                if (item1.Type == "Weapon" && item2.Type != "Weapon")
                {
                    return -1;
                }
                else if (item1.Type != "Weapon" && item2.Type == "Weapon")
                {
                    return 1;
                }
                else
                {
                    int rarityComparison = item2.Rarity.CompareTo(item1.Rarity);
                    if (rarityComparison == 0)
                    {
                        return item2.StackSize.CompareTo(item1.StackSize);
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
            Rectangle slotRect = new Rectangle(slotX, slotY, 44, 44);
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
                Rectangle slotRect = new Rectangle(x, y, 52, 52);
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