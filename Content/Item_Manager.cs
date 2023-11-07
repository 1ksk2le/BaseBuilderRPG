using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace BaseBuilderRPG.Content
{
    public class Item_Manager : DrawableGameComponent
    {
        SpriteBatch spriteBatch;
        public Dictionary<int, Item> itemDictionary;
        public List<Item> items;
        public List<Item> itemsToRemove;
        public List<Item> groundItems;

        public Item_Manager(Game game, SpriteBatch spriteBatch)
            : base(game)
        {
            this.spriteBatch = spriteBatch;

            items = new List<Item>();
            groundItems = new List<Item>();
            itemsToRemove = new List<Item>();
            itemDictionary = new Dictionary<int, Item>();

            string itemsJson = File.ReadAllText("Content/items.json");
            items = JsonConvert.DeserializeObject<List<Item>>(itemsJson);
            Main.amountOfItems = items.Count;
            foreach (var item in items)
            {
                itemDictionary.Add(item.ID, item);
            }

        }

        public void Load()
        {
            foreach (var item in items)
            {
                item.Texture = Game.Content.Load<Texture2D>(item.TexturePath);
            }
        }

        public Item NewItem(Item itemData, Vector2 position, int prefixID, int suffixID, int dropAmount, bool onGround)
        {
            return new Item(itemData.Texture, itemData.TexturePath, itemData.ID, itemData.Name, itemData.Type, itemData.DamageType, itemData.WeaponType, position, itemData.ShootSpeed, itemData.Shoot, itemData.Rarity, prefixID, suffixID, itemData.Damage, itemData.UseTime, itemData.StackLimit, dropAmount, onGround);
        }

        public void DropItem(int itemID, int prefixID, int suffixID, int dropAmount, Vector2 position)
        {
            if (itemDictionary.TryGetValue(itemID, out var itemData))
            {
                groundItems.Add(NewItem(itemData, position, prefixID, suffixID, dropAmount, true));
            }
        }

        public override void Update(GameTime gameTime)
        {
            foreach (Item item in groundItems)
            {
                item.Update(gameTime);
            }
            foreach (Item item in items)
            {
                if (!item.OnGround)
                {
                    itemsToRemove.Add(item);
                }
            }
            foreach (Item item in itemsToRemove)
            {
                groundItems.Remove(item);
                items.Remove(item);
            }
            base.Update(gameTime);
        }



        public override void Draw(GameTime gameTime)
        {
            spriteBatch.Begin();
            spriteBatch.DrawString(Main.TestFont, "ITEM MANAGER ITEM COUNT: " + items.Count.ToString(), new Vector2(10, 340), Color.Black, 0, Vector2.Zero, 1f, SpriteEffects.None, 1f);
            spriteBatch.DrawString(Main.TestFont, "ITEM MANAGER GROUND ITEMS COUNT: " + groundItems.Count.ToString(), new Vector2(10, 360), Color.Black, 0, Vector2.Zero, 1f, SpriteEffects.None, 1f);
            spriteBatch.End();

            foreach (Item item in groundItems)
            {
                if (item.Texture != null && item.OnGround)
                {
                    var RarityColor = item.RarityColor;
                    float levitationSpeed = 3f;
                    float levitationAmplitude = 0.5f;

                    float levitationOffset = (float)Math.Sin(item.LevitationTimer * levitationSpeed) * levitationAmplitude;

                    float itemScale = 1.0f + 0.25f * levitationOffset;

                    Color shadowColor = new Color(0, 0, 0, 200);
                    float shadowScaleFactor = 1.2f;
                    float shadowOffsetY = 6;

                    spriteBatch.Begin();

                    Vector2 shadowPosition = item.Position + new Vector2(item.Texture.Width / 2, item.Texture.Height / 2);
                    shadowPosition.Y += shadowOffsetY + levitationOffset;
                    spriteBatch.Draw(item.Texture, shadowPosition, null, shadowColor, 0, new Vector2(item.Texture.Width / 2, item.Texture.Height / 2), itemScale * shadowScaleFactor, SpriteEffects.None, 0);

                    spriteBatch.End();

                    Main.OutlineShader.Parameters["texelSize"].SetValue(new Vector2(1.0f / item.Texture.Width, 1.0f / item.Texture.Height));
                    Main.OutlineShader.Parameters["outlineColor"].SetValue(new Vector4(RarityColor.R / 255f, RarityColor.G / 255f, RarityColor.B / 255f, RarityColor.A / 255f));

                    spriteBatch.Begin(SpriteSortMode.Immediate, null, null, null, null, Main.OutlineShader, null);
                    spriteBatch.Draw(item.Texture, item.Position + new Vector2(item.Texture.Width / 2, item.Texture.Height / 2), null, Color.White, 0, new Vector2(item.Texture.Width / 2, item.Texture.Height / 2), itemScale, SpriteEffects.None, 0);

                    spriteBatch.End();

                    spriteBatch.Begin();
                    string itemName;
                    if (item.Type != "Weapon")
                    {
                        if (item.StackLimit == 1)
                        {
                            itemName = "[" + item.Name + "]";
                        }
                        else
                        {
                            itemName = "[" + item.Name + " x" + item.StackSize + "]";
                        }
                    }
                    else
                    {
                        itemName = "[" + item.PrefixName + " " + item.Name + " " + item.SuffixName + "]";
                    }

                    //spriteBatch.DrawString(Game1.TestFont, itemName, new Vector2(Position.X - Game1.TestFont.MeasureString(itemName).X / 2f + Texture.Width / 2, Position.Y + Texture.Height + 4), RarityColor);
                    spriteBatch.End();
                }
            }

            base.Draw(gameTime);
        }

    }
}
