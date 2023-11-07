using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BaseBuilderRPG.Content
{
    public class Player_Manager : DrawableGameComponent
    {
        SpriteBatch spriteBatch;
        public List<Player> players;
        private List<Item> items;
        private List<Item> groundItems;
        private List<Item> itemsToRemove;
        private Dictionary<int, Item> itemDictionary;
        private Item_Manager itemManager;
        private Texture2D _texture;
        private float shootTimer = 0;

        private Item hoveredItem;
        private Item mouseItem;

        private MouseState pMouse;
        private KeyboardState pKey;

        public Player_Manager(Game game, SpriteBatch spriteBatch, List<Item> _items, List<Item> _groundItems, List<Item> _itemsToRemove, Dictionary<int, Item> _itemDictionary, Item_Manager _itemManager, KeyboardState _keyboardState)
            : base(game)
        {
            this.spriteBatch = spriteBatch;

            _texture = game.Content.Load<Texture2D>("Textures/tex_Player");

            players = new List<Player>();
            items = _items;
            groundItems = _groundItems;
            itemsToRemove = _itemsToRemove;
            itemDictionary = _itemDictionary;
            itemManager = _itemManager;
            pKey = _keyboardState;

            players.Add(new Player(_texture, true, "East", 140, new Vector2(10, 500)));
            players.Add(new Player(_texture, false, "Dummy", 100, new Vector2(30, 500)));
            players.Add(new Player(_texture, false, "West", 100, new Vector2(50, 500)));
        }

        public void Load()
        {

        }

        public override void Update(GameTime gameTime)
        {
            foreach (Player player in players)
            {
                player.Update(gameTime);
            }

            Item originalItem = items.Find(item => item.ID == 5);
            if (originalItem != null)
            {
                Item itemToAdd = originalItem.Clone(2, -1, -1, 1, false);
                players[0].Inventory.equipmentSlots[0].EquippedItem = itemToAdd;
            }

            PlayerShoot(gameTime);
            PlayerSelect(players, Keys.E);
            PlayerPıckItem(players, groundItems, Keys.F);

            Random rand = new Random();
            PlayerSpawnItem(Keys.X, true, rand.Next(0, Main.amountOfItems));
            PlayerInventoryInteractions();
            PlayerSortInventory(Keys.Z);
            ClearItems(itemsToRemove, true, true, true, Keys.C);

            pKey = Keyboard.GetState();
            pMouse = Mouse.GetState();

            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            spriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, null, null, null, null, Matrix.Identity);
            foreach (Player player in players)
            {
                player.Draw(spriteBatch, gameTime);
            }

            spriteBatch.DrawString(Main.TestFont, "SHOOT TIMER: " + shootTimer.ToString(), new Vector2(10, 380), Color.Black, 0, Vector2.Zero, 1f, SpriteEffects.None, 1f);

            spriteBatch.End();
            spriteBatch.Begin();
            DrawInventory(gameTime);
            spriteBatch.End();

            base.Draw(gameTime);
        }

        private void DrawInventory(GameTime gameTime)
        {
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
        }

        private void PlayerShoot(GameTime gameTime)
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
                        Projectile_Manager.NewProjectile(equippedWeapon.Shoot, equippedWeapon.Damage, 2, 2f, equippedWeapon.ShootSpeed, player.Position, player, true);
                        shootTimer = equippedWeapon.UseTime;
                    }
                }
            }
        }

        private void PlayerSelect(List<Player> players, Keys key)
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

        private void PlayerPıckItem(List<Player> playerList, List<Item> itemsOnGround, Keys key)
        {
            foreach (Item item in itemsOnGround.ToList())
            {
                foreach (Player player in playerList)
                {
                    if (Keyboard.GetState().IsKeyDown(key) && !pKey.IsKeyDown(key))
                    {
                        if (item.PlayerClose(player, 40f) && player.IsActive)
                        {
                            player.Inventory.PickItem(item, itemsOnGround);
                        }
                    }
                }
            }
        }

        private void PlayerSpawnItem(Keys key, bool addInventory, int itemID)
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
                                player.Inventory.AddItem(itemManager.NewItem(itemData, Vector2.Zero, prefixID, suffixID, 1, false), groundItems);
                            }
                        }
                    }
                }
                else
                {
                    Vector2 mousePosition = new Vector2(Mouse.GetState().X, Mouse.GetState().Y);
                    itemManager.DropItem(rand.Next(items.Count), prefixID, suffixID, rand.Next(1, 4), mousePosition);
                }
            }
        }

        private void PlayerInventoryInteractions()
        {
            bool isMouseOverItem = false;

            foreach (Player player in players)
            {
                if (player.IsActive)
                {
                    for (int i = 0; i < player.Inventory.equipmentSlots.Count; i++)
                    {
                        Vector2 position = Main.EquipmentSlotPositions(i);
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
                            Vector2 position = Main.EquipmentSlotPositions(i);
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
                            Vector2 position = Main.EquipmentSlotPositions(i);
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

        private void PlayerSortInventory(Keys key)
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
    }
}
