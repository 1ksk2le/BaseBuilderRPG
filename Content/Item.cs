using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;

namespace BaseBuilderRPG
{
    public class Item
    {
        public Texture2D Texture { get; set; } // Unique item ID

        public int ID { get; set; } // Unique item ID
        public int PrefixID { get; set; } // ID of the prefix
        public int SuffixID { get; set; } // ID of the suffix
        public int Damage { get; set; } // ID of the suffix
        public int UseTime { get; set; } // ID of the suffix
        public int Rarity { get; set; } // Rarity level (e.g., common, rare, legendary)
        public int StackLimit { get; set; } // Rarity level (e.g., common, rare, legendary)
        public int StackSize; // Rarity level (e.g., common, rare, legendary)
        public string TexturePath { get; set; } // Unique item ID
        public string Name { get; set; } // Item name, including prefixes and suffixes
        public string FinalName { get; set; } // Item name, including prefixes and suffixes
        public string SuffixName { get; set; } // Item name, including prefixes and suffixes
        public string PrefixName { get; set; } // Item name, including prefixes and suffixes
        public string Type { get; set; } // Type of the item (e.g., weapon, armor, consumable)
        public string DamageType { get; set; } // Type of the item (e.g., weapon, armor, consumable)

        public Color RarityColor { get; set; } // Rarity level (e.g., common, rare, legendary)

        public Vector2 Position { get; set; } // ID of the suffix

        public bool OnGround { get; set; }
        public bool CanBeUsed { get; set; }
        public bool IsStackable { get; set; }

        public Item(Texture2D texture, string texturePath, int id, string name, string type, Vector2 position, int rarity, int prefixID, int suffixID, int damage, int useTime, int stackLimit, int dropAmount)
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
            Position = position;
            IsStackable = (Type != "weapon");

            if (IsStackable)
            {
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
        }


        public void Draw(SpriteBatch spriteBatch)
        {
            if (Texture != null && OnGround)
            {
                spriteBatch.DrawString(Game1.TestFont, "[" + ID + "]", Position + new Vector2(-Texture.Width * 2 / 2 - 18, 30), Color.Red, 0, Vector2.Zero, 1f, SpriteEffects.None, 1f);
                spriteBatch.DrawString(Game1.TestFont, PrefixName + "" + Name + " " + SuffixName + " x" + StackSize, Position + new Vector2(-Texture.Width * 2 / 2, 30), RarityColor, 0, Vector2.Zero, 1f, SpriteEffects.None, 1f);
                /* if (Type == "weapon")
                 {
                     spriteBatch.DrawString(Game1.TestFont, "Prefix: " + PrefixID.ToString(), Position + new Vector2(-Texture.Width * 2 / 2, -20), Color.Black, 0, Vector2.Zero, 1f, SpriteEffects.None, 1f);
                     spriteBatch.DrawString(Game1.TestFont, "Suffix: " + SuffixID.ToString(), Position + new Vector2(-Texture.Width * 2 / 2, -10), Color.Black, 0, Vector2.Zero, 1f, SpriteEffects.None, 1f);

                 }
                 */
                spriteBatch.Draw(Texture, Position, Color.White);
                spriteBatch.DrawString(Game1.TestFont, "Rarity: " + Rarity.ToString(), Position + new Vector2(-Texture.Width * 2 / 2, 40), Color.Black, 0, Vector2.Zero, 1f, SpriteEffects.None, 1f);
                spriteBatch.DrawString(Game1.TestFont, "Is Stackable: " + IsStackable.ToString(), Position + new Vector2(-Texture.Width * 2 / 2, 50), Color.Black, 0, Vector2.Zero, 1f, SpriteEffects.None, 1f);
                spriteBatch.DrawString(Game1.TestFont, "Type: " + Type, Position + new Vector2(-Texture.Width * 2 / 2, 60), Color.Black, 0, Vector2.Zero, 1f, SpriteEffects.None, 1f);
                spriteBatch.DrawString(Game1.TestFont, "On Ground: " + OnGround.ToString(), Position + new Vector2(-Texture.Width * 2 / 2, 70), Color.Black, 0, Vector2.Zero, 1f, SpriteEffects.None, 1f);

            }
        }

        public bool PlayerClose(Player player, GameTime gameTime)
        {
            float distance = Vector2.Distance(Position, player.Position);
            float mouseDistance = Vector2.Distance(Position, new Vector2(Mouse.GetState().X, Mouse.GetState().Y));

            if (distance <= 20f || mouseDistance <= 20f)
            {
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
            return new Item(Texture, TexturePath, ID, Name, Type, Position, Rarity, PrefixID, SuffixID, Damage, UseTime, StackLimit, StackSize);
        }

        public Item Clone(int itemID, int prefixID, int suffixID, int dropAmount)
        {
            return new Item(Texture, TexturePath, itemID, Name, Type, Position, Rarity, prefixID, suffixID, Damage, UseTime, StackLimit, dropAmount);
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
                    PrefixName = "Broken ";
                    break;

                case 1:
                    PrefixName = "Reinforced ";
                    break;

                case 2:
                    PrefixName = "Magical ";
                    break;

                case 3:
                    PrefixName = "Unwieldy ";
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
    }
}