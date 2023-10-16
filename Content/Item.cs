using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BaseBuilderRPG
{
    public class Item
    {
        public Texture2D Texture { get; set; } // Unique item ID
        public string TexturePath { get; set; } // Unique item ID
        public int ID { get; set; } // Unique item ID
        public string Name { get; set; } // Item name, including prefixes and suffixes
        public string Type { get; set; } // Type of the item (e.g., weapon, armor, consumable)
        public int Rarity { get; set; } // Rarity level (e.g., common, rare, legendary)
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
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (Texture != null)
            {
                spriteBatch.DrawString(Game1.TestFont, Name, Position + new Vector2(0, -40), Color.Black, 0, Vector2.Zero, 1f, SpriteEffects.None, 1f);
                spriteBatch.DrawString(Game1.TestFont, "Suffix: " + SuffixID.ToString(), Position + new Vector2(0, -20), Color.Black, 0, Vector2.Zero, 1f, SpriteEffects.None, 1f);
                spriteBatch.DrawString(Game1.TestFont, "Prefix: " + PrefixID.ToString(), Position + new Vector2(0, -30), Color.Black, 0, Vector2.Zero, 1f, SpriteEffects.None, 1f);
                spriteBatch.Draw(Texture, Position, Color.White);
            }
        }

        // Method to apply effects to the player
        public void ApplyEffects(Player player)
        {
            // Implement code to apply effects to the player based on the item's properties

        }

        // Other item-specific methods
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
