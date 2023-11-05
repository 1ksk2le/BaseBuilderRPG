using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace BaseBuilderRPG.Content
{
    public class Item
    {
        public Texture2D Texture { get; set; }

        public int ID { get; set; }
        public int PrefixID { get; set; }
        public int SuffixID { get; set; }
        public int Damage { get; set; }
        public int Rarity { get; set; }
        public int StackLimit { get; set; }
        public int StackSize { get; set; }
        public int Shoot { get; set; }
        public float UseTime { get; set; }
        public float ShootSpeed { get; set; }
        public string TexturePath { get; set; }
        public string Name { get; set; }
        public string SuffixName { get; set; }
        public string PrefixName { get; set; }
        public string Type { get; set; }
        public string DamageType { get; set; }
        public string WeaponType { get; set; }
        public Color RarityColor { get; set; }
        public Vector2 Position { get; set; }
        public bool OnGround { get; set; }
        public bool CanBeUsed { get; set; }
        public List<string> ToolTips { get; set; }

        private float levitationTimer = 0.0f;

        public Item(Texture2D texture, string texturePath, int id, string name, string type, string damageType, string weaponType, Vector2 position, float shootSpeed, int shoot, int rarity, int prefixID, int suffixID, int damage, float useTime, int stackLimit, int dropAmount, bool onGround)
        {
            Texture = texture;
            TexturePath = texturePath;
            ID = id;
            Name = name;
            Type = type;
            Rarity = rarity;
            PrefixID = prefixID;
            SuffixID = suffixID;
            UseTime = useTime;
            Damage = damage;
            DamageType = damageType;
            WeaponType = weaponType;
            Position = position;
            OnGround = onGround;

            if (Type != "Weapon")
            {
                DamageType = "";
                Shoot = -1;
                Damage = -1;
                PrefixID = -1;
                SuffixID = -1;
                UseTime = -1;
                Damage = -1;
                StackLimit = stackLimit;
                StackSize = StackLimit == 1 ? 1 : dropAmount;
            }
            else
            {
                StackLimit = 1;
                StackSize = 1;
                if (DamageType != "ranged")
                {
                    Shoot = -1;
                    ShootSpeed = 0f;
                }
                else
                {
                    Shoot = shoot;
                    ShootSpeed = shootSpeed;
                }
            }
            SetDefaults();

            ToolTips = new List<string>();

            ToolTips.Add(PrefixName + " " + Name + " " + SuffixName);


            if (Damage > 0)
            {
                ToolTips.Add("[" + Type + " - " + WeaponType + "]");
                ToolTips.Add(Damage.ToString() + " " + DamageType + " damage");
            }
            else
            {
                ToolTips.Add("[" + Type + "]");
            }
            if (UseTime > 0)
            {
                ToolTips.Add(UseTime.ToString() + " use time");
            }
            if (Shoot > -1)
            {
                ToolTips.Add(ShootSpeed.ToString() + " velocity");
            }
            TooltipsBasedOnID();
        }

        public void Update(GameTime gameTime)
        {
            levitationTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (Texture != null && OnGround)
            {
                float levitationSpeed = 3f;
                float levitationAmplitude = 0.5f;

                float levitationOffset = (float)Math.Sin(levitationTimer * levitationSpeed) * levitationAmplitude;

                float itemScale = 1.0f + 0.25f * levitationOffset;

                Color shadowColor = new Color(0, 0, 0, 200);
                float shadowScaleFactor = 1.2f;
                float shadowOffsetY = 6;

                spriteBatch.Begin();

                Vector2 shadowPosition = Position + new Vector2(Texture.Width / 2, Texture.Height / 2);
                shadowPosition.Y += shadowOffsetY + levitationOffset;
                spriteBatch.Draw(Texture, shadowPosition, null, shadowColor, 0, new Vector2(Texture.Width / 2, Texture.Height / 2), itemScale * shadowScaleFactor, SpriteEffects.None, 0);

                spriteBatch.End();

                Main.OutlineShader.Parameters["texelSize"].SetValue(new Vector2(1.0f / Texture.Width, 1.0f / Texture.Height));
                Main.OutlineShader.Parameters["outlineColor"].SetValue(new Vector4(RarityColor.R / 255f, RarityColor.G / 255f, RarityColor.B / 255f, RarityColor.A / 255f));

                spriteBatch.Begin(SpriteSortMode.Immediate, null, null, null, null, Main.OutlineShader, null);
                spriteBatch.Draw(Texture, Position + new Vector2(Texture.Width / 2, Texture.Height / 2), null, Color.White, 0, new Vector2(Texture.Width / 2, Texture.Height / 2), itemScale, SpriteEffects.None, 0);

                spriteBatch.End();

                spriteBatch.Begin();
                string itemName;
                if (Type != "Weapon")
                {
                    if (StackLimit == 1)
                    {
                        itemName = "[" + Name + "]";
                    }
                    else
                    {
                        itemName = "[" + Name + " x" + StackSize + "]";
                    }
                }
                else
                {
                    itemName = "[" + PrefixName + " " + Name + " " + SuffixName + "]";
                }

                //spriteBatch.DrawString(Game1.TestFont, itemName, new Vector2(Position.X - Game1.TestFont.MeasureString(itemName).X / 2f + Texture.Width / 2, Position.Y + Texture.Height + 4), RarityColor);
                spriteBatch.End();
            }
        }

        public bool PlayerClose(Player player, float pickRange)
        {
            float distance = Vector2.Distance(Position, player.Position);
            if (distance <= pickRange)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool InteractsWithMouse()
        {
            Rectangle slotRect = new((int)Position.X, (int)Position.Y, Texture.Width, Texture.Height);
            return slotRect.Contains(Mouse.GetState().X, Mouse.GetState().Y);
        }

        public void RemoveItem(List<Item> itemList)
        {
            itemList.Remove(this);
        }

        public Item Clone(bool onGround)
        {
            return new Item(Texture, TexturePath, ID, Name, Type, DamageType, WeaponType, Position, ShootSpeed, Rarity, Shoot, PrefixID, SuffixID, Damage, UseTime, StackLimit, StackSize, onGround);
        }

        public Item Clone(int itemID, int prefixID, int suffixID, int dropAmount, bool onGround)
        {
            return new Item(Texture, TexturePath, itemID, Name, Type, DamageType, WeaponType, Position, ShootSpeed, Rarity, Shoot, prefixID, suffixID, Damage, UseTime, StackLimit, dropAmount, OnGround);
        }

        public Item Clone(int itemID, int dropAmount, bool onGround)
        {
            return new Item(Texture, TexturePath, itemID, Name, Type, DamageType, WeaponType, Position, ShootSpeed, Rarity, Shoot, PrefixID, SuffixID, Damage, UseTime, StackLimit, dropAmount, onGround);
        }

        public void SetDefaults()
        {
            switch (Rarity)
            {
                case 0:
                    RarityColor = Color.LightGray;
                    break;

                case 1:
                    RarityColor = Color.White;
                    break;

                case 2:
                    RarityColor = new Color(30, 255, 0);
                    break;

                case 3:
                    RarityColor = new Color(0, 112, 221);
                    break;

                case 4:
                    RarityColor = new Color(163, 53, 238);
                    break;

                case 5:
                    RarityColor = Color.Gold;
                    break;

                case 6:
                    RarityColor = new Color(255, 128, 0);
                    break;

                case 7:
                    RarityColor = Color.Aqua;
                    break;

                default:
                    RarityColor = Color.Red;
                    break;
            }
            switch (PrefixID)
            {
                case 0:
                    PrefixName = "Broken";
                    break;

                case 1:
                    PrefixName = "Reinforced";
                    break;

                case 2:
                    PrefixName = "Magical";
                    break;

                case 3:
                    PrefixName = "Unwieldy";
                    break;

                default:
                    PrefixName = "";
                    break;
            }
            switch (SuffixID)
            {
                case 0:
                    SuffixName = "of Flames";
                    break;

                case 1:
                    SuffixName = "of Death";
                    break;

                case 2:
                    SuffixName = "of Arthur";
                    break;

                case 3:
                    SuffixName = "of King";
                    break;

                default:
                    SuffixName = "";
                    break;
            }
        }

        private void TooltipsBasedOnID()
        {
            if (ID == 5)
            {
                ToolTips.Add("'A strange gift from the Art Loving Officer...'");
            }
        }
    }
}