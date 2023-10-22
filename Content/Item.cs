using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
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
        public bool HasTooltip { get; set; }
        public List<string> ToolTips { get; set; }

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
            HasTooltip = false;

            if (Type != "Weapon")
            {
                DamageType = "";
                Damage = -1;
                PrefixID = -1;
                SuffixID = -1;
                UseTime = -1;
                Damage = -1;
                StackLimit = stackLimit;
                StackSize = dropAmount;
            }
            else
            {
                StackLimit = 1;
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

        public void Draw(SpriteBatch spriteBatch)
        {
            if (Texture != null && OnGround)
            {
                spriteBatch.DrawString(Game1.TestFont, "[" + ID + "]", Position + new Vector2(-Texture.Width * 2 / 2 - 18, 30), Color.Red, 0, Vector2.Zero, 1f, SpriteEffects.None, 1f);
                if (Type == "Weapon")
                {
                    spriteBatch.DrawString(Game1.TestFont, PrefixName + "" + Name + " " + SuffixName, Position + new Vector2(-Texture.Width * 2 / 2, 30), RarityColor, 0, Vector2.Zero, 1f, SpriteEffects.None, 1f);
                }
                else
                {
                    spriteBatch.DrawString(Game1.TestFont, PrefixName + "" + Name + " " + SuffixName + " x" + StackSize, Position + new Vector2(-Texture.Width * 2 / 2, 30), RarityColor, 0, Vector2.Zero, 1f, SpriteEffects.None, 1f);
                }

                spriteBatch.Draw(Texture, Position, Color.White);
                spriteBatch.DrawString(Game1.TestFont, "Rarity: " + Rarity.ToString(), Position + new Vector2(-Texture.Width * 2 / 2, 40), Color.Black, 0, Vector2.Zero, 1f, SpriteEffects.None, 1f);
                spriteBatch.DrawString(Game1.TestFont, "Type: " + Type, Position + new Vector2(-Texture.Width * 2 / 2, 60), Color.Black, 0, Vector2.Zero, 1f, SpriteEffects.None, 1f);
                spriteBatch.DrawString(Game1.TestFont, "On Ground: " + OnGround.ToString(), Position + new Vector2(-Texture.Width * 2 / 2, 70), Color.Black, 0, Vector2.Zero, 1f, SpriteEffects.None, 1f);
            }
        }

        public bool PlayerClose(Player player, bool includeMouse, float pickRange)
        {
            float distance = Vector2.Distance(Position, player.Position);
            float mouseDistance;
            mouseDistance = Vector2.Distance(Position, new Vector2(Mouse.GetState().X, Mouse.GetState().Y));
            if (distance <= pickRange)
            {
                if (includeMouse)
                {
                    if (mouseDistance <= pickRange)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                return true;

            }
            else
            {
                return false;
            }
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
                HasTooltip = true;
                ToolTips.Add("'The one and only jewel of King East the III.'");
            }
        }
    }
}