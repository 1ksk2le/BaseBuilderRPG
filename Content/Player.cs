using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace BaseBuilderRPG.Content
{
    public class Player
    {
        public Inventory Inventory { get; private set; }
        public Inventory BackPack { get; private set; }
        public Vector2 Position { get; set; }
        public int Health { get; set; }
        public int HealthMax { get; set; }
        public bool IsActive { get; set; }
        public string Name { get; set; }

        public Texture2D PlayerTexture;
        private Vector2 Velocity;
        private int Direction = 1;
        public Player(Texture2D texture, bool isActive, string name, int healthMax, Vector2 position)
        {
            PlayerTexture = texture;
            Position = position;
            Health = 100;
            IsActive = isActive;
            Name = name;
            HealthMax = healthMax;
            Health = HealthMax;

            Inventory = new Inventory(5, 4);
        }

        private float rotationAngle;
        public void Update(GameTime gameTime)
        {
            KeyboardState keyboardState = Keyboard.GetState();

            Movement(Vector2.Zero, keyboardState);
            OneHandedSwing(gameTime);
        }

        private void Movement(Vector2 movement, KeyboardState keyboardState, float Speed = 2f)
        {
            if (IsActive)
            {
                if (keyboardState.IsKeyDown(Keys.W))
                    movement.Y = -Speed;
                if (keyboardState.IsKeyDown(Keys.S))
                    movement.Y = Speed;
                if (keyboardState.IsKeyDown(Keys.A))
                {
                    Direction = -1;
                    movement.X = -Speed;
                }
                if (keyboardState.IsKeyDown(Keys.D))
                {
                    Direction = 1;
                    movement.X = Speed;
                }

                if (movement != Vector2.Zero)
                    movement.Normalize();

                Velocity = movement * Speed;

                Position += Velocity;
            }
            else
            {
                Velocity = Vector2.Zero;
            }
        }

        public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            SpriteEffects eff = (Direction == 1) ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
            PreDraw(spriteBatch);
            if (IsActive)
            {
                spriteBatch.Draw(PlayerTexture, Position, null, Color.Lime, 0f, Vector2.Zero, 1f, eff, 0.80f);
            }
            else
            {
                Rectangle slotRect = new Rectangle((int)Position.X, (int)Position.Y, PlayerTexture.Width, PlayerTexture.Height);
                if (slotRect.Contains(Mouse.GetState().X, Mouse.GetState().Y))
                {
                    spriteBatch.Draw(PlayerTexture, Position, null, Color.PaleGreen, 0f, Vector2.Zero, 1f, eff, 0.7f);
                }
                else
                {
                    spriteBatch.Draw(PlayerTexture, Position, null, Color.Red, 0f, Vector2.Zero, 1f, eff, 0.69f);
                }
            }
            PostDraw(spriteBatch, gameTime);

            // spriteBatch.DrawString(Game1.TestFont, "Health: " + Health.ToString() + "/" + HealthMax.ToString(), Position + new Vector2(-12, 30), (IsActive) ? Color.Black : Color.Transparent, 0, Vector2.Zero, 1f, SpriteEffects.None, 0.92f);
            string textToDisplay = "[" + Name + "]";
            Vector2 textSize = Main.TestFont.MeasureString(textToDisplay);
            Vector2 textPosition = Position + new Vector2(0, -14);
            textPosition.X = Position.X + PlayerTexture.Width / 2 - textSize.X / 2 * 0.8f;
            spriteBatch.DrawString(Main.TestFont, textToDisplay, textPosition + new Vector2(4, 0), IsActive ? Color.Yellow : Color.Black, 0, Vector2.Zero, 0.8f, SpriteEffects.None, 0.92f);

            if (IsActive)
            {
                Inventory.Draw(spriteBatch, this);
            }
        }
        public void PreDraw(SpriteBatch spriteBatch)
        {
            var equippedWeapon = Inventory.equipmentSlots[0].EquippedItem;
            if (equippedWeapon != null)
            {
                if (equippedWeapon.WeaponType == "One Handed Sword")
                {
                    float end = (Direction == 1) ? 110 * MathHelper.Pi / 180 : -290 * MathHelper.Pi / 180;
                    SpriteEffects eff = (Direction == 1) ? SpriteEffects.None : SpriteEffects.FlipVertically;

                    Vector2 weaponPosition = Position + new Vector2(7.5f, PlayerTexture.Height / 2);
                    Vector2 weaponOrigin = (Direction == 1) ? new Vector2(0, PlayerTexture.Height) : new Vector2(0, 0);
                    if (isSwinging)
                    {
                        spriteBatch.Draw(equippedWeapon.Texture, weaponPosition, null, Color.White, rotationAngle, weaponOrigin, 0.8f, eff, IsActive ? 0.81f : 0.71f);
                    }
                    else
                    {
                        spriteBatch.Draw(equippedWeapon.Texture, weaponPosition, null, Color.White, end, weaponOrigin, 0.8f, eff, IsActive ? 0.81f : 0.71f);
                    }
                }
            }
        }
        public void PostDraw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            if (Inventory.equipmentSlots[2].EquippedItem != null) //Offhand
            {
                SpriteEffects eff = (Direction == 1) ? SpriteEffects.None : SpriteEffects.None;
                spriteBatch.Draw(Inventory.equipmentSlots[2].EquippedItem.Texture, Position + new Vector2(PlayerTexture.Width / 2, PlayerTexture.Height / 1.5f), null, Color.White, 0f, new Vector2(PlayerTexture.Width / 2, PlayerTexture.Height / 2), 0.8f, eff, IsActive ? 0.79f : 0.68f);
            }
        }
        private bool isSwinging;
        private float swingTime;

        private void OneHandedSwing(GameTime gameTime)
        {
            var equippedWeapon = Inventory.equipmentSlots[0].EquippedItem;
            if (equippedWeapon != null && equippedWeapon.WeaponType == "One Handed Sword" && IsActive)
            {
                float start = (Direction == 1) ? -90 * MathHelper.Pi / 180 : -90 * MathHelper.Pi / 180;
                float end = (Direction == 1) ? 110 * MathHelper.Pi / 180 : -290 * MathHelper.Pi / 180;



                if (Mouse.GetState().LeftButton == ButtonState.Pressed)
                {
                    if (!isSwinging)
                    {
                        isSwinging = true;
                        swingTime = 0;
                        rotationAngle = start;
                    }
                }

                if (isSwinging)
                {
                    swingTime += (float)gameTime.ElapsedGameTime.TotalSeconds;

                    if (swingTime >= Inventory.equipmentSlots[0].EquippedItem.UseTime)
                    {
                        isSwinging = false;
                        swingTime = 0;
                        rotationAngle = end;
                    }
                    else
                    {
                        float progress = swingTime / Inventory.equipmentSlots[0].EquippedItem.UseTime;
                        rotationAngle = MathHelper.Lerp(start, end, progress);
                    }
                }
            }
        }
    }
}