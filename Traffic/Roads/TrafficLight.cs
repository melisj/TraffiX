using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

// TL = TrafficLight
/*
 * an TL is the object that is drawn to the screen and is interacteable.
 * one TL can have an influence on more roads that are connected in the same direction.
 * this is because the there is an seperate array that controls the parent and children roads that one TL can control.
 * the color for an TL in the bitmap is green. but for roads that dont need an extra TL to be drawn to the screen the color is yellow.
 */

namespace Traffic
{
    class TrafficLight : GameObject
    {
        private SoundEffect lightSound;

        private Texture2D texture;
        private RoadAnalysis roadAnalysis;
        public Vector2 rotation;
        private Vector2 roadOffset = new Vector2 (50, 20); // offset from spawnlocation on the road

        public bool state = false; // green/red
        private Rectangle textureState; // the color of the light in spritesheet
        private Vector2 textureOffset = new Vector2(35,0); // centering it in the end 

        

        private MouseState mouseState;
        private MouseState prevMouseState;

        public TrafficLight(Vector2 position, RoadAnalysis roadAnalysis)
        {
            this.position = position;
            texture = Traffic.ContentManager.Load<Texture2D>("Overlay/trafficlight");
            lightSound = Traffic.ContentManager.Load<SoundEffect>("Sounds/Light Click");
            this.roadAnalysis = roadAnalysis;
            CheckRoad();
        }

        public void CheckRoad()
        {
            // checks all the roads to see which one belongs to this TL
            for (int i = 0; i < roadAnalysis.roads.Count; i++)
            {
                Vector2 roadLocation = roadAnalysis.roads[i];

                // gives position based on th direction of the road 
                if (position.X == roadLocation.X || position.Y == roadLocation.Y)
                {
                    Vector2 roadDirection = roadAnalysis.roadDirection[i];
                    rotation = roadDirection;

                    if (roadDirection.X == 1 && roadDirection.Y == 1)
                        position += new Vector2(-roadOffset.Y, roadOffset.X);
                    if (roadDirection.X == -1 && roadDirection.Y == -1)
                        position += new Vector2(roadOffset.X, roadOffset.Y);
                    if (roadDirection.X == -1 && roadDirection.Y == 1)
                        position += new Vector2(roadOffset.Y, -roadOffset.X);
                    if (roadDirection.X == 1 && roadDirection.Y == -1)
                        position += new Vector2(-roadOffset.X, -roadOffset.Y);

                    position -= new Vector2(texture.Width / 2, texture.Height / 2); // centering texture
                }
            }
           
        }

        public override void InputHelper()
        {
            prevMouseState = mouseState;
            mouseState = Mouse.GetState();
            Point mousePos = mouseState.Position;

            // checks if mouse is pressed on the TL
            if (mousePos.X >= position.X + textureOffset.X &&
                mousePos.Y >= position.Y &&
                mousePos.X <= position.X + textureState.Width + textureOffset.X &&
                mousePos.Y <= position.Y + textureState.Height)
            {
                if (mouseState.LeftButton == ButtonState.Pressed && prevMouseState.LeftButton == ButtonState.Released)
                {
                    state = !state;
                    lightSound.Play();
                }
            }

            base.InputHelper();
        }

        public override void Update(GameTime gameTime)
        {
            // depending on the state it changes the rectangel offset of the texture
            if (!state)
                textureState = new Rectangle(new Point(35, 0), new Point(35, 50));
            else
                textureState = new Rectangle(new Point(70, 0), new Point(35, 50));

            base.Update(gameTime);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {

            spriteBatch.Draw(texture, position + textureOffset, null, textureState, null, 0, null, Color.White);

            base.Draw(spriteBatch);
        }

    }
}
