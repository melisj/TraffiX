using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

/*
 * - this part of the code reads the bitmap that contains info about the road layout and the trafficlight within the level.
 * - this class collects all the color data in the image that is given and checks for certain colors.
 * - the colors that identify the different parts of the road are contstants that are defined in the class.
 * - if an color is detected of certain part it reads the coordinats and puts them in the appropriate list.
 */

namespace Traffic
{
    class RoadAnalysis : GameObject
    {
        private Texture2D bitmap;
        private Color[] mappedImage;
        public Vector2 mapSize;

        // colorvalues 
        private static Color roadColor = new Color(255, 0, 0);
        private static Color crossingColor = new Color(0, 0, 255);
        private static Color TLColor = new Color(0, 255, 0); // TL = TrafficLights
        private static Color childTLColor = new Color(255, 255, 0);
        private static Color railColor = new Color(0, 255, 255);

        public int roadDistance = 21; // distance between two roads if they are next to eachother

        public List<Vector2> roads = new List<Vector2>(); // locations of road spawn points
        public List<Vector2> roadDirection = new List<Vector2>();

        public List<Vector2> railroads = new List<Vector2>(); // locations of road spawn points
        public List<Vector2> railDirection = new List<Vector2>();

        public List<Vector2> crossingPoint = new List<Vector2>(); // locations of crossings

        // this list contains an list of all traffic lights with list of the parent and all its children roads
        public List<List<Vector2>> parentTLPoint = new List<List<Vector2>>(); 
        public List<Vector2> childTLPoints = new List<Vector2>();

        public RoadAnalysis(string levelName)
        {
            bitmap = Traffic.ContentManager.Load<Texture2D>(levelName);
            // all colors containd in the image are put in an array
            mappedImage = new Color[bitmap.Width * bitmap.Height];
            mapSize = new Vector2(bitmap.Width, bitmap.Height);

            Init();
        }

        public void Init()
        {
            bitmap.GetData(mappedImage);

            CheckRoads();
        }

        public void CheckRoads()
        {
            for (int i = 0; i < mappedImage.Length; i++)
            {
                Color pixelColor = mappedImage[i];

                // checks for child traffic light stops that belong to an parents TS
                // an child of an traffic light is an road that is also controlled by the same traffic light as the parent location
                // road has to be "roadDistance" close to be detected 
                if (pixelColor == childTLColor)
                {
                    Vector2 location;
                    location.Y = i / bitmap.Width;
                    location.X = i - location.Y * bitmap.Width;

                    childTLPoints.Add(location);
                }
            }

            for (int i = 0; i < mappedImage.Length; i++)
            {
                Color pixelColor = mappedImage[i];

                // checks for roads
                if (pixelColor == roadColor)
                {
                    // all roads begin on the first pixel within the screensize
                    // so on pixel 0 or pixel on the screensize - 1
                    int xLocation, yLocation, direction = 0, allignment = 0;
                    // getting the coordinates by checking the array of color
                    yLocation = i / bitmap.Width;
                    xLocation = i - yLocation * bitmap.Width;

                    if (xLocation == 0 || yLocation == 0)
                        direction = 1; // down & right 
                    else
                        direction = -1; // up & left

                    if (xLocation == 0 || xLocation == mapSize.X - 1)
                        allignment = 1; // horizontal
                    else
                        allignment = -1; // vertical

                    roadDirection.Add(new Vector2(direction, allignment));
                    roads.Add(new Vector2(xLocation, yLocation));
                }

                // checks for traffic lights
                if (pixelColor == TLColor)
                {
                    Vector2 location;
                    location.Y = i / bitmap.Width;
                    location.X = i - location.Y * bitmap.Width;

                    // list with all roads that are controlled by one traffic light
                    List<Vector2> TSPoints = new List<Vector2>();
                    TSPoints.Add(location);

                    foreach (Vector2 childTS in childTLPoints)
                    {
                        // checks distance
                        Vector2 vectorDis = location - childTS;
                        if (vectorDis.Length() % roadDistance == 0)
                        {
                            TSPoints.Add(childTS);
                        }
                    }
                    parentTLPoint.Add(TSPoints);
                }

                // checks for crossings
                if (pixelColor == crossingColor)
                {
                    int xLocation, yLocation;
                    yLocation = i / bitmap.Width;
                    xLocation = i - yLocation * bitmap.Width;

                    crossingPoint.Add(new Vector2(xLocation, yLocation));
                }

                // checks for roads
                if (pixelColor == railColor)
                {
                    int xLocation, yLocation, direction = 0, allignment = 0;
                    yLocation = i / bitmap.Width;
                    xLocation = i - yLocation * bitmap.Width;

                    if (xLocation == 0 || yLocation == 0)
                        direction = 1; // down & right 
                    else
                        direction = -1; // up & left

                    if (xLocation == 0 || xLocation == mapSize.X - 1)
                        allignment = 1; // vertical
                    else
                        allignment = -1; // horizontal

                    railDirection.Add(new Vector2(direction, allignment));
                    railroads.Add(new Vector2(xLocation, yLocation));
                }

            }
        }

        // the bitmap is drawable if needed for testing
        public override void Draw(SpriteBatch spriteBatch)
        {
            // \/ \/ \/ remove comment to view bitmap on level \/ \/ \/
            //spriteBatch.Draw(bitmap, Vector2.Zero, Color.White);
        }
    }
}
