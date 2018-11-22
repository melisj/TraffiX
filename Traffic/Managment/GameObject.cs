using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Traffic
{
    class GameObject
    {
        public Vector2 position;
        public Vector2 velocity;

        public GameObject()
        {

        }

        public virtual void InputHelper()
        {
        }

        public virtual void Update(GameTime gameTime)
        {
            position += velocity;
            InputHelper();
        }

        public virtual void Draw(SpriteBatch spriteBatch)
        {
        }
    }
}
