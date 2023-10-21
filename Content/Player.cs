using BaseBuilderRPG.Content;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace BaseBuilderRPG
{
    public class Player
    {
        public Inventory Inventory { get; private set; }
        public Inventory BackPack { get; private set; }
        public Vector2 Position { get; set; }
        public float Speed { get; set; }
        public int Health { get; set; }
        public bool IsActive { get; set; }
        public string Name { get; set; }

        private Texture2D playerTexture;

        public Player(Texture2D texture, bool isActive, string name, Vector2 position, int invX, int invY)
        {
            playerTexture = texture;
            Position = position;
            Speed = 1f;
            Health = 100;
            IsActive = isActive;
            Name = name;

            Inventory = new Inventory(invX, invY);
        }

        public void Update(GameTime gameTime)
        {
            Speed = 2f;

            KeyboardState keyboardState = Keyboard.GetState();
            Vector2 movement = Vector2.Zero;

            if (IsActive)
            {
                if (keyboardState.IsKeyDown(Keys.W))
                    movement.Y -= Speed;
                if (keyboardState.IsKeyDown(Keys.S))
                    movement.Y += Speed;
                if (keyboardState.IsKeyDown(Keys.A))
                    movement.X -= Speed;
                if (keyboardState.IsKeyDown(Keys.D))
                    movement.X += Speed;

                Position += movement;
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (IsActive)
            {
                Inventory.Draw(spriteBatch, this);
                spriteBatch.Draw(playerTexture, Position, Color.Lime);

            }
            else
            {
                if (Vector2.Distance(Position, new Vector2(Mouse.GetState().X, Mouse.GetState().Y)) <= 30f)
                {
                    spriteBatch.Draw(playerTexture, Position, Color.PaleGreen);
                }
                else
                {
                    spriteBatch.Draw(playerTexture, Position, Color.Red);
                }
            }
            spriteBatch.DrawString(Game1.TestFont, "[" + Name + "]", Position + new Vector2(-12, -20), (IsActive) ? Color.Yellow : Color.Black, 0, Vector2.Zero, 1f, SpriteEffects.None, 1f);
        }
    }
}