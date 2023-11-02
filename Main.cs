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
    public class Main : Game
    {
        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;
        private MouseState pMouse;
        private KeyboardState pKey;

        private int amountOfItems;

        private List<Player> players;

        private List<Item> items;
        private List<Item> itemsToRemove;
        private List<Item> groundItems;

        private List<Projectile> projectiles;
        private List<Projectile> projectilesToRemove;

        private Texture2D texPlayer;
        public static Texture2D texInventory;
        public static Texture2D texInventorySlotBackground;
        public static Texture2D texAccessorySlotBackground;
        public static Texture2D texMainSlotBackground;

        public static SpriteFont TestFont;

        public static Effect OutlineShader;

        private Item hoveredItem;
        private Item mouseItem;

        private Dictionary<int, Projectile> projectileDictionary;
        private Dictionary<int, Item> itemDictionary;

        private float shootTimer = 0;
        public static Vector2 basePosInventory = new Vector2(0, 0);

        public Main()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            graphics.PreferredBackBufferWidth = 1680;
            graphics.PreferredBackBufferHeight = 1050;
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

            itemDictionary = new Dictionary<int, Item>();
            string itemsJson = File.ReadAllText("Content/items.json");
            items = JsonConvert.DeserializeObject<List<Item>>(itemsJson);
            foreach (Item item in items)
            {
                item.Texture = Content.Load<Texture2D>(item.TexturePath);
                itemDictionary.Add(item.ID, item);
            }
            amountOfItems = items.Count;

            projectileDictionary = new Dictionary<int, Projectile>();
            string projectilesJson = File.ReadAllText("Content/projectiles.json");
            projectiles = JsonConvert.DeserializeObject<List<Projectile>>(projectilesJson);
            foreach (Projectile projectile in projectiles)
            {
                projectile.Texture = Content.Load<Texture2D>(projectile.TexturePath);
                projectileDictionary.Add(projectile.ID, projectile);
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
            projectilesToRemove = new List<Projectile>();
            groundItems = new List<Item>();
            itemsToRemove = new List<Item>();

            Random rand = new Random();
            foreach (Item item in items)
            {
                DropItem(item.ID, -1, -1, 1, new Vector2(500 + item.ID * 50, 300));
            }
        }

        protected override void Update(GameTime gameTime)
        {
            foreach (Player player in players)
            {
                player.Update(gameTime);
            }
            foreach (Item item in groundItems)
            {
                item.Update(gameTime);
            }
            foreach (Item item in items)
            {
                if (!item.OnGround)
                {
                    itemsToRemove.Add(item);
                }
            }
            foreach (Item item in itemsToRemove)
            {
                groundItems.Remove(item);
                items.Remove(item);
            }
            foreach (Projectile projectile in projectiles)
            {
                if (projectile.IsAlive)
                {
                    projectile.Update(gameTime);
                }
                else
                {
                    projectilesToRemove.Add(projectile);
                }
            }
            foreach (Projectile projectile in projectilesToRemove)
            {
                projectiles.Remove(projectile);
            }

            Item originalItem = items.Find(item => item.ID == 5);
            if (originalItem != null)
            {
                Item itemToAdd = originalItem.Clone(5, -1, -1, 1, false);
                players[0].Inventory.equipmentSlots[0].EquippedItem = itemToAdd;
            }

            Random rand = new Random();
            SpawnItem(Keys.X, true, rand.Next(0, amountOfItems));
            SelectPlayer(players, Keys.E);
            PickItemsUp(players, groundItems, Keys.F);
            ClearItems(itemsToRemove, true, true, true, Keys.C);
            SortInventory(Keys.Z);
            HandleInventoryInteractions();

            Shoot(gameTime);
            pMouse = Mouse.GetState();
            pKey = Keyboard.GetState();



            base.Update(gameTime);
        }

        private void Shoot(GameTime gameTime)
        {
            foreach (Player player in players)
            {
                var equippedWeapon = player.Inventory.equipmentSlots[0].EquippedItem;
                if (equippedWeapon != null && equippedWeapon.Shoot > -1 && player.IsActive)
                {
                    if (shootTimer > 0)
                    {
                        shootTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds;
                    }

                    if (Mouse.GetState().LeftButton == ButtonState.Pressed && shootTimer <= 0)
                    {
                        if (projectileDictionary.TryGetValue(equippedWeapon.Shoot, out var projectileData))
                        {
                            Projectile proj = new Projectile(
                                texture: Content.Load<Texture2D>(projectileData.TexturePath),
                                texturePath: projectileData.TexturePath,
                                name: projectileData.Name,
                                id: equippedWeapon.Shoot,
                                ai: projectileData.AI,
                                damage: equippedWeapon.Damage,
                                lifeTime: projectileData.LifeTime,
                                knockBack: projectileData.KnockBack,
                                position: player.Position,
                                owner: player,
                                isAlive: true
                            );

                            proj.Speed = equippedWeapon.ShootSpeed;

                            projectiles.Add(proj);

                            shootTimer = equippedWeapon.UseTime;
                        }
                    }
                }
            }
        }



        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Gray);


            foreach (Item item in groundItems)
            {
                item.Draw(spriteBatch);
            }

            foreach (Projectile projectile in projectiles)
            {
                projectile.Draw(spriteBatch);
            }

            spriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, null, null, null, null, Matrix.Identity);
            foreach (Player player in players)
            {
                player.Draw(spriteBatch);
            }
            spriteBatch.End();

            spriteBatch.Begin();

            spriteBatch.DrawString(Main.TestFont, "ELAPSED GAME TIME: " + gameTime.ElapsedGameTime.TotalSeconds.ToString(), new Vector2(10, 300), Color.Black, 0, Vector2.Zero, 1f, SpriteEffects.None, 1f);
            spriteBatch.DrawString(Main.TestFont, "AMOUNT OF PROJS: " + projectiles.Count.ToString(), new Vector2(10, 320), Color.Black, 0, Vector2.Zero, 1f, SpriteEffects.None, 1f);
            spriteBatch.DrawString(Main.TestFont, "AMOUNT OF ITEMS (DICTIONARY KULLAN): " + items.Count.ToString(), new Vector2(10, 340), Color.Black, 0, Vector2.Zero, 1f, SpriteEffects.None, 1f);
            spriteBatch.DrawString(Main.TestFont, "AMOUNT OF GROUND ITEMS: " + groundItems.Count.ToString(), new Vector2(10, 360), Color.Black, 0, Vector2.Zero, 1f, SpriteEffects.None, 1f);
            spriteBatch.DrawString(Main.TestFont, "SHOOT TIMER: " + shootTimer.ToString(), new Vector2(10, 380), Color.Black, 0, Vector2.Zero, 1f, SpriteEffects.None, 1f);

            if (hoveredItem != null && mouseItem == null)
            {
                float maxTextWidth = 0;
                foreach (string tooltip in hoveredItem.ToolTips)
                {
                    Vector2 textSize = Main.TestFont.MeasureString(tooltip);
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

                    Vector2 textSize = Main.TestFont.MeasureString(hoveredItem.ToolTips[i]);
                    Vector2 backgroundSize = new Vector2(maxTextWidth, textSize.Y);

                    int tooltipY = (int)Mouse.GetState().Y + i * ((int)textSize.Y);

                    spriteBatch.DrawRectangle(new Rectangle((int)Mouse.GetState().X + xOffSet - 4, tooltipY + 4, (int)backgroundSize.X + 8, (int)backgroundSize.Y + 4), hoveredItem.RarityColor);
                    spriteBatch.DrawRectangle(new Rectangle((int)Mouse.GetState().X + xOffSet - 2, tooltipY + 6, (int)backgroundSize.X + 4, (int)backgroundSize.Y), bgColor);

                    if (i == 0 || i == 1)
                    {
                        spriteBatch.DrawString(Main.TestFont, hoveredItem.ToolTips[i], new Vector2((int)Mouse.GetState().X + xOffSet + (maxTextWidth - textSize.X) / 2, tooltipY + 5), toolTipColor);
                    }
                    else
                    {
                        spriteBatch.DrawString(Main.TestFont, hoveredItem.ToolTips[i], new Vector2((int)Mouse.GetState().X + xOffSet, tooltipY + 5), toolTipColor);
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

        private void SpawnItem(Keys key, bool addInventory, int itemID)
        {
            if (Keyboard.GetState().IsKeyDown(key) && !pKey.IsKeyDown(key))
            {
                Random rand = new Random();
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
                            if (itemDictionary.TryGetValue(itemID, out var itemData))
                            {
                                Item item = new Item(
                                    texture: Content.Load<Texture2D>(itemData.TexturePath),
                                    texturePath: itemData.TexturePath,
                                    id: itemID,
                                    name: itemData.Name,
                                    type: itemData.Type,
                                    damageType: itemData.DamageType,
                                    position: itemData.Position,
                                    shootSpeed: itemData.ShootSpeed,
                                    rarity: itemData.Rarity,
                                    shoot: itemData.Shoot,
                                    prefixID: prefixID,
                                    suffixID: suffixID,
                                    damage: itemData.Damage,
                                    useTime: itemData.UseTime,
                                    stackLimit: itemData.StackLimit,
                                    dropAmount: (itemData.StackLimit == 1) ? 1 : itemData.StackSize,
                                    onGround: itemData.OnGround
                                );

                                player.Inventory.AddItem(item, groundItems);
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
            if (itemDictionary.TryGetValue(itemID, out var itemData))
            {
                Item item = new Item(
                    texture: Content.Load<Texture2D>(itemData.TexturePath),
                    texturePath: itemData.TexturePath,
                    id: itemID,
                    name: itemData.Name,
                    type: itemData.Type,
                    damageType: itemData.DamageType,
                    position: position,
                    shootSpeed: itemData.ShootSpeed,
                    rarity: itemData.Rarity,
                    shoot: itemData.Shoot,
                    prefixID: prefixID,
                    suffixID: suffixID,
                    damage: itemData.Damage,
                    useTime: itemData.UseTime,
                    stackLimit: itemData.StackLimit,
                    dropAmount: (itemData.StackLimit == 1) ? 1 : dropAmount,
                    onGround: true
                );

                groundItems.Add(item);
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