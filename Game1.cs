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
        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;

        private MouseState pMouse, cMouse;
        private KeyboardState pKey, cKey;

        private Player player;

        private List<Item> items;
        private List<Item> droppedItems;

        private Texture2D texPlayer;

        public static SpriteFont TestFont;

        public static Effect outline;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            graphics.PreferredBackBufferWidth = 1000;
            graphics.PreferredBackBufferHeight = 1000;
            graphics.ApplyChanges();
        }

        protected override void Initialize()
        {
            TestFont = Content.Load<SpriteFont>("Font_Test");
            texPlayer = Content.Load<Texture2D>("Textures/tex_Player");
            string json = File.ReadAllText("Content/items.json");
            items = JsonConvert.DeserializeObject<List<Item>>(json);
            foreach (Item item in items)
            {
                item.Texture = Content.Load<Texture2D>(item.TexturePath);
            }

            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            outline = Content.Load<Effect>("Shaders/shader_Outline");
            player = new Player(texPlayer, new Vector2(200, 200), 5, 5);
            droppedItems = new List<Item>();
        }

        protected override void Update(GameTime gameTime)
        {
            player.Update(gameTime);

            if (Keyboard.GetState().IsKeyDown(Keys.F) && !pKey.IsKeyDown(Keys.F))
            {
                List<Item> itemsToRemove = new List<Item>();
                foreach (Item item in droppedItems)
                {
                    if (item.PlayerClose(player, gameTime))
                    {
                        Item newItem = item.Clone();
                        if (player.Inventory.AddItem(newItem, droppedItems))
                        {
                            item.StackSize = 0;
                            itemsToRemove.Add(item);
                        }
                    }
                }

                // Remove the picked up items from the droppedItems list
                foreach (Item item in itemsToRemove)
                {
                    droppedItems.Remove(item);
                }
            }

            if (Keyboard.GetState().IsKeyDown(Keys.X) && !pKey.IsKeyDown(Keys.X))
            {
                Random rand = new Random();
                int itemID = rand.Next(0, items.Count);
                int prefixID;
                int suffixID;

                prefixID = rand.Next(0, 4);
                suffixID = rand.Next(0, 4);
                Vector2 mousePosition = new Vector2(Mouse.GetState().X, Mouse.GetState().Y);
                DropItem(rand.Next(items.Count), prefixID, suffixID, rand.Next(1, 4), mousePosition);
                //DropItem(2, -1, -1, 1, mousePosition);
            }

            if (Keyboard.GetState().IsKeyDown(Keys.V) && !pKey.IsKeyDown(Keys.V))
            {
                foreach (Item item in items)
                {
                    Item newItem = item.Clone();
                    player.Inventory.AddItemByID(newItem, newItem.ID, 0, 0, 1);
                }
            }

            pKey = Keyboard.GetState();

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.DarkSlateBlue);

            spriteBatch.Begin();

            for (int i = 0; i < droppedItems.Count; i++)
            {
                Item item = droppedItems[i];
                spriteBatch.DrawString(Game1.TestFont, "[" + i.ToString() + "]", new Vector2(8, 260 + 10 * i), Color.Yellow, 0, Vector2.Zero, 1f, SpriteEffects.None, 1f);
                spriteBatch.DrawString(Game1.TestFont, item.PrefixName + item.Name + " " + item.SuffixName + " x" + item.StackSize, new Vector2(40, 260 + 10 * i), Color.White, 0, Vector2.Zero, 1f, SpriteEffects.None, 1f);
            }

            // In your Draw method:
            foreach (Item item in droppedItems)
            {
                item.Draw(spriteBatch);

                if (item.PlayerClose(player, gameTime))
                {
                    if (player.Inventory.IsFull())
                    {
                        spriteBatch.DrawString(Game1.TestFont, "[INVENTORY IS FULL!!!]", player.Position + new Vector2(-30, 40), Color.Red, 0, Vector2.Zero, 1f, SpriteEffects.None, 1f);
                    }
                    else
                    {
                        spriteBatch.DrawString(Game1.TestFont, "[Press F to pick up]", player.Position + new Vector2(-30, 40), Color.Red, 0, Vector2.Zero, 1f, SpriteEffects.None, 1f);
                        spriteBatch.DrawString(Game1.TestFont, item.PrefixName + item.Name + " " + item.SuffixName, player.Position + new Vector2(-30, 50), Color.White, 0, Vector2.Zero, 1f, SpriteEffects.None, 1f);
                    }
                }
            }

            player.Draw(spriteBatch);

            spriteBatch.DrawString(Game1.TestFont, "Items on the ground: " + droppedItems.Count, new Vector2(10, 10), Color.Red, 0, Vector2.Zero, 1f, SpriteEffects.None, 1f);

            spriteBatch.End();

            base.Draw(gameTime);
        }

        private void DropItem(int itemID, int prefixID, int suffixID, int dropAmount, Vector2 position)
        {
            // Find the item with the specified itemID from the loaded items list
            Item originalItem = items.Find(item => item.ID == itemID);

            // If the item is found, clone it with custom prefix and suffix IDs
            if (originalItem != null)
            {
                Item spawnedItem = originalItem.Clone(itemID, prefixID, suffixID, dropAmount);
                spawnedItem.Position = position; // Set the position correctly
                spawnedItem.OnGround = true;
                droppedItems.Add(spawnedItem);
                // Now, you can use the spawnedItem in your game logic, such as adding it to a list of items to be drawn and updated.
            }
        }
    }
}