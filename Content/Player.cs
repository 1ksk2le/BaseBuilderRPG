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
        public Vector2 Velocity;
        public int Direction = 1;
        public bool InventoryVisible;
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

            Inventory = new Inventory(5, 6);
            InventoryVisible = true;
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
            MouseState mouseState = Mouse.GetState();


            if (IsActive)
            {
                if (Position.X > mouseState.X)
                {
                    Direction = -1;
                }
                else
                {
                    Direction = 1;
                }


                if (keyboardState.IsKeyDown(Keys.W))
                    movement.Y = -Speed;
                if (keyboardState.IsKeyDown(Keys.S))
                    movement.Y = Speed;
                if (keyboardState.IsKeyDown(Keys.A))
                    movement.X = -Speed;
                if (keyboardState.IsKeyDown(Keys.D))
                    movement.X = Speed;

                if (movement != Vector2.Zero)
                    movement.Normalize();

                Velocity = movement * Speed;

                Position += Velocity;

                if (keyboardState.IsKeyDown(Keys.K))
                {
                    Health--;
                }
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

            float maxHeadRotation = MathHelper.ToRadians(20);
            float rotation = (float)Math.Atan2(directionToMouse.Y * Direction, directionToMouse.X * Direction);
            rotation = MathHelper.Clamp(rotation, -maxHeadRotation, maxHeadRotation);

            spriteBatch.Draw(PlayerTexture, Position, null, GetSkinColor(SkinColor), 0f, Vector2.Zero, 1f, eff, IsActive ? 0.851f : 0.751f);

            if (IsActive)
            {
                Vector2 headOrigin = new Vector2(PlayerHeadTexture.Width / 2, PlayerHeadTexture.Height);
                Vector2 eyesOrigin = new Vector2(PlayerEyeTexture.Width / 2, (PlayerEyeTexture.Height) / 2);

                spriteBatch.Draw(PlayerHeadTexture, Position + new Vector2(PlayerHeadTexture.Width / 2 - 2 * Direction, PlayerHeadTexture.Height), null, GetSkinColor(SkinColor), rotation, headOrigin, 1f, eff, 0.852f);

                Rectangle sourceRect = new Rectangle(0, 0, PlayerEyeTexture.Width, PlayerEyeTexture.Height / 2);
                spriteBatch.Draw(PlayerEyeTexture, Position + new Vector2(PlayerEyeTexture.Width / 2 - 2 * Direction, PlayerEyeTexture.Height / 2), sourceRect, Color.White, rotation, eyesOrigin, 1f, eff, 0.853f);
            }
            else
            {
                spriteBatch.Draw(PlayerHeadTexture, Position + new Vector2(-2 * Direction, 0), null, GetSkinColor(SkinColor), 0f, Vector2.Zero, 1f, eff, 0.752f);

                Rectangle sourceRect = new Rectangle(0, 0, PlayerEyeTexture.Width, PlayerEyeTexture.Height / 2);
                spriteBatch.Draw(PlayerEyeTexture, Position + new Vector2(-2 * Direction, 0), sourceRect, Color.White, 0f, Vector2.Zero, 1f, eff, 0.753f);
            }

            PostDraw(spriteBatch, gameTime, rotation);

            if (IsActive && InventoryVisible)
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

                    Vector2 weaponPosition = Position + new Vector2(PlayerTexture.Width / 2 + equippedWeapon.Texture.Height / 5 * Direction, PlayerTexture.Height / 2);
                    Vector2 weaponOrigin = (Direction == 1) ? new Vector2(0, equippedWeapon.Texture.Height) : new Vector2(0, 0);

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
        public void PostDraw(SpriteBatch spriteBatch, GameTime gameTime, float headRot) //0.8616f : 0.7616f MAX
        {
            if (Health < HealthMax)
            {
                float healthBarWidth = (PlayerTexture.Width) * ((float)Health / (float)HealthMax);
                int offSetY = 6;

                Rectangle healthBarRectangleBackground = new Rectangle((int)(Position.X - 2), (int)Position.Y + PlayerTexture.Height + offSetY - 1, (int)(PlayerTexture.Width) + 4, 5);
                Rectangle healthBarRectangleBackgroundRed = new Rectangle((int)(Position.X), (int)Position.Y + PlayerTexture.Height + offSetY, (int)(PlayerTexture.Width), 3);
                Rectangle healthBarRectangle = new Rectangle((int)(Position.X), (int)Position.Y + PlayerTexture.Height + offSetY, (int)healthBarWidth, 3);

                Color healthBarColor = Color.Lime;
                spriteBatch.DrawRectangle(healthBarRectangleBackground, Color.Black, IsActive ? 0.8613f : 0.7613f);
                spriteBatch.DrawRectangle(healthBarRectangleBackgroundRed, Color.Red, IsActive ? 0.8614f : 0.7614f);
                spriteBatch.DrawRectangle(healthBarRectangle, healthBarColor, IsActive ? 0.8615f : 0.7615f);
            }


            SpriteEffects eff = (Direction == 1) ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
            if (Inventory.equipmentSlots[2].EquippedItem != null) //Offhand
            {
                spriteBatch.Draw(Inventory.equipmentSlots[2].EquippedItem.Texture, Position + new Vector2(Direction == 1 ? 0 : 22, PlayerTexture.Height / 1.2f), null, Color.White, 0f, new Vector2(PlayerTexture.Width / 2, PlayerTexture.Height / 2), 0.8f, SpriteEffects.None, IsActive ? 0.8612f : 0.7612f);
            }
            if (Inventory.equipmentSlots[1].EquippedItem != null)//Body Armor
            {
                spriteBatch.Draw(Inventory.equipmentSlots[1].EquippedItem.Texture, Position + new Vector2(0, 22), null, Color.White, 0f, Vector2.Zero, 1f, eff, IsActive ? 0.8519f : 0.7519f);
            }
            if (Inventory.equipmentSlots[4].EquippedItem != null)//Head Armor
            {
                Vector2 headOrigin = new Vector2(Inventory.equipmentSlots[4].EquippedItem.Texture.Width / 2, Inventory.equipmentSlots[4].EquippedItem.Texture.Height);
                int headOffset;
                switch (Inventory.equipmentSlots[4].EquippedItem.ID)
                {
                    case 6:
                        headOffset = 2;
                        break;

                    default:
                        headOffset = 0;
                        break;
                }
                spriteBatch.Draw(Inventory.equipmentSlots[4].EquippedItem.Texture, Position + new Vector2(PlayerHeadTexture.Width / 2 - headOffset * Direction, PlayerHeadTexture.Height), null, Color.White, IsActive ? headRot : 0f, headOrigin, 1f, eff, IsActive ? 0.8611f : 0.7611f);
            }
        }
        private bool isSwinging;
        private float useTimer;

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
                        useTimer = 0;
                        rotationAngle = start;
                    }
                }

                if (isSwinging)
                {
                    useTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;

                    if (useTimer >= Inventory.equipmentSlots[0].EquippedItem.UseTime)
                    {
                        isSwinging = false;
                        useTimer = 0;
                        rotationAngle = end;
                    }
                    else
                    {
                        float progress = useTimer / Inventory.equipmentSlots[0].EquippedItem.UseTime;
                        rotationAngle = MathHelper.Lerp(start, end, progress);
                    }
                }
            }
        }
    }
}