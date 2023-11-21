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
        public static Texture2D texInventory;
        public static Texture2D texInventoryExtras;
        public static Texture2D texInventorySlotBackground;
        public static Texture2D texAccessorySlotBackground;
        public static Texture2D texMainSlotBackground;

        public static SpriteFont testFont;

        public static Effect outlineShader;

        public static Vector2 inventoryPos;
        public static int inventorySlotSize;
        public static int inventorySlotStartPos = 148;

        public static Text_Manager textManager;

        public static Global_NPC globalNPC;
        public static Global_Projectile globalProjectile;
        public static Global_Player globalPlayer;
        public static Global_Item globalItem;

        public static Dictionary<int, Item> itemDictionary;

        public static List<Item> items;
        public List<Item> itemsToRemove;
        public List<Item> groundItems;
        public List<Player> players;
        public List<Projectile> projectiles;
        public List<NPC> npcs;

        public static bool drawDebugRectangles;
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

            globalProjectile = new Global_Projectile(this, spriteBatch);
            projectiles = globalProjectile.projectiles;

            textManager = new Text_Manager(testFont);

            globalItem = new Global_Item(this, spriteBatch);
            items = globalItem.items;
            itemsToRemove = globalItem.itemsToRemove;
            groundItems = globalItem.groundItems;
            itemDictionary = globalItem.itemDictionary;


            globalPlayer = new Global_Player(this, spriteBatch, npcs, items, groundItems, itemsToRemove, itemDictionary, globalItem, globalProjectile, textManager);
            players = globalPlayer.players;

            globalNPC = new Global_NPC(this, spriteBatch, globalItem, textManager, players, projectiles);
            npcs = globalNPC.npcs;
            globalPlayer.npcs = globalNPC.npcs;

            Components.Add(globalItem);
            Components.Add(globalNPC);
            Components.Add(globalProjectile);
            Components.Add(globalPlayer);


            inventoryPos = new Vector2(graphics.PreferredBackBufferWidth - 200, graphics.PreferredBackBufferHeight - 400);
            inventorySlotSize = 38;

            globalNPC.NewNPC(0, new Vector2(200, 500));

            drawDebugRectangles = true;
            base.Initialize();
        }

        protected override void LoadContent()
        {
            texInventory = Content.Load<Texture2D>("Textures/UI/tex_UI_Inventory");
            texInventoryExtras = Content.Load<Texture2D>("Textures/UI/tex_UI_Inventory_Extras");
            texInventorySlotBackground = Content.Load<Texture2D>("Textures/UI/tex_UI_Inventory_Slot_Background");
            texMainSlotBackground = Content.Load<Texture2D>("Textures/UI/tex_UI_Main_Slot_Background");
            pixel = new Texture2D(GraphicsDevice, 1, 1);
            pixel.SetData(new[] { Color.White });

            base.LoadContent();
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

            if (npcs.Count <= 0)
            {
                for (int i = 0; i < 10; i++)
                {
                    Random rnd = new Random();
                    Random rnd2 = new Random();
                    globalNPC.NewNPC(1, new Vector2(rnd.Next(0, graphics.PreferredBackBufferWidth), rnd2.Next(0, graphics.PreferredBackBufferHeight)));
                }
            }

            foreach (Player p in players)
            {
                if (p.position.X > graphics.PreferredBackBufferWidth || p.position.Y > graphics.PreferredBackBufferHeight)
                {
                    p.position = new Vector2(100, 100);
                }
            }

            Rectangle closeInvSlotRectangle = new Rectangle((int)Main.inventoryPos.X, (int)Main.inventoryPos.Y - 22, 170, 24);
            Rectangle inventoryRectangle = new Rectangle((int)Main.inventoryPos.X, (int)Main.inventoryPos.Y - 24, 190, Main.texInventory.Height + Main.texInventoryExtras.Height);

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

            if (inputManager.IsKeySinglePress(Keys.G))
            {
                globalNPC.NewNPC(1, inputManager.mousePosition);
            }
            if (inputManager.IsKeySinglePress(Keys.V))
            {
                foreach (NPC npc in npcs)
                {
                    npc.health = -1;
                    npc.Kill(globalItem);
                }
            }
            if (inputManager.IsKeySinglePress(Keys.L))
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
            base.Update(gameTime);
        }




        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Gray);

            base.Draw(gameTime);

            spriteBatch.Begin();


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
            spriteBatch.DrawStringWithOutline(Main.testFont, "Controls", new Vector2(10, 20), Color.Black, Color.Yellow, 1f, 0.99f);
            spriteBatch.DrawStringWithOutline(Main.testFont, "E = Control player", new Vector2(10, 40), Color.Black, Color.White, 1f, 0.99f);
            spriteBatch.DrawStringWithOutline(Main.testFont, "X = Spawn item", new Vector2(10, 60), Color.Black, Color.White, 1f, 0.99f);
            spriteBatch.DrawStringWithOutline(Main.testFont, "C = Clear items", new Vector2(10, 80), Color.Black, Color.White, 1f, 0.99f);
            spriteBatch.DrawStringWithOutline(Main.testFont, "I = Open / Close inventory", new Vector2(10, 100), Color.Black, Color.White, 1f, 0.99f);
            spriteBatch.DrawStringWithOutline(Main.testFont, "F = Pick item", new Vector2(10, 120), Color.Black, Color.White, 1f, 0.99f);
            spriteBatch.DrawStringWithOutline(Main.testFont, "G = Spawn a slime", new Vector2(10, 140), Color.Black, Color.White, 1f, 0.99f);
            spriteBatch.DrawStringWithOutline(Main.testFont, "V = Kill npcs", new Vector2(10, 160), Color.Black, Color.White, 1f, 0.99f);
            spriteBatch.DrawStringWithOutline(Main.testFont, "L = Turn on / off debug mode", new Vector2(10, 180), Color.Black, Color.White, 1f, 0.99f);
            spriteBatch.DrawStringWithOutline(Main.testFont, "Left Shift + Left Mouse = Select players", new Vector2(10, 200), Color.Black, Color.White, 1f, 0.99f);
            spriteBatch.DrawStringWithOutline(Main.testFont, "Right Mouse = Move selected players", new Vector2(10, 220), Color.Black, Color.White, 1f, 0.99f);
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
    }
}