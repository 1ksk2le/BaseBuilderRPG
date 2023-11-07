using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace BaseBuilderRPG.Content
{
    public class Player
    {
        public Inventory Inventory { get; private set; }
        public Inventory BackPack { get; private set; }
        public Vector2 Position { get; set; }
        public int Health { get; set; }
        public int HealthMax { get; set; }
        public float SkinColor { get; set; }
        public bool IsActive { get; set; }
        public string Name { get; set; }

        public Color FinalSkinColor = Color.White;
        public Texture2D PlayerTexture;
        public Texture2D PlayerHeadTexture;
        public Texture2D PlayerEyeTexture;
        private Vector2 Velocity;
        public int Direction = 1;
        public Player(Texture2D texture, Texture2D headTexture, Texture2D eyeTexture, bool isActive, string name, int healthMax, float skinColor, Vector2 position)
        {
            PlayerTexture = texture;
            PlayerHeadTexture = headTexture;
            PlayerEyeTexture = eyeTexture;
            Position = position;
            Health = 100;
            IsActive = isActive;
            Name = name;
            HealthMax = healthMax;
            SkinColor = skinColor;
            Health = HealthMax;

            FinalSkinColor = GetSkinColor(SkinColor);

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

        private Color GetSkinColor(float progress)
        {
            Color black = new Color(94, 54, 33);
            Color white = new Color(255, 220, 185);
            return Color.Lerp(black, white, progress);
        }

        public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            SpriteEffects eff = (Direction == 1) ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
            PreDraw(spriteBatch);

            MouseState mouseState = Mouse.GetState();
            Vector2 mousePosition = new Vector2(mouseState.X, mouseState.Y);
            Vector2 directionToMouse = mousePosition - Position;

            float maxHeadRotation = MathHelper.ToRadians(30);

            float rotation = (float)Math.Atan2(directionToMouse.Y * Direction, directionToMouse.X * Direction);

            rotation = MathHelper.Clamp(rotation, -maxHeadRotation, maxHeadRotation);

            spriteBatch.Draw(PlayerTexture, Position, null, GetSkinColor(SkinColor), 0f, Vector2.Zero, 1f, eff, IsActive ? 0.851f : 0.751f);

            if (IsActive)
            {
                Vector2 headOrigin = new Vector2(PlayerHeadTexture.Width / 2, PlayerHeadTexture.Height);
                Vector2 eyesOrigin = new Vector2(PlayerEyeTexture.Width / 2, (PlayerEyeTexture.Height) / 2);

                spriteBatch.Draw(PlayerHeadTexture, Position + new Vector2(PlayerHeadTexture.Width / 2, PlayerHeadTexture.Height), null, GetSkinColor(SkinColor), rotation, headOrigin, 1f, eff, 0.852f);

                Rectangle sourceRect = new Rectangle(0, 0, PlayerEyeTexture.Width, PlayerEyeTexture.Height / 2);
                spriteBatch.Draw(PlayerEyeTexture, Position + new Vector2(PlayerEyeTexture.Width / 2, PlayerEyeTexture.Height / 2), sourceRect, Color.White, rotation, eyesOrigin, 1f, eff, 0.853f);
            }
            else
            {
                spriteBatch.Draw(PlayerHeadTexture, Position, null, GetSkinColor(SkinColor), 0f, Vector2.Zero, 1f, eff, 0.752f);

                Rectangle sourceRect = new Rectangle(0, 0, PlayerEyeTexture.Width, PlayerEyeTexture.Height / 2);
                spriteBatch.Draw(PlayerEyeTexture, Position, sourceRect, Color.White, 0f, Vector2.Zero, 1f, eff, 0.753f);
            }

            PostDraw(spriteBatch, gameTime);

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
                    float start = (Direction == 1) ? -90 * MathHelper.Pi / 180 : -90 * MathHelper.Pi / 180;
                    float end = (Direction == 1) ? 110 * MathHelper.Pi / 180 : -290 * MathHelper.Pi / 180;
                    SpriteEffects eff = (Direction == 1) ? SpriteEffects.None : SpriteEffects.FlipVertically;

                    Vector2 weaponPosition = Position + new Vector2((Direction == 1) ? 22 : 2, (Direction == 1) ? PlayerTexture.Height / 2 + 5 : PlayerTexture.Height / 2 + 4);
                    Vector2 weaponOrigin = (Direction == 1) ? new Vector2(0, PlayerTexture.Height / 1.5f) : new Vector2(0, 0);

                    if (isSwinging)
                    {
                        spriteBatch.Draw(equippedWeapon.Texture, weaponPosition, null, Color.White, rotationAngle, weaponOrigin, 0.8f, eff, IsActive ? 0.841f : 0.741f);
                    }
                    else
                    {
                        spriteBatch.Draw(equippedWeapon.Texture, weaponPosition, null, Color.White, end, weaponOrigin, 0.8f, eff, IsActive ? 0.841f : 0.741f);
                    }
                }
            }
        }
        public void PostDraw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            if (Inventory.equipmentSlots[2].EquippedItem != null) //Offhand
            {
                spriteBatch.Draw(Inventory.equipmentSlots[2].EquippedItem.Texture, Position + new Vector2(Direction == 1 ? 0 : 22, PlayerTexture.Height / 1.2f), null, Color.White, 0f, new Vector2(PlayerTexture.Width / 2, PlayerTexture.Height / 2), 0.8f, SpriteEffects.None, IsActive ? 0.861f : 0.761f);
            }
        }
        private bool isSwinging;
        private float swingTime;

        private void OneHandedSwing(GameTime gameTime)
        {
            var equippedWeapon = Inventory.equipmentSlots[0].EquippedItem;
            if (equippedWeapon != null && equippedWeapon.WeaponType == "One Handed Sword")
            {
                float start = (Direction == 1) ? -90 * MathHelper.Pi / 180 : -90 * MathHelper.Pi / 180;
                float end = (Direction == 1) ? 110 * MathHelper.Pi / 180 : -290 * MathHelper.Pi / 180;



                if (Mouse.GetState().LeftButton == ButtonState.Pressed && IsActive)
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