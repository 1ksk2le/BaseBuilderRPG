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
        private static Dictionary<int, Projectile> projectileDictionary;
        private static List<Projectile> projectiles;
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

        public void Load()
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
            for (int i = projectiles.Count - 1; i >= 0; i--)
            {
                if (projectiles.Count > 0)
                {
                    Projectile projectile = projectiles[i];
                    if (projectile.IsAlive)
                    {
                        projectile.Update(gameTime);
                    }
                    else
                    {
                        projectile.Kill();
                        projectilesToRemove.Add(projectile);
                    }
                }
            }

            foreach (Projectile proj in projectiles)
            {
                if (proj.Owner == null)
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



        public override void Draw(GameTime gameTime)
        {
            spriteBatch.Begin();
            spriteBatch.DrawString(Main.TestFont, "PROJECTILE MANAGER PROJ COUNT: " + projectiles.Count.ToString(), new Vector2(10, 300), Color.Black, 0, Vector2.Zero, 1f, SpriteEffects.None, 1f);
            foreach (Projectile p in projectiles)
            {
                if (p.Texture != null && p.IsAlive)
                {
                    float scale = 1.0f;

                    if (p.AI == 0)
                    {
                        if (p.CurrentLifeTime < p.LifeTime / 2)
                        {
                            scale = MathHelper.Lerp(0.3f, 1f, p.CurrentLifeTime / (p.LifeTime / 2));
                        }
                        else
                        {
                            scale = MathHelper.Lerp(1f, 0.3f, (p.CurrentLifeTime - p.LifeTime / 2) / (p.LifeTime / 2));
                        }
                    }

                    spriteBatch.Draw(p.Texture, p.Position + new Vector2(0, 10), null, new Color(0, 0, 0, 200), p.Rotation, new Vector2(p.Texture.Width / 2, p.Texture.Height / 2), scale * 1.2f, SpriteEffects.None, 0);
                    spriteBatch.Draw(p.Texture, p.Position, null, Color.White, p.Rotation, new Vector2(p.Texture.Width / 2, p.Texture.Height / 2), scale, SpriteEffects.None, 0);
                }

            }
            spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}
