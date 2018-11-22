using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Traffic
{
    class Anim : Sprite
    {
        private int frames; // amount of frames
        public int frame = 1; // begin at frame 1
        private int fps; // amount of frames per sec
        private int timer; // time gone by since Anim is intialized
        public int timesPlayed; // time animation is played
        Vector2 frameSize; // size of the rextangle
        Texture2D animTexture; // texture has to be horizontaly placed
        Rectangle textureFrame; 

        public Anim(String assetName, Vector2 position, float rotation,int frames, int fps) : base(assetName, position, rotation, false)
        {
            animTexture = Traffic.ContentManager.Load<Texture2D>(assetName);
            this.frames = frames;
            this.fps = fps;
            timesPlayed = 0;
            frameSize = new Vector2(animTexture.Width / frames, animTexture.Height);
        }

        public override void Update(GameTime gameTime)
        {
            timer++;
            if (timer % (60 / fps) == 0)
            {
                frame++;
            }
            if (frame >= frames)
            {
                frame = 0;
                timesPlayed++;
            }

            textureFrame = new Rectangle(new Point((int)(frameSize.X * frame), 0), new Point((int)frameSize.X, (int)frameSize.Y));

            textureRect = textureFrame;

            base.Update(gameTime);
        }

    }
}
