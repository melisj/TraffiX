using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Traffic
{
    class MenuState : ObjectList
    {
        private ObjectList buttons = new ObjectList();
        private Cursor cursor;

        public RoadAnalysis roadAnalysis; // contains info about roads and trafficlights
        public RoadPlacement roadPlacement; // draws the road textures with the appropriate markings
        public ObjectList vehicles = new ObjectList();

        float ratioTC = 0.2f; // Truck Car ratio
        float ratioCE = 0.02f; // Car Emergency Vehicle ratio
        float spawnTimer = 0; // timer for the spawning of the cars 
        int prevSpawnNum = 100;

        public MenuState()
        {
            AddRemove(new Sprite("Roads/grassbackground", Traffic.screenSize / 2, 0, true), "add");
            AddRemove(roadAnalysis = new RoadAnalysis("Levels/menubackground"), "add");
            AddRemove(roadPlacement = new RoadPlacement(roadAnalysis, new ObjectList()), "add");
            AddRemove(vehicles, "add");

            buttons.AddRemove(new Button("play", new Vector2(Traffic.screenSize.X / 2, Traffic.screenSize.Y / 2)), "add");
            buttons.AddRemove(new Button("levels", new Vector2(Traffic.screenSize.X / 2, Traffic.screenSize.Y / 2 + 100)), "add");
            buttons.AddRemove(new Button("quit", new Vector2(Traffic.screenSize.X / 2, Traffic.screenSize.Y / 2 + 300)), "add");
            buttons.AddRemove(new Button("settings", new Vector2(Traffic.screenSize.X / 2, Traffic.screenSize.Y / 2 + 200)), "add");
            AddRemove(buttons, "add");
            AddRemove(new Sprite("Overlay/logo", new Vector2(Traffic.screenSize.X / 2, 200), 0, true), "add");
            AddRemove(cursor = new Cursor(), "add");
        }

        public override void Update(GameTime gameTime)
        {
            // adds vehicles on the road
            if (spawnTimer >= 5)
            {
                int spawnNum = Traffic.Random.Next(0, roadAnalysis.roads.Count);
                while (prevSpawnNum == spawnNum)
                {
                    spawnNum = Traffic.Random.Next(0, roadAnalysis.roads.Count);
                }

                if (Traffic.Random.NextDouble() > ratioTC)
                {
                    if (Traffic.Random.NextDouble() > ratioCE)
                        vehicles.AddRemove(new Car(roadAnalysis, new ObjectList(), vehicles, spawnNum), "add");
                    else
                        vehicles.AddRemove(new EmergencyVehic(roadAnalysis, new ObjectList(), vehicles, spawnNum), "add");
                }
                else
                    vehicles.AddRemove(new Truck(roadAnalysis, new ObjectList(), vehicles, spawnNum), "add");
                spawnTimer = 0;

                prevSpawnNum = spawnNum;
            }

            spawnTimer++;
            // removes vehicles if they are out of bounds
            foreach (Vehicle vehicle in vehicles.children)
            {
                    if ((vehicle.position.X < -vehicle.texture.Width / 2 && vehicle.velocity.X < 0) ||
                        (vehicle.position.X > Traffic.screenSize.X + vehicle.texture.Width / 2 && vehicle.velocity.X > 0) ||
                        (vehicle.position.Y < -vehicle.texture.Width / 2 && vehicle.velocity.Y < 0) ||
                        (vehicle.position.Y > Traffic.screenSize.Y + vehicle.texture.Width / 2 && vehicle.velocity.Y > 0))
                    {
                        vehicles.AddRemove(vehicle, "remove");
                        break;
                    }

                if (vehicle.soundEffectsInstances.Count != 0)
                    vehicle.soundEffectsInstances[0].Stop();

                if (vehicle.removeVehic)
                {
                    vehicles.AddRemove(vehicle, "remove");
                    break;
                }
            }


            // what to do with each button pressed
            foreach (Button button in buttons.children)
            {
                if (button.clicked)
                {
                    switch (button.buttonID)
                    {
                        case "play":
                            Traffic.SwitchState(1, "1");
                            vehicles.RemoveAll();
                            break;
                        case "levels":
                            Traffic.SwitchState(2);
                            vehicles.RemoveAll();
                            break;
                        case "settings":
                            Traffic.SwitchState(3);
                            break;
                        case "quit":
                            Traffic.exit = true;
                            break;
                    }
                }
            }
            base.Update(gameTime);
        }
    }
}
