using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Traffic
{
    class Train : Vehicle
    {
        SoundEffect crossingsound;
        Anim crossingAnim;

        // time before train arrives
        public int spawnTimer = 300; //frames

        public Train(RoadAnalysis roadAnalysis, ObjectList trafficLights, ObjectList vehicles, int SpawnNum) : base( "Vehicles/train", roadAnalysis, trafficLights, vehicles, SpawnNum)
        {
            // putting the animation in the right spot
            Vector2 animationPoint = roadAnalysis.railroads[SpawnNum];
            if (roadAnalysis.railDirection[SpawnNum].X == -1) {
                if (roadAnalysis.railDirection[SpawnNum].Y == -1)
                    animationPoint -= new Vector2(0, 50 + (roadAnalysis.railroads[SpawnNum].Y - Traffic.screenSize.Y));
                else
                    animationPoint -= new Vector2(75 + (roadAnalysis.railroads[SpawnNum].X - Traffic.screenSize.X), 0);
            }
            crossingAnim = new Anim("Overlay/trainsign", animationPoint, 0, 2, 8);
        }

        public override void Init()
        {
            base.Init();

            beginSpeed = 7;

            crossingsound = Traffic.ContentManager.Load<SoundEffect>("Sounds/railroad crossing");

            soundEffectsInstances.Add(crossingsound.CreateInstance());
            soundEffectsInstances[0].Play();
            soundEffectsInstances[0].Volume = 0.8f;
            soundEffectsInstances[0].IsLooped = true;
        }

        public override void Update(GameTime gameTime)
        {
            if (!Traffic.pauzed)
            {

                base.Update(gameTime);

                // if timer is busy let the train roll slowly closer to the screen
                if (spawnTimer <= 0)
                {
                    soundEffectsInstances[0].Stop();
                    velocity = speed;
                }
                else
                {
                    spawnTimer--;
                    velocity = speed / 100;
                    crossingAnim.Update(gameTime);
                }

                // stop train when collision occures
                if (collision)
                {
                    speed -= new Vector2((speed.X == 0) ? 0 : speed.X, (speed.Y == 0) ? 0 : speed.Y) / 100;
                }
            }

        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            // draw animation if timer is still active
            if (spawnTimer > 0)
                crossingAnim.Draw(spriteBatch);

            base.Draw(spriteBatch);
        }

        public override void CheckForCars() { }

        public override void CheckForLight() { }
    }
}
