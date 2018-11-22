using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;


namespace Traffic
{
    class Button : GameObject
    {
        private SoundEffect clickSound;

        private Texture2D texture;

        private SpriteFont font;
        private Vector2 textLocation;

        MouseState mouseState;
        MouseState prevMouseState;

        public String buttonID;

        public bool clicked = false;
        public bool hover = false;

        public Button(String buttonName, Vector2 position, String assetName = "Overlay/button")
        {
            buttonID = buttonName;
            this.position = position;
            font = Traffic.ContentManager.Load<SpriteFont>("Fonts/font-24");
            clickSound = Traffic.ContentManager.Load<SoundEffect>("Sounds/Light Click");

            texture = Traffic.ContentManager.Load<Texture2D>(assetName);
            this.position -= new Vector2(texture.Width / 2, texture.Height / 2);
        }

        public override void InputHelper()
        {
            prevMouseState = mouseState;
            mouseState = Mouse.GetState();
            Point mousePos = mouseState.Position;

            hover = false;
            clicked = false;
            // checks if mouse is hovering on the button
            if (mousePos.X >= position.X &&
                mousePos.Y >= position.Y &&
                mousePos.X <= position.X + texture.Width&&
                mousePos.Y <= position.Y + texture.Height)
            {
                hover = true;
                clicked = false;
                // checks if clicked
                if (mouseState.LeftButton == ButtonState.Pressed && prevMouseState.LeftButton == ButtonState.Released && prevMouseState.LeftButton != ButtonState.Pressed)
                {
                    clicked = true;
                    clickSound.Play();
                }
                
            }

            base.InputHelper();
        }

        public override void Update(GameTime gameTime)
        {
            // centers text in the button
            position += velocity;
            textLocation = position - font.MeasureString(buttonID) / 2 + new Vector2(texture.Width / 2, texture.Height / 2);
            base.Update(gameTime);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            // edits color of the button when in different states
            Color buttonColor;
            if (clicked)
            {
                buttonColor = Color.Green;
            }
            else if (hover)
            {
                buttonColor = Color.LightSteelBlue;
            }
            else
            {
                buttonColor = Color.White;
            }
            spriteBatch.Draw(texture, position, buttonColor);
            spriteBatch.DrawString(font, buttonID, textLocation, buttonColor);

            base.Draw(spriteBatch);
        }

    }
}
