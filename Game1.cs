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

        private Player player;
        private List<Item> items;
        private List<Item> droppedItems;

        private Texture2D texPlayer;

        public static SpriteFont TestFont;

        private KeyboardState previousKeyboardState;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            previousKeyboardState = Keyboard.GetState();
            TestFont = Content.Load<SpriteFont>("Font_Test");
            texPlayer = Content.Load<Texture2D>("Textures/tex_Player");
            player = new Player(texPlayer, new Vector2(200, 200));

            string json = File.ReadAllText("Content/items.json");
            droppedItems = new List<Item>();
            items = JsonConvert.DeserializeObject<List<Item>>(json);
            foreach (Item item in items)
            {
                item.Texture = Content.Load<Texture2D>(item.TexturePath);
            }
            droppedItems = new List<Item>();
        }

        protected override void Update(GameTime gameTime)
        {
            MouseState mouseState = Mouse.GetState();
            KeyboardState keyboardState = Keyboard.GetState();

            player.Update(gameTime);

            List<Item> itemsToRemove = new List<Item>();

            foreach (Item item in droppedItems)
            {
                if (item.PlayerClose(player, gameTime))
                {
                    item.InteractPlayer(player, gameTime);

                    if (item.Kill && keyboardState.IsKeyDown(Keys.F) && !previousKeyboardState.IsKeyDown(Keys.F))
                    {
                        itemsToRemove.Add(item);
                    }
                }
            }

            foreach (Item item in itemsToRemove)
            {
                droppedItems.Remove(item);
            }



            // Check if the "X" key is pressed
            if (keyboardState.IsKeyDown(Keys.X) && !previousKeyboardState.IsKeyDown(Keys.X))
            {
                // Generate random itemID, prefixID, and suffixID
                Random rand = new Random();
                int itemID = rand.Next(0, items.Count); // Random item ID within the range of available items
                int prefixID;
                int suffixID;

                prefixID = rand.Next(0, 4);
                suffixID = rand.Next(0, 4);
                Vector2 mousePosition = new Vector2(mouseState.X, mouseState.Y);
                SpawnItem(itemID, prefixID, suffixID, mousePosition);
            }
            previousKeyboardState = keyboardState;

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.DarkSlateBlue);

            spriteBatch.Begin();

            foreach (Item item in droppedItems)
            {
                item.Draw(spriteBatch);
                if (item.Kill && item.PlayerClose(player, gameTime))
                {
                    spriteBatch.DrawString(Game1.TestFont, "[Press F to pick up]", player.Position + new Vector2(-30, 40), Color.Red, 0, Vector2.Zero, 1f, SpriteEffects.None, 1f);
                }
            }

            player.Draw(spriteBatch);

            spriteBatch.End();

            base.Draw(gameTime);
        }

        private void SpawnItem(int itemID, int prefixID, int suffixID, Vector2 position)
        {
            // Find the item with the specified itemID from the loaded items list
            Item originalItem = items.Find(item => item.ID == itemID);

            // If the item is found, clone it with custom prefix and suffix IDs
            if (originalItem != null)
            {
                Item spawnedItem = originalItem.Clone(itemID, prefixID, suffixID);
                spawnedItem.Position = position; // Set the position correctly
                droppedItems.Add(spawnedItem);
                // Now, you can use the spawnedItem in your game logic, such as adding it to a list of items to be drawn and updated.
            }
        }
    }
}