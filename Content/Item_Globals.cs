﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace BaseBuilderRPG.Content
{
    public class Item_Globals : DrawableGameComponent
    {
        private SpriteBatch spriteBatch;
        public Dictionary<int, Item> itemDictionary;
        public List<Item> items;
        public List<Item> groundItems;
        private readonly Particle_Globals globalParticleBelow;
        private readonly Particle_Globals globalParticleAbove;

        public Item_Globals(Game game, SpriteBatch spriteBatch, Particle_Globals globalParticleBelow, Particle_Globals globalParticleAbove) : base(game)
        {
            this.spriteBatch = spriteBatch;

            items = new List<Item>();
            groundItems = new List<Item>();
            itemDictionary = new Dictionary<int, Item>();
            this.globalParticleBelow = globalParticleBelow;
            this.globalParticleAbove = globalParticleAbove;

            string itemsJson = File.ReadAllText("Content/items.json");
            items = JsonConvert.DeserializeObject<List<Item>>(itemsJson);
            for (int i = 0; i < items.Count; i++)
            {
                items[i].id = i;
                itemDictionary.Add(items[i].id, items[i]);
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
            return new Item(itemData.texture, itemData.texturePath, itemData.id, itemData.name, itemData.type, itemData.damageType, itemData.weaponType, position, itemData.shootSpeed, itemData.shootID, itemData.rarity, prefixID, suffixID, itemData.damage, itemData.knockBack, itemData.useTime, itemData.animationTime, itemData.stackLimit, dropAmount, itemData.stringSColorHex, itemData.stringEColorHex, onGround);
        }

        public void DropItem(int itemID, int prefixID, int suffixID, int dropAmount, Vector2 position)
        {
            if (itemDictionary.TryGetValue(itemID, out var itemData))
            {
                groundItems.Add(NewItem(itemData, position, prefixID, suffixID, dropAmount, true));
            }
        }

        public Item GetItem(int id)
        {
            if (itemDictionary.TryGetValue(id, out var i))
            {
                return i;
            }
            return null;
        }

        public override void Update(GameTime gameTime)
        {
            foreach (Item item in groundItems)
            {
                if (item.onGround)
                {
                    item.Update(gameTime, globalParticleBelow);
                }
            }

            //items.RemoveAll(item => !item.onGround); DELETE THIS UPON RELEASE
            groundItems.RemoveAll(item => !item.onGround);

            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            spriteBatch.Begin();
            spriteBatch.DrawStringWithOutline(Main.testFont, "ITEM MANAGER ITEM COUNT: " + items.Count.ToString(), new Vector2(10, 340), Color.Black, Color.White, 1f, 0.99f);
            spriteBatch.DrawStringWithOutline(Main.testFont, "ITEM MANAGER GROUND ITEM COUNT: " + groundItems.Count.ToString(), new Vector2(10, 360), Color.Black, Color.White, 1f, 0.99f);
            spriteBatch.End();

            foreach (Item item in groundItems)
            {
                if (item.texture != null && item.onGround)
                {
                    float levitationOffset = (float)Math.Sin(item.levTimer * 3f) * 0.5f;
                    float scale = 1.0f + 0.25f * levitationOffset;

                    spriteBatch.Begin();

                    Vector2 shadowPosition = item.position + new Vector2(item.texture.Width / 2, item.texture.Height / 2);
                    shadowPosition.Y += 6 + levitationOffset;
                    spriteBatch.Draw(item.texture, shadowPosition, null, new Color(0, 0, 0, 200), 0, new Vector2(item.texture.Width / 2, item.texture.Height / 2), scale * 1.2f, SpriteEffects.None, 1f);

                    spriteBatch.End();

                    Main.outlineShader.Parameters["texelSize"].SetValue(new Vector2(1.0f / item.texture.Width, 1.0f / item.texture.Height));
                    Main.outlineShader.Parameters["outlineColor"].SetValue(new Vector4(item.rarityColor.R / 255f, item.rarityColor.G / 255f, item.rarityColor.B / 255f, item.rarityColor.A / 255f));

                    spriteBatch.Begin(SpriteSortMode.Immediate, null, null, null, null, Main.outlineShader, null);
                    spriteBatch.Draw(item.texture, item.center, null, Color.White, 0, item.origin, scale, SpriteEffects.None, 1f);
                    spriteBatch.End();

                    spriteBatch.Begin();
                    if (Main.drawDebugRectangles)
                    {
                        spriteBatch.DrawRectangleBorder(item.rectangle, Color.Yellow, 1f, 1f);
                    }

                    spriteBatch.End();
                }
            }

            base.Draw(gameTime);
        }
    }
}