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
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here
            player.Update(gameTime);

            MouseState mouseState = Mouse.GetState();
            KeyboardState keyboardState = Keyboard.GetState();

            // Check if the "X" key is pressed
            if (keyboardState.IsKeyDown(Keys.X) && !previousKeyboardState.IsKeyDown(Keys.X))
            {
                // Generate random itemID, prefixID, and suffixID
                Random rand = new Random();
                int itemID = rand.Next(0, items.Count); // Random item ID within the range of available items
                int prefixID = rand.Next(0, 100); // Random prefix ID within your desired range
                int suffixID = rand.Next(0, 100); // Random suffix ID within your desired range

                Vector2 mousePosition = new Vector2(mouseState.X, mouseState.Y);
                SpawnItem(itemID, prefixID, suffixID, mousePosition);
            }
            previousKeyboardState = keyboardState;

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            spriteBatch.Begin();

            // Draw the player
            player.Draw(spriteBatch);

            // Draw the spawned items
            foreach (Item item in droppedItems)
            {
                item.Draw(spriteBatch);
            }

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
