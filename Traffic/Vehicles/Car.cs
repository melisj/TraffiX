using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Traffic
{
    class Car : Vehicle
    {
        Texture2D brakeLight;

        public Car(RoadAnalysis roadAnalysis, ObjectList trafficLights, ObjectList vehicles, int spawnNum) : base("Vehicles/car-1-red", roadAnalysis, trafficLights, vehicles, spawnNum)
        {
            brakeLight = Traffic.ContentManager.Load<Texture2D>("Vehicles/brakelight");
        }

        public override void Init()
        {
            base.Init();

            maxAcceleration = 0.03f;
            beginSpeed = (float)Traffic.Random.NextDouble() * 1.5f + 2; // startspeed of the vehicle without an direction
            maxAcceleration += (float)Traffic.Random.NextDouble() * 0.03f;
            maxBrakeDis = 20;
            carDistance = 60;
            carDistance += (float)Traffic.Random.NextDouble() * 15;
        }

        public override void InitTexture()
        {
            string carName = "", carColor = "", carLook = "";

            // car look
            switch (Traffic.Random.Next(0, 3))
            {
                case 0:
                    carLook = "1";
                    break;
                case 1:
                    carLook = "2";
                    break;
                case 2:
                    carLook = "3";
                    break;
            }
            // car color
            switch (Traffic.Random.Next(0, 4))
            {
                case 0:
                    carColor = "red";
                    break;
                case 1:
                    carColor = "green";
                    break;
                case 2:
                    carColor = "yellow";
                    break;
                case 3:
                    carColor = "blue";
                    break;
            }
            carName = "Vehicles/car-" + carLook + "-" + carColor;
            texture = Traffic.ContentManager.Load<Texture2D>(carName);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            // car is completely stopping it should diplay braking texture
            if (hardBrake)
            {
                Vector2 brakeLightPos = position;

                if (rotation == 0)
                    brakeLightPos += new Vector2(-texture.Width / 2 ,0);
                else if (rotation == (float)Math.PI)
                    brakeLightPos += new Vector2(texture.Width / 2, 0);
                else if (rotation == 0.5f * (float)Math.PI)
                    brakeLightPos += new Vector2(0, -texture.Width / 2);
                else if (rotation == 1.5f * (float)Math.PI)
                    brakeLightPos += new Vector2(0, texture.Width / 2);


                spriteBatch.Draw(brakeLight, brakeLightPos, null, null, new Vector2(brakeLight.Width / 2, brakeLight.Height / 2), rotation, null, Color.White);
            }

            base.Draw(spriteBatch);
        }
    }
}