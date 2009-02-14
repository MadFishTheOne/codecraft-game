using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;
using CoreNamespace;
using MiniGameInterfaces;

namespace MiniGame
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class MiniGame : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        Core core;

        bool playingNow; //is game in progress

        List<IAI> plugins; //available AI plugins
        List<IAI> players; //currently selected players

        public MiniGame()
        {
            Content.RootDirectory = "Content";
            graphics = new GraphicsDeviceManager(this);
#if DEBUG
            graphics.IsFullScreen = false;
            graphics.PreferredBackBufferWidth = 640;
            graphics.PreferredBackBufferHeight = 480;
#else
            graphics.IsFullScreen = true;
            graphics.PreferredBackBufferWidth = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width;
            graphics.PreferredBackBufferHeight = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height;
#endif
            graphics.SynchronizeWithVerticalRetrace = false;

            IsFixedTimeStep = false;

            LoadPlugins();

            players = new List<IAI>();
            //Here you have to create a list of active players. Somehow :)
            if (plugins.Count == 0)
                throw new Exception("EPIC FAIL! No plugins found");
            else
            {
/*                foreach (IAI plugin in plugins)
                {
                    players.Add(plugin);
                }*/
                players.Add(Activator.CreateInstance(plugins[0].GetType()) as IAI);
                players.Add(Activator.CreateInstance(plugins[0].GetType()) as IAI);
            }
            core = new Core(graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight, Content, graphics);
            playingNow = true;
            core.Reset(players);
        }

        private void LoadPlugins()
        {
            plugins = new List<IAI>();
            DirectoryInfo di = new DirectoryInfo(Environment.CurrentDirectory);
            foreach (FileInfo f in di.GetFiles("*.dll"))
            {
                
                Assembly a = Assembly.LoadFile(f.FullName);
                foreach (Type t in a.GetTypes())
                {
                    if (t.GetInterface("IAI") != null)
                    {
                        IAI obj = Activator.CreateInstance(t) as IAI;
                        plugins.Add(obj);
                        
                        //System.Windows.Forms.MessageBox.Show("Plugin loaded! "+obj.Author+" "+obj.Description);
                    }
                }
            }
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            Core.viewer.LoadContent();
            // TODO: use this.Content to load your game content here
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        KeyboardState oldState, newState;

        /// <summary>
        /// determines whether specified key is released
        /// </summary>
        bool IsKeyReleased(Keys key)
        {
            return oldState.IsKeyDown(key) && newState.IsKeyUp(key);
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            newState = Keyboard.GetState();

            if (IsKeyReleased(Keys.Q))
                System.Diagnostics.Debugger.Break(); //entering debug mode
            if (playingNow)
            {
                if (IsKeyReleased(Keys.Escape))
                    playingNow = false;
                if (IsKeyReleased(Keys.PageUp))
                    Core.Timing.TimeSpeed *= 2;
                if (IsKeyReleased(Keys.PageDown))
                    Core.Timing.TimeSpeed /= 2;
                if (IsKeyReleased(Keys.Pause))
                    Core.Timing.Paused = !Core.Timing.Paused;
            }
            else
            {
                if (IsKeyReleased(Keys.Escape))
                    this.Exit();
            }
            if (IsKeyReleased(Keys.Enter))
            {
                //Start a new game
                core.Reset(players);
                playingNow = true;
            }
            oldState = newState;

            if (playingNow)
            {
                Core.CameraPosition.Z = 300 - Mouse.GetState().ScrollWheelValue * 0.5f;
                if (Mouse.GetState().X > Core.viewer.screenWidth - 30 || newState.IsKeyDown(Keys.Right))
                    Core.CameraPosition.X -= ((float)gameTime.ElapsedRealTime.TotalSeconds) * 500.0f;
                if (Mouse.GetState().X < 30 || newState.IsKeyDown(Keys.Left))
                    Core.CameraPosition.X += ((float)gameTime.ElapsedRealTime.TotalSeconds) * 500.0f;
                if (Mouse.GetState().Y > Core.viewer.screenHeight - 30 || newState.IsKeyDown(Keys.Down))
                    Core.CameraPosition.Y += ((float)gameTime.ElapsedRealTime.TotalSeconds) * 500.0f;
                if (Mouse.GetState().Y < 30 || newState.IsKeyDown(Keys.Up))
                    Core.CameraPosition.Y -= ((float)gameTime.ElapsedRealTime.TotalSeconds) * 500.0f;
                Core.Timing.Update();
                while (Core.Timing.DeltaTimeGlobal > 0)
                {
                    core.Update();
                    Core.Timing.DeltaTimeGlobal -= Core.Timing.DeltaTime;
                }
            }
            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            graphics.GraphicsDevice.Clear(Color.CornflowerBlue);
            if (playingNow)
                core.Draw();
            base.Draw(gameTime);
        }
    }
}
