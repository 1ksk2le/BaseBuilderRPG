using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;

namespace BaseBuilderRPG.Content
{
    public class NPC_Globals : DrawableGameComponent
    {
        private SpriteBatch spriteBatch;
        private static Dictionary<int, NPC> npcDictionary;
        public List<NPC> npcs;
        private readonly List<Player> players;
        private readonly List<Projectile> projectiles;
        private readonly Item_Globals globalItem;
        private readonly Particle_Globals globalParticle;
        private readonly Text_Manager textManager;

        public NPC_Globals(Game game, SpriteBatch spriteBatch, Item_Globals globalItem, Particle_Globals globalParticle, Text_Manager textManager, List<Player> players, List<Projectile> projectiles)
            : base(game)
        {
            this.spriteBatch = spriteBatch;

            npcs = new List<NPC>();
            npcDictionary = new Dictionary<int, NPC>();

            string npcsJson = File.ReadAllText("Content/npcs.json");
            npcs = JsonConvert.DeserializeObject<List<NPC>>(npcsJson);

            for (int i = 0; i < npcs.Count; i++)
            {
                npcs[i].id = i;
                npcDictionary.Add(npcs[i].id, npcs[i]);
            }

            this.players = players;
            this.globalItem = globalItem;
            this.globalParticle = globalParticle;
            this.textManager = textManager;
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
                    npc.Update(gameTime, players, projectiles, textManager, globalItem, globalParticle);
                }
            }

            npcs.RemoveAll(npc => !npc.isAlive);

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
                    spriteBatch.DrawStringWithOutline(Main.testFont, npc.name, new Vector2(npc.center.X - textSize.X / 2, npc.center.Y - npc.height), Color.Black, Color.Red, 1f, 0.693f);
                }
            }
            spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}
