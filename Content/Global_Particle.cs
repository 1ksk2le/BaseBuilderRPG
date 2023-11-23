using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;

namespace BaseBuilderRPG.Content
{
    public class Global_Particle : DrawableGameComponent
    {
        SpriteBatch spriteBatch;
        private Vector2 pos;
        private List<Particle> particles;
        private List<Particle> particlesToRemove;
        private static Dictionary<int, Particle> particleDictionary;

        public Global_Particle(Game game, SpriteBatch spriteBatch)
            : base(game)
        {
            this.spriteBatch = spriteBatch;

            particles = new List<Particle>();
            particlesToRemove = new List<Particle>();
            particleDictionary = new Dictionary<int, Particle>();

            string projectilesJson = File.ReadAllText("Content/particles.json");
            particles = JsonConvert.DeserializeObject<List<Particle>>(projectilesJson);
            for (int i = 0; i < particles.Count; i++)
            {
                particles[i].id = i;
                particleDictionary.Add(particles[i].id, particles[i]);
            }
        }

        public void Load()
        {
            foreach (var particle in particles)
            {
                particle.texture = Game.Content.Load<Texture2D>(particle.texturePath);
            }
        }

        public void NewParticle(int id, int ai, Vector2 position, Vector2 velocity, Vector2 origin, float lifeTime, float scale, Color color)
        {
            if (particleDictionary.TryGetValue(id, out var p))
            {
                particles.Add(new Particle(p.texture, p.texturePath, p.name, id, ai, position, velocity, origin, lifeTime, scale, color, true));
            }
        }

        public override void Update(GameTime gameTime)
        {
            for (int i = particles.Count - 1; i >= 0; i--)
            {
                if (particles.Count > 0)
                {
                    Particle particle = particles[i];
                    if (particle.isAlive)
                    {
                        particle.Update(gameTime);
                    }
                    else
                    {
                        particlesToRemove.Add(particle);
                    }
                }
            }

            /*foreach (Particle particle in particles)
            {
                if (particle.position == Vector2.Zero)
                {
                    particlesToRemove.Add(particle);
                }
            }*/

            foreach (Particle particle in particlesToRemove)
            {
                particles.Remove(particle);
            }

            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            spriteBatch.Begin();
            spriteBatch.DrawStringWithOutline(Main.testFont, "PARTICLE MANAGER PARTICLE COUNT: " + particles.Count.ToString(), new Vector2(10, 400), Color.Black, Color.White, 1f, 0.99f);
            foreach (Particle p in particles)
            {
                p.Draw(spriteBatch);
            }
            spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}
