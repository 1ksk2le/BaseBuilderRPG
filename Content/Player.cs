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
        public int MaxHealth { get; set; }
        public float SkinColor { get; set; }
        public bool IsActive { get; set; }
        public string Name { get; set; }

        public Color FinalSkinColor = Color.White;
        public Texture2D PlayerTexture;
        public Texture2D PlayerHeadTexture;
        public Texture2D PlayerEyeTexture;
        public Vector2 Velocity;
        public int Direction = 1;
        public float ImmunityTime, MaxImmunityTime, UseTimer;
        public bool InventoryVisible, IsImmune, IsSwinging, CanHit;
        public Item EquippedWeapon;
        public Vector2 Target;
        public Player(Texture2D texture, Texture2D headTexture, Texture2D eyeTexture, bool isActive, string name, int healthMax, float skinColor, Vector2 position)
        {
            PlayerTexture = texture;
            PlayerHeadTexture = headTexture;
            PlayerEyeTexture = eyeTexture;
            Position = position;
            Health = 100;
            IsActive = isActive;
            Name = name;
            MaxHealth = healthMax;
            SkinColor = skinColor;
            Health = MaxHealth;
            MaxImmunityTime = 0.4f;
            ImmunityTime = 0f;
            Target = Vector2.Zero;

            FinalSkinColor = GetSkinColor(SkinColor);

            Inventory = new Inventory(5, 6);
            InventoryVisible = true;
            CanHit = true;
        }

        public float TotalDamageDealt = 0f;
        public float TotalElapsedTime = 0f;
        public float RotationAngle;
        public void Update(GameTime gameTime)
        {
            KeyboardState keyboardState = Keyboard.GetState();

            TotalElapsedTime += (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (ImmunityTime >= 0f)
            {
                IsImmune = true;
                ImmunityTime -= (float)gameTime.ElapsedGameTime.TotalSeconds;
            }
            else
            {
                IsImmune = false;
            }

            if (Inventory.equipmentSlots[0].EquippedItem != null)
            {
                EquippedWeapon = Inventory.equipmentSlots[0].EquippedItem;
            }

            Movement(Vector2.Zero, keyboardState);
            OneHandedSwing(gameTime);
        }

        private Rectangle CalculateRotatedRectangle(Vector2 position, int width, int height, float rotation)
        {
            Matrix transform = Matrix.CreateRotationZ(rotation) * Matrix.CreateTranslation(position.X, position.Y, 0);

            Vector2 leftTop = Vector2.Transform(new Vector2(-width / 2, -height / 2), transform);
            Vector2 rightTop = Vector2.Transform(new Vector2(width / 2, -height / 2), transform);
            Vector2 leftBottom = Vector2.Transform(new Vector2(-width / 2, height / 2), transform);
            Vector2 rightBottom = Vector2.Transform(new Vector2(width / 2, height / 2), transform);

            Vector2 min = Vector2.Min(Vector2.Min(leftTop, rightTop), Vector2.Min(leftBottom, rightBottom));
            Vector2 max = Vector2.Max(Vector2.Max(leftTop, rightTop), Vector2.Max(leftBottom, rightBottom));

            return new Rectangle((int)min.X, (int)min.Y, (int)(max.X - min.X), (int)(max.Y - min.Y));
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (Main.DrawDebugRectangles)
            {
                if (EquippedWeapon != null)
                {
                    Vector2 Pos = (Direction == 1) ? new Vector2(PlayerTexture.Width + EquippedWeapon.Texture.Height * 0.2f, PlayerTexture.Height / 2f)
                                                    : new Vector2(-EquippedWeapon.Texture.Height * 0.2f, PlayerTexture.Height / 2f);
                    Rectangle playerWeaponRectangle = CalculateRotatedRectangle(Position + Pos, EquippedWeapon.Texture.Width, EquippedWeapon.Texture.Height, RotationAngle);
                    spriteBatch.Draw(Main.DebugTexture, playerWeaponRectangle, null, Color.Green, 0f, Vector2.Zero, SpriteEffects.None, 0.011f);
                }
                spriteBatch.Draw(Main.DebugTexture, new Rectangle((int)Position.X, (int)Position.Y, (int)(PlayerTexture.Width), (int)(PlayerTexture.Height)), null, Color.Lime, 0f, Vector2.Zero, SpriteEffects.None, 0.012f);
            }

            PreDraw(spriteBatch);

            string textToDisplay = Name;
            Vector2 textSize = Main.TestFont.MeasureString(textToDisplay);
            Vector2 textPosition = Position + new Vector2(0, -14);
            Color nameColor;
            Rectangle slotRect = new((int)Position.X, (int)Position.Y, PlayerTexture.Width, PlayerTexture.Height);
            if (slotRect.Contains(Mouse.GetState().X, Mouse.GetState().Y))
            {
                nameColor = Color.Lime;
            }
            else
            {
                nameColor = Color.White;
            }

            textPosition.X = Position.X + PlayerTexture.Width / 2 - textSize.X / 2;

            spriteBatch.DrawStringWithOutline(Main.TestFont, textToDisplay, textPosition, Color.Black, IsActive ? Color.Yellow : nameColor, 1f, IsActive ? 0.8616f : 0.7616f);

            SpriteEffects eff = (Direction == 1) ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
            MouseState mouseState = Mouse.GetState();
            Vector2 mousePosition = new Vector2(mouseState.X, mouseState.Y);
            Vector2 directionToMouse = mousePosition - Position;

            float maxHeadRotation = MathHelper.ToRadians(20);
            float rotation = (float)Math.Atan2(directionToMouse.Y * Direction, directionToMouse.X * Direction);
            rotation = MathHelper.Clamp(rotation, -maxHeadRotation, maxHeadRotation);

            spriteBatch.Draw(PlayerTexture, Position, null, Color.Lerp(FinalSkinColor, Color.DarkRed, ImmunityTime), 0f, Vector2.Zero, 1f, eff, IsActive ? 0.851f : 0.751f);

            if (IsActive)
            {
                //spriteBatch.DrawStringWithOutline(Main.TestFont, "DPS: " + (TotalDamageDealt / TotalElapsedTime).ToString("F2"), Position + new Vector2(0, 50), Color.Black, Color.White, 1f, IsActive ? 0.8617f : 0.7617f);

                Vector2 headOrigin = new Vector2(PlayerHeadTexture.Width / 2, PlayerHeadTexture.Height);
                Vector2 eyesOrigin = new Vector2(PlayerEyeTexture.Width / 2, (PlayerEyeTexture.Height) / 2);

                spriteBatch.Draw(PlayerHeadTexture, Position + new Vector2(PlayerHeadTexture.Width / 2 - 2 * Direction, PlayerHeadTexture.Height), null, Color.Lerp(FinalSkinColor, Color.DarkRed, ImmunityTime), rotation, headOrigin, 1f, eff, 0.852f);

                Rectangle sourceRect = new Rectangle(0, 0, PlayerEyeTexture.Width, PlayerEyeTexture.Height / 2);
                spriteBatch.Draw(PlayerEyeTexture, Position + new Vector2(PlayerEyeTexture.Width / 2 - 2 * Direction, PlayerEyeTexture.Height / 2), sourceRect, Color.Lerp(Color.White, Color.DarkRed, ImmunityTime), rotation, eyesOrigin, 1f, eff, 0.853f);
            }
            else
            {
                spriteBatch.Draw(PlayerHeadTexture, Position + new Vector2(-2 * Direction, 0), null, Color.Lerp(FinalSkinColor, Color.DarkRed, ImmunityTime), 0f, Vector2.Zero, 1f, eff, 0.752f);

                Rectangle sourceRect = new Rectangle(0, 0, PlayerEyeTexture.Width, PlayerEyeTexture.Height / 2);
                spriteBatch.Draw(PlayerEyeTexture, Position + new Vector2(-2 * Direction, 0), sourceRect, Color.Lerp(Color.White, Color.DarkRed, ImmunityTime), 0f, Vector2.Zero, 1f, eff, 0.753f);
            }

            PostDraw(spriteBatch, rotation);

            if (IsActive && InventoryVisible)
            {
                Inventory.Draw(spriteBatch, this);
            }
        }


        public void PreDraw(SpriteBatch spriteBatch)
        {
            if (EquippedWeapon != null)
            {
                if (EquippedWeapon.WeaponType == "One Handed Sword")
                {
                    float end = (Direction == 1) ? 110 * MathHelper.Pi / 180 : -290 * MathHelper.Pi / 180;
                    SpriteEffects eff = (Direction == 1) ? SpriteEffects.None : SpriteEffects.FlipVertically;

                    Vector2 Pos = (Direction == 1) ? new Vector2(PlayerTexture.Width, PlayerTexture.Height / 2)
                                                    : new Vector2(0, PlayerTexture.Height / 2);
                    Vector2 weaponPosition = Position + Pos;
                    Vector2 weaponOrigin = (Direction == 1) ? new Vector2(0, EquippedWeapon.Texture.Height) : new Vector2(0, 0);

                    if (IsSwinging)
                    {
                        spriteBatch.Draw(EquippedWeapon.Texture, weaponPosition, null, Color.White, RotationAngle, weaponOrigin, 0.8f, eff, IsActive ? 0.841f : 0.741f);
                    }
                    else
                    {
                        spriteBatch.Draw(EquippedWeapon.Texture, weaponPosition, null, Color.White, end, weaponOrigin, 0.8f, eff, IsActive ? 0.841f : 0.741f);
                    }
                }
            }
        }
        public void PostDraw(SpriteBatch spriteBatch, float headRot) //0.8616f : 0.7616f MAX
        {
            if (Health <= MaxHealth)
            {
                float healthBarWidth = (PlayerTexture.Width) * ((float)Health / (float)MaxHealth);
                int offSetY = 6;

                Rectangle healthBarRectangleBackground = new Rectangle((int)(Position.X - 2), (int)Position.Y + PlayerTexture.Height + offSetY - 1, (int)(PlayerTexture.Width) + 4, 4);
                Rectangle healthBarRectangleBackgroundRed = new Rectangle((int)(Position.X), (int)Position.Y + PlayerTexture.Height + offSetY, (int)(PlayerTexture.Width), 2);
                Rectangle healthBarRectangle = new Rectangle((int)(Position.X), (int)Position.Y + PlayerTexture.Height + offSetY, (int)healthBarWidth, 2);


                if (Health < MaxHealth)
                {
                    spriteBatch.DrawRectangle(healthBarRectangleBackground, Color.Black, IsActive ? 0.8613f : 0.7613f);
                    spriteBatch.DrawRectangle(healthBarRectangleBackgroundRed, Color.Red, IsActive ? 0.8614f : 0.7614f);
                    spriteBatch.DrawRectangle(healthBarRectangle, Color.Lime, IsActive ? 0.8615f : 0.7615f);
                }
            }


            SpriteEffects eff = (Direction == 1) ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
            if (Inventory.equipmentSlots[2].EquippedItem != null) //Offhand
            {
                spriteBatch.Draw(Inventory.equipmentSlots[2].EquippedItem.Texture, Position + new Vector2(Direction == 1 ? 0 : 22, PlayerTexture.Height / 1.2f), null, Color.Lerp(Color.White, Color.DarkRed, ImmunityTime), 0f, new Vector2(PlayerTexture.Width / 2, PlayerTexture.Height / 2), 0.8f, SpriteEffects.None, IsActive ? 0.8612f : 0.7612f);
            }
            if (Inventory.equipmentSlots[1].EquippedItem != null)//Body Armor
            {
                spriteBatch.Draw(Inventory.equipmentSlots[1].EquippedItem.Texture, Position + new Vector2(0, 22), null, Color.Lerp(Color.White, Color.DarkRed, ImmunityTime), 0f, Vector2.Zero, 1f, eff, IsActive ? 0.8519f : 0.7519f);
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
                spriteBatch.Draw(Inventory.equipmentSlots[4].EquippedItem.Texture, Position + new Vector2(PlayerHeadTexture.Width / 2 - headOffset * Direction, PlayerHeadTexture.Height), null, Color.Lerp(Color.White, Color.DarkRed, ImmunityTime), IsActive ? headRot : 0f, headOrigin, 1f, eff, IsActive ? 0.8611f : 0.7611f);
            }
        }

        public bool AIAttackCheck = false;
        private void OneHandedSwing(GameTime gameTime)
        {
            if (EquippedWeapon != null && EquippedWeapon.WeaponType == "One Handed Sword")
            {
                float start = (Direction == 1) ? -90 * MathHelper.Pi / 180 : -90 * MathHelper.Pi / 180;
                float end = (Direction == 1) ? 110 * MathHelper.Pi / 180 : -290 * MathHelper.Pi / 180;

                bool aiCheck = false;

                if (IsActive)
                {
                    if (Mouse.GetState().LeftButton == ButtonState.Pressed)
                    {
                        if (!IsSwinging)
                        {
                            IsSwinging = true;
                            UseTimer = 0;
                            RotationAngle = start;
                            CanHit = true;
                        }
                    }

                }
                else
                {
                    if (IsSwinging && !AIAttackCheck)
                    {
                        CanHit = true;
                        AIAttackCheck = true;
                    }
                }


                if (IsSwinging)
                {
                    UseTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;

                    if (UseTimer >= EquippedWeapon.UseTime)
                    {
                        IsSwinging = false;
                        UseTimer = 0;
                        RotationAngle = end;
                        AIAttackCheck = false;
                    }
                    else
                    {
                        float progress = UseTimer / EquippedWeapon.UseTime;
                        RotationAngle = MathHelper.Lerp(start, end, progress);
                    }
                }
            }
        }


        private void Movement(Vector2 movement, KeyboardState keyboardState, float Speed = 1.5f)
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

        public void GetDamaged(Display_Text_Manager texMan, int damage)
        {
            Health -= damage;
            texMan.AddFloatingText("-" + damage.ToString(), "", new Vector2(Position.X + PlayerTexture.Width / 2, Position.Y), Color.Red, Color.Transparent, 2f, 1.1f);
            ImmunityTime = MaxImmunityTime;
        }
    }
}