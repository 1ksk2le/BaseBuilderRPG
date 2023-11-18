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
        private List<Player> pickedPlayers;
        public List<Player> playersToRemove;
        private List<Player> pickedPlayersToRemove;
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
            pickedPlayers = new List<Player>();
            playersToRemove = new List<Player>();
            pickedPlayersToRemove = new List<Player>();
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
            players.Add(new Player(_texture, _textureHead, _textureEyes, "Warrior", new Vector2(300, 300), 200000, 0.2f, false));
            players.Add(new Player(_texture, _textureHead, _textureEyes, "Archer", new Vector2(500, 500), 200000, 0.8f, false));
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
                    MovementOrder(player);
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

            base.Update(gameTime);
        }




        private void MovementOrder(Player player)
        {
            if (Mouse.GetState().RightButton == ButtonState.Pressed && pMouse.RightButton == ButtonState.Released)
            {
                if (player.isPicked)
                {
                    player.targetMovement = new Vector2(Mouse.GetState().X, Mouse.GetState().Y);
                    player.hasMovementOrder = true;
                }
            }
        }

        public override void Draw(GameTime gameTime) //8617f max
        {
            spriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, Matrix.Identity);
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
