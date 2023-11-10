using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace BaseBuilderRPG.Content
{
    public class Display_Text_Manager
    {
        private List<Display_Text> _texts;
        private SpriteFont _font;


        public Display_Text_Manager(SpriteFont font)
        {
            this._texts = new List<Display_Text>();
            this._font = font;
        }

        public void AddFloatingText(string text, Vector2 position, Color color, float duration)
        {
            Display_Text floatingText = new Display_Text(text, position, color, duration);
            _texts.Add(floatingText);
        }

        public void Update(GameTime gameTime)
        {
            for (int i = _texts.Count - 1; i >= 0; i--)
            {
                _texts[i].Update(gameTime);
                if (_texts[i]._isAlive)
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