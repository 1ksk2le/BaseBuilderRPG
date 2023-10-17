using System.Collections.Generic;

namespace BaseBuilderRPG.Content
{
    public class Inventory
    {
        private List<Item> items;
        private int rows;
        private int columns;

        public Inventory(int rows, int columns)
        {
            this.rows = rows;
            this.columns = columns;
            items = new List<Item>(rows * columns);
        }

        public bool AddItem(Item item)
        {
            if (items.Count < rows * columns)
            {
                items.Add(item);
                return true;
            }
            return false; // Inventory is full
        }

        public void RemoveItem(Item item)
        {
            items.Remove(item);
        }

        public List<Item> GetItems()
        {
            return items;
        }
    }

}
