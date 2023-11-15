using BaseBuilderRPG.Content;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;

namespace BaseBuilderRPG
{
    public class Main : Game
    {
        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;
        private KeyboardState pKey;

        public static Texture2D texInventory;
        public static Texture2D texInventoryExtras;
        public static Texture2D texInventorySlotBackground;
        public static Texture2D texAccessorySlotBackground;
        public static Texture2D texMainSlotBackground;
        public static Texture2D DebugTexture;

        public static SpriteFont TestFont;

        public static Effect OutlineShader;

        public static Vector2 inventoryPos;
        public static int inventorySlotSize;
        public static int inventorySlotStartPos = 148;

        public static Display_Text_Manager disTextManager;
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

        public static int amountOfItems;
        public static bool DrawDebugRectangles;
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

            OutlineShader = Content.Load<Effect>("Shaders/Outline");
            TestFont = Content.Load<SpriteFont>("Font_Test");

            projManager = new Projectile_Manager(this, spriteBatch);
            projectiles = projManager.projectiles;

            disTextManager = new Display_Text_Manager(TestFont);

            itemManager = new Item_Manager(this, spriteBatch);
            items = itemManager.items;
            itemsToRemove = itemManager.itemsToRemove;
            groundItems = itemManager.groundItems;
            itemDictionary = itemManager.itemDictionary;


            playerManager = new Player_Manager(this, spriteBatch, npcs, items, groundItems, itemsToRemove, itemDictionary, itemManager, disTextManager, pKey);
            players = playerManager.players;

            npcManager = new NPC_Manager(this, spriteBatch, itemManager, disTextManager, players, projectiles);
            npcs = npcManager.npcs;
            playerManager.npcs = npcManager.npcs;

            Components.Add(itemManager);
            Components.Add(npcManager);
            Components.Add(projManager);
            Components.Add(playerManager);


            inventoryPos = new Vector2(graphics.PreferredBackBufferWidth - 200, graphics.PreferredBackBufferHeight - 400);
            inventorySlotSize = 38;

            npcManager.NewNPC(0, new Vector2(200, 500));

            DrawDebugRectangles = false;
            base.Initialize();
        }

        protected override void LoadContent()
        {
            texInventory = Content.Load<Texture2D>("Textures/UI/tex_UI_Inventory");
            texInventoryExtras = Content.Load<Texture2D>("Textures/UI/tex_UI_Inventory_Extras");
            texInventorySlotBackground = Content.Load<Texture2D>("Textures/UI/tex_UI_Inventory_Slot_Background");
            texMainSlotBackground = Content.Load<Texture2D>("Textures/UI/tex_UI_Main_Slot_Background");

            DebugTexture = new Texture2D(GraphicsDevice, 1, 1);
            DebugTexture.SetData(new Color[] { Color.White });

            base.LoadContent();
            projManager.Load();
            itemManager.Load();
            playerManager.Load();
            npcManager.Load();

            foreach (Item item in items)
            {
                itemManager.DropItem(item.ID, -1, -1, 1, new Vector2(500 + item.ID * 50, 300));
            }
        }

        private bool isDragging = false;
        private Vector2 previousMousePosition;

        protected override void Update(GameTime gameTime)
        {
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

            disTextManager.Update(gameTime);

            if (Keyboard.GetState().IsKeyDown(Keys.G) && !pKey.IsKeyDown(Keys.G))
            {
                npcManager.NewNPC(1, new Vector2(Mouse.GetState().X, Mouse.GetState().Y));
            }
            if (Keyboard.GetState().IsKeyDown(Keys.V) && !pKey.IsKeyDown(Keys.V))
            {
                foreach (NPC npc in npcs)
                {
                    npc.Health = -1;
                    npc.Kill(itemManager);
                }
            }
            if (Keyboard.GetState().IsKeyDown(Keys.L) && !pKey.IsKeyDown(Keys.L))
            {
                if (DrawDebugRectangles)
                {
                    DrawDebugRectangles = false;
                }
                else
                {
                    DrawDebugRectangles = true;
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
            spriteBatch.DrawString(Main.TestFont, "Controls: ", new Vector2(10, 20), Color.Black, 0, Vector2.Zero, 1f, SpriteEffects.None, 1f);
            spriteBatch.DrawString(Main.TestFont, "E = Control player", new Vector2(10, 40), Color.Black, 0, Vector2.Zero, 1f, SpriteEffects.None, 1f);
            spriteBatch.DrawString(Main.TestFont, "X = Spawn item", new Vector2(10, 60), Color.Black, 0, Vector2.Zero, 1f, SpriteEffects.None, 1f);
            spriteBatch.DrawString(Main.TestFont, "C = Clear items", new Vector2(10, 80), Color.Black, 0, Vector2.Zero, 1f, SpriteEffects.None, 1f);
            spriteBatch.DrawString(Main.TestFont, "I = Open / Close inventory", new Vector2(10, 100), Color.Black, 0, Vector2.Zero, 1f, SpriteEffects.None, 1f);
            spriteBatch.DrawString(Main.TestFont, "K = Damage player", new Vector2(10, 120), Color.Black, 0, Vector2.Zero, 1f, SpriteEffects.None, 1f);
            spriteBatch.DrawString(Main.TestFont, "F = Pick item up from ground", new Vector2(10, 140), Color.Black, 0, Vector2.Zero, 1f, SpriteEffects.None, 1f);
            spriteBatch.DrawString(Main.TestFont, "G = Spawn a slime", new Vector2(10, 160), Color.Black, 0, Vector2.Zero, 1f, SpriteEffects.None, 1f);
            spriteBatch.DrawString(Main.TestFont, "V = Kill npcs", new Vector2(10, 180), Color.Black, 0, Vector2.Zero, 1f, SpriteEffects.None, 1f);
            spriteBatch.DrawString(Main.TestFont, "L = Draw debug rectangles", new Vector2(10, 200), Color.Black, 0, Vector2.Zero, 1f, SpriteEffects.None, 1f);
            spriteBatch.DrawString(Main.TestFont, "AMOUNT OF ITEMS ADDED: " + amountOfItems.ToString(), new Vector2(10, 320), Color.Black, 0, Vector2.Zero, 1f, SpriteEffects.None, 1f);
            disTextManager.Draw(spriteBatch);
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