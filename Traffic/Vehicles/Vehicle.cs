using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;
using System;
using System.Collections.Generic;

namespace Traffic
{
    class Vehicle : GameObject
    {
        protected float beginSpeed; // maxspeed without direction
        protected Vector2 speed; // maxspeed with an direction

        public Texture2D texture;
        public Vector2 size;
        public float rotation;
        protected Vector2 direction;

        protected RoadAnalysis roadAnalysis;
        protected ObjectList trafficLights;
        protected ObjectList otherVehicles;

        public Rectangle collisionBox;

        SoundEffect crashSound;

        public List<SoundEffectInstance> soundEffectsInstances = new List<SoundEffectInstance>();

        //List<Vector2[]> points = new List<Vector2[]>(); // for new collsion

        // vehicle settings
        protected float minBrakeDis = 5; // for TL's
        protected float maxBrakeDis; // relative to speed given
        protected float maxAcceleration; // vehicles wont accelerate in an instant
        protected float carDistance; // distance before braking for other vehicles

        protected bool hardBrake; // if true, vehicle is coming to an standstill
        protected bool braking; // if true, vehicle is braking

        protected bool brakeForTL = false;
        protected bool brakeForCar = false;

        public bool collision = false;
        public bool removeVehic = false; // if true vehicle needs to be removed because it is wrongly spawned

        public Vehicle(String assetName, RoadAnalysis roadAnalysis, ObjectList trafficLights, ObjectList vehicles, int SpawnNum)
        {
            texture = Traffic.ContentManager.Load<Texture2D>(assetName);
            crashSound = Traffic.ContentManager.Load<SoundEffect>("Sounds/Crash Sound");

            this.roadAnalysis = roadAnalysis;
            this.trafficLights = trafficLights;
            otherVehicles = vehicles;

            Init();
            // other info for different vehicles
            if(this is Train)
                Spawn(roadAnalysis.railroads, roadAnalysis.railDirection, SpawnNum);
            else
                Spawn(roadAnalysis.roads, roadAnalysis.roadDirection, SpawnNum);
        }

        public virtual void Init()
        {
            InitTexture();
        }

        public virtual void InitTexture() { }

        public override void Update(GameTime gameTime)
        {
            if (!Traffic.pauzed)
            {
                // if vehicle is not braking it accelerates to startspeed
                if (!collision)
                {
                    // normal behaviour
                    brakeForCar = false;
                    brakeForTL = false;
                    braking = false;
                    hardBrake = false;
                    CheckForLight();
                    CheckForCars();

                    position += velocity;
                    if (!braking)
                        Accelerate();
                }
                else
                {
                    // collision behaviour
                    velocity *= 0.9f;
                    position += velocity;

                    if (velocity.LengthSquared() >= 0.1f && !(this is Train)) // vehicles wont go and spin like crazy
                    {
                        // adjust rotation slowly
                        if (direction.Y == 1)
                        {
                                rotation += (float)Math.Atan(velocity.X / velocity.Y) * 0.025f;
                        }
                        if (direction.Y == -1)
                        {
                                rotation -= (float)Math.Atan(velocity.Y / velocity.X) * 0.025f;
                        }
                    }

                }

                // for each rotation there is an different position needed for the collision
                switch (direction.Y)
                {
                    case 1:
                        collisionBox = new Rectangle(new Point((int)(position.X - size.X / 2), (int)(position.Y - size.Y / 2)), new Point((int)size.X, (int)size.Y));
                        break;
                    case -1:
                        collisionBox = new Rectangle(new Point((int)(position.X - size.Y / 2), (int)(position.Y - size.X / 2)), new Point((int)size.Y, (int)size.X));
                        break;
                }
                DetectCollision();
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(texture, position, null, null, new Vector2(size.X / 2, size.Y / 2), rotation, null, Color.White);

            /* // for new collision
            if (points.Count != 0)
            {
                for (int i = 0; i < points.Count; i++)
                {
                    Vector2[] pointers = points[i];
                    for (int j = 0; j < pointers.Length; j++)
                    {
                        Rectangle rect1 = new Rectangle(new Point((int)pointers[j].X, (int)pointers[j].Y), new Point(2, 2));
                        spriteBatch.Draw(texture, rect1, Color.Black);
                    }
                }
            }
            */

            // \/ \/ \/ remove comment to draw collision rectangle \/ \/ \/
            //spriteBatch.Draw(texture, collisionBox, Color.Black);
        }

        public virtual void Spawn(List<Vector2> spawnPoints, List<Vector2> directions, int spawnNum)
        {
            // gives car an random road to drive on
            position = spawnPoints[spawnNum];
            float spawnOffset = 200;
            if (this is Train)
                spawnOffset = texture.Width;

            // gives car position offset outside the screen and gives rotation
            if (position.X == 0 || position.X == roadAnalysis.mapSize.X - 1) // left & right
            {
                velocity.X = beginSpeed * directions[spawnNum].X;
                if (position.X == 0) // right
                    position.X -= spawnOffset;
                else // left
                {
                    position.X += spawnOffset;
                    rotation = (float)Math.PI;
                }
            }
            else if (position.Y == 0 || position.Y == roadAnalysis.mapSize.Y - 1) // up & down
            {
                velocity.Y = beginSpeed * directions[spawnNum].X;
                if (position.Y == 0) // down
                {
                    position.Y -= spawnOffset;
                    rotation = 0.5f * (float)Math.PI;
                }
                else // up
                {
                    position.Y += spawnOffset;
                    rotation = 1.5f * (float)Math.PI;
                }
            }

            speed = velocity; // gives the speed an direction based on calculations above
            
            direction = directions[spawnNum];

            // adding randomized speeds to braking zones
            maxBrakeDis *= beginSpeed;

            size = new Vector2(texture.Width, texture.Height); // size of texture
        }

        public virtual void CheckForLight()
        {
            foreach (TrafficLight trafficLight in trafficLights.children)
            {
                // gets index of the light that is contained in an other list with parent and children
                int i = trafficLights.children.IndexOf(trafficLight);
                List<Vector2> TLLocation = roadAnalysis.parentTLPoint[i]; // list of parent and children

                // if the location of the car is gonna be closer the next frame it needs to brake
                foreach (Vector2 childLocation in TLLocation) {
                    if (childLocation.X == position.X || childLocation.Y == position.Y)
                    {
                        float distance = DistanceBetweenVector(position, childLocation);
                        float nextDistance = DistanceBetweenVector((position + speed), childLocation);

                        if (nextDistance > distance)
                            continue; // next light

                        // checks if car is in the braking range of the trafficlight and the parent light (trafficlight class) also needs to be red
                        if (distance <= maxBrakeDis && distance >= minBrakeDis && !trafficLight.state)
                        {
                            Brake(distance, 0);
                            brakeForTL = true;
                        }
                    }
                }
            }
        }

        public virtual void CheckForCars()
        {
            float oldCarDistance = carDistance;
            foreach (Vehicle otherVehicle in otherVehicles.children)
            {
                carDistance = oldCarDistance;

                if (otherVehicle != this)
                {
                    // checks if position of the cars is on the same line
                    if ((position.X == otherVehicle.position.X) || (position.Y == otherVehicle.position.Y))
                    {
                        float distance = DistanceBetweenVector(position, otherVehicle.position);

                        // checks if other car is in front or not
                        if (direction.Y == 1) // right & left
                        {
                            if (position.X * direction.X - otherVehicle.position.X * direction.X >= 0)
                                continue;
                        }
                        else if (direction.Y == -1) // up & down
                        {
                            if (position.Y * direction.X - otherVehicle.position.Y * direction.X >= 0)
                                continue;
                        }
                        carDistance += (otherVehicle.size.X + size.X / 2);

                        // checks if car is in the braking distance
                        if (distance <= carDistance && distance >= 0)
                        {
                            Brake(distance, (direction.Y == 1) ? otherVehicle.velocity.X * direction.X : otherVehicle.velocity.Y * direction.X);
                            brakeForCar = true;
                        }
                    }
                }
            }

        }

        // braking calculation requires an distance between the object and the car, it also needs the speed of the other object.
        public void Brake(float distance, float otherSpeed)
        {
            if(otherSpeed <= 0.5f)
            {
                hardBrake = true;
            }
            
            braking = true;

            // deceleration is calculated 
            Vector2 deceleration = speed / (distance / (velocity.Length() - otherSpeed));

            velocity -= deceleration;
        }

        public void Accelerate()
        {
            // if the velocity doesnt exceed the velocity it acclerates
            if (velocity.Length() < beginSpeed)
            {
                if (speed.X > 0)
                    velocity.X += maxAcceleration;
                else if (speed.X < 0)
                    velocity.X -= maxAcceleration;
                if (speed.Y > 0)
                    velocity.Y += maxAcceleration;
                else if (speed.Y < 0)
                    velocity.Y -= maxAcceleration;
            }
        }

        // checks distance bewtween two vectors
        public float DistanceBetweenVector(Vector2 thisPos, Vector2 otherPos)
        {
            Vector2 vectorDis = otherPos - thisPos;
            return vectorDis.Length();
        }

        public virtual void DetectCollision()
        {
            foreach (Vehicle otherVehicle in otherVehicles.children)
            {
                if(otherVehicle == this)
                    continue;

                float distance = DistanceBetweenVector(position, otherVehicle.position);

                // if vehicle is spawned too close it will be remove outside the screen
                if (otherVehicle.rotation == rotation && (position.X == otherVehicle.position.X || position.Y == otherVehicle.position.Y) && !(this is Train))
                {
                    if ((position.X < -texture.Width / 2 - 50 ||
                        position.X > Traffic.screenSize.X + texture.Width / 2 + 50 ||
                        position.Y < -texture.Width / 2 - 50||
                        position.Y > Traffic.screenSize.Y + texture.Width / 2 + 50)
                        && distance < otherVehicle.texture.Width * 1.1f + texture.Width * 1.1f)
                    {
                        removeVehic = true;
                        break;
                    }
                }
                
                if (distance < size.X + otherVehicle.size.X && otherVehicle.rotation != rotation)
                {
                    Vector2 originPoint = position;
                    Vector2 otherOriginPoint = otherVehicle.position;
                    // positions of the corners of the other vehicle
                    Vector2[] cornerPoints = {new Vector2(collisionBox.X, collisionBox.Y),
                        new Vector2(collisionBox.X + collisionBox.Width, collisionBox.Y),
                        new Vector2(collisionBox.X, collisionBox.Y + collisionBox.Height),
                        new Vector2(collisionBox.X + collisionBox.Width, collisionBox.Y + collisionBox.Height)};

                    

                    for (int i = 0; i < cornerPoints.Length; i++)
                    {
                        /*
                        
                        // rotate the other vehicle so that every point has an position relative to the rotation of the vehicle
                        cornerPoints[i] -= originPoint;

                        float yPos = cornerPoints[i].Length() * (float)Math.Sin(rotation); // sinus rule
                        float xPos = (float)Math.Sqrt(cornerPoints[i].LengthSquared() - Math.Pow(yPos, 2D)); // pythagoras

                        cornerPoints[i] = new Vector2(xPos, yPos); // put the origin position back in
                        cornerPoints[i] += originPoint;

                        */
                        
                        // rotate the other vehicle so that every point is straight relative to this vehicle
                        // this makes it easier because now it can use the box collision of this vehicle

                        /*

                        cornerPoints[i] -= originPoint;
                        float relatYPos = cornerPoints[i].Length() * (float)Math.Sin(otherVehicle.rotation);
                        float relatXPos = (float)Math.Sqrt(cornerPoints[i].LengthSquared() - Math.Pow(relatYPos, 2D));

                        cornerPoints[i] = new Vector2(relatXPos, relatYPos) + originPoint;

                        */

                        // check every point for collision
                        if (otherVehicle.collisionBox.X <= cornerPoints[i].X &&
                        otherVehicle.collisionBox.Y <= cornerPoints[i].Y &&
                        otherVehicle.collisionBox.X + otherVehicle.collisionBox.Width >= cornerPoints[i].X &&
                        otherVehicle.collisionBox.Y + otherVehicle.collisionBox.Height >= cornerPoints[i].Y)
                        {
                            // if speed is not yet added to each other, do it now!
                            if (!collision)
                            {
                                // train doesn't move when hit by vehicles
                                if(this is Train && otherVehicle is Train || !(this is Train) && !(otherVehicle is Train))
                                {
                                    Vector2 oldVelocity = velocity;
                                    velocity += otherVehicle.velocity;
                                    otherVehicle.velocity += oldVelocity;
                                }
                                else if (this is Train)
                                    otherVehicle.velocity += velocity;
                                else if (otherVehicle is Train)
                                    velocity += otherVehicle.velocity;

                                crashSound.Play();
                            }

                            collision = true;
                            otherVehicle.collision = true;
                            break;
                        }
                       

                    }

                    /*
                    points.Add(cornerPoints);
                    */

                    /* old collision
                    if (collisionBox.X < otherVehicle.collisionBox.X + otherVehicle.collisionBox.Width &&
                        collisionBox.Y < otherVehicle.collisionBox.Y + otherVehicle.collisionBox.Height &&
                        collisionBox.X + collisionBox.Width > otherVehicle.collisionBox.X &&
                        collisionBox.Y + collisionBox.Height > otherVehicle.collisionBox.Y)
                    {
                        collision = true;
                        Collide(otherVehicle);
                        break;
                    }*/
                }

            }
        }

    }
}
