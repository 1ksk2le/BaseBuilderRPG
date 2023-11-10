using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BaseBuilderRPG.Content
{
    public class Display_Text
    {
        private string _text;
        private Vector2 _pos;
        private Color _color;
        private float _lifeTime;
        private float _timer;
        public bool _isAlive => _timer >= _lifeTime;


        public Display_Text(string text, Vector2 pos, Color color, float lifeTime)
        {
            _text = text;
            _pos = pos;
            _color = color;
            _lifeTime = lifeTime;
            _timer = 0f;
        }

        public void Update(GameTime gameTime)
        {
            if (_timer <= _lifeTime)
            {
                _timer += (float)gameTime.ElapsedGameTime.TotalSeconds;
                _pos.Y -= 15f * (float)gameTime.ElapsedGameTime.TotalSeconds;
            }
        }

        public void Draw(SpriteBatch spriteBatch, SpriteFont font)
        {
            if (font != null)
            {
                float alpha = 1 - (_timer / _lifeTime);
                Vector2 origin = font.MeasureString(_text) / 2;

                spriteBatch.DrawString(font, _text, _pos, _color * alpha, 0f, origin, 1.1f, SpriteEffects.None, 0f);
            }

        }
    }
}