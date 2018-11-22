using Microsoft.Xna.Framework.Graphics;

namespace Traffic
{
    class Truck : Vehicle
    {
        public Truck(RoadAnalysis roadAnalysis, ObjectList trafficLights, ObjectList vehicles, int spawnNum) : base ("Vehicles/truck", roadAnalysis, trafficLights, vehicles, spawnNum)
        {
           
        }

        public override void Init()
        {
            base.Init();

            beginSpeed = 2.5f;
            maxBrakeDis = 50;
            carDistance = 90;
            maxAcceleration = 0.02f;
            carDistance += (float)Traffic.Random.NextDouble() * 15;
            InitTexture();
        }

        public override void InitTexture()
        {
            string truckName = "";

            switch (Traffic.Random.Next(0,5))
            {
                case 0:
                    truckName = "Vehicles/truck-1";
                    break;
                case 1:
                    truckName = "Vehicles/truck-2";
                    break;
                case 2:
                    truckName = "Vehicles/truck-3";
                    break;
                case 3:
                    truckName = "Vehicles/truck-4";
                    break;
                case 4:
                    truckName = "Vehicles/bus-1";
                    break;
            }

            
            texture = Traffic.ContentManager.Load<Texture2D>(truckName);

        }
    }
}
