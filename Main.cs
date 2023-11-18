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
        private KeyboardState pKey;

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
        public static NPC_Manager npcManager;
        public static Projectile_Manager projManager;
        public static Player_Manager playerManager;

        public static Item_Manager itemManager;
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

        private Vector2 lightPosition;
        protected override void Initialize()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            outlineShader = Content.Load<Effect>("Shaders/Outline");
            testFont = Content.Load<SpriteFont>("Font_Test");

            projManager = new Projectile_Manager(this, spriteBatch);
            projectiles = projManager.projectiles;

            textManager = new Text_Manager(testFont);

            itemManager = new Item_Manager(this, spriteBatch);
            items = itemManager.items;
            itemsToRemove = itemManager.itemsToRemove;
            groundItems = itemManager.groundItems;
            itemDictionary = itemManager.itemDictionary;


            playerManager = new Player_Manager(this, spriteBatch, npcs, items, groundItems, itemsToRemove, itemDictionary, itemManager, projManager, textManager, pKey);
            players = playerManager.players;

            npcManager = new NPC_Manager(this, spriteBatch, itemManager, textManager, players, projectiles);
            npcs = npcManager.npcs;
            playerManager.npcs = npcManager.npcs;

            Components.Add(itemManager);
            Components.Add(npcManager);
            Components.Add(projManager);
            Components.Add(playerManager);


            inventoryPos = new Vector2(graphics.PreferredBackBufferWidth - 200, graphics.PreferredBackBufferHeight - 400);
            inventorySlotSize = 38;

            npcManager.NewNPC(0, new Vector2(200, 500));

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
            projManager.Load();
            itemManager.Load();
            playerManager.Load();
            npcManager.Load();

            foreach (Item item in items)
            {
                itemManager.DropItem(item.id, -1, -1, 1, new Vector2(500 + item.id * 50, 300));
            }
        }

        private bool isDragging = false;
        private Vector2 previousMousePosition;

        protected override void Update(GameTime gameTime)
        {
            lightPosition = new Vector2(Mouse.GetState().X, Mouse.GetState().Y);
            if (npcs.Count <= 0)
            {
                for (int i = 0; i < 10; i++)
                {
                    Random rnd = new Random();
                    Random rnd2 = new Random();
                    npcManager.NewNPC(1, new Vector2(rnd.Next(0, graphics.PreferredBackBufferWidth), rnd2.Next(0, graphics.PreferredBackBufferHeight)));
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

            if (closeInvSlotRectangle.Contains(Mouse.GetState().X, Mouse.GetState().Y))
            {
                if (Mouse.GetState().LeftButton == ButtonState.Pressed)
                {

                    if (!isDragging)
                    {
                        isDragging = true;
                        previousMousePosition = new Vector2(Mouse.GetState().X, Mouse.GetState().Y);
                    }

                    float deltaX = Mouse.GetState().X - previousMousePosition.X;
                    float deltaY = Mouse.GetState().Y - previousMousePosition.Y;

                    inventoryPos.X += deltaX;
                    inventoryPos.Y += deltaY;

                    previousMousePosition = new Vector2(Mouse.GetState().X, Mouse.GetState().Y);
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

            if (Keyboard.GetState().IsKeyDown(Keys.G) && !pKey.IsKeyDown(Keys.G))
            {
                npcManager.NewNPC(1, new Vector2(Mouse.GetState().X, Mouse.GetState().Y));
            }
            if (Keyboard.GetState().IsKeyDown(Keys.V) && !pKey.IsKeyDown(Keys.V))
            {
                foreach (NPC npc in npcs)
                {
                    npc.health = -1;
                    npc.Kill(itemManager);
                }
            }
            if (Keyboard.GetState().IsKeyDown(Keys.L) && !pKey.IsKeyDown(Keys.L))
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


            pKey = Keyboard.GetState();
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
            spriteBatch.DrawStringWithOutline(Main.testFont, "K = Damage player", new Vector2(10, 120), Color.Black, Color.White, 1f, 0.99f);
            spriteBatch.DrawStringWithOutline(Main.testFont, "F = Pick item", new Vector2(10, 140), Color.Black, Color.White, 1f, 0.99f);
            spriteBatch.DrawStringWithOutline(Main.testFont, "G = Spawn a slime", new Vector2(10, 160), Color.Black, Color.White, 1f, 0.99f);
            spriteBatch.DrawStringWithOutline(Main.testFont, "V = Kill npcs", new Vector2(10, 180), Color.Black, Color.White, 1f, 0.99f);
            spriteBatch.DrawStringWithOutline(Main.testFont, "L = Turn on / off debug mode", new Vector2(10, 200), Color.Black, Color.White, 1f, 0.99f);
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