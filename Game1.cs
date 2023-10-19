using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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
            player = new Player(texPlayer, new Vector2(200, 200), 5, 1);
            droppedItems = new List<Item>();
        }

        protected override void Update(GameTime gameTime)
        {
            player.Update(gameTime);

            List<Item> itemsToRemove = new List<Item>();
            foreach (Item item in droppedItems.ToList()) // Create a copy of the list to avoid concurrent modification issues
            {
                if (item.PlayerClose(player, gameTime))
                {
                    player.Inventory.PickItem(item, droppedItems);
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
                DropItem(rand.Next(items.Count - 1), prefixID, suffixID, rand.Next(1, 4), mousePosition + new Vector2(30, 30));
                DropItem(2, -1, -1, 2, mousePosition);
            }

            if (Keyboard.GetState().IsKeyDown(Keys.V) && !pKey.IsKeyDown(Keys.V))
            {
                player.Inventory.AddItem(2, 3);
                player.Inventory.AddItem(1, 3);
            }

            if (Keyboard.GetState().IsKeyDown(Keys.Z) && !pKey.IsKeyDown(Keys.Z))
            {
                // Create a copy of the items list to avoid modifying it during enumeration
                List<Item> itemsCopy = new List<Item>(player.Inventory.GetItems());

                foreach (Item item in droppedItems)
                {
                    itemsToRemove.Add(item);
                }
                foreach (Item item in itemsCopy)
                {
                    player.Inventory.RemoveItem(item);
                }

            }

            foreach (Item item in itemsToRemove)
            {
                droppedItems.Remove(item);
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
                spriteBatch.DrawString(Game1.TestFont, (i + 1).ToString() + "-", new Vector2(8, 260 + 10 * i), Color.Yellow, 0, Vector2.Zero, 1f, SpriteEffects.None, 1f);
                spriteBatch.DrawString(Game1.TestFont, "[" + item.ID + "]", new Vector2(26, 260 + 10 * i), Color.Red, 0, Vector2.Zero, 1f, SpriteEffects.None, 1f);
                if (item.Type == "weapon")
                {
                    spriteBatch.DrawString(Game1.TestFont, item.PrefixName + item.Name + " " + item.SuffixName, new Vector2(44, 260 + 10 * i), Color.White, 0, Vector2.Zero, 1f, SpriteEffects.None, 1f);
                }
                else
                {
                    spriteBatch.DrawString(Game1.TestFont, item.PrefixName + item.Name + " " + item.SuffixName + " x" + item.StackSize, new Vector2(44, 260 + 10 * i), Color.White, 0, Vector2.Zero, 1f, SpriteEffects.None, 1f);

                }

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