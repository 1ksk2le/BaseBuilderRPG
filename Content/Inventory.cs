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
                    // Items with StackLimit 1 cannot be stacked, so add the item directly to the inventory
                    items.Add(item);
                }
                else if (existingStack.StackSize < existingStack.StackLimit)
                {
                    // Calculate available space in the existing stack
                    int spaceAvailable = existingStack.StackLimit - existingStack.StackSize;

                    if (spaceAvailable >= item.StackSize)
                    {
                        // There's enough space in the existing stack to pick up the entire incoming stack
                        existingStack.StackSize += item.StackSize;
                        item.StackSize = 0; // The entire incoming stack has been added

                        if (item.OnGround)
                        {
                            // Remove the item from the ground
                            droppedItems.Remove(item);
                        }
                    }
                    else
                    {
                        // The existing stack can't fit the entire incoming stack
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
                    // Remove the item from the ground
                    droppedItems.Remove(item);
                }
            }
            else
            {
                return;
            }
        }




        public bool AddItem(int itemID, int dropAmount)
        {
            Item newItem = CreateItem(itemID, dropAmount);

            if (newItem != null)
            {
                if (items.Count < Width * Height)
                {
                    Item existingItem = items.Find(item => item.ID == itemID);

                    if (existingItem != null)
                    {
                        int spaceAvailable = existingItem.StackLimit - existingItem.StackSize;

                        if (spaceAvailable >= newItem.StackSize)
                        {
                            existingItem.StackSize += newItem.StackSize;
                            return true;
                        }
                        else
                        {
                            existingItem.StackSize = existingItem.StackLimit;
                            newItem.StackSize -= spaceAvailable;
                        }
                    }
                    else
                    {
                        items.Add(newItem);
                    }
                }
                else
                {
                }
            }
            return false;
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

        public void RemoveItem(Item item)
        {
            items.Remove(item);
        }

        public List<Item> GetItems()
        {
            return items;
        }

        public void Draw(SpriteBatch spriteBatch, Player player)
        {
            int slotSize = 32;
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

                    spriteBatch.DrawRectangle(new Rectangle(x, y, slotSize, slotSize), slotColor);

                    int index = width * maxHeight + height;
                    if (index < items.Count)
                    {
                        Item item = items[index];
                        spriteBatch.DrawRectangle(new Rectangle(x, y, slotSize, slotSize), item.RarityColor);
                        spriteBatch.Draw(item.Texture, new Vector2(x, y), Color.White);
                        if (item.StackSize > 1)
                        {
                            spriteBatch.DrawString(Game1.TestFont, item.StackSize.ToString(), new Vector2(x + 22, y + 16), Color.Black, 0, Vector2.Zero, 1.5f, SpriteEffects.None, 1f);
                        }
                    }
                }
            }
        }
    }
}