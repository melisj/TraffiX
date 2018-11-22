using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Traffic
{
    class Cursor : GameObject 
    {

        public Vector2 position;
        public Texture2D texture;

        public Cursor()
        {
            texture = Traffic.ContentManager.Load<Texture2D>("Overlay/cursor");
        }


        public override void InputHelper()
        {
            // positie van de muis ophalen
            var mouseState = Mouse.GetState();
            Point mousePos = mouseState.Position;

            position = new Vector2(mousePos.X - texture.Width / 2, mousePos.Y - texture.Height / 2);

            base.InputHelper();
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(texture, position);

            base.Draw(spriteBatch);
        }

    }
}
