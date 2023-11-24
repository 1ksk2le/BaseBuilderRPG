using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;

namespace BaseBuilderRPG.Content
{
    public class Global_Projectile : DrawableGameComponent
    {
        SpriteBatch spriteBatch;
        private static Dictionary<int, Projectile> projectileDictionary;
        public List<Projectile> projectiles;
        private Global_Particle globalParticle;

        public Global_Projectile(Game game, SpriteBatch spriteBatch, Global_Particle globalParticle)
            : base(game)
        {
            this.spriteBatch = spriteBatch;

            projectiles = new List<Projectile>();
            projectileDictionary = new Dictionary<int, Projectile>();

            string projectilesJson = File.ReadAllText("Content/projectiles.json");
            projectiles = JsonConvert.DeserializeObject<List<Projectile>>(projectilesJson);
            for (int i = 0; i < projectiles.Count; i++)
            {
                projectiles[i].id = i;
                projectileDictionary.Add(projectiles[i].id, projectiles[i]);
            }

            this.globalParticle = globalParticle;

        }

        public void Load()
        {
            foreach (var projectile in projectiles)
            {
                projectile.texture = Game.Content.Load<Texture2D>(projectile.texturePath);
            }
        }

        public void NewProjectile(int id, Vector2 position, Vector2 target, int damage, float speed, Player owner, bool isAlive)
        {
            if (projectileDictionary.TryGetValue(id, out var p))
            {
                projectiles.Add(new Projectile(p.texture, p.texturePath, id, p.ai, position, target, speed, p.name, damage, p.penetrate, p.lifeTimeMax, p.knockBack, owner, isAlive, p.width, p.height));
            }
        }

        public override void Update(GameTime gameTime)
        {
            foreach (Projectile projectile in projectiles)
            {
                if (projectile.isAlive)
                {
                    projectile.Update(gameTime, this, globalParticle);
                }
            }

            projectiles.RemoveAll(projectile => !projectile.isAlive);

            base.Update(gameTime);
        }

        public Projectile GetProjectile(int id)
        {
            if (projectileDictionary.TryGetValue(id, out var p))
            {
                return p;
            }
            return null;
        }

        public override void Draw(GameTime gameTime)
        {
            spriteBatch.Begin();
            spriteBatch.DrawStringWithOutline(Main.testFont, "PROJECTILE MANAGER PROJECTILE COUNT: " + projectiles.Count.ToString(), new Vector2(10, 320), Color.Black, Color.White, 1f, 0.99f);
            foreach (Projectile p in projectiles)
            {
                p.Draw(spriteBatch);
            }
            spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}
