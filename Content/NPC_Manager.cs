using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;

namespace BaseBuilderRPG.Content
{
    public class NPC_Manager : DrawableGameComponent
    {
        SpriteBatch spriteBatch;
        private static Dictionary<int, NPC> npcDictionary;

        public List<NPC> npcs;
        private List<NPC> npcsToRemove;
        private List<Player> players;
        private List<Projectile> projectiles;
        private Item_Manager itemManager;
        private Display_Text_Manager disTextManager;

        public NPC_Manager(Game game, SpriteBatch spriteBatch, Item_Manager _itemManager, Display_Text_Manager _disTextManager, List<Player> _players, List<Projectile> projectiles)
            : base(game)
        {
            this.spriteBatch = spriteBatch;

            npcs = new List<NPC>();
            npcsToRemove = new List<NPC>();
            npcDictionary = new Dictionary<int, NPC>();

            string npcsJson = File.ReadAllText("Content/npcs.json");
            npcs = JsonConvert.DeserializeObject<List<NPC>>(npcsJson);
            foreach (var npc in npcs)
            {
                npcDictionary.Add(npc.id, npc);
            }

            players = _players;
            itemManager = _itemManager;
            disTextManager = _disTextManager;
            this.projectiles = projectiles;
        }

        public void Load()
        {
            foreach (var npc in npcs)
            {
                npc.texture = Game.Content.Load<Texture2D>(npc.texturePath);
            }
        }

        public void NewNPC(int id, Vector2 position)
        {
            if (npcDictionary.TryGetValue(id, out var npc))
            {
                npcs.Add(new NPC(npc.texture, npc.texturePath, id, npc.ai, position, npc.name, npc.damage, npc.maxHealth, npc.knockBack, npc.knockBackRes, npc.numFrames, true));
            }
        }

        public override void Update(GameTime gameTime)
        {
            foreach (NPC npc in npcs)
            {
                if (npc.isAlive)
                {
                    npc.Update(gameTime, players, projectiles, disTextManager, itemManager);
                }
                else
                {
                    npcsToRemove.Add(npc);
                }
            }

            foreach (NPC npc in npcsToRemove)
            {
                npcs.Remove(npc);
            }

            base.Update(gameTime);
        }



        public override void Draw(GameTime gameTime)
        {
            spriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, Matrix.Identity);
            spriteBatch.DrawString(Main.testFont, "NPC MANAGER NPC COUNT: " + npcs.Count.ToString(), new Vector2(10, 400), Color.Black, 0, Vector2.Zero, 1f, SpriteEffects.None, 1f);
            foreach (NPC npc in npcs)
            {
                if (npc.texture != null && npc.isAlive)
                {
                    npc.Draw(spriteBatch);
                    if (npc.health < npc.maxHealth)
                    {
                        var pos = npc.position + new Vector2(0, npc.height + 6);
                        float healthBarWidth = npc.width * ((float)npc.health / (float)npc.maxHealth);

                        Rectangle healthBarRectangleBackground = new Rectangle((int)(pos.X - 2), (int)pos.Y - 1, (int)(npc.width) + 4, 4);
                        Rectangle healthBarRectangleBackgroundRed = new Rectangle((int)(pos.X), (int)pos.Y, (int)(npc.width), 2);
                        Rectangle healthBarRectangle = new Rectangle((int)(pos.X), (int)pos.Y, (int)healthBarWidth, 2);

                        spriteBatch.DrawRectangle(healthBarRectangleBackground, Color.Black, 0.691f);
                        spriteBatch.DrawRectangle(healthBarRectangleBackgroundRed, Color.Red, 0.692f);
                        spriteBatch.DrawRectangle(healthBarRectangle, Color.Lime, 0.693f);
                    }

                    Vector2 textSize = Main.testFont.MeasureString(npc.name);
                    spriteBatch.DrawStringWithOutline(Main.testFont, npc.name, new Vector2(npc.position.X + npc.width / 2 - textSize.X / 2, npc.position.Y - 14), Color.Black, Color.White, 1f, 0.693f);
                }
            }
            spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}
