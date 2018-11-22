using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System;
using System.Collections.Generic;
using System.IO;

namespace Traffic
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Traffic : Game
    {
        static public GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        protected static Random random;
        static protected ContentManager content;
        static public Vector2 screenSize;
        static private List<ObjectList> states = new List<ObjectList>();
        static private ObjectList currentState;
        static public int stateNumber;
        static public bool exit = false;
        static public int amountLevels = 30;
        static public bool[] lockedLevels = new bool[amountLevels];

        static public bool pauzed;

        static private PlayingState playingState;
        static private MenuState menuState;
        static private LevelSelectState levelSelect;
        static private SettingsMenu settings;

        static public Song backgroundMusic;

        public static Random Random
        {
            get { return random; }
        }

        public static ContentManager ContentManager
        {
            get { return content; }
        }

        public static bool FullScreen
        {
            get { return graphics.IsFullScreen; }
            set { graphics.IsFullScreen = value; }
        }

        public Traffic()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            content = Content;
            random = new Random();

            // resolutie verandering 
            screenSize = new Vector2(GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width, GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height);
            graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
            graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
            graphics.ApplyChanges();
        }

        protected override void Initialize()
        {
            
            var fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Content", "save.png");

            if (!File.Exists(fullPath))
            {
                byte[] bit = { 1 };

                File.WriteAllBytes(fullPath, bit);
            }

            // lock every level except 1
            for (int i = 0; i < lockedLevels.Length; i++)
            {
                lockedLevels[i] = true;
            }
            for (int i = 0; i < File.ReadAllBytes(fullPath)[0]; i++)
            {
                lockedLevels[i] = false;
            }
            
            MediaPlayer.Volume = 1f;
            MediaPlayer.IsRepeating = true;

            base.Initialize();            
        }

        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            states.Add(menuState = new MenuState());
            states.Add(playingState = new PlayingState("1"));
            states.Add(levelSelect = new LevelSelectState());
            states.Add(settings = new SettingsMenu());

            backgroundMusic = ContentManager.Load<Song>("Sounds/ertrii - Supremacy");
            MediaPlayer.Play(backgroundMusic);

            SwitchState(0);
        }

        protected override void UnloadContent()
        { }

        protected override void Update(GameTime gameTime)
        {
            if (exit)
                Exit();
                
            if (graphics.IsFullScreen && screenSize.Y == 1000)
            {
                screenSize = new Vector2(GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width, GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height);
                graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
                graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
                graphics.ApplyChanges();
            }
            else if (!graphics.IsFullScreen && screenSize.Y == 1080)
            {
                screenSize = new Vector2(1920, 1000);
                graphics.PreferredBackBufferWidth = 1920;
                graphics.PreferredBackBufferHeight = 1000;
                graphics.ApplyChanges();
            }

            currentState.Update(gameTime);
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.ForestGreen);

            spriteBatch.Begin();
            currentState.Draw(spriteBatch);
            spriteBatch.End();
        
            base.Draw(gameTime);
        }

        static public void SwitchState(int stateNum, String level = "1")
        {
            if (stateNum == 1)
            {
                stateNumber = stateNum;
                playingState.SwitchToLevel(level);
            }

            pauzed = false;
            currentState = states[stateNum];
        }

        static public void SafeGame()
        {
            int amountNotLocked = 0;

            for (int i = 0; i < lockedLevels.Length; i++)
            {
                if (!lockedLevels[i])
                    amountNotLocked++;
            }
            byte[] bit = { (byte)amountNotLocked };

            var fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Content", "save.png");

            File.WriteAllBytes(fullPath, bit);
        }
    }
}
