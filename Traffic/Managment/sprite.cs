using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Traffic
{
    class Sprite : GameObject
    {
        public Texture2D texture;
        public Rectangle textureRect;
        public float rotation;

        public Sprite(String assetName, Vector2 position, float rotation, bool center = false)
        {
            texture = Traffic.ContentManager.Load<Texture2D>(assetName);
            this.position = position;

            // image can be centered if is told to by constructor
            if (center)
            {
                this.position -= new Vector2(texture.Width / 2, texture.Height / 2);
            }
            this.rotation = rotation;
        }

        public override void Update(GameTime gameTime)
        {
            position += velocity;

            base.Update(gameTime);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            
            // if the rectangle is empty it will draw the whole texture
            // rectangle is used for animations
            if (textureRect.IsEmpty)
            {
                spriteBatch.Draw(texture, position, Color.White);
            }
            else
            {
                spriteBatch.Draw(texture, position, null, textureRect, null, rotation, null, Color.White);
            }
            base.Draw(spriteBatch);
        }
    }
}
