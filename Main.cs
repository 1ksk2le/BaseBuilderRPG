using BaseBuilderRPG.Content;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace BaseBuilderRPG
{
    public class Main : Game
    {
        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;

        public static Texture2D pixel;
        public static Texture2D tex_Inventory;
        public static Texture2D tex_InventoryExtras;
        public static Texture2D tex_InventorySlotBackground;
        public static Texture2D tex_AccessorySlotBackground;
        public static Texture2D tex_MainSlotBackground;
        public static Texture2D tex_EffShadow;

        public static SpriteFont testFont;

        public static Effect outlineShader;

        public static Vector2 inventoryPos;
        public static int inventorySlotSize;
        public static int inventorySlotStartPos = 148;

        public static Text_Manager textManager;

        public static NPC_Globals globalNPC;
        public static Projectile_Globals globalProjectile;
        public static Player_Globals globalPlayer;
        public static Item_Globals globalItem;
        public static Particle_Globals globalParticleBelow;
        public static Particle_Globals globalParticleAbove;

        public static Dictionary<int, Item> itemDictionary;

        public static List<Item> items;
        public List<Item> itemsToRemove;
        public List<Item> groundItems;
        public List<Player> players;
        public List<Projectile> projectiles;
        public List<NPC> npcs;

        public static bool drawDebugRectangles;

        public static Item hoverItem;


        public static Random random;

        #region CONSOLE VARIABLES
        private string command;
        private List<string> commandHistory = new List<string>();
        private int currentCommandIndex = -1;
        public static bool isConsoleVisible;
        #endregion

        public static bool inventoryVisible = true;

        public Main()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            graphics.PreferredBackBufferWidth = 1500;
            graphics.PreferredBackBufferHeight = 800;
            graphics.ApplyChanges();
        }

        protected override void Initialize()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            outlineShader = Content.Load<Effect>("Shaders/Outline");
            testFont = Content.Load<SpriteFont>("Font_Test");

            globalParticleBelow = new Particle_Globals(this, spriteBatch);
            globalParticleAbove = new Particle_Globals(this, spriteBatch);

            globalProjectile = new Projectile_Globals(this, spriteBatch, globalParticleBelow, globalParticleAbove);
            projectiles = globalProjectile.projectiles;

            textManager = new Text_Manager(testFont);

            globalItem = new Item_Globals(this, spriteBatch, globalParticleBelow, globalParticleAbove);
            items = globalItem.items;
            groundItems = globalItem.groundItems;
            itemDictionary = globalItem.itemDictionary;

            globalPlayer = new Player_Globals(this, spriteBatch, npcs, items, groundItems, itemDictionary, globalItem, globalProjectile, textManager, globalParticleBelow, globalParticleAbove);
            players = globalPlayer.players;

            globalNPC = new NPC_Globals(this, spriteBatch, globalItem, globalParticleBelow, globalParticleAbove, globalProjectile, textManager, players, projectiles);
            npcs = globalNPC.npcs;
            globalPlayer.npcs = globalNPC.npcs;
            globalProjectile.npcs = globalNPC.npcs;


            Components.Add(globalParticleBelow);
            Components.Add(globalItem);
            Components.Add(globalNPC);
            Components.Add(globalProjectile);
            Components.Add(globalPlayer);
            Components.Add(globalParticleAbove);

            inventoryPos = new Vector2(graphics.PreferredBackBufferWidth - 200, graphics.PreferredBackBufferHeight - 400);
            inventorySlotSize = 38;

            globalNPC.NewNPC(0, new Vector2(200, 500));

            drawDebugRectangles = false;

            random = Main_Globals.GetRandomInstance();
            command = "";
            isConsoleVisible = false;

            hoverItem = null;

            base.Initialize();
        }

        protected override void LoadContent()
        {
            tex_Inventory = Content.Load<Texture2D>("Textures/UI/tex_UI_Inventory");
            tex_InventoryExtras = Content.Load<Texture2D>("Textures/UI/tex_UI_Inventory_Extras");
            tex_InventorySlotBackground = Content.Load<Texture2D>("Textures/UI/tex_UI_Inventory_Slot_Background");
            tex_MainSlotBackground = Content.Load<Texture2D>("Textures/UI/tex_UI_Main_Slot_Background");
            tex_EffShadow = Content.Load<Texture2D>("Textures/Effects/Shadow");
            pixel = new Texture2D(GraphicsDevice, 1, 1);
            pixel.SetData(new[] { Color.White });

            base.LoadContent();
            globalParticleBelow.Load();
            globalParticleAbove.Load();
            globalProjectile.Load();
            globalItem.Load();
            globalPlayer.Load();
            globalNPC.Load();

            foreach (Item item in items)
            {
                globalItem.DropItem(item.id, -1, -1, 1, new Vector2(500 + item.id * 50, 300));
            }
        }

        private bool isDragging = false;

        protected override void Update(GameTime gameTime)
        {
            var inputManager = Input_Manager.Instance;
            inputManager.PreUpdate();
            inputManager.PostUpdate(gameTime);

            Rectangle inventoryRectangle = new Rectangle((int)Main.inventoryPos.X, (int)Main.inventoryPos.Y - 24, 190, 24);

            if (inventoryRectangle.Contains(inputManager.mousePosition))
            {
                if (inputManager.IsButtonPressed(true))
                {
                    if (!isDragging)
                    {
                        isDragging = true;
                    }

                    float deltaX = (float)(inputManager.mousePosition.X - inputManager.previousMouseState.X);
                    float deltaY = (float)(inputManager.mousePosition.Y - inputManager.previousMouseState.Y);

                    inventoryPos.X += deltaX;
                    inventoryPos.Y += deltaY;
                }
                else
                {
                    isDragging = false;
                }
            }
            else
            {
                isDragging = false;
            }

            textManager.Update(gameTime);

            ConsoleCommands(inputManager);

            if (inputManager.IsKeySinglePress(Keys.Tab))
            {
                command = "";
                isConsoleVisible = isConsoleVisible ? false : true;
            }

            if (!isConsoleVisible)
            {
                if (inputManager.IsKeySinglePress(Keys.G))
                {
                    globalNPC.NewNPC(0, inputManager.mousePosition);
                }
            }

            foreach (Item item in groundItems)
            {
                if (item.InteractsWithMouse())
                {
                    hoverItem = item;
                }
                else
                {
                    hoverItem = null;
                }
            }
            base.Update(gameTime);
        }

        private void ConsoleCommands(Input_Manager inputManager)
        {
            if (isConsoleVisible)
            {
                if (inputManager.IsKeySinglePress(Keys.Enter))
                {
                    if (!string.IsNullOrWhiteSpace(command))
                    {
                        if (inputManager.IsKeySinglePress(Keys.Enter))
                        {
                            if (command.StartsWith("ADDITEM"))
                            {
                                string[] commandParts = command.Split(' ');

                                if (commandParts.Length >= 2)
                                {
                                    if (int.TryParse(commandParts[1], out int itemID))
                                    {
                                        foreach (Player player in players)
                                        {
                                            if (itemID < items.Count)
                                            {
                                                if (player.isControlled)
                                                {
                                                    commandHistory.Insert(0, "Given " + globalItem.GetItem(itemID).name + " to " + player.name);
                                                    currentCommandIndex = -1;
                                                    player.controlHandler.AddItemQuick(itemID, itemDictionary, globalItem, groundItems);
                                                }
                                            }
                                            else
                                            {
                                                commandHistory.Insert(0, "[!] Invalid item ID");
                                                currentCommandIndex = -1;
                                            }
                                        }
                                    }
                                }
                            }
                            else if (command == "HELP")
                            {
                                commandHistory.Insert(0, "");
                                commandHistory.Insert(0, "[?] SPAWNNPC [ID] [AMOUNT]- Spawns NPCs around the visible screen");
                                commandHistory.Insert(0, "[?] KILLNPCS - Kills all NPCs");
                                commandHistory.Insert(0, "[?] ADDITEM [ID] - Adds an item to the controlled player");
                                commandHistory.Insert(0, "[?] ITEMLIST - Displays added items");
                                commandHistory.Insert(0, "[?] DEVMODE - Displays stats for developers or hides them");
                                commandHistory.Insert(0, "[?] CONTROLS - Displays controls");
                                commandHistory.Insert(0, "[?] CLEAR - Clears commamnd prompt");
                                commandHistory.Insert(0, "[?] HELP - Displays all commands");
                                commandHistory.Insert(0, "");
                            }
                            else if (command == "CLEAR")
                            {
                                commandHistory.Clear();
                            }
                            else if (command == "ITEMLIST")
                            {
                                for (int i = items.Count - 1; i > -1; i--)
                                {
                                    commandHistory.Insert(0, "ID: " + i + " - " + items[i].name);
                                }
                            }
                            else if (command == "DEVMODE")
                            {
                                if (drawDebugRectangles)
                                {
                                    drawDebugRectangles = false;
                                }
                                else
                                {
                                    drawDebugRectangles = true;
                                }
                            }
                            else if (command == "KILLNPCS")
                            {
                                commandHistory.Insert(0, "Killed all NPCs");
                                foreach (NPC npc in npcs)
                                {
                                    npc.health = -1;
                                    npc.Kill(globalItem, globalParticleBelow);
                                }
                            }
                            else if (command == "CONTROLS")
                            {
                                commandHistory.Insert(0, "E- Control player at mouse position");
                                commandHistory.Insert(0, "C- Clear items");
                                commandHistory.Insert(0, "X- Add a random item to the controlled player");
                                commandHistory.Insert(0, "I- Open/close inventory");
                                commandHistory.Insert(0, "F- Pick item");
                                commandHistory.Insert(0, "G- Spawn Test Enemy");
                                commandHistory.Insert(0, "Left Shift + Left Mouse Button- Select players");
                                commandHistory.Insert(0, "Right Mouse Button- Move selected players");
                            }
                            else if (command.StartsWith("SPAWNNPC"))
                            {
                                string[] commandParts = command.Split(' ');

                                if (commandParts.Length == 3)
                                {
                                    if (int.TryParse(commandParts[1], out int npcID) && int.TryParse(commandParts[2], out int amount))
                                    {
                                        if (npcID >= 0 && npcID < 2)
                                        {
                                            Random rnd = new Random();
                                            commandHistory.Insert(0, "Spawned " + amount + " " + globalNPC.GetNPC(npcID).name + "(s)");
                                            for (int i = 0; i < amount; i++)
                                            {
                                                int x = rnd.Next(0, graphics.PreferredBackBufferWidth);
                                                int y = rnd.Next(0, graphics.PreferredBackBufferHeight);

                                                globalNPC.NewNPC(npcID, new Vector2(x, y));
                                            }
                                        }
                                        else
                                        {
                                            commandHistory.Insert(0, "[!] Invalid npc ID");
                                        }
                                    }
                                    else
                                    {
                                        commandHistory.Insert(0, "[!] Invalid NPC ID or amount");
                                    }
                                }
                                else
                                {
                                    commandHistory.Insert(0, "[!] Invalid command format");
                                }
                            }
                            else
                            {
                                commandHistory.Insert(0, "[!] Invalid command format");
                            }
                        }
                    }
                    if (command.Length > 0)
                    {
                        command = "";
                    }
                }
                if (!inputManager.IsKeySinglePress(Keys.Back))
                {
                    command += inputManager.GetPressedKeys();
                }
                else
                {
                    if (command.Length > 0)
                    {
                        command = command.Remove(command.Length - 1, 1);
                    }
                }



                if (inputManager.IsKeySinglePress(Keys.Down))
                {
                    if (currentCommandIndex < commandHistory.Count - 1)
                    {
                        currentCommandIndex++;
                        command = commandHistory[currentCommandIndex];
                    }
                }
                else if (inputManager.IsKeySinglePress(Keys.Up))
                {
                    if (currentCommandIndex >= 0)
                    {
                        currentCommandIndex--;
                        if (currentCommandIndex >= 0)
                        {
                            command = commandHistory[currentCommandIndex];
                        }
                        else
                        {
                            command = "";
                        }
                    }
                }
            }
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Gray);

            base.Draw(gameTime);

            spriteBatch.Begin();

            foreach (Player player in players)
            {
                if (player.isControlled && inventoryVisible)
                {
                    player.inventory.Draw(spriteBatch, player);
                }
            }
            if (!Input_Manager.Instance.IsMouseOnInventory(inventoryVisible))
            {
                DrawGlobalHoverItem(spriteBatch, hoverItem);
            }


            if (drawDebugRectangles)
            {
                spriteBatch.DrawRectangle(new Rectangle(10, graphics.PreferredBackBufferHeight - 32, 16, 16), Color.Cyan, 1f);
                spriteBatch.DrawStringWithOutline(Main.testFont, "Player AI attack range", new Vector2(32, graphics.PreferredBackBufferHeight - 28), Color.Black, Color.White, 1f, 0.99f);
                spriteBatch.DrawRectangle(new Rectangle(10, graphics.PreferredBackBufferHeight - 52, 16, 16), Color.Blue, 1f);
                spriteBatch.DrawStringWithOutline(Main.testFont, "Player hitbox rectangle", new Vector2(32, graphics.PreferredBackBufferHeight - 48), Color.Black, Color.White, 1f, 0.99f);
                spriteBatch.DrawRectangle(new Rectangle(10, graphics.PreferredBackBufferHeight - 72, 16, 16), Color.Red, 1f);
                spriteBatch.DrawStringWithOutline(Main.testFont, "NPC hitbox rectangle", new Vector2(32, graphics.PreferredBackBufferHeight - 68), Color.Black, Color.White, 1f, 0.99f);
                spriteBatch.DrawRectangle(new Rectangle(10, graphics.PreferredBackBufferHeight - 92, 16, 16), Color.Lime, 1f);
                spriteBatch.DrawStringWithOutline(Main.testFont, "Projectile hitbox rectangle", new Vector2(32, graphics.PreferredBackBufferHeight - 88), Color.Black, Color.White, 1f, 0.99f);
                spriteBatch.DrawRectangle(new Rectangle(10, graphics.PreferredBackBufferHeight - 112, 16, 16), Color.Yellow, 1f);
                spriteBatch.DrawStringWithOutline(Main.testFont, "Item hitbox rectangle", new Vector2(32, graphics.PreferredBackBufferHeight - 108), Color.Black, Color.White, 1f, 0.99f);
            }
            if (isConsoleVisible)
            {
                int consoleHeight = 55 + commandHistory.Count * 15;

                spriteBatch.DrawRectangle(new Rectangle(0, 0, graphics.PreferredBackBufferWidth, consoleHeight), new Color(0, 0, 0, 125), 1f);
                spriteBatch.DrawString(Main.testFont, "[Command Prompt]", new Vector2(10, 15), Color.Red);

                for (int i = 0; i < commandHistory.Count; i++)
                {
                    Color commandColor = Color.Gray;
                    if (commandHistory[i].StartsWith("[!]"))
                    {
                        commandColor = Color.Red;
                    }
                    if (commandHistory[i].StartsWith("[?]"))
                    {
                        commandColor = Color.LightSkyBlue;
                    }
                    if (commandHistory[i].StartsWith("Given"))
                    {
                        commandColor = Color.Lime;
                    }
                    spriteBatch.DrawString(Main.testFont, commandHistory[i], new Vector2(10, 30 + (i + 1) * 15), commandColor);
                }

                spriteBatch.DrawString(Main.testFont, ">> " + command, new Vector2(10, 30), Color.Yellow);
            }


            /*spriteBatch.DrawStringWithOutline(Main.testFont, "Controls", new Vector2(10, 20), Color.Black, Color.Yellow, 1f, 0.99f);
            spriteBatch.DrawStringWithOutline(Main.testFont, "E = Control player", new Vector2(10, 40), Color.Black, Color.White, 1f, 0.99f);
            spriteBatch.DrawStringWithOutline(Main.testFont, "X = Spawn item", new Vector2(10, 60), Color.Black, Color.White, 1f, 0.99f);
            spriteBatch.DrawStringWithOutline(Main.testFont, "C = Clear items", new Vector2(10, 80), Color.Black, Color.White, 1f, 0.99f);
            spriteBatch.DrawStringWithOutline(Main.testFont, "I = Open / Close inventory", new Vector2(10, 100), Color.Black, Color.White, 1f, 0.99f);
            spriteBatch.DrawStringWithOutline(Main.testFont, "F = Pick item", new Vector2(10, 120), Color.Black, Color.White, 1f, 0.99f);
            spriteBatch.DrawStringWithOutline(Main.testFont, "G = Spawn a slime", new Vector2(10, 140), Color.Black, Color.White, 1f, 0.99f);
            spriteBatch.DrawStringWithOutline(Main.testFont, "V = Kill npcs", new Vector2(10, 160), Color.Black, Color.White, 1f, 0.99f);
            spriteBatch.DrawStringWithOutline(Main.testFont, "L = Turn on / off debug mode", new Vector2(10, 180), Color.Black, Color.White, 1f, 0.99f);
            spriteBatch.DrawStringWithOutline(Main.testFont, "Left Shift + Left Mouse = Select players", new Vector2(10, 200), Color.Black, Color.White, 1f, 0.99f);
            spriteBatch.DrawStringWithOutline(Main.testFont, "Right Mouse = Move selected players", new Vector2(10, 220), Color.Black, Color.White, 1f, 0.99f);*/
            textManager.Draw(spriteBatch);
            spriteBatch.End();
        }

        public static Vector2 EquipmentSlotPositions(int i)
        {
            Vector2 position;
            switch (i)
            {
                case 0: //weapon
                    position = inventoryPos + new Vector2(28, 52);
                    break;

                case 1: //body armor
                    position = inventoryPos + new Vector2(74, 52); //+46
                    break;

                case 2: //off hand
                    position = inventoryPos + new Vector2(120, 52);
                    break;

                case 3://boots
                    position = inventoryPos + new Vector2(74, 98);
                    break;

                case 4: //head armor
                    position = inventoryPos + new Vector2(74, 6);
                    break;

                case 5: //accessory
                    position = new Vector2(302, 52);
                    break;

                default:
                    position = Vector2.Zero;
                    break;
            }
            return position;
        }

        private void DrawGlobalHoverItem(SpriteBatch spriteBatch, Item hoverItem)
        {
            if (hoverItem != null)
            {
                float maxTextWidth = 0;
                foreach (string tooltip in hoverItem.toolTips)
                {
                    Vector2 textSize = Main.testFont.MeasureString(tooltip);
                    maxTextWidth = Math.Max(maxTextWidth, textSize.X);
                }

                int initialX = (int)Input_Manager.Instance.mousePosition.X + 18;
                int initialY = (int)Input_Manager.Instance.mousePosition.Y;

                for (int i = 0; i < hoverItem.toolTips.Count; i++)
                {
                    Color toolTipColor, bgColor;
                    switch (i)
                    {
                        case 0:
                            toolTipColor = Color.White;
                            bgColor = hoverItem.rarityColor;
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

                    if (hoverItem.toolTips[i].StartsWith("'"))
                    {
                        toolTipColor = Color.Aquamarine;
                    }

                    Vector2 textSize = Main.testFont.MeasureString(hoverItem.toolTips[i]);
                    Vector2 backgroundSize = new Vector2(maxTextWidth, textSize.Y);

                    int tooltipX = initialX;
                    int tooltipY = initialY + i * ((int)textSize.Y);

                    if (tooltipX + (int)backgroundSize.X + 8 > /*GraphicsDevice.Viewport.Width*/1500)
                    {
                        tooltipX = (int)Input_Manager.Instance.mousePosition.X - (int)backgroundSize.X - 18;
                    }

                    spriteBatch.DrawRectangle(new Rectangle(tooltipX - 8, tooltipY + 8, (int)backgroundSize.X + 6, (int)backgroundSize.Y + 6), Color.Black, 0.98210f);
                    spriteBatch.DrawRectangle(new Rectangle(tooltipX - 4, tooltipY + 4, (int)backgroundSize.X + 8, (int)backgroundSize.Y + 4), hoverItem.rarityColor, 0.98211f);
                    spriteBatch.DrawRectangle(new Rectangle(tooltipX - 2, tooltipY + 6, (int)backgroundSize.X + 4, (int)backgroundSize.Y), bgColor, 0.98212f);

                    if (i == 0 || i == 1)
                    {
                        spriteBatch.DrawStringWithOutline(Main.testFont, hoverItem.toolTips[i], new Vector2(tooltipX + (maxTextWidth - textSize.X) / 2, tooltipY + 5), Color.Black, toolTipColor, 1f, 0.9822f);
                    }
                    else
                    {
                        spriteBatch.DrawStringWithOutline(Main.testFont, hoverItem.toolTips[i], new Vector2(tooltipX, tooltipY + 5), Color.Black, toolTipColor, 1f, 0.9822f);
                    }
                }
            }
        }
    }
}