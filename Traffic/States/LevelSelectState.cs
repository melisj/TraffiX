using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Traffic
{
    class LevelSelectState : ObjectList
    {

        public Cursor cursor;
        public ObjectList lvButtons = new ObjectList();
        public Button backButton;
        public ObjectList lockedIcons = new ObjectList();
        public ObjectList texts = new ObjectList();
        public int levelsPerRow = 10;
        public float rowOffSet;

        public ObjectList trafficLights = new ObjectList();
        public RoadAnalysis roadAnalysis; // contains info about roads and trafficlights
        public RoadPlacement roadPlacement; // draws the road textures with the appropriate markings
        public ObjectList vehicles = new ObjectList();

        KeyboardState keyBoardState;
        KeyboardState prevKeyBoardState;

        float ratioTC = 0.2f; // Truck Car ratio
        float ratioCE = 0.02f; // Car Emergency Vehicle ratio
        float spawnTimer = 0; // timer for the spawning of the cars 
        int prevSpawnNum = 100;

        public LevelSelectState()
        {
            AddRemove(new Sprite("Roads/grassbackground", Traffic.screenSize / 2, 0, true), "add");
            AddRemove(roadAnalysis = new RoadAnalysis("Levels/levelselectbackground"), "add");
            AddRemove(roadPlacement = new RoadPlacement(roadAnalysis, trafficLights), "add");
            AddRemove(vehicles, "add");

            rowOffSet = (Traffic.screenSize.X / 2) - ((levelsPerRow * 100f) / 2) + 50;

            AddRemove(new Text("Level Select", new Vector2(Traffic.screenSize.X / 2, 100), Color.White,"Fonts/font-50"), "add");
            AddRemove(backButton = new Button("back", new Vector2(Traffic.screenSize.X / 2, Traffic.screenSize.Y - 100)), "add");
            AddRemove(lvButtons, "add");
            AddRemove(lockedIcons, "add");
            for (int i = 0; i < Traffic.amountLevels; i++)
            {
                lvButtons.AddRemove(new Button((i + 1).ToString(), new Vector2((i % levelsPerRow) * 100 + rowOffSet, (i / levelsPerRow) * 100 + 200), "Overlay/levelbutton"), "add");
                if (Traffic.lockedLevels[i])
                    lockedIcons.AddRemove(new Sprite("Overlay/lockedlevel", new Vector2((i % levelsPerRow) * 100 + rowOffSet, (i / levelsPerRow) * 100 + 200), 0, true), "add");
            }

            texts.AddRemove(new Text("rural roads", new Vector2(Traffic.screenSize.X / 6, 200), Color.LightGreen), "add");
            texts.AddRemove(new Text("city roads", new Vector2(Traffic.screenSize.X / 6, 300), Color.YellowGreen), "add");
            texts.AddRemove(new Text("rushhour", new Vector2(Traffic.screenSize.X / 6, 400), Color.PaleVioletRed), "add");

            AddRemove(texts, "add");
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
                        vehicles.AddRemove(new Car(roadAnalysis, trafficLights, vehicles, spawnNum), "add");
                    else
                        vehicles.AddRemove(new EmergencyVehic(roadAnalysis, trafficLights, vehicles, spawnNum), "add");
                }
                else
                    vehicles.AddRemove(new Truck(roadAnalysis, trafficLights, vehicles, spawnNum), "add");
                spawnTimer = 0;

                prevSpawnNum = spawnNum;
            }

            spawnTimer++;
            // removes vehicles if they are out of bounds
            foreach (Vehicle vehicle in vehicles.children)
            {
                if (vehicle.soundEffectsInstances.Count != 0)
                    vehicle.soundEffectsInstances[0].Stop();

                if ((vehicle.position.X < -vehicle.texture.Width / 2 && vehicle.velocity.X < 0) ||
                    (vehicle.position.X > Traffic.screenSize.X + vehicle.texture.Width / 2 && vehicle.velocity.X > 0) ||
                    (vehicle.position.Y < -vehicle.texture.Width / 2 && vehicle.velocity.Y < 0) ||
                    (vehicle.position.Y > Traffic.screenSize.Y + vehicle.texture.Width / 2 && vehicle.velocity.Y > 0)
                    )
                {
                    vehicles.AddRemove(vehicle, "remove");
                    break;
                }

                if (vehicle.removeVehic)
                {
                    vehicles.AddRemove(vehicle, "remove");
                    break;
                }
            }

            // each levelbutton connects to its own levelnumber
            foreach (Button lvButton in lvButtons.children)
            {
                if (lvButton.clicked && !Traffic.lockedLevels[Int32.Parse(lvButton.buttonID) - 1])
                {
                    Traffic.SwitchState(1, lvButton.buttonID);
                }
            }

            int amountLocked = 0;
            // checks amount of locked levels
            for (int i = 0; i < Traffic.amountLevels; i++)
            {
                if (Traffic.lockedLevels[i])
                {
                    amountLocked++;
                }
            }

            // checks if there is an differece between the amount locked and the amount images
            foreach (Sprite image in lockedIcons.children)
            {
                if (lockedIcons.children.Count != amountLocked)
                {
                    lockedIcons.AddRemove(image, "remove");
                    break;
                }

            }

            // back to menu
            if (backButton.clicked)
            {
                Traffic.SwitchState(0);
                vehicles.RemoveAll();
            }


                base.Update(gameTime);
        }


        public override void InputHelper()
        {
            prevKeyBoardState = keyBoardState;
            keyBoardState = Keyboard.GetState();

            // cheat code to unlock everything
            if (keyBoardState.IsKeyDown(Keys.G) && keyBoardState.IsKeyDown(Keys.O))
            {
                for (int i = 0; i < Traffic.lockedLevels.Length; i++)
                {
                    Traffic.lockedLevels[i] = false;
                }
            }
            base.InputHelper();
        }
    }
}
