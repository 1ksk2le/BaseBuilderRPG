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
        private MouseState pMouse;
        private KeyboardState pKey;

        public static Texture2D texInventory;
        public static Texture2D texInventorySlotBackground;
        public static Texture2D texAccessorySlotBackground;
        public static Texture2D texMainSlotBackground;

        public static SpriteFont TestFont;

        public static Effect OutlineShader;

        public static Vector2 basePosInventory = new Vector2(0, 0);

        public static Projectile_Manager projManager;
        public static Player_Manager playerManager;

        public static Item_Manager itemManager;
        public static Dictionary<int, Item> itemDictionary;
        public static List<Item> items;
        public List<Item> itemsToRemove;
        public List<Item> groundItems;
        public List<Player> players;

        public static int amountOfItems;
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
            spriteBatch = new SpriteBatch(GraphicsDevice);

            projManager = new Projectile_Manager(this, spriteBatch);

            itemManager = new Item_Manager(this, spriteBatch);
            items = itemManager.items;
            itemsToRemove = itemManager.itemsToRemove;
            groundItems = itemManager.groundItems;
            itemDictionary = itemManager.itemDictionary;


            playerManager = new Player_Manager(this, spriteBatch, items, groundItems, itemsToRemove, itemDictionary, itemManager, pKey);
            players = playerManager.players;

            Components.Add(projManager);
            Components.Add(itemManager);
            Components.Add(playerManager);

            OutlineShader = Content.Load<Effect>("Shaders/Outline");
            TestFont = Content.Load<SpriteFont>("Font_Test");
            texInventory = Content.Load<Texture2D>("Textures/tex_UI_Inventory");
            texInventorySlotBackground = Content.Load<Texture2D>("Textures/tex_UI_Inventory_Slot_Background");
            texAccessorySlotBackground = Content.Load<Texture2D>("Textures/tex_UI_Accessory_Slot_Background");
            texMainSlotBackground = Content.Load<Texture2D>("Textures/tex_UI_Main_Slot_Background");

            base.Initialize();
        }

        protected override void LoadContent()
        {
            base.LoadContent();
            projManager.Load();
            itemManager.Load();
            playerManager.Load();

            Random rand = new Random();
            foreach (Item item in items)
            {
                itemManager.DropItem(item.ID, -1, -1, 1, new Vector2(500 + item.ID * 50, 300));
            }
        }

        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

        }



        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Gray);

            spriteBatch.Begin();
            spriteBatch.DrawString(Main.TestFont, "AMOUNT OF ITEMS ADDED: " + amountOfItems.ToString(), new Vector2(10, 320), Color.Black, 0, Vector2.Zero, 1f, SpriteEffects.None, 1f);

            spriteBatch.End();

            base.Draw(gameTime);
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