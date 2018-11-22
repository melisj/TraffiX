using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Traffic
{
    class Text : GameObject
    {
        private SpriteFont font;
        public String text;
        private bool moveable;
        private Color textColor;
        public Vector2 originalPos;

        public Text(String text, Vector2 position, Color color, String fontName = "Fonts/font-24")
        {
            font = Traffic.ContentManager.Load<SpriteFont>(fontName);

            this.text = text;
            textColor = color;

            originalPos = position;
            this.position = position;
        }

        public override void Update(GameTime gameTime)
        {
            // if position is not edited it will center text
            if (text != null)
            {
                position = originalPos - font.MeasureString(text) / 2;
            }
            base.Update(gameTime);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {

            if (text != null)
            {
                spriteBatch.DrawString(font, text, position, textColor);
            }

            base.Draw(spriteBatch);
        }

    }
}
