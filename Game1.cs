using BaseBuilderRPG.Content;
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
        private MouseState pMouse;
        private KeyboardState pKey;
        private List<Player> players;
        private List<Item> items;
        private List<Item> groundItems;
        private Texture2D texPlayer;
        public static Texture2D texInventory;
        public static Texture2D texInventorySlotBackground;
        public static Texture2D texAccessorySlotBackground;
        public static Texture2D texMainSlotBackground;
        public static SpriteFont TestFont;

        public static Effect OutlineShader;

        private Item hoveredItem;
        private Item mouseItem;

        public static Vector2 basePosInventory = new Vector2(0, 0);

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
            OutlineShader = Content.Load<Effect>("Shaders/Outline");
            TestFont = Content.Load<SpriteFont>("Font_Test");
            texPlayer = Content.Load<Texture2D>("Textures/tex_Player");
            texInventory = Content.Load<Texture2D>("Textures/tex_UI_Inventory");
            texInventorySlotBackground = Content.Load<Texture2D>("Textures/tex_UI_Inventory_Slot_Background");
            texAccessorySlotBackground = Content.Load<Texture2D>("Textures/tex_UI_Accessory_Slot_Background");
            texMainSlotBackground = Content.Load<Texture2D>("Textures/tex_UI_Main_Slot_Background");
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
            players = new List<Player>();
            players.Add(new Player(texPlayer, true, "East", 140, new Vector2(10, 500)));
            players.Add(new Player(texPlayer, false, "Dummy", 100, new Vector2(30, 500)));
            players.Add(new Player(texPlayer, false, "West", 100, new Vector2(50, 500)));
            groundItems = new List<Item>();
        }

        protected override void Update(GameTime gameTime)
        {
            List<Item> itemsToRemove = new List<Item>();

            foreach (Player player in players)
            {
                player.Update(gameTime);
            }

            foreach (Item item in groundItems)
            {
                item.Update(gameTime);
            }

            SpawnItem(Keys.X, true);
            SelectPlayer(players, Keys.E);
            PickItemsUp(players, groundItems, Keys.F);
            ClearItems(itemsToRemove, true, true, true, Keys.C);
            SortInventory(Keys.Z);
            HandleInventoryInteractions();

            pMouse = Mouse.GetState();
            pKey = Keyboard.GetState();

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.DarkSlateBlue);



            foreach (Item item in groundItems)
            {
                item.Draw(spriteBatch);
            }

            spriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, null, null, null, null, Matrix.Identity);
            foreach (Player player in players)
            {
                player.Draw(spriteBatch);
            }
            spriteBatch.End();

            spriteBatch.Begin();

            spriteBatch.DrawString(Game1.TestFont, "Items on the ground: " + groundItems.Count, new Vector2(10, 10), Color.Red, 0, Vector2.Zero, 1f, SpriteEffects.None, 1f);
            spriteBatch.DrawString(Game1.TestFont, "[INVENTORY]", new Vector2(10, 25), Color.Black, 0, Vector2.Zero, 1.5f, SpriteEffects.None, 1f);

            if (hoveredItem != null && mouseItem == null)
            {
                float maxTextWidth = 0;
                foreach (string tooltip in hoveredItem.ToolTips)
                {
                    Vector2 textSize = Game1.TestFont.MeasureString(tooltip);
                    maxTextWidth = Math.Max(maxTextWidth, textSize.X);
                }

                for (int i = 0; i < hoveredItem.ToolTips.Count; i++)
                {
                    int xOffSet = 18;
                    Color toolTipColor, bgColor;
                    switch (i)
                    {
                        case 0:
                            toolTipColor = Color.Black;
                            bgColor = hoveredItem.RarityColor;
                            break;

                        case 1:
                            toolTipColor = Color.Yellow;
                            bgColor = Color.Black;
                            break;

                        default:
                            toolTipColor = Color.White;
                            bgColor = Color.Black;
                            break;
                    }

                    if (hoveredItem.ToolTips[i].StartsWith("'"))
                    {
                        toolTipColor = Color.Aquamarine;
                    }

                    Vector2 textSize = Game1.TestFont.MeasureString(hoveredItem.ToolTips[i]);
                    Vector2 backgroundSize = new Vector2(maxTextWidth, textSize.Y);

                    int tooltipY = (int)Mouse.GetState().Y + i * ((int)textSize.Y);

                    spriteBatch.DrawRectangle(new Rectangle((int)Mouse.GetState().X + xOffSet - 4, tooltipY + 4, (int)backgroundSize.X + 8, (int)backgroundSize.Y + 4), hoveredItem.RarityColor);
                    spriteBatch.DrawRectangle(new Rectangle((int)Mouse.GetState().X + xOffSet - 2, tooltipY + 6, (int)backgroundSize.X + 4, (int)backgroundSize.Y), bgColor);

                    if (i == 0 || i == 1)
                    {
                        spriteBatch.DrawString(Game1.TestFont, hoveredItem.ToolTips[i], new Vector2((int)Mouse.GetState().X + xOffSet + (maxTextWidth - textSize.X) / 2, tooltipY + 5), toolTipColor);
                    }
                    else
                    {
                        spriteBatch.DrawString(Game1.TestFont, hoveredItem.ToolTips[i], new Vector2((int)Mouse.GetState().X + xOffSet, tooltipY + 5), toolTipColor);
                    }
                }
            }

            if (mouseItem != null)
            {
                spriteBatch.Draw(mouseItem.Texture, new Vector2(Mouse.GetState().X, Mouse.GetState().Y), Color.White);
            }

            spriteBatch.End();

            base.Draw(gameTime);
        }

        private void SpawnItem(Keys key, bool addInventory)
        {
            if (Keyboard.GetState().IsKeyDown(key) && !pKey.IsKeyDown(key))
            {
                Random rand = new Random();
                int itemID = rand.Next(0, items.Count);
                int prefixID;
                int suffixID;

                prefixID = rand.Next(0, 4);
                suffixID = rand.Next(0, 4);
                if (addInventory)
                {
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
                else
                {
                    Vector2 mousePosition = new Vector2(Mouse.GetState().X, Mouse.GetState().Y);
                    DropItem(rand.Next(items.Count), prefixID, suffixID, rand.Next(1, 4), mousePosition);
                }
            }
        }

        private void HandleInventoryInteractions()
        {
            bool isMouseOverItem = false;

            foreach (Player player in players)
            {
                if (player.IsActive)
                {
                    for (int i = 0; i < player.Inventory.equipmentSlots.Count; i++)
                    {
                        Vector2 position = EquipmentSlotPositions(i);
                        if (player.Inventory.equipmentSlots[i].EquippedItem != null)
                        {
                            if (player.Inventory.IsEquipmentSlotHovered((int)position.X, (int)position.Y, i))
                            {
                                isMouseOverItem = true;
                                hoveredItem = player.Inventory.GetEquippedItem(i); // Adjust the slot parameter as needed
                            }
                        }
                    }
                    for (int y = 0; y < player.Inventory.Height; y++)
                    {
                        for (int x = 0; x < player.Inventory.Width; x++)
                        {
                            int slotSize = 44;
                            int slotSpacing = 0;
                            int slotX = x * (slotSize + slotSpacing) + 10;
                            int slotY = y * (slotSize + slotSpacing) + 50;

                            if (player.Inventory.IsSlotHovered(slotX, slotY))
                            {
                                hoveredItem = player.Inventory.GetItem(x, y);
                                isMouseOverItem = true;
                            }
                        }
                    }
                }
            }

            foreach (Item item in groundItems)
            {
                if (item.InteractsWithMouse())
                {
                    hoveredItem = item;
                    isMouseOverItem = true;
                }
            }

            if (!isMouseOverItem)
            {
                hoveredItem = null;
            }

            if (Mouse.GetState().LeftButton == ButtonState.Pressed && pMouse.LeftButton == ButtonState.Released)
            {
                foreach (Player player in players)
                {
                    if (player.IsActive)
                    {
                        for (int y = 0; y < player.Inventory.Height; y++)
                        {
                            for (int x = 0; x < player.Inventory.Width; x++)
                            {
                                int slotSize = 44;
                                int slotSpacing = 0;
                                int slotX = x * (slotSize + slotSpacing) + 10;
                                int slotY = y * (slotSize + slotSpacing) + 50;

                                if (player.Inventory.IsSlotHovered(slotX, slotY))
                                {
                                    if (mouseItem == null)
                                    {
                                        mouseItem = player.Inventory.GetItem(x, y);
                                        player.Inventory.RemoveItem(x, y);
                                    }
                                    else
                                    {
                                        Item temp = mouseItem;
                                        mouseItem = player.Inventory.GetItem(x, y);
                                        player.Inventory.SetItem(x, y, temp);
                                    }
                                }
                            }
                        }
                        for (int i = 0; i < player.Inventory.equipmentSlots.Count; i++)
                        {
                            Vector2 position = EquipmentSlotPositions(i);
                            if (player.Inventory.IsEquipmentSlotHovered((int)position.X, (int)position.Y, i))
                            {
                                var equipSlot = player.Inventory.equipmentSlots[i];
                                if (mouseItem == null)
                                {
                                    mouseItem = player.Inventory.GetEquippedItem(i);
                                    equipSlot.EquippedItem = null;
                                }
                                else
                                {
                                    if (mouseItem.Type == equipSlot.SlotType)
                                    {
                                        Item temp = mouseItem;
                                        mouseItem = player.Inventory.GetEquippedItem(i);
                                        equipSlot.EquippedItem = temp;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            if (Mouse.GetState().RightButton == ButtonState.Pressed && pMouse.RightButton == ButtonState.Released)
            {
                foreach (Player player in players)
                {
                    if (player.IsActive)
                    {
                        if (mouseItem != null)
                        {
                            mouseItem.Position = player.Position;
                            mouseItem.OnGround = true;
                            groundItems.Add(mouseItem);
                            mouseItem = null;
                        }

                        for (int y = 0; y < player.Inventory.Height; y++)
                        {
                            for (int x = 0; x < player.Inventory.Width; x++)
                            {
                                int slotSize = 44;
                                int slotSpacing = 0;
                                int slotX = x * (slotSize + slotSpacing) + 10;
                                int slotY = y * (slotSize + slotSpacing) + 50;
                                if (player.Inventory.IsSlotHovered(slotX, slotY))
                                {
                                    hoveredItem = player.Inventory.GetItem(x, y);
                                    if (hoveredItem != null)
                                    {
                                        player.Inventory.EquipItem(hoveredItem, groundItems, x, y);
                                    }
                                }
                            }
                        }

                        for (int i = 0; i < player.Inventory.equipmentSlots.Count; i++)
                        {
                            Vector2 position = EquipmentSlotPositions(i);
                            if (player.Inventory.IsEquipmentSlotHovered((int)position.X, (int)position.Y, i))
                            {
                                if (!player.Inventory.IsFull() && player.Inventory.equipmentSlots[i].EquippedItem != null)
                                {
                                    player.Inventory.AddItem(player.Inventory.equipmentSlots[i].EquippedItem, groundItems);
                                    player.Inventory.equipmentSlots[i].EquippedItem = null;
                                }
                            }
                        }
                    }
                }
            }
        }

        private void SortInventory(Keys key)
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

        private void SelectPlayer(List<Player> players, Keys key)
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
                    Rectangle slotRect = new Rectangle((int)player.Position.X, (int)player.Position.Y, player.PlayerTexture.Width, player.PlayerTexture.Height);
                    if (slotRect.Contains(Mouse.GetState().X, Mouse.GetState().Y))
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

        private void PickItemsUp(List<Player> playerList, List<Item> itemsOnGround, Keys key)
        {
            foreach (Item item in itemsOnGround.ToList())
            {
                foreach (Player player in playerList)
                {
                    if (Keyboard.GetState().IsKeyDown(key) && !pKey.IsKeyDown(key))
                    {
                        if (item.PlayerClose(player, 20f) && player.IsActive)
                        {
                            player.Inventory.PickItem(item, itemsOnGround);
                        }
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

        private void ClearItems(List<Item> itemsToRemove, bool clearInventory, bool clearGroundItems, bool clearEquippedItems, Keys key)
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
                    if (clearEquippedItems && player.IsActive)
                    {
                        player.Inventory.ClearEquippedItems();
                    }
                }
            }
            foreach (Item item in itemsToRemove)
            {
                groundItems.Remove(item);
            }
        }

        public static Vector2 EquipmentSlotPositions(int i)
        {
            Vector2 position;
            switch (i)
            {
                case 0:
                    position = new Vector2(242, 114);
                    break;

                case 1:
                    position = new Vector2(302, 114);
                    break;

                case 2:
                    position = new Vector2(362, 114);
                    break;

                case 3:
                    position = new Vector2(302, 174);
                    break;

                case 4:
                    position = new Vector2(302, 54);
                    break;

                case 5:
                    position = new Vector2(302, 52);
                    break;

                default:
                    position = Vector2.Zero;
                    break;
            }
            return position;
        }
    }
}