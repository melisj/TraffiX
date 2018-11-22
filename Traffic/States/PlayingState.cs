using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System;
using System.Linq;

namespace Traffic
{
    class PlayingState : ObjectList
    {
        public ObjectList vehicles = new ObjectList();
        public ObjectList trafficLights = new ObjectList();
        public Cursor cursor;
        public RoadAnalysis roadAnalysis; // contains info about roads and trafficlights
        public RoadPlacement roadPlacement; // draws the road textures with the appropriate markings
        public Overlay overlay;

        public ObjectList popUp = new ObjectList();
        public bool popUpState = false;
        public ObjectList buttons = new ObjectList();
        public ObjectList turtorial = new ObjectList();
        public ObjectList animations = new ObjectList();

        SoundEffect winSound;
        SoundEffect loseSound;
        SoundEffect vehicleDeliverySound;

        KeyboardState keyBoardState;
        KeyboardState prevKeyBoardState;

        static public int levelNumber;
        // level info --------------------------  1   2   3   4   5   6   7   8   9   10   11   12  13  14  15  16  17  18  19  20   21   22   23   24   25   26   27   28   29   30
        static public int[] amountCarsLevel = {   5, 20, 30, 50, 40, 50, 50, 30, 50, 100,  80, 100, 60, 70, 70, 50, 80, 80, 70, 80, 200, 200, 200, 220, 150, 250, 250, 300, 400, 500};
        static public int[] amountTimeLevel = { 200, 40, 50, 80, 60, 70, 80, 60, 80, 110, 120, 100, 80, 80, 90, 60, 90, 75, 70, 80, 120, 120, 125, 140, 100, 180, 180, 180, 180, 240};
        static public int[] spawnRateLevel = {  200, 60, 60, 60, 60, 60, 60, 70, 70,  50,  60,  45, 45, 45, 45, 50, 45, 45, 45, 45,  30,  30,  30,  30,  30,  25,  25,  25,  20,  15};
        public int levelTimer = 0;
        public int arrivedCars = 0;

        public bool crashedState, winState, timeState, jammedState;
        public Text crashCounter;
        public float crashTimer = 180;
        public float crashCount = 0;

        float ratioTC = 0.2f; // Truck Car ratio
        float ratioCE = 0f; // Car Emergency Vehicle ratio
        float spawnTimer = 0; // timer for the spawning of the cars 
        int prevSpawnNum = 100;

        int spawnTrainCounter = 0;
        int spawnTrainTime = 1800;

        public PlayingState(String levelNum)
        {
            SwitchToLevel(levelNum);
            for (int i = 0; i < amountTimeLevel.Length; i++)
            {
                amountTimeLevel[i] *= 60;
            }

            winSound = Traffic.ContentManager.Load<SoundEffect>("Sounds/Win Sound");
            loseSound = Traffic.ContentManager.Load<SoundEffect>("Sounds/Fail Sound");
            vehicleDeliverySound = Traffic.ContentManager.Load<SoundEffect>("Sounds/Traffic Sound");
        }

        public override void InputHelper()
        {
            prevKeyBoardState = keyBoardState;
            keyBoardState = Keyboard.GetState();

            if (keyBoardState.IsKeyDown(Keys.Escape) && prevKeyBoardState.IsKeyUp(Keys.Escape) == keyBoardState.IsKeyDown(Keys.Escape) && !winState && !crashedState && !jammedState && !timeState)
            {
                popUpState = !popUpState;
            }
            base.InputHelper();
        }

        public override void Update(GameTime gameTime)
        {
            // spawns the popup else it wil place things where they belong and let timers work
            if (popUpState)
            {
                PopUp();
                if (!winState && !crashedState && !jammedState && !timeState)
                {
                    Traffic.pauzed = true;
                }
            }
            else
            {
                Traffic.pauzed = false;

                buttons.RemoveAll();
                popUp.RemoveAll();
                buttons = new ObjectList();
                popUp = new ObjectList();
                overlay.otherDisplayPos = overlay.otherDisplayStartPos;
                overlay.lvDisplayPos = overlay.lvDisplayStartPos;
                overlay.timeDisplayPos = overlay.timeDisplayStartPos;

                spawnTrainCounter++;
                spawnTimer++;
                levelTimer--;

                overlay.time = levelTimer / 60;

                foreach (Anim anim in animations.children)
                {
                    if (anim.timesPlayed >= 1)
                    {
                        animations.AddRemove(anim, "remove");
                        break;
                    }
                }
            }

            // checks if an turtorial needs to be drawn
            if (turtorial.children.Count == 0)
            {
                switch(levelNumber)
                {
                    case 1:
                        TurtorialLV1();
                        break;
                    case 11:
                        TurtorialLV11();
                        break;
                    case 21:
                        TurtorialLV21();
                        break;
                    case 26:
                        TurtorialLV26();
                        break;
                }
            }
            // checks if the time is up
            if (levelTimer < 0)
            {
                timeState = true;
                popUpState = true;
            }

            // checks if enough cars made it
            if(amountCarsLevel[levelNumber - 1] <= arrivedCars)
            {
                winState = true;
                Traffic.SafeGame();
            }

            // adds trains
            if (levelNumber >= 21)
            {
                if (spawnTrainCounter >= spawnTrainTime)
                {
                    vehicles.AddRemove(new Train(roadAnalysis, trafficLights, vehicles, Traffic.Random.Next(0, roadAnalysis.railroads.Count)), "add");

                    spawnTrainCounter = 0;
                }
            }

            // adds vehicles on the road
            if (spawnTimer >= spawnRateLevel[levelNumber - 1] && !crashedState && !popUpState)
            {
                int spawnNum = Traffic.Random.Next(0, roadAnalysis.roads.Count);
                while (prevSpawnNum == spawnNum)
                {
                    spawnNum = Traffic.Random.Next(0, roadAnalysis.roads.Count);
                }

                if (Traffic.Random.NextDouble() > ratioTC) {
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
            
            // removes vehicles if they are out of bounds
            foreach (Vehicle vehicle in vehicles.children)
            {
                if (vehicle.collision)
                {
                    crashedState = true;
                    foreach (TrafficLight trafficLight in trafficLights.children)
                    {
                        trafficLight.state = false;
                    }
                }

                if (vehicle.removeVehic)
                {
                    if (vehicle.soundEffectsInstances.Count != 0)
                        vehicle.soundEffectsInstances[0].Stop();
                    vehicles.AddRemove(vehicle, "remove");
                    break;
                }

                if (!popUpState)
                {

                    if ((vehicle.position.X < -vehicle.texture.Width / 2 && vehicle.velocity.X < 0) || 
                        (vehicle.position.X > Traffic.screenSize.X + vehicle.texture.Width / 2 && vehicle.velocity.X > 0) ||
                        (vehicle.position.Y < -vehicle.texture.Width / 2 && vehicle.velocity.Y < 0) || 
                        (vehicle.position.Y > Traffic.screenSize.Y + vehicle.texture.Width / 2 && vehicle.velocity.Y > 0)
                        )
                    {
                        arrivedCars++;
                        overlay.arrivedCars = arrivedCars;
                        Vector2 animationPos = Vector2.Zero;

                        if (vehicle.rotation == 0)
                            animationPos = new Vector2(Traffic.screenSize.X - 40, vehicle.position.Y - roadPlacement.asphaltTexture.Height / 2);
                        else if (vehicle.rotation == (float)Math.PI)
                            animationPos = new Vector2(40, vehicle.position.Y + roadPlacement.asphaltTexture.Height / 2);
                        else if (vehicle.rotation == 0.5f * (float)Math.PI)
                            animationPos = new Vector2(vehicle.position.X + roadPlacement.asphaltTexture.Height / 2, Traffic.screenSize.Y - 40);
                        else if (vehicle.rotation == 1.5f * (float)Math.PI)
                            animationPos = new Vector2(vehicle.position.X - roadPlacement.asphaltTexture.Height / 2, 40);

                        if (vehicle.soundEffectsInstances.Count != 0)
                            vehicle.soundEffectsInstances[0].Stop();

                        animations.AddRemove(new Anim("Vehicles/vehicledeliveranim", animationPos, vehicle.rotation, 8, 24), "add");
                        vehicles.AddRemove(vehicle, "remove");

                        SoundEffectInstance vehicleDeliverySoundInstance = vehicleDeliverySound.CreateInstance();
                        vehicleDeliverySoundInstance.Volume = 0.5f;
                        vehicleDeliverySoundInstance.Play();
                        
                        break;
                    }

                    if (vehicle.position.X < -vehicle.texture.Width / 2 ||
                        vehicle.position.X > Traffic.screenSize.X + vehicle.texture.Width / 2 ||
                        vehicle.position.Y < -vehicle.texture.Width / 2 ||
                        vehicle.position.Y > Traffic.screenSize.Y + vehicle.texture.Width / 2)
                    {
                        if (vehicle.velocity.Length() < 0.01f && !(vehicle is Train))
                        {
                            jammedState = true;
                            popUpState = true;
                        }
                    }
                }
            }

            // checks buttons on screen
            foreach (Button button in buttons.children)
            {
                if (button.clicked)
                {
                    switch (button.buttonID)
                    {
                        case "next":
                            SwitchToLevel((levelNumber + 1).ToString());
                            break;
                        case "restart":
                            SwitchToLevel(levelNumber.ToString());
                            break;
                        case "menu":
                            SwitchToLevel(levelNumber.ToString());
                            Traffic.SwitchState(0);
                            break;
                        case "quit":
                            Traffic.exit = true;
                            break;
                    }
                }
            }
            // checks if an car has crashed
            if (crashedState && !winState)
            {
                foreach (Vehicle vehicle in vehicles.children)
                {
                    if(!vehicle.collision)
                    vehicle.Brake(30, 0);
                }

                if (!popUpState)
                    crashCounter.text = Math.Ceiling(crashCount / 60).ToString();
                else
                    crashCounter.text = null;

                if (crashCount >= crashTimer)
                {
                    popUpState = true;
                }
                
                crashCount++;
            }
            // unlocks the next level
            if (winState)
            {
                popUpState = true;
                if(levelNumber != Traffic.amountLevels)
                    Traffic.lockedLevels[levelNumber] = false;
            }
            
            base.Update(gameTime);
        }

        // resets variables and adds new empty array's
        public void SwitchToLevel(String levelNum)
        {
            levelNumber = Int32.Parse(levelNum);
            String levelName = "Levels/level-" + levelNum;
            arrivedCars = 0;
            crashCount = 0;
            spawnTimer = 0;
            spawnTrainCounter = 0;
            levelTimer = amountTimeLevel[levelNumber - 1];
            winState = false;
            crashedState = false;
            timeState = false;
            jammedState = false;
            popUpState = false;

            if (levelNumber > 10) // above level 10 emergency vehicles will spawn
                ratioCE = 0.03f;
            else
                ratioCE = 0;

            foreach (Vehicle vehicle in vehicles.children)
            {
                if (vehicle.soundEffectsInstances.Count != 0)
                        vehicle.soundEffectsInstances[0].Stop();
            }
            MediaPlayer.Stop();
            MediaPlayer.Play(Traffic.backgroundMusic);

            RemoveAll();

            trafficLights = new ObjectList();
            vehicles = new ObjectList();
            popUp = new ObjectList();
            buttons = new ObjectList();
            turtorial = new ObjectList();

            AddRemove(new Sprite("Roads/grassbackground", Traffic.screenSize / 2, 0, true), "add");
            AddRemove(roadAnalysis = new RoadAnalysis(levelName), "add");
            AddRemove(trafficLights, "add");
            for (int i = 0; i < roadAnalysis.parentTLPoint.Count; i++)
            {
                trafficLights.AddRemove(new TrafficLight(roadAnalysis.parentTLPoint[i].ElementAt(0), roadAnalysis), "add");
            }
            AddRemove(roadPlacement = new RoadPlacement(roadAnalysis, trafficLights), "add");
            AddRemove(vehicles, "add");
            AddRemove(crashCounter = new Text(null, Traffic.screenSize / 2, Color.Red, "Fonts/font-50" ), "add");
            AddRemove(turtorial, "add");
            AddRemove(overlay = new Overlay(levelNum, amountCarsLevel[levelNumber - 1], amountTimeLevel[levelNumber - 1]), "add");
            AddRemove(animations, "add");
            AddRemove(cursor = new Cursor(), "add");

            if (roadAnalysis.railroads.Count != 0) 
                spawnTrainTime = 1800 / roadAnalysis.railroads.Count; // the more tracks there are the shorter the wait time will be
        }


        // popup info
        public void PopUp()
        {
            float stopPosY = Traffic.screenSize.Y / 2 - 400; // position where the popup texture needs to be
            float startPosY = -400; // position outside the screen
            float popVelocity = 0;
            float popSpeed = 20; // the higher the number the slower

            if (popUp.children.Count == 0) // if not added yet
            {
                Button backToMenu;
                Button quitButton;
                Button restartButton;
                
                buttons.AddRemove(restartButton = new Button("restart", new Vector2(Traffic.screenSize.X / 2, startPosY)), "add");
                buttons.AddRemove(backToMenu = new Button("menu", new Vector2(Traffic.screenSize.X / 2, startPosY + 75)), "add");
                buttons.AddRemove(quitButton = new Button("quit", new Vector2(Traffic.screenSize.X / 2, startPosY + 150)), "add");
                popUp.AddRemove(new Sprite("Overlay/popup", new Vector2(Traffic.screenSize.X / 2, startPosY), 0, true), "add");
                popUp.AddRemove(restartButton, "add");
                popUp.AddRemove(backToMenu, "add");
                popUp.AddRemove(quitButton, "add");

                if (winState) // winstate text and textures
                {
                    if (levelNumber == 30)
                    {
                        //popUp.AddRemove(new Anim("Overlay/reward", new Vector2(Traffic.screenSize.X / 2 - 80, startPosY - 330), 0, 7, 12), "add");
                        popUp.AddRemove(new Text("Thanks for playing!", new Vector2(Traffic.screenSize.X / 2, startPosY + 300), Color.LightGreen), "add");
                        popUp.AddRemove(new Text("Have A Reward!", new Vector2(Traffic.screenSize.X / 2, startPosY + 330), Color.LightGreen), "add");
                        popUp.AddRemove(new Text("Kinda?", new Vector2(Traffic.screenSize.X / 2, startPosY + 360), Color.LightGreen), "add");
                    }
                    else
                    {
                        Button nextLevel;
                        buttons.AddRemove(nextLevel = new Button("next", new Vector2(Traffic.screenSize.X / 2, startPosY - 100)), "add");
                        popUp.AddRemove(nextLevel, "add");
                        popUp.AddRemove(new Sprite("Overlay/completed", new Vector2(Traffic.screenSize.X / 2, startPosY - 300), 0, true), "add");
                        popUp.AddRemove(new Text("Cleared", new Vector2(Traffic.screenSize.X / 2, startPosY + 250), Color.White, "Fonts/font-50"), "add");
                    }
                    winSound.Play();
                }
                else if (crashedState) // crashedState text and textures
                {
                    popUp.AddRemove(new Sprite("Overlay/crashed", new Vector2(Traffic.screenSize.X / 2, startPosY - 250), 0, true), "add");
                    popUp.AddRemove(new Text("Crashed", new Vector2(Traffic.screenSize.X / 2, startPosY + 349), Color.DarkRed, "Fonts/font-50"), "add");
                }
                else if (jammedState) // jammedState text and textures
                {
                    popUp.AddRemove(new Sprite("Overlay/jammed", new Vector2(Traffic.screenSize.X / 2, startPosY - 250), 0, true), "add");
                    popUp.AddRemove(new Text("Jammed Up", new Vector2(Traffic.screenSize.X / 2, startPosY + 349), Color.DarkRed, "Fonts/font-50"), "add");
                }
                else if (timeState) // timeState text and textures
                {
                    popUp.AddRemove(new Sprite("Overlay/outoftime", new Vector2(Traffic.screenSize.X / 2, startPosY - 250), 0, true), "add");
                    popUp.AddRemove(new Text("Time's Up", new Vector2(Traffic.screenSize.X / 2, startPosY + 349), Color.DarkRed, "Fonts/font-50"), "add");
                }
                else // pauzed text
                {
                    popUp.AddRemove(new Text("Pauzed", new Vector2(Traffic.screenSize.X / 2, startPosY + 250), Color.White, "Fonts/font-50"), "add");
                }

                if(timeState || crashedState || jammedState)
                {
                    loseSound.Play();
                }

                AddRemove(popUp, "add");

                OrderCursor();
            }
            // every object in the popup menu gets an velocity depending on the distance between currentpos and stoppos
            foreach (GameObject gameObject in popUp.children)
            {
                if (popVelocity == 0)
                {
                    if (gameObject is Sprite)
                    {

                        gameObject.velocity = new Vector2(0, (stopPosY - gameObject.position.Y) / popSpeed);
                        popVelocity = gameObject.velocity.Y;
                    }
                }
                if(gameObject is Text)
                {
                    Text textGameObject = popUp.children.ElementAt(popUp.children.IndexOf(gameObject)) as Text;
                    textGameObject.originalPos.Y += popVelocity;
                }
                gameObject.velocity.Y = popVelocity;
            }
            // gets rid of overlay
            overlay.lvDisplayPos.Y += popVelocity * 2;
            overlay.otherDisplayPos.Y += popVelocity;
            overlay.timeDisplayPos.Y += popVelocity;
        }

        public void TurtorialLV1()
        {
            turtorial.AddRemove(new Text("Let's Start With Full Control", new Vector2(Traffic.screenSize.X / 1.6f, Traffic.screenSize.Y / 4), Color.White, "Fonts/font-50"), "add");
            turtorial.AddRemove(new Text("But don't let it back up", new Vector2(Traffic.screenSize.X / 1.6f, Traffic.screenSize.Y / 3.5f), Color.White), "add");
            turtorial.AddRemove(new Text("Deliver all the vehicles needed within the time limit", new Vector2(Traffic.screenSize.X / 1.3f, Traffic.screenSize.Y / 1.2f), Color.White), "add");
            turtorial.AddRemove(new Text("Amount of vehicles that need to leave the road", new Vector2(Traffic.screenSize.X / 9.9f, Traffic.screenSize.Y / 1.2f), Color.White), "add");
        }

        public void TurtorialLV11()
        {
            turtorial.AddRemove(new Text("Let's Start Simple Again", new Vector2(Traffic.screenSize.X / 1.35f, Traffic.screenSize.Y / 4), Color.White, "Fonts/font-50"), "add");
            turtorial.AddRemove(new Text("But this time emergency vehicles are a thing to watch out for", new Vector2(Traffic.screenSize.X / 1.35f, Traffic.screenSize.Y / 3.5f), Color.White), "add");
            turtorial.AddRemove(new Text("and they won't wait long for a traffic light", new Vector2(Traffic.screenSize.X / 1.35f, Traffic.screenSize.Y / 3.25f), Color.White), "add");
            vehicles.AddRemove(new EmergencyVehic(roadAnalysis, trafficLights, vehicles, roadAnalysis.roads.Count - 2), "add");
        }

        public void TurtorialLV21()
        {
            turtorial.AddRemove(new Text("It is time for some rushhour traffic", new Vector2(Traffic.screenSize.X / 1.5f, Traffic.screenSize.Y / 4), Color.White, "Fonts/font-50"), "add");
            turtorial.AddRemove(new Text("Oh, also, let's add some unstopable trains", new Vector2(Traffic.screenSize.X / 1.5f, Traffic.screenSize.Y / 3.5f), Color.White), "add");
        }

        public void TurtorialLV26()
        {
            turtorial.AddRemove(new Text("Let's Descend Into", new Vector2(Traffic.screenSize.X / 1.35f, Traffic.screenSize.Y / 4), Color.White, "Fonts/font-50"), "add");
            turtorial.AddRemove(new Text("MADNESS", new Vector2(Traffic.screenSize.X / 1.35f, Traffic.screenSize.Y / 3.3f), Color.White, "Fonts/font-50"), "add");
        }

        // puts cursor back in order, so it will be drawn ontop of everything
        public void OrderCursor()
        {
            foreach (GameObject gameObject in children)
            {
                if (gameObject is Cursor)
                {
                    AddRemove(gameObject, "remove");
                    break;
                }
            }
            AddRemove(cursor = new Cursor(), "add");
        }
    }
}
