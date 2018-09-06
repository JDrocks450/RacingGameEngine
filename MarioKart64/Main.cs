using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;

namespace KartEngine
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Main : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        #region STATIC VARIABLES
        public static UserInterface UILayer;
        public static ContentManager Manager;
        public static InputHelper GlobalInput;
        public static Texture2D BaseTexture2D;
        public static GameServiceContainer GameServices;
        public static Point CenterScreen;
        #endregion

        public Ground GameGround;
        public List<GameObject3D> Renders = new List<GameObject3D>();

        public GameCamera Camera;

        public Main()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            graphics.PreferredBackBufferWidth = 1024;
            graphics.PreferredBackBufferHeight = 768;
            Deactivated += FocusLost;
            IsMouseVisible = true;            
            Activated += FocusGained;
        }

        private void FocusGained(object sender, System.EventArgs e)
        {
            if (Camera != null)
            Camera.IsLockingMouse = true;
        }

        private void FocusLost(object sender, System.EventArgs e)
        {
            if (Camera != null)
                Camera.IsLockingMouse = false;
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            Camera = new GameCamera(graphics);
            GlobalInput = new InputHelper();
            Manager = Content;
            UILayer = new UserInterface(Manager, 
                new Point(GraphicsDevice.Viewport.Width,
                GraphicsDevice.Viewport.Height));
            BaseTexture2D = new Texture2D(GraphicsDevice, 1, 1);
            BaseTexture2D.SetData(new Color[] { Color.White });
            SetRasterizerSettings();        
            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            GameGround = new Ground(GraphicsDevice);
            GameGround.Load(Content);
            GameGround.Initialize();
            var track = new GameObject3D(Content.Load<Model>("track01_"));
            track.Initialize();
            Renders.Add(track);
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }
        int frame;
        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();                       
            CenterScreen = new Point(GraphicsDevice.Viewport.Width / 2, GraphicsDevice.Viewport.Height / 2);            
            if (frame != 0)
                Camera.Update(gameTime);                       
            frame++;
            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);            
            GameGround.Draw(Camera);
            foreach (var render in Renders)
                render.Draw(Camera);
            //User Interface spritebatch
            if (UILayer.Components.Count > 0)
            {
                spriteBatch.Begin();
                UILayer.Draw(spriteBatch);
                spriteBatch.End();
            }

            base.Draw(gameTime);
        }

        public void SetRasterizerSettings()
        {
            RasterizerState rs = new RasterizerState();
            rs.MultiSampleAntiAlias = true;
            GraphicsDevice.RasterizerState = rs;
        }
    }
}
