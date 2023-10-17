using BaseBuilderRPG.Content;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace BaseBuilderRPG
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch spriteBatch;

        private MouseState pMouse, cMouse;
        private KeyboardState pKey, cKey;

        private StreamWriter logFile;

        private Player player;
        private List<Item> items;
        private List<Item> droppedItems;

        private Texture2D texPlayer;

        public static SpriteFont TestFont;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;


            logFile = new StreamWriter("log.txt");
        }

        protected override void Initialize()
        {
            logFile.WriteLine(DateTime.Now.ToString("HH:mm:ss") + ":          [GAME START]");
            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            TestFont = Content.Load<SpriteFont>("Font_Test");
            texPlayer = Content.Load<Texture2D>("Textures/tex_Player");
            player = new Player(texPlayer, new Vector2(200, 200), 10, 5);

            string json = File.ReadAllText("Content/items.json");
            items = JsonConvert.DeserializeObject<List<Item>>(json);
            foreach (Item item in items)
            {
                item.Texture = Content.Load<Texture2D>(item.TexturePath);
            }

            droppedItems = new List<Item>();
        }

        protected override void Update(GameTime gameTime)
        {
            player.Update(gameTime);

            List<Item> itemsToRemove = new List<Item>();



            if (Keyboard.GetState().IsKeyDown(Keys.F) && !pKey.IsKeyDown(Keys.F))
            {
                foreach (Item item in droppedItems)
                {
                    if (item.PlayerClose(player, gameTime))
                    {
                        Item newItem = item.Clone();
                        if (player.Inventory.AddItem(newItem))
                        {
                            itemsToRemove.Add(item);
                        }
                    }
                }
            }

            foreach (Item item in itemsToRemove)
            {
                droppedItems.Remove(item);
            }

            if (Keyboard.GetState().IsKeyDown(Keys.X) && !pKey.IsKeyDown(Keys.X))
            {
                logFile.WriteLine(DateTime.Now.ToString("HH:mm:ss") + ": X key pressed.");
                Random rand = new Random();
                int itemID = rand.Next(0, items.Count);
                int prefixID;
                int suffixID;

                prefixID = rand.Next(0, 4);
                suffixID = rand.Next(0, 4);
                Vector2 mousePosition = new Vector2(Mouse.GetState().X, Mouse.GetState().Y);
                DropItem(itemID, prefixID, suffixID, mousePosition);
            }
            pKey = Keyboard.GetState();

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.DarkSlateBlue);

            spriteBatch.Begin();

            foreach (Item item in droppedItems)
            {
                item.Draw(spriteBatch);
                if (item.PlayerClose(player, gameTime))
                {
                    spriteBatch.DrawString(Game1.TestFont, "[Press F to pick up]", player.Position + new Vector2(-30, 40), Color.Red, 0, Vector2.Zero, 1f, SpriteEffects.None, 1f);
                }
            }


            player.Draw(spriteBatch);

            DrawInventoryGrid(spriteBatch, player);

            spriteBatch.DrawString(Game1.TestFont, "Items on the ground: " + droppedItems.Count, new Vector2(10, 10), Color.Red, 0, Vector2.Zero, 1f, SpriteEffects.None, 1f);

            spriteBatch.End();

            base.Draw(gameTime);
        }

        private void DrawInventoryGrid(SpriteBatch spriteBatch, Player player)
        {
            // Set the dimensions and position of the inventory grid
            int slotSize = 32; // Adjust the size of each slot
            int slotSpacing = 10; // Adjust the spacing between slots
            int rows = 10;
            int cols = 5;
            int xStart = 10; // Adjust the starting X position
            int yStart = 50; // Adjust the starting Y position

            Color slotColor = Color.White; // Color of the inventory slots

            List<Item> items = player.Inventory.GetItems();

            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < cols; col++)
                {
                    int x = xStart + col * (slotSize + slotSpacing);
                    int y = yStart + row * (slotSize + slotSpacing);

                    // Draw a white box for each inventory slot
                    spriteBatch.DrawRectangle(new Rectangle(x, y, slotSize, slotSize), slotColor);

                    // Render items in the inventory using spriteBatch.Draw
                    int index = row * cols + col;
                    if (index < items.Count)
                    {
                        Item item = items[index];
                        spriteBatch.DrawRectangle(new Rectangle(x, y, slotSize, slotSize), item.RarityColor);
                        spriteBatch.Draw(item.Texture, new Vector2(x, y), Color.White);
                        // You can customize the item's position, scaling, or rotation here if needed.

                    }
                }
            }
        }




        private void DropItem(int itemID, int prefixID, int suffixID, Vector2 position)
        {
            // Find the item with the specified itemID from the loaded items list
            Item originalItem = items.Find(item => item.ID == itemID);

            // If the item is found, clone it with custom prefix and suffix IDs
            if (originalItem != null)
            {
                Item spawnedItem = originalItem.Clone(itemID, prefixID, suffixID);
                spawnedItem.Position = position; // Set the position correctly
                spawnedItem.OnGround = true;
                droppedItems.Add(spawnedItem);
                // Now, you can use the spawnedItem in your game logic, such as adding it to a list of items to be drawn and updated.
            }
        }

        protected override void OnExiting(object sender, EventArgs args)
        {
            logFile.WriteLine(DateTime.Now.ToString("HH:mm:ss") + ": Items on ground: " + droppedItems.Count);
            logFile.WriteLine(DateTime.Now.ToString("HH:mm:ss") + ":          [GAME END]");
            logFile.Close();
            base.OnExiting(sender, args);
        }
    }
}