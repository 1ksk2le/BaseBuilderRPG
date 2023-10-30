using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace BaseBuilderRPG
{
    public class Item
    {
        public Texture2D Texture { get; set; }

        public int ID { get; set; }
        public int PrefixID { get; set; }
        public int SuffixID { get; set; }
        public int Damage { get; set; }
        public int UseTime { get; set; }
        public int Rarity { get; set; }
        public int StackLimit { get; set; }
        public int StackSize { get; set; }
        public string TexturePath { get; set; }
        public string Name { get; set; }
        public string SuffixName { get; set; }
        public string PrefixName { get; set; }
        public string Type { get; set; }
        public string DamageType { get; set; }
        public Color RarityColor { get; set; }
        public Vector2 Position { get; set; }
        public bool OnGround { get; set; }
        public bool CanBeUsed { get; set; }
        public List<string> ToolTips { get; set; }

        private float levitationTimer = 0.0f;

        public Item(Texture2D texture, string texturePath, int id, string name, string type, string damageType, Vector2 position, int rarity, int prefixID, int suffixID, int damage, int useTime, int stackLimit, int dropAmount)
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
            Position = position;

            if (Type != "Weapon")
            {
                DamageType = "";
                Damage = -1;
                PrefixID = -1;
                SuffixID = -1;
                UseTime = -1;
                Damage = -1;
                StackLimit = stackLimit;
                StackSize = (StackLimit == 1) ? 1 : dropAmount;
            }
            else
            {
                StackLimit = 1;
                StackSize = 1;
            }

            SetDefaults();

            ToolTips = new List<string>();

            ToolTips.Add(PrefixName + " " + Name + " " + SuffixName);

            ToolTips.Add("[" + Type + "]");
            if (Damage > 0)
            {
                ToolTips.Add(Damage.ToString() + " " + DamageType + " damage");
            }
            if (UseTime > 0)
            {
                ToolTips.Add(UseTime.ToString() + " use time");
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

                Color shadowColor = new Color(0, 0, 0, 150);
                float shadowScaleFactor = 1.2f;
                float shadowOffsetY = 6;



                spriteBatch.Begin();

                Vector2 shadowPosition = Position + new Vector2(Texture.Width / 2, Texture.Height / 2);
                shadowPosition.Y += shadowOffsetY + levitationOffset;
                spriteBatch.Draw(Texture, shadowPosition, null, shadowColor, 0, new Vector2(Texture.Width / 2, Texture.Height / 2), itemScale * shadowScaleFactor, SpriteEffects.None, 0);

                spriteBatch.End();

                Game1.OutlineShader.Parameters["texelSize"].SetValue(new Vector2(1.0f / Texture.Width, 1.0f / Texture.Height));
                Game1.OutlineShader.Parameters["outlineColor"].SetValue(new Vector4(RarityColor.R / 255f, RarityColor.G / 255f, RarityColor.B / 255f, RarityColor.A / 255f));

                spriteBatch.Begin(SpriteSortMode.Immediate, null, null, null, null, Game1.OutlineShader, null);
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
            Rectangle slotRect = new((int)Position.X, (int)Position.Y, this.Texture.Width, this.Texture.Height);
            return slotRect.Contains(Mouse.GetState().X, Mouse.GetState().Y);
        }

        public void RemoveItem(List<Item> itemList)
        {
            itemList.Remove(this);
        }

        public Item Clone()
        {
            return new Item(Texture, TexturePath, ID, Name, Type, DamageType, Position, Rarity, PrefixID, SuffixID, Damage, UseTime, StackLimit, StackSize);
        }

        public Item Clone(int itemID, int prefixID, int suffixID, int dropAmount)
        {
            return new Item(Texture, TexturePath, itemID, Name, Type, DamageType, Position, Rarity, prefixID, suffixID, Damage, UseTime, StackLimit, dropAmount);
        }

        public Item Clone(int itemID, int dropAmount)
        {
            return new Item(Texture, TexturePath, itemID, Name, Type, DamageType, Position, Rarity, PrefixID, SuffixID, Damage, UseTime, StackLimit, dropAmount);
        }

        public void SetDefaults()
        {
            switch (Rarity)
            {
                case 0:
                    RarityColor = Color.SlateGray;
                    break;

                case 1:
                    RarityColor = Color.White;
                    break;

                case 2:
                    RarityColor = Color.Chartreuse;
                    break;

                case 3:
                    RarityColor = Color.DodgerBlue;
                    break;

                case 4:
                    RarityColor = Color.MediumSlateBlue;
                    break;

                case 5:
                    RarityColor = Color.Yellow;
                    break;

                case 6:
                    RarityColor = Color.OrangeRed;
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
            if (ID == 3)
            {
                ToolTips.Add("'The one and only jewel of King East the III.'");
                ToolTips.Add("'This is the first item that has a tooltip'");
            }
        }
    }
}