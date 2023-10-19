using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;

namespace BaseBuilderRPG.Content
{
    public class Inventory
    {
        private List<Item> items;
        public int rows;
        public int columns;

        public Inventory(int rows, int columns)
        {
            this.rows = rows;
            this.columns = columns;
            items = new List<Item>(rows * columns);
        }
        public bool AddItem(Item item, List<Item> droppedItems)
        {
            if (item.IsStackable)
            {
                // Check if there's an existing stack with the same ID
                Item existingStack = items.FirstOrDefault(existingItem => existingItem.ID == item.ID && existingItem.StackSize < existingItem.StackLimit);

                if (existingStack != null)
                {
                    // Calculate available space in the existing stack
                    int spaceAvailable = existingStack.StackLimit - existingStack.StackSize;

                    if (spaceAvailable >= item.StackSize)
                    {
                        // There's enough space in the existing stack for the entire incoming stack
                        existingStack.StackSize += item.StackSize;
                        // Remove the item from the ground since it's picked up
                        if (item.OnGround)
                        {
                            item.StackSize = 0;
                            droppedItems.Remove(item);
                        }
                        return true;
                    }
                    else
                    {
                        // The existing stack can't fit the entire incoming stack
                        existingStack.StackSize = existingStack.StackLimit;
                        item.StackSize -= spaceAvailable;
                    }
                }
            }

            // If there's no existing stack or the item is not stackable, add it as a new item
            items.Add(item);

            // Remove the item from the ground if it's picked up
            if (item.OnGround)
            {
                droppedItems.Remove(item);
            }

            return true; // Return true if the item was successfully added
        }

        public bool IsFull()
        {
            if (items.Count < rows * columns)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public void AddItemByID(Item item, int itemID, int prefixID, int suffixID, int dropAmount)
        {
            Item spawnedItem = item.Clone(itemID, prefixID, suffixID, dropAmount);
            if (items.Count < rows * columns)
            {
                items.Add(spawnedItem);
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
            int rows = player.Inventory.rows;
            int cols = player.Inventory.columns;
            int xStart = 10;
            int yStart = 50;

            Color slotColor = Color.White;

            List<Item> items = player.Inventory.GetItems();

            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < cols; col++)
                {
                    int x = xStart + col * (slotSize + slotSpacing);
                    int y = yStart + row * (slotSize + slotSpacing);

                    spriteBatch.DrawRectangle(new Rectangle(x, y, slotSize, slotSize), slotColor);

                    int index = row * cols + col;
                    if (index < items.Count)
                    {
                        Item item = items[index];
                        spriteBatch.DrawRectangle(new Rectangle(x, y, slotSize, slotSize), item.RarityColor);
                        spriteBatch.Draw(item.Texture, new Vector2(x, y), Color.White);
                        if (item.IsStackable)
                        {
                            spriteBatch.DrawString(Game1.TestFont, item.StackSize.ToString(), new Vector2(x + 22, y + 16), Color.Black, 0, Vector2.Zero, 1.5f, SpriteEffects.None, 1f);
                        }
                    }
                }
            }
        }
    }
}