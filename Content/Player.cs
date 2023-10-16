using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace BaseBuilderRPG
{
    public class Player
    {
        // Player attributes
        public Vector2 Position { get; set; }
        public float Speed { get; set; }
        public int Health { get; set; }

        // Texture for the player character
        private Texture2D playerTexture;

        public Player(Texture2D texture, Vector2 position)
        {
            playerTexture = texture;
            Position = position;
            Speed = 1f; // Adjust the speed as needed
            Health = 100; // Set the initial health value
        }

        public void Update(GameTime gameTime)
        {
            Speed = 1f;

            KeyboardState keyboardState = Keyboard.GetState();
            Vector2 movement = Vector2.Zero;

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

        public void Draw(SpriteBatch spriteBatch)
        {
            // Draw the player character on the screen
            spriteBatch.Draw(playerTexture, Position, Color.White);
        }
    }

}
