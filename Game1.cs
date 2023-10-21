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
        private List<Player> players;
        private List<Item> items;
        private List<Item> groundItems;
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
            players = new List<Player>();
            players.Add(new Player(texPlayer, true, "East the Developer", new Vector2(200, 200), 5, 3));
            players.Add(new Player(texPlayer, false, "Dummy", new Vector2(300, 200), 3, 5));
            groundItems = new List<Item>();
        }

        protected override void Update(GameTime gameTime)
        {
            List<Item> itemsToRemove = new List<Item>();

            foreach (Player player in players)
            {
                player.Update(gameTime);
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
                foreach (Player player in players)
                {
                    if (player.IsActive)
                    {
                        Item originalItem = items.Find(item => item.ID == itemID);

                        if (originalItem != null)
                        {
                            Item itemToAdd = originalItem.Clone(itemID, prefixID, suffixID, rand.Next(1, 4));
                            player.Inventory.AddItem(itemToAdd, groundItems);
                        }
                    }
                }
            }

            SelectPlayer(players, Keys.E, 30f);
            PickItemsUp(players, groundItems);
            ClearItems(itemsToRemove, true, true, Keys.C);
            SortInventory(Keys.Z);

            pKey = Keyboard.GetState();

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.DarkSlateBlue);

            spriteBatch.Begin();

            foreach (Item item in groundItems)
            {
                item.Draw(spriteBatch);
            }

            foreach (Player player in players)
            {
                player.Draw(spriteBatch);
            }

            spriteBatch.DrawString(Game1.TestFont, "Items on the ground: " + groundItems.Count, new Vector2(10, 10), Color.Red, 0, Vector2.Zero, 1f, SpriteEffects.None, 1f);
            spriteBatch.DrawString(Game1.TestFont, "[INVENTORY]", new Vector2(10, 25), Color.Black, 0, Vector2.Zero, 1.5f, SpriteEffects.None, 1f);

            spriteBatch.End();

            base.Draw(gameTime);
        }

        public void SortInventory(Keys key)
        {
            if (Keyboard.GetState().IsKeyDown(key) && !pKey.IsKeyDown(key))
            {
                foreach (Player player in players)
                {
                    if (player.IsActive)
                    {
                        player.Inventory.SortItems();
                    }
                }
            }
        }

        public void SelectPlayer(List<Player> players, Keys key, float selectionDistance)
        {
            if (Keyboard.GetState().IsKeyDown(key) && !pKey.IsKeyDown(key))
            {
                Player activePlayer = players.FirstOrDefault(p => p.IsActive);

                if (activePlayer != null)
                {
                    activePlayer.IsActive = false;
                }

                Player closestPlayer = null;

                foreach (Player player in players)
                {
                    float distance = Vector2.Distance(player.Position, new Vector2(Mouse.GetState().X, Mouse.GetState().Y));

                    if (distance <= selectionDistance)
                    {
                        closestPlayer = player;
                    }
                }

                if (closestPlayer != null)
                {
                    closestPlayer.IsActive = true;
                }
            }
        }

        public void PickItemsUp(List<Player> playerList, List<Item> itemsOnGround)
        {
            foreach (Item item in itemsOnGround.ToList())
            {
                foreach (Player player in playerList)
                {
                    if (item.PlayerClose(player, false, 20f) && player.IsActive)
                    {
                        player.Inventory.PickItem(item, itemsOnGround);
                    }
                }
            }
        }

        private void DropItem(int itemID, int prefixID, int suffixID, int dropAmount, Vector2 position)
        {
            Item originalItem = items.Find(item => item.ID == itemID);

            if (originalItem != null)
            {
                Item spawnedItem = originalItem.Clone(itemID, prefixID, suffixID, dropAmount);
                spawnedItem.Position = position;
                spawnedItem.OnGround = true;
                groundItems.Add(spawnedItem);
            }
        }

        public void ClearItems(List<Item> itemsToRemove, bool clearInventory, bool clearGroundItems, Keys key)
        {
            if (Keyboard.GetState().IsKeyDown(key) && !pKey.IsKeyDown(key))
            {
                foreach (Player player in players)
                {
                    if (clearGroundItems)
                    {
                        foreach (Item item in groundItems)
                        {
                            itemsToRemove.Add(item);
                        }
                    }
                    if (clearInventory && player.IsActive)
                    {
                        player.Inventory.ClearInventory();
                    }
                }
            }
            foreach (Item item in itemsToRemove)
            {
                groundItems.Remove(item);
            }
        }
    }
}