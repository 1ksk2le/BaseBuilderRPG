using BaseBuilderRPG.Content;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace BaseBuilderRPG
{
    public class Inventory
    {
        private Item[,] items; // 2D array to store items
        public int Width { get; }
        public int Height { get; }

        public Inventory(int width, int height)
        {
            Width = width;
            Height = height;
            items = new Item[width, height];
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

        public void Draw(SpriteBatch spriteBatch, Player player)
        {
            int slotSize = 36;
            int slotSpacing = 10;
            int xStart = 10;
            int yStart = 50;

            Color slotColor = Color.White;

            for (int width = 0; width < player.Inventory.Width; width++)
            {
                for (int height = 0; height < player.Inventory.Height; height++)
                {
                    int x = xStart + width * (slotSize + slotSpacing);
                    int y = yStart + height * (slotSize + slotSpacing);

                    spriteBatch.DrawRectangle(new Rectangle(x, y, slotSize, slotSize), slotColor);

                    Item item = player.Inventory.GetItem(width, height);

                    if (item != null)
                    {
                        spriteBatch.DrawRectangle(new Rectangle(x, y, slotSize, slotSize), item.RarityColor);

                        spriteBatch.Draw(item.Texture, new Vector2(x + 11, y + 4), Color.White);

                        if (item.StackSize > 1)
                        {
                            spriteBatch.DrawString(Game1.TestFont, item.StackSize.ToString(), new Vector2(x + 26, y + 20), Color.Black, 0, Vector2.Zero, 1.5f, SpriteEffects.None, 1f);
                        }
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
                if (item1.Type == "weapon" && item2.Type != "weapon")
                {
                    return -1;
                }
                else if (item1.Type != "weapon" && item2.Type == "weapon")
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

    }
}

