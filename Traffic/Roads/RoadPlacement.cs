using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace Traffic
{
    class RoadPlacement : GameObject
    {

        private Texture2D railroadTexture; // textures come 100 pixel in width
        public Texture2D asphaltTexture; 
        private Texture2D asphaltSideTexture;
        private Texture2D roadMarkingSide; 
        private Texture2D roadMarkingMid;
        private Texture2D roadMarkingMidOpposit;
        private Texture2D crossingTexture; // not here
        private Texture2D stopMarkTexture; // not here

        private RoadAnalysis roadAnalysis; // info about roads and TL's
        private ObjectList trafficLights;

        private List<Vector2> roadMarkingPos = new List<Vector2>(); // road markings
        private List<float> roadMarkingDirection = new List<float>();
        private List<float> roadMarkingRot = new List<float>();
        private List<Texture2D> markingTexture = new List<Texture2D>();

        private List<Vector2> stopMarkPos = new List<Vector2>(); // stop markings
        private List<float> stopMarkRot = new List<float>();
        private List<bool> stopMarkState = new List<bool>();

        private List<Vector2> roadPartPos = new List<Vector2>(); // road parts
        private List<float> roadPartRot = new List<float>();

        private List<Vector2> railPartPos = new List<Vector2>(); // rail parts
        private List<float> railPartRot = new List<float>();

        public RoadPlacement(RoadAnalysis roadAnalysis, ObjectList trafficLights)
        {

            railroadTexture = Traffic.ContentManager.Load<Texture2D>("Roads/railroad");
            asphaltTexture = Traffic.ContentManager.Load<Texture2D>("Roads/asphalt");
            asphaltSideTexture = Traffic.ContentManager.Load<Texture2D>("Roads/asphaltside");
            roadMarkingSide = Traffic.ContentManager.Load<Texture2D>("Roads/whiteLaneSide");
            roadMarkingMid = Traffic.ContentManager.Load<Texture2D>("Roads/whiteLaneMid");
            roadMarkingMidOpposit = Traffic.ContentManager.Load<Texture2D>("Roads/whiteLaneMidOpposit");
            crossingTexture = Traffic.ContentManager.Load<Texture2D>("Roads/crossing");
            stopMarkTexture = Traffic.ContentManager.Load<Texture2D>("Roads/stopmark");
            this.roadAnalysis = roadAnalysis;
            this.trafficLights = trafficLights;
            Init();
        }

        public void Init()
        {
            SpawnRoad();
            SpawnMarkings();
            SpawnRailRoad();
            SpawnStopMarks();
        }


        public void SpawnRoad()
        {
            // collects data from every road
            for (int iRoad = 0; iRoad < roadAnalysis.roads.Count; iRoad++)
            {
                Vector2 startlocation = roadAnalysis.roads[iRoad];
                Vector2 roadDirection = roadAnalysis.roadDirection[iRoad];
                // length of the road is being checked
                float roadLength;
                if (roadDirection.Y == 1)
                    roadLength = Traffic.screenSize.X;
                else
                    roadLength = Traffic.screenSize.Y;
                // checks amount of parts needed to fill road
                int amountParts = (int)Math.Ceiling(roadLength / asphaltTexture.Width) + 1;

                // gives every part of the road an piece and gives an appropriate location and rotation
                for (int iPart = 0; iPart < amountParts; iPart++)
                {
                    if (roadDirection.Y == 1)
                    {
                        roadPartRot.Add(0);
                        roadPartPos.Add(new Vector2(iPart * asphaltTexture.Width, startlocation.Y));
                    }
                    else if (roadDirection.Y == -1)
                    {
                        roadPartRot.Add(0.5f * (float)Math.PI);
                        roadPartPos.Add(new Vector2(startlocation.X, iPart * asphaltTexture.Width));
                    }
                    if (roadDirection.X == 1)
                        roadMarkingDirection.Add(1);
                    else
                        roadMarkingDirection.Add(-1);
                }
            }
        }

        public void SpawnMarkings()
        {
            // checks every part of the road
            for (int iPart = 0; iPart < roadPartPos.Count; iPart++)
            {
                // add by default two markings for each part of the road
                markingTexture.Add(roadMarkingSide);
                markingTexture.Add(roadMarkingSide);

                // checks distance between other road parts
                for (int iOtherP = 0; iOtherP < roadPartPos.Count; iOtherP++)
                {
                    if (iOtherP != iPart && roadPartRot[iPart] == roadPartRot[iOtherP])
                    {
                        // if any of the roadparts are next to eachother the marking gets changed on the correct side
                        float distanceRoadX = roadPartPos[iPart].X - roadPartPos[iOtherP].X;
                        float distanceRoadY = roadPartPos[iPart].Y - roadPartPos[iOtherP].Y;

                        if (distanceRoadY == roadAnalysis.roadDistance || distanceRoadX == -roadAnalysis.roadDistance)
                        {
                            if(roadMarkingDirection[iPart] - roadMarkingDirection[iOtherP] == 0)
                                markingTexture[iPart * 2] = roadMarkingMid;
                            else
                                markingTexture[iPart * 2] = roadMarkingMidOpposit;
                        }
                        else if (distanceRoadY == -roadAnalysis.roadDistance || distanceRoadX == roadAnalysis.roadDistance)
                        {
                            if (roadMarkingDirection[iPart] - roadMarkingDirection[iOtherP] == 0)
                                markingTexture[iPart * 2 + 1] = roadMarkingMid;
                            else
                                markingTexture[iPart * 2 + 1] = roadMarkingMidOpposit;
                        }
                    }
                }
                // position and rotation is the same as the road on the first marking
                roadMarkingPos.Add(roadPartPos[iPart]);
                roadMarkingRot.Add(roadPartRot[iPart]);

                // second marking has an offset depending on rotation of the roadpart
                if (roadPartRot[iPart] == 0)
                    roadMarkingPos.Add(roadPartPos[iPart] + new Vector2(0, asphaltTexture.Height - markingTexture[iPart * 2 + 1].Height));
                else
                    roadMarkingPos.Add(roadPartPos[iPart] + new Vector2(-asphaltTexture.Height + markingTexture[iPart * 2 + 1].Height, 0));
                roadMarkingRot.Add(roadPartRot[iPart]);
            }
        }

        public void SpawnStopMarks()
        {
            foreach (TrafficLight trafficLight in trafficLights.children)
            {
                int i = trafficLights.children.IndexOf(trafficLight);
                List<Vector2> TLLocation = roadAnalysis.parentTLPoint[i];

                foreach (Vector2 Tl in TLLocation)
                {
                    stopMarkPos.Add(Tl);

                    stopMarkRot.Add((trafficLight.rotation.Y == 1) ? 0.5f * (float)Math.PI : 0);
                    stopMarkState.Add(trafficLight.state);
                }
            }
        }

        public void SpawnRailRoad()
        {
            // collects data from every railroad
            for (int iRail = 0; iRail < roadAnalysis.railroads.Count; iRail++)
            {
                Vector2 startlocation = roadAnalysis.railroads[iRail];
                Vector2 railDirection = roadAnalysis.railDirection[iRail];

                // length of the track is being checked
                float railLength;
                if (railDirection.Y == 1)
                    railLength = Traffic.screenSize.X;
                else
                    railLength = Traffic.screenSize.Y;
                // checks amount of parts needed to fill track
                int amountParts = (int)Math.Ceiling(railLength / railroadTexture.Width) + 1;

                // gives every part of the track an piece and gives an appropriate location and rotation
                for (int iPart = 0; iPart < amountParts; iPart++)
                {
                    if (railDirection.Y == 1)
                    {
                        railPartRot.Add(0);
                        railPartPos.Add(new Vector2(iPart * railroadTexture.Width, startlocation.Y));
                    }
                    else if (railDirection.Y == -1)
                    {
                        railPartRot.Add(0.5f * (float)Math.PI);
                        railPartPos.Add(new Vector2(startlocation.X, iPart * railroadTexture.Width));
                    }
                }
            }
        }

        public override void Update(GameTime gameTime)
        {
            // changes color of the stopmark next to the trafficlight
            int TLNumber = 0;
            foreach (TrafficLight trafficLight in trafficLights.children)
            {
                int i = trafficLights.children.IndexOf(trafficLight);
                List<Vector2> TLLocation = roadAnalysis.parentTLPoint[i];

                for (int iStop = 0; iStop < TLLocation.Count; iStop++)
                {
                    stopMarkState[TLNumber] = trafficLight.state;
                    TLNumber++;
                }
            }
                    base.Update(gameTime);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            // drawing all the road textures
            for (int iRoad = 0; iRoad < roadPartPos.Count; iRoad++)
            {
                spriteBatch.Draw(asphaltTexture, roadPartPos[iRoad], null, null, new Vector2(asphaltTexture.Width / 2, asphaltTexture.Height / 2), roadPartRot[iRoad]);
                spriteBatch.Draw(asphaltSideTexture, roadPartPos[iRoad], null, null, new Vector2(asphaltSideTexture.Width / 2, asphaltSideTexture.Height / 2), roadPartRot[iRoad]);
            }
            // drawing all road markings
            for (int iMark = 0; iMark < roadMarkingPos.Count; iMark++)
            {
                spriteBatch.Draw(markingTexture[iMark], roadMarkingPos[iMark], null, null, new Vector2(asphaltTexture.Width / 2, asphaltTexture.Height / 2), roadMarkingRot[iMark]);
            }

            // drawing all the rail textures
            for (int iRail = 0; iRail < railPartPos.Count; iRail++)
            {
                spriteBatch.Draw(railroadTexture, railPartPos[iRail], null, null, new Vector2(railroadTexture.Width / 2, railroadTexture.Height / 2), railPartRot[iRail]);
            }

            // drawning all crossing textures
            for (int iCross = 0; iCross < roadAnalysis.crossingPoint.Count; iCross++)
            {
                spriteBatch.Draw(crossingTexture, roadAnalysis.crossingPoint[iCross], null, null, new Vector2(crossingTexture.Width / 2, crossingTexture.Height / 2));
            }
            for (int iStop = 0; iStop < stopMarkPos.Count; iStop++)
            {
                Color stopColor;
                if (stopMarkState[iStop])
                    stopColor = Color.ForestGreen;
                else
                    stopColor = Color.Red;
                spriteBatch.Draw(stopMarkTexture, stopMarkPos[iStop], null, null, new Vector2(stopMarkTexture.Width / 2, stopMarkTexture.Height / 2), stopMarkRot[iStop], null, stopColor);
            }

            base.Draw(spriteBatch);
        }
    }
}
