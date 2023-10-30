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
        public int HealthMax { get; set; }
        public bool IsActive { get; set; }
        public string Name { get; set; }

        private Texture2D PlayerTexture;

        public Player(Texture2D texture, bool isActive, string name, int healthMax, Vector2 position)
        {
            PlayerTexture = texture;
            Position = position;
            Speed = 1f;
            Health = 100;
            IsActive = isActive;
            Name = name;
            HealthMax = healthMax;
            Health = HealthMax;

            Inventory = new Inventory(5, 4);
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
            PreDraw(spriteBatch);
            if (IsActive)
            {
                spriteBatch.Draw(PlayerTexture, Position, Color.Lime);
            }
            else
            {
                if (Vector2.Distance(Position, new Vector2(Mouse.GetState().X, Mouse.GetState().Y)) <= 30f)
                {
                    spriteBatch.Draw(PlayerTexture, Position, Color.PaleGreen);
                }
                else
                {
                    spriteBatch.Draw(PlayerTexture, Position, Color.Red);
                }
            }
            PostDraw(spriteBatch);

            spriteBatch.DrawString(Game1.TestFont, "Health: " + Health.ToString() + "/" + HealthMax.ToString(), Position + new Vector2(-12, 30), (IsActive) ? Color.Black : Color.Transparent, 0, Vector2.Zero, 1f, SpriteEffects.None, 1f);
            spriteBatch.DrawString(Game1.TestFont, "[" + Name + "]", Position + new Vector2(-12, -20), (IsActive) ? Color.Yellow : Color.Black, 0, Vector2.Zero, 1f, SpriteEffects.None, 1f);

            if (IsActive)
            {
                Inventory.Draw(spriteBatch, this);
            }

        }

        public void PreDraw(SpriteBatch spriteBatch)
        {
            if (Inventory.equipmentSlots[2].EquippedItem != null)
            {
                spriteBatch.Draw(Inventory.equipmentSlots[2].EquippedItem.Texture, Position, Color.White);
            }

        }
        public void PostDraw(SpriteBatch spriteBatch)
        {
            if (Inventory.equipmentSlots[0].EquippedItem != null)
            {
                spriteBatch.Draw(Inventory.equipmentSlots[0].EquippedItem.Texture, Position + new Vector2(PlayerTexture.Width, PlayerTexture.Height), null, Color.White, 90f, new Vector2(PlayerTexture.Width / 2, PlayerTexture.Height / 2), 1f, SpriteEffects.None, 0f);
            }
        }
    }
}