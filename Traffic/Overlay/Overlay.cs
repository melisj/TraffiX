using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Traffic
{
    class Overlay : GameObject
    {
        private Texture2D levelDisplay;
        private SpriteFont font;
        public Vector2 lvDisplayPos;
        public Vector2 lvDisplayStartPos;

        private Texture2D otherDisplay;
        public Vector2 otherDisplayPos;
        public Vector2 otherDisplayStartPos;

        private Texture2D timeDisplay;
        public Vector2 timeDisplayPos;
        public Vector2 timeDisplayStartPos;

        public String levelText;
        private Vector2 lvTextPos;

        private String carsText;
        private Vector2 carsTextPos;

        private String timerText;
        private Vector2 timerTextPos;

        public int arrivedCars = 0;
        private int amountCarsLevel;
        public int time = 0;
        public int amountTime;
        public String levelNum;

        public Overlay(String levelNum, int amountCarsLevel, int amountTime)
        {
            levelDisplay = Traffic.ContentManager.Load<Texture2D>("Overlay/leveloverlay");
            otherDisplay = Traffic.ContentManager.Load<Texture2D>("Overlay/bottomoverlay");
            timeDisplay = Traffic.ContentManager.Load<Texture2D>("Overlay/timerdisplay");

            timeDisplayStartPos = new Vector2(Traffic.screenSize.X - timeDisplay.Width, Traffic.screenSize.Y - timeDisplay.Height);
            timeDisplayPos = timeDisplayStartPos;
            lvDisplayStartPos = new Vector2(Traffic.screenSize.X / 2 - levelDisplay.Width / 2, 0);
            lvDisplayPos = lvDisplayStartPos;
            otherDisplayStartPos = new Vector2(0, Traffic.screenSize.Y - otherDisplay.Height);
            otherDisplayPos = otherDisplayStartPos;

            this.amountTime = amountTime / 60;
            this.amountCarsLevel = amountCarsLevel;
            this.levelNum = levelNum;

            font = Traffic.ContentManager.Load<SpriteFont>("Fonts/font-24");
            levelText = "LEVEL: ";
            carsText = "CARS: " + arrivedCars + " / " + amountCarsLevel;
            timerText = "TIME: " + time;
        }

        public override void Update(GameTime gameTime)
        {
            carsText = "CARS: " + arrivedCars + " / " + amountCarsLevel;
            timerText = "TIME: " + time;

            lvTextPos = lvDisplayPos + font.MeasureString(levelText) / 2 + new Vector2(-1, -5);
            carsTextPos = otherDisplayPos + new Vector2(levelDisplay.Width / 10, 80);
            timerTextPos = timeDisplayPos - new Vector2(-timeDisplay.Width / 10, -80);

            base.Update(gameTime);
        }


        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(levelDisplay, lvDisplayPos, null, null, null, 0, null,Color.White);
            spriteBatch.Draw(otherDisplay, otherDisplayPos, null, null, null, 0, null, Color.White);
            spriteBatch.Draw(timeDisplay, timeDisplayPos, null, null, null, 0, null, Color.White);
            spriteBatch.DrawString(font, levelText + levelNum, lvTextPos, Color.White);
            spriteBatch.DrawString(font, carsText, carsTextPos, Color.White);
            spriteBatch.DrawString(font, timerText, timerTextPos, Color.White);

            base.Draw(spriteBatch);
        }
    }
}
