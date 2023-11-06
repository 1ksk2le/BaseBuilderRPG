using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;

namespace BaseBuilderRPG.Content
{
    public class Projectile_Manager : DrawableGameComponent
    {
        SpriteBatch spriteBatch;
        public static Dictionary<int, Projectile> projectileDictionary;
        static List<Projectile> projectiles;
        private List<Projectile> projectilesToRemove;

        public Projectile_Manager(Game game, SpriteBatch spriteBatch)
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
                projectileDictionary.Add(projectile.ID, projectile);
            }
        }

        public void LoadContentExternally()
        {
            foreach (var projectile in projectiles)
            {
                projectile.Texture = Game.Content.Load<Texture2D>(projectile.TexturePath);
            }
        }

        public static void NewProjectile(int id, int damage, float lifeTime, float knockBack, float speed, Vector2 position, Player owner, bool isAlive)
        {
            if (projectileDictionary.TryGetValue(id, out var p))
            {
                projectiles.Add(new Projectile(p.Texture, p.TexturePath, p.Name, id, p.AI, damage, lifeTime, knockBack, speed, position, owner, isAlive));
            }
        }

        public override void Update(GameTime gameTime)
        {
            foreach (Projectile projectile in projectiles)
            {
                if (projectile.IsAlive)
                {
                    projectile.Update(gameTime);
                }
                else
                {
                    projectilesToRemove.Add(projectile);
                }
            }
            foreach (Projectile projectile in projectilesToRemove)
            {
                projectiles.Remove(projectile);
            }
            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            spriteBatch.Begin();
            spriteBatch.DrawString(Main.TestFont, "PROJECTILE MANAGER PROJ COUNT: " + projectiles.Count.ToString(), new Vector2(10, 320), Color.Black, 0, Vector2.Zero, 1f, SpriteEffects.None, 1f);
            foreach (Projectile p in projectiles)
            {
                p.Draw(spriteBatch);

            }
            spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}
