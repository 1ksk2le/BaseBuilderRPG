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
        private SpriteBatch spriteBatch;
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
                itemDictionary.Add(item.id, item);
            }
        }

        public void Load()
        {
            foreach (var item in items)
            {
                item.texture = Game.Content.Load<Texture2D>(item.texturePath);
            }
        }

        public Item NewItem(Item itemData, Vector2 position, int prefixID, int suffixID, int dropAmount, bool onGround)
        {
            return new Item(itemData.texture, itemData.texturePath, itemData.id, itemData.name, itemData.type, itemData.damageType, itemData.weaponType, position, itemData.shootSpeed, itemData.shootID, itemData.rarity, prefixID, suffixID, itemData.damage, itemData.knockBack, itemData.useTime, itemData.stackLimit, dropAmount, onGround);
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
                if (!item.onGround)
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
            spriteBatch.DrawString(Main.testFont, "ITEM MANAGER ITEM COUNT: " + items.Count.ToString(), new Vector2(10, 340), Color.Black, 0, Vector2.Zero, 1f, SpriteEffects.None, 1f);
            spriteBatch.DrawString(Main.testFont, "ITEM MANAGER GROUND ITEMS COUNT: " + groundItems.Count.ToString(), new Vector2(10, 360), Color.Black, 0, Vector2.Zero, 1f, SpriteEffects.None, 1f);
            spriteBatch.End();

            foreach (Item item in groundItems)
            {
                if (item.texture != null && item.onGround)
                {
                    var RarityColor = item.rarityColor;
                    float levitationSpeed = 3f;
                    float levitationAmplitude = 0.5f;

                    float levitationOffset = (float)Math.Sin(item.levTimer * levitationSpeed) * levitationAmplitude;

                    float itemScale = 1.0f + 0.25f * levitationOffset;

                    Color shadowColor = new Color(0, 0, 0, 200);
                    float shadowScaleFactor = 1.2f;
                    float shadowOffsetY = 6;

                    spriteBatch.Begin();

                    Vector2 shadowPosition = item.position + new Vector2(item.texture.Width / 2, item.texture.Height / 2);
                    shadowPosition.Y += shadowOffsetY + levitationOffset;
                    spriteBatch.Draw(item.texture, shadowPosition, null, shadowColor, 0, new Vector2(item.texture.Width / 2, item.texture.Height / 2), itemScale * shadowScaleFactor, SpriteEffects.None, 0);

                    spriteBatch.End();

                    Main.outlineShader.Parameters["texelSize"].SetValue(new Vector2(1.0f / item.texture.Width, 1.0f / item.texture.Height));
                    Main.outlineShader.Parameters["outlineColor"].SetValue(new Vector4(RarityColor.R / 255f, RarityColor.G / 255f, RarityColor.B / 255f, RarityColor.A / 255f));

                    spriteBatch.Begin(SpriteSortMode.Immediate, null, null, null, null, Main.outlineShader, null);
                    spriteBatch.Draw(item.texture, item.position + new Vector2(item.texture.Width / 2, item.texture.Height / 2), null, Color.White, 0, new Vector2(item.texture.Width / 2, item.texture.Height / 2), itemScale, SpriteEffects.None, 0);

                    spriteBatch.End();

                    spriteBatch.Begin();
                    string itemName;
                    if (item.type != "Weapon")
                    {
                        if (item.stackLimit == 1)
                        {
                            itemName = "[" + item.name + "]";
                        }
                        else
                        {
                            itemName = "[" + item.name + " x" + item.stackSize + "]";
                        }
                    }
                    else
                    {
                        itemName = "[" + item.prefixName + " " + item.name + " " + item.suffixName + "]";
                    }

                    spriteBatch.End();
                }
            }

            base.Draw(gameTime);
        }
    }
}