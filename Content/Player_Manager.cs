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
        private Text_Manager textManager;
        public Texture2D _texture;
        public Texture2D _textureHead;
        public Texture2D _textureEyes;
        private KeyboardState pKey;
        private MouseState pMouse;

        public Player_Manager(Game game, SpriteBatch spriteBatch, List<NPC> _npcs, List<Item> _items, List<Item> _groundItems, List<Item> _itemsToRemove, Dictionary<int,
            Item> _itemDictionary, Item_Manager _itemManager, Projectile_Manager _projManager, Text_Manager _textManager, KeyboardState _keyboardState)
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
        }

        public void Load()
        {
            Random rand = new Random();
            for (int i = 0; i < 10; i++)
            {
                players.Add(new Player(_texture, _textureHead, _textureEyes, (i < 5) ? "Warrior" : "Ranged", new Vector2(rand.Next(200, 600), rand.Next(200, 600)), 30000, (i < 5) ? 1f : 0.5f, false));
                if (itemDictionary.TryGetValue((i < 5) ? 10 : 3, out var itemData))
                {
                    players[i].equippedWeapon = itemData;
                }
            }
        }

        private Vector2 startPos, endPos;
        private bool isSelecting;

        public override void Update(GameTime gameTime)
        {
            foreach (Player player in players)
            {
                if (player.health > 0)
                {
                    player.Update(gameTime, itemDictionary, itemManager, textManager, projManager, npcs, groundItems, items);
                    if (player.isPicked)
                    {
                        PlayerMovementOrder();
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

            PlayerSelect(players, Keys.E);
            ClearItems(itemsToRemove, true, true, true, Keys.C);

            pMouse = Mouse.GetState();
            base.Update(gameTime);
        }

        private Vector2 lineStart = Vector2.Zero;
        private Vector2 lineEnd = Vector2.Zero;
        private List<Vector2> previewPositions = new List<Vector2>();
        private List<Vector2> previewPositionsRed = new List<Vector2>();

        private void PlayerMovementOrder()
        {
            if (Mouse.GetState().RightButton == ButtonState.Pressed && pMouse.RightButton == ButtonState.Released)
            {
                lineStart = new Vector2(Mouse.GetState().X, Mouse.GetState().Y);
            }

            if (Mouse.GetState().RightButton == ButtonState.Pressed)
            {
                lineEnd = new Vector2(Mouse.GetState().X, Mouse.GetState().Y);

                UpdatePreviewPositions();
            }

            if (Mouse.GetState().RightButton == ButtonState.Released && pMouse.RightButton == ButtonState.Pressed)
            {
                previewPositions.Clear();
                previewPositionsRed.Clear();

                List<Player> selectedPlayers = players.Where(p => p.isPicked).ToList();
                int numberOfPlayers = selectedPlayers.Count;

                if (numberOfPlayers > 1)
                {
                    float totalDistance = Vector2.Distance(lineStart, lineEnd);
                    if (totalDistance > numberOfPlayers * 16)
                    {
                        int spacing = (int)(totalDistance / (numberOfPlayers - 1));

                        previewPositions.Add(lineStart);

                        for (int i = 1; i < numberOfPlayers - 1; i++)
                        {
                            Vector2 offset = Vector2.Normalize(lineEnd - lineStart) * spacing * i;
                            Vector2 playerPosition = lineStart + offset;

                            previewPositions.Add(playerPosition);
                        }

                        previewPositions.Add(lineEnd);

                        for (int i = 0; i < numberOfPlayers; i++)
                        {
                            selectedPlayers[i].targetMovement = previewPositions[i];
                            selectedPlayers[i].hasMovementOrder = true;
                        }
                    }
                    else
                    {
                        int squareSize = (int)Math.Ceiling(Math.Sqrt(numberOfPlayers));
                        int sideLength = 50; // Adjust this value based on the desired spacing

                        for (int i = 0; i < numberOfPlayers; i++)
                        {
                            int row = i / squareSize;
                            int col = i % squareSize;

                            Vector2 playerPosition = new Vector2(lineStart.X + col * sideLength, lineStart.Y + row * sideLength);
                            previewPositions.Add(playerPosition);

                            selectedPlayers[i].targetMovement = playerPosition;
                            selectedPlayers[i].hasMovementOrder = true;
                        }
                    }

                }
                else if (numberOfPlayers == 1)
                {
                    Vector2 playerPosition = new Vector2(Mouse.GetState().X, Mouse.GetState().Y);
                    previewPositions.Add(playerPosition);
                    selectedPlayers[0].targetMovement = playerPosition;
                    selectedPlayers[0].hasMovementOrder = true;
                }

                previewPositions.Clear();
                previewPositionsRed.Clear();
            }
        }

        private void UpdatePreviewPositions()
        {
            previewPositions.Clear();
            previewPositionsRed.Clear();

            List<Player> selectedPlayers = players.Where(p => p.isPicked).ToList();
            int numberOfPlayers = selectedPlayers.Count;

            if (numberOfPlayers > 1)
            {
                float totalDistance = Vector2.Distance(lineStart, lineEnd);
                if (totalDistance > numberOfPlayers * 16)
                {
                    int spacing = (int)(totalDistance / (numberOfPlayers - 1));

                    previewPositions.Add(lineStart);

                    for (int i = 1; i < numberOfPlayers - 1; i++)
                    {
                        Vector2 offset = Vector2.Normalize(lineEnd - lineStart) * spacing * i;
                        Vector2 playerPosition = lineStart + offset;

                        previewPositions.Add(playerPosition);
                    }

                    previewPositions.Add(lineEnd);
                }
                else
                {
                    int spacing = (int)(totalDistance / (numberOfPlayers - 1));

                    previewPositionsRed.Add(lineStart);

                    for (int i = 1; i < numberOfPlayers - 1; i++)
                    {
                        Vector2 offset = Vector2.Normalize(lineEnd - lineStart) * spacing * i;
                        Vector2 playerPosition = lineStart + offset;

                        previewPositionsRed.Add(playerPosition);
                    }

                    previewPositionsRed.Add(lineEnd);
                }

            }
            else if (numberOfPlayers == 1)
            {
                previewPositions.Add(new Vector2(Mouse.GetState().X, Mouse.GetState().Y));
            }
        }

        private void PlayerSelect(List<Player> players, Keys key)
        {
            if (Keyboard.GetState().IsKeyDown(key) && !pKey.IsKeyDown(key))
            {

                Player activePlayer = players.FirstOrDefault(p => p.isControlled);

                if (activePlayer != null)
                {
                    activePlayer.isControlled = false;
                }

                Player closestPlayer = null;

                foreach (Player player in players)
                {
                    Rectangle slotRect = new Rectangle((int)player.position.X, (int)player.position.Y, player.textureBody.Width, player.textureBody.Height);
                    if (slotRect.Contains(Mouse.GetState().X, Mouse.GetState().Y))
                    {
                        closestPlayer = player;
                    }
                }

                if (closestPlayer != null)
                {
                    closestPlayer.isControlled = true;
                }
            }

            if (!Keyboard.GetState().IsKeyDown(Keys.LeftShift))
            {
                if (Mouse.GetState().LeftButton == ButtonState.Pressed)
                {
                    isSelecting = true;
                    startPos = new Vector2(Mouse.GetState().X, Mouse.GetState().Y);
                }

                if (!isSelecting)
                {
                    startPos = Vector2.Zero;
                    endPos = new Vector2(Mouse.GetState().X, Mouse.GetState().Y);
                }

                if (Mouse.GetState().LeftButton == ButtonState.Released && isSelecting)
                {
                    foreach (Player player in players)
                    {
                        player.isPicked = false;
                    }

                    bool anyPlayerSelected = false;

                    foreach (Player player in players)
                    {
                        if (player.rectangle.Intersects(RectangleBetweenTwoPoints(startPos, endPos)))
                        {
                            if (!player.isControlled)
                            {
                                player.isPicked = true;
                                anyPlayerSelected = true;
                            }
                            else
                            {
                                player.isPicked = false;
                            }
                        }
                    }

                    if (!anyPlayerSelected)
                    {
                        startPos = Vector2.Zero;
                        endPos = Vector2.Zero;
                    }

                    isSelecting = false;
                }
            }
            else
            {
                foreach (Player player in players)
                {
                    if (player.rectangle.Contains(new Vector2(Mouse.GetState().X, Mouse.GetState().Y)))
                    {
                        if (Mouse.GetState().LeftButton == ButtonState.Released && pMouse.LeftButton == ButtonState.Pressed)
                        {
                            if (player.isPicked)
                            {
                                player.isPicked = false;
                            }
                            else
                            {
                                player.isPicked = true;

                            }

                        }
                    }
                }
            }
        }

        public override void Draw(GameTime gameTime) //8617f max
        {
            spriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, Matrix.Identity);
            spriteBatch.DrawStringWithOutline(Main.testFont, "PLAYER COUNT: " + players.Count, new Vector2(10, 300), Color.Black, Color.White, 1f, 0.99f);
            if (Main.drawDebugRectangles)
            {
                spriteBatch.DrawStringWithOutline(Main.testFont, lineStart.ToString(), lineStart, Color.Black, Color.White, 1f, 0.99f);
                spriteBatch.DrawStringWithOutline(Main.testFont, lineEnd.ToString(), lineEnd, Color.Black, Color.White, 1f, 0.99f);
            }

            foreach (Vector2 position in previewPositions)
            {
                spriteBatch.DrawCircle(position, 8, Color.Aquamarine, 16, 0);
            }
            foreach (Vector2 position in previewPositionsRed)
            {
                spriteBatch.DrawCircle(position, 8, Color.Coral, 16, 0);
            }
            foreach (Player player in players)
            {
                player.Draw(spriteBatch);
            }

            if (startPos != Vector2.Zero)
            {
                spriteBatch.DrawRectangleOutlineBetweenPoints(startPos, endPos, Color.Black, 1f);
            }
            spriteBatch.End();
            base.Draw(gameTime);
        }

        public static Rectangle RectangleBetweenTwoPoints(Vector2 startPoint, Vector2 endPoint)
        {
            float x = Math.Min(startPoint.X, endPoint.X);
            float y = Math.Min(startPoint.Y, endPoint.Y);
            float width = Math.Abs(endPoint.X - startPoint.X);
            float height = Math.Abs(endPoint.Y - startPoint.Y);

            return new Rectangle((int)x, (int)y, (int)width, (int)height);
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
                    if (clearInventory)
                    {
                        player.inventory.ClearInventory();
                    }
                    if (clearEquippedItems)
                    {
                        player.inventory.ClearEquippedItems();
                    }
                }
            }
        }
    }
}
