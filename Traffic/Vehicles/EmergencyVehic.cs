using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Traffic
{
    class EmergencyVehic : Car
    {
        Anim lightAnim;
        float lightAnimOffset = 0;
        Text timerText;

        // timer before vehicle drives through red lights
        int timer = 300; //frames

        SoundEffect siren;

        public EmergencyVehic(RoadAnalysis roadAnalysis, ObjectList trafficLights, ObjectList vehicles, int spawnNum) : base(roadAnalysis, trafficLights, vehicles, spawnNum)
        {
            siren = Traffic.ContentManager.Load<SoundEffect>("Sounds/Police Sound");
        }

        public override void Init()
        {
            base.Init();

            timerText = new Text(null, position, Color.Red);

            maxAcceleration = 0.04f;
            beginSpeed = 4f; // startspeed of the vehicle without an direction
            maxAcceleration += (float)Traffic.Random.NextDouble() * 0.03f;
            maxBrakeDis = 20;
            carDistance = 60;
            carDistance += (float)Traffic.Random.NextDouble() * 15;
        }

        public override void InitTexture()
        {
            string carName = "";

            // different texture light animation and offset with different sound files 
            switch (Traffic.Random.Next(0, 3))
            {
                case 0:
                    carName = "Vehicles/emergvehic-1";
                    lightAnim = new Anim("Vehicles/emergencylight-1", position, rotation, 2, 8);
                    lightAnimOffset = 5;
                    siren = Traffic.ContentManager.Load<SoundEffect>("Sounds/Police Sound");
                    break;
                case 1:
                    carName = "Vehicles/emergvehic-2";
                    lightAnim = new Anim("Vehicles/emergencylight-23", position, rotation, 2, 8);
                    lightAnimOffset = 3;
                    siren = Traffic.ContentManager.Load<SoundEffect>("Sounds/Ambulance Sound");
                    break;
                case 2:
                    carName = "Vehicles/emergvehic-3";
                    lightAnim = new Anim("Vehicles/emergencylight-23", position, rotation, 2, 8);
                    lightAnimOffset = 3;
                    siren = Traffic.ContentManager.Load<SoundEffect>("Sounds/Firetruck Sound");
                    break;
            }

            soundEffectsInstances.Add(siren.CreateInstance());
            soundEffectsInstances[0].Play();
            soundEffectsInstances[0].Volume = 0.8f;

            texture = Traffic.ContentManager.Load<Texture2D>(carName);
        }

        public override void Update(GameTime gameTime)
        {
            // position for the light animation
            if (rotation == 0)
                lightAnim.position = new Vector2(position.X - lightAnimOffset, position.Y - texture.Height);
            else if (rotation == (float)Math.PI)
                lightAnim.position = new Vector2( position.X + lightAnimOffset, position.Y + texture.Height);
            else if (rotation == 0.5f * (float)Math.PI)
                lightAnim.position = new Vector2(position.X + texture.Height,  position.Y - lightAnimOffset);
            else if (rotation == 1.5f * (float)Math.PI)
                lightAnim.position = new Vector2(position.X - texture.Height, position.Y + lightAnimOffset);

            lightAnim.rotation = rotation;
            

            lightAnim.Update(gameTime);
            timerText.Update(gameTime);

            timerText.position = position;

            base.Update(gameTime);
        }

        public override void CheckForLight()
        {
            
            timerText.text = null;

            if (timer >= 0)
            {
                base.CheckForLight();
            }

            // display 0 if timer = 0
            if(timer <= 0)
                timerText.text = "0";

            if (brakeForTL && !brakeForCar) // if braking for traffic light, count down
            {
                timer--;
                timerText.text = (timer / 60).ToString(); // update text
            }

        }

        public override void Draw(SpriteBatch spriteBatch)
        {

            base.Draw(spriteBatch);

            if (!collision)
                lightAnim.Draw(spriteBatch);

            timerText.Draw(spriteBatch);
        }
    }
}
