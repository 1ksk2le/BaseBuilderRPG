using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace BaseBuilderRPG.Content
{
    public class Display_Text
    {
        private string _text1;
        private string _text2;
        private Vector2 _pos;
        private Color _color1;
        private Color _color2;
        private float _lifeTime;
        private float _timer;
        private float _scale;

        public bool _isAlive => _timer <= _lifeTime;

        public Display_Text(string text1, string text2, Vector2 pos, Color color1, Color color2, float lifeTime, float scale)
        {
            _text1 = text1;
            _text2 = text2;
            _pos = pos + new Vector2(0, -10);
            _color1 = color1;
            _color2 = color2;
            _lifeTime = lifeTime;
            _timer = 0f;
            _scale = scale;
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
            if (font != null && _timer <= _lifeTime)
            {
                string fullText = _text1 + _text2;

                Vector2 fullTextSize = font.MeasureString(fullText) * _scale;

                Vector2 text2Position = _pos + new Vector2(fullTextSize.X / 2, 0);

                DrawStringWithOutline(spriteBatch, font, _text1, _pos, _color1, _timer, _lifeTime);
                DrawStringWithOutline(spriteBatch, font, _text2, text2Position, _color2, _timer, _lifeTime);
            }
        }

        private void DrawStringWithOutline(SpriteBatch spriteBatch, SpriteFont font, string text, Vector2 position, Color color, float timer, float lifeTime)
        {
            float outlineThickness = 1.5f;

            for (float i = 0; i < 360; i += 45)
            {
                float angle = MathHelper.ToRadians(i);
                Vector2 offset = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * outlineThickness;

                Color outlineColor = Color.FromNonPremultiplied(0, 0, 0, (int)(color.A * (1 - timer / lifeTime)));

                spriteBatch.DrawString(font, text, position + offset, outlineColor, 0f, font.MeasureString(text) / 2, _scale, SpriteEffects.None, 0f);
            }

            Color mainTextColor = new Color(color.R, color.G, color.B, (int)(color.A * (1 - timer / lifeTime)));

            spriteBatch.DrawString(font, text, position, mainTextColor, 0f, font.MeasureString(text) / 2, _scale, SpriteEffects.None, 0f);
        }
    }
}