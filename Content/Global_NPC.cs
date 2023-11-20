using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;

namespace BaseBuilderRPG.Content
{
    public class Global_NPC : DrawableGameComponent
    {
        SpriteBatch spriteBatch;
        private static Dictionary<int, NPC> npcDictionary;

        public List<NPC> npcs;
        private readonly List<NPC> npcsToRemove;
        private readonly List<Player> players;
        private readonly List<Projectile> projectiles;
        private readonly Global_Item globalItem;
        private readonly Text_Manager disTextManager;

        public Global_NPC(Game game, SpriteBatch spriteBatch, Global_Item _globalItem, Text_Manager _disTextManager, List<Player> _players, List<Projectile> projectiles)
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
            globalItem = _globalItem;
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
                npcs.Add(new NPC(npc.texture, npc.texturePath, id, npc.ai, position, npc.name, npc.damage, npc.healthMax, npc.knockBack, npc.knockBackRes, npc.targetRange, npc.numFrames, true));
            }
        }

        public override void Update(GameTime gameTime)
        {
            foreach (NPC npc in npcs)
            {
                if (npc.isAlive)
                {
                    npc.Update(gameTime, players, projectiles, disTextManager, globalItem);
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
            spriteBatch.DrawStringWithOutline(Main.testFont, "NPC MANAGER NPC COUNT: " + npcs.Count.ToString(), new Vector2(10, 380), Color.Black, Color.White, 1f, 0.99f);
            foreach (NPC npc in npcs)
            {
                if (npc.texture != null && npc.isAlive)
                {
                    npc.Draw(spriteBatch);
                    if (npc.health < npc.healthMax)
                    {
                        var pos = npc.position + new Vector2(0, npc.height);
                        float healthBarWidth = npc.width * ((float)npc.health / (float)npc.healthMax);

                        Rectangle healthBarRectangleBackground = new Rectangle((int)(pos.X - 2), (int)pos.Y - 1, (int)(npc.width) + 4, 4);
                        Rectangle healthBarRectangleBackgroundRed = new Rectangle((int)(pos.X), (int)pos.Y, (int)(npc.width), 2);
                        Rectangle healthBarRectangle = new Rectangle((int)(pos.X), (int)pos.Y, (int)healthBarWidth, 2);

                        spriteBatch.DrawRectangle(healthBarRectangleBackground, Color.Black, 0.691f);
                        spriteBatch.DrawRectangle(healthBarRectangleBackgroundRed, Color.Red, 0.692f);
                        spriteBatch.DrawRectangle(healthBarRectangle, Color.Lime, 0.693f);
                    }

                    Vector2 textSize = Main.testFont.MeasureString(npc.name);
                    spriteBatch.DrawStringWithOutline(Main.testFont, npc.name, new Vector2(npc.center.X - textSize.X / 2, npc.center.Y - npc.height), Color.Black, Color.Coral, 1f, 0.693f);
                }
            }
            spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}
