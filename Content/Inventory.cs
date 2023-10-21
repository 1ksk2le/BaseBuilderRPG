using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;

namespace BaseBuilderRPG.Content
{
    public class Inventory
    {
        private List<Item> items;
        public int Width;
        public int Height;

        public Inventory(int w, int h)
        {
            this.Width = h;
            this.Height = w;
            items = new List<Item>(w * h);
        }

        public void PickItem(Item item, List<Item> droppedItems)
        {
            Item existingStack = items.FirstOrDefault(existingItem => existingItem.ID == item.ID && existingItem.StackSize < existingItem.StackLimit);
            if (existingStack != null)
            {
                if (existingStack.StackLimit == 1)
                {
                    items.Add(item);
                }
                else if (existingStack.StackSize < existingStack.StackLimit)
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
                    }
                    else
                    {
                        existingStack.StackSize = existingStack.StackLimit;
                        item.StackSize -= spaceAvailable;
                    }
                }
            }
            else if (!IsFull())
            {
                items.Add(item);

                if (item.OnGround)
                {
                    droppedItems.Remove(item);
                }
            }
            else
            {
                return;
            }
        }



        private Item CreateItem(int itemID, int dropAmount)
        {
            Item originalItem = items.Find(item => item.ID == itemID);
            if (originalItem != null)
            {
                Item spawnedItem = originalItem.Clone(itemID, dropAmount);
                spawnedItem.OnGround = false;
                return spawnedItem;
            }
            return null;
        }

        public bool IsFull()
        {
            if (items.Count < Width * Height)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public void RemoveFromInventory(Item item)
        {
            items.Remove(item);
        }

        public List<Item> GetItems()
        {
            return items;
        }

        public void Sort()
        {
            SortItemsCustom(items);
        }
        private void SortItemsCustom(List<Item> itemList)
        {
            itemList.Sort((item1, item2) =>
            {
                if (item1.Type == "weapon" && item2.Type != "weapon")
                {
                    // Sort weapons before non-weapons.
                    return -1;
                }
                else if (item1.Type != "weapon" && item2.Type == "weapon")
                {
                    // Sort non-weapons after weapons.
                    return 1;
                }
                else
                {
                    // If both items are either weapons or non-weapons, sort by rarity.
                    int rarityComparison = item2.Rarity.CompareTo(item1.Rarity);
                    if (rarityComparison == 0)
                    {
                        // If rarity is the same, prioritize items with lower stack sizes.
                        return item2.StackSize.CompareTo(item1.StackSize);
                    }
                    return rarityComparison;
                }
            });
        }




        public void Draw(SpriteBatch spriteBatch, Player player)
        {
            int slotSize = 36;
            int slotSpacing = 10;
            int maxWidth = player.Inventory.Width;
            int maxHeight = player.Inventory.Height;
            int xStart = 10;
            int yStart = 50;

            Color slotColor = Color.White;

            List<Item> items = player.Inventory.GetItems();

            for (int width = 0; width < maxWidth; width++)
            {
                for (int height = 0; height < maxHeight; height++)
                {
                    int x = xStart + height * (slotSize + slotSpacing);
                    int y = yStart + width * (slotSize + slotSpacing);

                    spriteBatch.DrawRectangle(new Rectangle(x - 2, y - 2, 40, 40), Color.Black);
                    spriteBatch.DrawRectangle(new Rectangle(x, y, slotSize, slotSize), slotColor);

                    int index = width * maxHeight + height;
                    if (index < items.Count)
                    {
                        Item item = items[index];

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
    }
}