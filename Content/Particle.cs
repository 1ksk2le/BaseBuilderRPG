using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BaseBuilderRPG.Content
{
    public class Particle
    {
        public Texture2D texture { get; set; }
        public int id { get; set; }
        public int ai { get; set; }
        public float lifeTimeMax { get; set; }
        public float lifeTime { get; set; }
        public float scale { get; set; }
        public string texturePath { get; set; }
        public string name { get; set; }
        public bool isAlive { get; set; }
        public Vector2 position { get; set; }
        public Vector2 velocity { get; set; }
        public Vector2 origin { get; set; }
        public Color color { get; set; }


        public Particle(Texture2D texture, string texturePath, string name, int id, int ai, Vector2 position, Vector2 velocity, Vector2 origin, float lifeTime, float scale, Color color, bool isAlive)
        {
            this.name = name;
            this.texture = texture;
            this.texturePath = texturePath;
            this.id = id;
            this.ai = ai;
            this.lifeTimeMax = lifeTime;
            this.velocity = velocity;
            this.position = position;
            this.scale = scale;
            this.origin = origin;
            this.isAlive = isAlive;
            this.lifeTime = 0f;
            this.color = color;
        }

        public void Update(GameTime gameTime)
        {
            if (lifeTime >= lifeTimeMax)
            {
                Kill();
            }
            else
            {
                lifeTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
                position += velocity * (float)gameTime.ElapsedGameTime.TotalSeconds;
            }
        }

        public void Kill()
        {
            isAlive = false;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (texture != null && isAlive)
            {
                spriteBatch.Draw(texture, position, null, color, 0f/*rot*/, origin, scale, SpriteEffects.None, 0);
            }
        }
    }
}