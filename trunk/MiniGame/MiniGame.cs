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

        List<IAI> plugins; //available AI plugins

        public MiniGame()
        {
            Content.RootDirectory = "Content";
            graphics = new GraphicsDeviceManager(this);
            //Content = new ContentManager(Services);
            IsFixedTimeStep = false;

            LoadPlugins();

            //Here you have to create a list of active players. Somehow :)
            List<IAI> players = new List<IAI>();
            if (plugins.Count == 0)
                throw new Exception("EPIC FAIL! No plugins found");
            else
            {
                //Currently only one player to control first ship
                foreach (IAI player in plugins)
                players.Add(player);
            }
#if DEBUG
            core = new Core(false, Content, graphics, players);
#else
            core = new Core(true, Content, graphics,players);
#endif
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

        bool prevSpacePressed;
        bool prevPlusPressed;
        bool prevMinusPressed;
        bool prevPausePressed;
        bool prevEnterPressed;
        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
                this.Exit();

            if (prevEnterPressed && Keyboard.GetState().IsKeyUp(Keys.Enter))
            {

            }

            if (prevSpacePressed && Keyboard.GetState().IsKeyUp(Keys.Space))
            {
                core.Reset();
            }
            if (prevPlusPressed && Keyboard.GetState().IsKeyUp(Keys.PageUp))
            {
                Core.Timing.TimeSpeed *= 2;
            }
            if (prevMinusPressed && Keyboard.GetState().IsKeyUp(Keys.PageDown))
            {
                Core.Timing.TimeSpeed /= 2;
            }
            if (prevPausePressed && Keyboard.GetState().IsKeyUp(Keys.Pause))
            {
                Core.Timing.Paused = !Core.Timing.Paused;
            }
            prevSpacePressed = Keyboard.GetState().IsKeyDown(Keys.Space);
            prevPlusPressed = Keyboard.GetState().IsKeyDown(Keys.PageUp);
            prevMinusPressed = Keyboard.GetState().IsKeyDown(Keys.PageDown);
            prevPausePressed = Keyboard.GetState().IsKeyDown(Keys.Pause);
            prevEnterPressed = Keyboard.GetState().IsKeyDown(Keys.Enter);
            Core.CameraPosition.Z = 300 - Mouse.GetState().ScrollWheelValue * 0.5f;
            if (Mouse.GetState().X > Core.viewer.screenWidth - 30 || Keyboard.GetState().IsKeyDown(Keys.Right))
                Core.CameraPosition.X -= ((float)gameTime.ElapsedRealTime.TotalSeconds) * 500.0f;
            if (Mouse.GetState().X < 30 || Keyboard.GetState().IsKeyDown(Keys.Left))
                Core.CameraPosition.X += ((float)gameTime.ElapsedRealTime.TotalSeconds) * 500.0f;
            if (Mouse.GetState().Y > Core.viewer.screenHeight - 30 || Keyboard.GetState().IsKeyDown(Keys.Down))
                Core.CameraPosition.Y += ((float)gameTime.ElapsedRealTime.TotalSeconds) * 500.0f;
            if (Mouse.GetState().Y < 30 || Keyboard.GetState().IsKeyDown(Keys.Up))
                Core.CameraPosition.Y -= ((float)gameTime.ElapsedRealTime.TotalSeconds) * 500.0f;
            // TODO: Add your update logic here
            Core.Timing.Update();
            while (Core.Timing.DeltaTimeGlobal > 0)
            {
                core.Update();
                Core.Timing.DeltaTimeGlobal -= Core.Timing.DeltaTime;
            }
            // TODO: Add your update logic here

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            core.Draw();
            base.Draw(gameTime);
        }
    }
}
