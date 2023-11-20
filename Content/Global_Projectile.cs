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
        private List<Projectile> projectilesToRemove;

        public Global_Projectile(Game game, SpriteBatch spriteBatch)
            : base(game)
        {
            this.spriteBatch = spriteBatch;

            projectiles = new List<Projectile>();
            projectilesToRemove = new List<Projectile>();
            projectileDictionary = new Dictionary<int, Projectile>();

            string projectilesJson = File.ReadAllText("Content/projectiles.json");
            projectiles = JsonConvert.DeserializeObject<List<Projectile>>(projectilesJson);
            foreach (var projectile in projectiles)
            {
                projectileDictionary.Add(projectile.id, projectile);
            }
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
                projectiles.Add(new Projectile(p.texture, p.texturePath, id, p.ai, position, target, p.name, damage, p.penetrate, p.lifeTimeMax, p.knockBack, speed, owner, isAlive, p.width, p.height));
            }
        }

        public override void Update(GameTime gameTime)
        {
            for (int i = projectiles.Count - 1; i >= 0; i--)
            {
                if (projectiles.Count > 0)
                {
                    Projectile projectile = projectiles[i];
                    if (projectile.isAlive)
                    {
                        projectile.Update(gameTime, this);
                    }
                    else
                    {
                        projectilesToRemove.Add(projectile);
                    }
                }
            }

            foreach (Projectile proj in projectiles)
            {
                if (proj.owner == null)
                {
                    projectilesToRemove.Add(proj);
                }
            }

            foreach (Projectile projectile in projectilesToRemove)
            {
                projectiles.Remove(projectile);
            }

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
