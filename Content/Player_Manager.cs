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
        public List<NPC> npcs;
        public List<Player> players;
        public List<Player> playersToRemove;
        private List<Item> items;
        private List<Item> groundItems;
        private List<Item> itemsToRemove;
        private Dictionary<int, Item> itemDictionary;
        private Item_Manager itemManager;
        private Projectile_Manager projManager;
        private Display_Text_Manager textManager;
        private Texture2D _texture;
        private Texture2D _textureHead;
        private Texture2D _textureEyes;
        private float useTimer = 0;

        private Item hoveredItem;
        private Item mouseItem;

        private MouseState pMouse;
        private KeyboardState pKey;

        public Player_Manager(Game game, SpriteBatch spriteBatch, List<NPC> _npcs, List<Item> _items, List<Item> _groundItems, List<Item> _itemsToRemove, Dictionary<int,
            Item> _itemDictionary, Item_Manager _itemManager, Projectile_Manager _projManager, Display_Text_Manager _textManager, KeyboardState _keyboardState)
            : base(game)
        {
            this.spriteBatch = spriteBatch;

            _texture = game.Content.Load<Texture2D>("Textures/Player/tex_Player_Body");
            _textureHead = game.Content.Load<Texture2D>("Textures/Player/tex_Player_Head");
            _textureEyes = game.Content.Load<Texture2D>("Textures/Player/tex_Player_Eyes");

            players = new List<Player>();
            playersToRemove = new List<Player>();
            npcs = _npcs;
            items = _items;
            groundItems = _groundItems;
            itemsToRemove = _itemsToRemove;
            itemDictionary = _itemDictionary;
            itemManager = _itemManager;
            textManager = _textManager;
            projManager = _projManager;
            pKey = _keyboardState;

            players.Add(new Player(_texture, _textureHead, _textureEyes, true, "East", 999999, 0.9f, new Vector2(10, 500)));
            //players.Add(new Player(_texture, _textureHead, _textureEyes, false, "Dummy", 99999, 1f, new Vector2(500, 500)));

            players[0].Inventory.equipmentSlots[0].EquippedItem = items[10];
            // players[1].Inventory.equipmentSlots[0].EquippedItem = items[10];

            for (int i = 0; i < items.Count; i++)
            {
                players[0].Inventory.AddItem(items[i], groundItems);
            }
        }

        public void Load()
        {

        }

        private void AI(Player player, List<NPC> npcs)
        {
            if (player.EquippedWeapon != null && player.EquippedWeapon.damageType == "melee")
            {
                NPC closestNPC = null;
                float closestDistance = float.MaxValue;

                foreach (NPC npc in npcs)
                {
                    float distance = Vector2.DistanceSquared(player.Position, npc.position);

                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestNPC = npc;
                    }
                }

                if (closestNPC != null)
                {
                    player.Target = closestNPC.position + new Vector2(closestNPC.width / 2, closestNPC.height / 2);

                    if (player.Position.X > player.Target.X)
                    {
                        player.Direction = -1;
                    }
                    else
                    {
                        player.Direction = 1;
                    }

                    Vector2 Pos = (player.Direction == 1) ? new Vector2(player.PlayerTexture.Width + player.EquippedWeapon.texture.Height * 0.2f, player.PlayerTexture.Height / 2)
                                                    : new Vector2(-player.EquippedWeapon.texture.Height * 0.2f, player.PlayerTexture.Height / 2);
                    Rectangle playerWeaponRectangle = CalculateRotatedRectangle(player.Position + Pos, (int)(player.EquippedWeapon.texture.Width * 1.75f), (int)(player.EquippedWeapon.texture.Height * 1.75f), player.RotationAngle);
                    Rectangle targetRectangle = new Rectangle((int)closestNPC.position.X, (int)closestNPC.position.Y, (int)(closestNPC.width), (int)(closestNPC.height));

                    //if (Vector2.Distance(player.Position, player.Target) >= player.EquippedWeapon.Texture.Height * 0.8f)
                    if (!playerWeaponRectangle.Intersects(targetRectangle))
                    {
                        Vector2 direction = player.Target - player.Position;
                        direction.Normalize();
                        player.Position += direction * 1.5f;
                    }
                    else
                    {
                        player.IsSwinging = true;
                    }
                }
            }
        }

        private Rectangle CalculateRotatedRectangle(Vector2 position, int width, int height, float rotation)
        {
            Matrix transform = Matrix.CreateRotationZ(rotation) * Matrix.CreateTranslation(position.X, position.Y, 0);

            Vector2 leftTop = Vector2.Transform(new Vector2(-width / 2, -height / 2), transform);
            Vector2 rightTop = Vector2.Transform(new Vector2(width / 2, -height / 2), transform);
            Vector2 leftBottom = Vector2.Transform(new Vector2(-width / 2, height / 2), transform);
            Vector2 rightBottom = Vector2.Transform(new Vector2(width / 2, height / 2), transform);

            Vector2 min = Vector2.Min(Vector2.Min(leftTop, rightTop), Vector2.Min(leftBottom, rightBottom));
            Vector2 max = Vector2.Max(Vector2.Max(leftTop, rightTop), Vector2.Max(leftBottom, rightBottom));

            return new Rectangle((int)min.X, (int)min.Y, (int)(max.X - min.X), (int)(max.Y - min.Y));
        }

        public override void Update(GameTime gameTime)
        {
            foreach (Player player in players)
            {
                if (player.Health > 0)
                {
                    player.Update(gameTime);
                    if (player.IsActive)
                    {
                        if (Keyboard.GetState().IsKeyDown(Keys.K) && !pKey.IsKeyDown(Keys.K))
                        {
                            Random rand2 = new Random();
                            int damage = rand2.Next(-10, -1) + 1;
                            player.Health += damage;
                            Color textColor = (damage > 0) ? Color.Lime : Color.Red;
                            textManager.AddFloatingText(damage.ToString(), "", player.Position - new Vector2(-player.PlayerTexture.Width / 2, 0), textColor, Color.Transparent, 2f, 1.1f);

                        }
                    }
                    else
                    {
                        AI(player, npcs);
                    }
                }
                else
                {
                    playersToRemove.Add(player);
                }
            }

            foreach (Player player in playersToRemove)
            {
                players.Remove(player);
            }

            PlayerShoot(gameTime);
            PlayerSelect(players, Keys.E);
            PlayerPickItem(players, groundItems, Keys.F);

            Random rand = new Random();
            PlayerSpawnItem(Keys.X, true, rand.Next(0, Main.amountOfItems));
            PlayerInventoryInteractions(Keys.I);
            PlayerSortInventory(Keys.Z);
            ClearItems(itemsToRemove, true, true, true, Keys.C);

            pKey = Keyboard.GetState();
            pMouse = Mouse.GetState();

            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime) //8617f max
        {
            spriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, Matrix.Identity);
            foreach (Player player in players)
            {
                player.Draw(spriteBatch);
            }
            spriteBatch.End();

            spriteBatch.Begin();
            spriteBatch.DrawString(Main.testFont, "SHOOT TIMER: " + useTimer.ToString(), new Vector2(10, 380), Color.Black, 0, Vector2.Zero, 1f, SpriteEffects.None, 1f);
            DrawInventory(gameTime);
            spriteBatch.End();

            base.Draw(gameTime);
        }

        private void DrawInventory(GameTime gameTime)
        {
            if (hoveredItem != null && mouseItem == null)
            {
                float maxTextWidth = 0;
                foreach (string tooltip in hoveredItem.toolTips)
                {
                    Vector2 textSize = Main.testFont.MeasureString(tooltip);
                    maxTextWidth = Math.Max(maxTextWidth, textSize.X);
                }

                // Calculate the initial position of the tooltip
                int initialX = Mouse.GetState().X + 18;
                int initialY = Mouse.GetState().Y;

                for (int i = 0; i < hoveredItem.toolTips.Count; i++)
                {
                    Color toolTipColor, bgColor;
                    switch (i)
                    {
                        case 0:
                            toolTipColor = Color.Black;
                            bgColor = hoveredItem.rarityColor;
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

                    if (hoveredItem.toolTips[i].StartsWith("'"))
                    {
                        toolTipColor = Color.Aquamarine;
                    }

                    Vector2 textSize = Main.testFont.MeasureString(hoveredItem.toolTips[i]);
                    Vector2 backgroundSize = new Vector2(maxTextWidth, textSize.Y);

                    int tooltipX = initialX;
                    int tooltipY = initialY + i * ((int)textSize.Y);

                    if (tooltipX + (int)backgroundSize.X + 8 > GraphicsDevice.Viewport.Width)
                    {
                        tooltipX = Mouse.GetState().X - (int)backgroundSize.X - 18;
                    }

                    // Draw the tooltip background
                    spriteBatch.DrawRectangle(new Rectangle(tooltipX - 4, tooltipY + 4, (int)backgroundSize.X + 8, (int)backgroundSize.Y + 4), hoveredItem.rarityColor, 0.922f);
                    spriteBatch.DrawRectangle(new Rectangle(tooltipX - 2, tooltipY + 6, (int)backgroundSize.X + 4, (int)backgroundSize.Y), bgColor, 0.922f);

                    if (i == 0 || i == 1)
                    {
                        spriteBatch.DrawString(Main.testFont, hoveredItem.toolTips[i], new Vector2(tooltipX + (maxTextWidth - textSize.X) / 2, tooltipY + 5), toolTipColor);
                    }
                    else
                    {
                        spriteBatch.DrawString(Main.testFont, hoveredItem.toolTips[i], new Vector2(tooltipX, tooltipY + 5), toolTipColor);
                    }
                }
            }

            if (mouseItem != null)
            {
                spriteBatch.Draw(mouseItem.texture, new Vector2(Mouse.GetState().X, Mouse.GetState().Y), Color.White);
            }
        }


        private void PlayerShoot(GameTime gameTime)
        {
            foreach (Player player in players)
            {
                if (player.EquippedWeapon != null && player.EquippedWeapon.shootID > -1 && player.IsActive)
                {
                    if (useTimer > 0)
                    {
                        useTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds;
                    }

                    if (Mouse.GetState().LeftButton == ButtonState.Pressed && useTimer <= 0)
                    {
                        projManager.NewProjectile(player.EquippedWeapon.shootID, player.Position + new Vector2(player.PlayerTexture.Width / 2, (player.PlayerTexture.Height) / 2), player.EquippedWeapon.damage, player.EquippedWeapon.shootSpeed, player, true);
                        useTimer = player.EquippedWeapon.useTime;
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

        private void PlayerPickItem(List<Player> playerList, List<Item> itemsOnGround, Keys key)
        {
            foreach (Item item in itemsOnGround.ToList())
            {
                foreach (Player player in playerList)
                {
                    if (Keyboard.GetState().IsKeyDown(key) && !pKey.IsKeyDown(key))
                    {
                        if (item.PlayerClose(player, 40f) && player.IsActive && !player.Inventory.IsFull())
                        {
                            string text = "Picked: " + item.prefixName + " " + item.name + " " + item.suffixName;
                            Vector2 textSize = Main.testFont.MeasureString(text);

                            Vector2 textPos = player.Position + new Vector2(-textSize.X / 5f, -20);
                            player.Inventory.PickItem(item, itemsOnGround);

                            textManager.AddFloatingText("Picked: ", (item.prefixName + " " + item.name + " " + item.suffixName), textPos, Color.White, item.rarityColor, 2f, 0.9f);
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

        private void PlayerInventoryInteractions(Keys key)
        {
            bool isMouseOverItem = false;

            foreach (Player player in players)
            {
                Rectangle closeInvSlotRectangle = new Rectangle((int)Main.inventoryPos.X + 170, (int)Main.inventoryPos.Y - 22, 20, 20);
                if (closeInvSlotRectangle.Contains(Mouse.GetState().X, Mouse.GetState().Y) && Mouse.GetState().LeftButton == ButtonState.Pressed && pMouse.LeftButton == ButtonState.Released)
                {
                    player.InventoryVisible = false;
                }
                if (player.IsActive && Keyboard.GetState().IsKeyDown(key) && !pKey.IsKeyDown(key))
                {
                    if (player.InventoryVisible)
                    {
                        player.InventoryVisible = false;
                    }
                    else
                    {
                        Main.inventoryPos = new Vector2(Mouse.GetState().X - Main.texInventory.Width / 2, Mouse.GetState().Y);
                        player.InventoryVisible = true;
                    }
                }

                if (player.IsActive && player.InventoryVisible)
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
                            int slotSize = Main.inventorySlotSize;
                            int slotX = (int)Main.inventoryPos.X + x * slotSize;
                            int slotY = (int)Main.inventoryPos.Y + y * slotSize + Main.inventorySlotStartPos;


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
                    if (player.IsActive && player.InventoryVisible)
                    {
                        for (int y = 0; y < player.Inventory.Height; y++)
                        {
                            for (int x = 0; x < player.Inventory.Width; x++)
                            {
                                int slotSize = Main.inventorySlotSize;
                                int slotX = (int)Main.inventoryPos.X + x * slotSize;
                                int slotY = (int)Main.inventoryPos.Y + y * slotSize + Main.inventorySlotStartPos;

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
                                    if (mouseItem.type == equipSlot.SlotType)
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
                            mouseItem.position = player.Position;
                            mouseItem.onGround = true;
                            groundItems.Add(mouseItem);
                            mouseItem = null;
                        }

                        if (player.InventoryVisible)
                        {
                            for (int y = 0; y < player.Inventory.Height; y++)
                            {
                                for (int x = 0; x < player.Inventory.Width; x++)
                                {
                                    int slotSize = Main.inventorySlotSize;
                                    int slotX = (int)Main.inventoryPos.X + x * slotSize;
                                    int slotY = (int)Main.inventoryPos.Y + y * slotSize + Main.inventorySlotStartPos;
                                    if (player.Inventory.IsSlotHovered(slotX, slotY))
                                    {
                                        hoveredItem = player.Inventory.GetItem(x, y);
                                        if (hoveredItem != null)
                                        {
                                            player.Inventory.EquipItem(hoveredItem, x, y);
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
        }

        private void PlayerSortInventory(Keys key)
        {
            Rectangle slotRect = new Rectangle((int)Main.inventoryPos.X + 84, (int)Main.inventoryPos.Y + 374, 20, 20);
            if (slotRect.Contains(Mouse.GetState().X, Mouse.GetState().Y))
            {
                foreach (Player player in players)
                {
                    if (player.IsActive)
                    {
                        if (Mouse.GetState().LeftButton == ButtonState.Pressed && pMouse.LeftButton == ButtonState.Released)
                        {
                            player.Inventory.SortItems();
                        }
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
