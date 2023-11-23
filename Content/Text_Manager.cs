using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace BaseBuilderRPG.Content
{
    public class Text_Manager
    {
        private List<Floating_Text> _texts;
        private SpriteFont _font;

        public Text_Manager(SpriteFont font)
        {
            this._texts = new List<Floating_Text>();
            this._font = font;
        }

        public void AddFloatingText(string text1, string text2, Vector2 position, Vector2 velocity, Color color1, Color color2, float duration, float scale)
        {
            Floating_Text floatingText = new Floating_Text(text1, text2, position, velocity, color1, color2, duration, scale);
            _texts.Add(floatingText);
        }

        public void Update(GameTime gameTime)
        {
            for (int i = _texts.Count - 1; i >= 0; i--)
            {
                _texts[i].Update(gameTime);
                if (!_texts[i]._isAlive)
                {
                    _texts.RemoveAt(i);
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            foreach (var floatingText in _texts)
            {
                floatingText.Draw(spriteBatch, _font);
            }
        }
    }
}