using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace BaseBuilderRPG
{
    public class Item
    {
        public Texture2D Texture { get; set; } // Unique item ID
        public string TexturePath { get; set; } // Unique item ID
        public int ID { get; set; } // Unique item ID
        public string Name { get; set; } // Item name, including prefixes and suffixes
        public string FinalName { get; set; } // Item name, including prefixes and suffixes
        public string SuffixName { get; set; } // Item name, including prefixes and suffixes
        public string PrefixName { get; set; } // Item name, including prefixes and suffixes
        public string Type { get; set; } // Type of the item (e.g., weapon, armor, consumable)
        public int Rarity { get; set; } // Rarity level (e.g., common, rare, legendary)
        public Color RarityColor { get; set; } // Rarity level (e.g., common, rare, legendary)
        public int PrefixID { get; set; } // ID of the prefix
        public int SuffixID { get; set; } // ID of the suffix
        public int Damage { get; set; } // ID of the suffix
        public int UseTime { get; set; } // ID of the suffix
        public Vector2 Position { get; set; } // ID of the suffix

        public Item(Texture2D texture, string texturePath, int id, string name, string type, Vector2 position, int rarity, int prefixID, int suffixID, int damage, int useTime)
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
            SetDefaults();
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
                    RarityColor = Color.LightGreen;
                    break;
                case 3:
                    RarityColor = Color.DodgerBlue;
                    break;
                case 4:
                    RarityColor = Color.Fuchsia;
                    break;
                case 5:
                    RarityColor = Color.Gold;
                    break;
                case 6:
                    RarityColor = Color.Orange;
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
        public void Draw(SpriteBatch spriteBatch)
        {
            if (Texture != null)
            {
                spriteBatch.DrawString(Game1.TestFont, "[" + ID + "]", Position + new Vector2(-Texture.Width * 2 / 2 - 18, -50), Color.Red, 0, Vector2.Zero, 1f, SpriteEffects.None, 1f);
                spriteBatch.DrawString(Game1.TestFont, PrefixName + " " + Name + " " + SuffixName, Position + new Vector2(-Texture.Width * 2 / 2, -50), RarityColor, 0, Vector2.Zero, 1f, SpriteEffects.None, 1f);
                spriteBatch.DrawString(Game1.TestFont, "Suffix: " + SuffixID.ToString(), Position + new Vector2(-Texture.Width * 2 / 2, -30), Color.Black, 0, Vector2.Zero, 1f, SpriteEffects.None, 1f);
                spriteBatch.DrawString(Game1.TestFont, "Prefix: " + PrefixID.ToString(), Position + new Vector2(-Texture.Width * 2 / 2, -40), Color.Black, 0, Vector2.Zero, 1f, SpriteEffects.None, 1f);
                spriteBatch.DrawString(Game1.TestFont, "Rarity: " + Rarity.ToString(), Position + new Vector2(-Texture.Width * 2 / 2, -20), Color.Black, 0, Vector2.Zero, 1f, SpriteEffects.None, 1f);
                spriteBatch.DrawString(Game1.TestFont, "Rarity: " + Rarity.ToString(), Position + new Vector2(-Texture.Width * 2 / 2, -20), Color.Black, 0, Vector2.Zero, 1f, SpriteEffects.None, 1f);
                spriteBatch.DrawString(Game1.TestFont, "Rarity: " + Rarity.ToString(), Position + new Vector2(-Texture.Width * 2 / 2, -20), Color.Black, 0, Vector2.Zero, 1f, SpriteEffects.None, 1f);
                spriteBatch.Draw(Texture, Position, Color.White);
            }
        }
        public bool Kill { get; private set; }

        public void InteractPlayer(Player player, GameTime gameTime)
        {
            float distance = Vector2.Distance(Position, player.Position);

            if (distance <= 10f)
            {
                Kill = true;
            }
        }

        public bool PlayerClose(Player player, GameTime gameTime)
        {
            float distance = Vector2.Distance(Position, player.Position);

            if (distance <= 20f)
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
            // Create a deep copy of the item
            return new Item(Texture, TexturePath, ID, Name, Type, Position, Rarity, PrefixID, SuffixID, Damage, UseTime);
        }

        public Item Clone(int itemID, int prefixID, int suffixID)
        {
            // Create a deep copy of the item with custom itemID, prefixID, and suffixID
            return new Item(Texture, TexturePath, itemID, Name, Type, Position, Rarity, prefixID, suffixID, Damage, UseTime);
        }

    }

}
