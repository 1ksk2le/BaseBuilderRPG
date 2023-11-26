using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BaseBuilderRPG.Content
{
    public class Player_Globals : DrawableGameComponent
    {
        private SpriteBatch spriteBatch;
        public List<NPC> npcs;
        public List<Player> players;
        private List<Item> items;
        private List<Item> groundItems;
        private Dictionary<int, Item> itemDictionary;
        private Item_Globals globalItem;
        private Projectile_Globals globalProjectile;
        private Particle_Globals globalParticle;
        private Text_Manager textManager;
        public Texture2D _texture;
        public Texture2D _textureHead;
        public Texture2D _textureEyes;

        public Player_Globals(Game game, SpriteBatch spriteBatch, List<NPC> npcs, List<Item> items, List<Item> groundItems, Dictionary<int,
            Item> itemDictionary, Item_Globals globalItem, Projectile_Globals globalProjectile, Text_Manager textManager, Particle_Globals globalParticle)
            : base(game)
        {
            this.spriteBatch = spriteBatch;

            _texture = game.Content.Load<Texture2D>("Textures/Player/tex_Player_Body");
            _textureHead = game.Content.Load<Texture2D>("Textures/Player/tex_Player_Head");
            _textureEyes = game.Content.Load<Texture2D>("Textures/Player/tex_Player_Eyes");

            players = new List<Player>();
            this.npcs = npcs;
            this.items = items;
            this.groundItems = groundItems;
            this.itemDictionary = itemDictionary;
            this.globalItem = globalItem;
            this.textManager = textManager;
            this.globalProjectile = globalProjectile;
            this.globalParticle = globalParticle;
        }

        public void Load()
        {
            for (int i = 0; i < 1; i++)
            {
                players.Add(new Player(_texture, _textureHead, _textureEyes, (i < 5) ? "Player AI" : "Ranged", new Vector2(Main.random.Next(200, 600), Main.random.Next(200, 600)), 30000, (i < 5) ? 1f : 0.5f, true));
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
                    player.Update(gameTime, itemDictionary, globalItem, textManager, globalProjectile, globalParticle, npcs, groundItems, items);
                    if (player.isPicked)
                    {
                        PlayerMovementOrder(player);
                    }
                }
            }

            players.RemoveAll(player => (player.health <= 0));

            PlayerSelect(players, Keys.E);
            ClearItems(true, true, true, Keys.C);

            base.Update(gameTime);
        }

        private Vector2 lineStart = Vector2.Zero;
        private Vector2 lineEnd = Vector2.Zero;
        private List<Vector2> previewPositions = new List<Vector2>();
        private List<Vector2> previewPositionsRed = new List<Vector2>();

        private void PlayerMovementOrder(Player player)
        {
            var inputManager = Input_Manager.Instance;
            if (!inputManager.IsMouseOnInventory(player.inventoryVisible))
            {
                if (inputManager.IsButtonSingleClick(false))
                {
                    lineStart = new Vector2(inputManager.currentMouseState.X, inputManager.currentMouseState.Y);
                }

                if (inputManager.IsButtonPressed(false))
                {
                    lineEnd = new Vector2(inputManager.currentMouseState.X, inputManager.currentMouseState.Y);

                    UpdatePreviewPositions();
                }

                if (inputManager.currentMouseState.RightButton == ButtonState.Released && inputManager.previousMouseState.RightButton == ButtonState.Pressed)
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
                            int sideLength = 50;

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
                        Vector2 playerPosition = inputManager.mousePosition;
                        previewPositions.Add(playerPosition);
                        selectedPlayers[0].targetMovement = playerPosition;
                        selectedPlayers[0].hasMovementOrder = true;
                    }

                    previewPositions.Clear();
                    previewPositionsRed.Clear();
                }
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
                previewPositions.Add(Input_Manager.Instance.mousePosition);
            }
        }

        private void PlayerSelect(List<Player> players, Keys key)
        {
            var inputManager = Input_Manager.Instance;
            if (inputManager.IsKeySinglePress(key))
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
                    if (slotRect.Contains(inputManager.mousePosition))
                    {
                        closestPlayer = player;
                    }
                }

                if (closestPlayer != null)
                {
                    closestPlayer.isControlled = true;
                }
            }

            if (inputManager.IsKeyDown(Keys.LeftShift))
            {
                if (inputManager.IsButtonPressed(true))
                {
                    isSelecting = true;
                    startPos = inputManager.mousePosition;
                }

                if (!isSelecting)
                {
                    startPos = Vector2.Zero;
                    endPos = inputManager.mousePosition;
                }

                if (inputManager.IsButtonReleased(true) && isSelecting)
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

            foreach (Player player in players)
            {
                if (inputManager.IsButtonSingleClick(true) && inputManager.IsKeyDown(Keys.LeftShift))
                {
                    if (player.rectangle.Contains(inputManager.mousePosition))
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
                player.visualHandler.Draw(spriteBatch);
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

        private void ClearItems(bool clearInventory, bool clearGroundItems, bool clearEquippedItems, Keys key)
        {
            var inputManager = Input_Manager.Instance;
            if (inputManager.IsKeySinglePress(key))
            {
                foreach (Player player in players)
                {
                    if (clearGroundItems)
                    {
                        foreach (Item item in groundItems)
                        {
                            item.onGround = false;
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