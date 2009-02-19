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

        int cursorPosition; //menu cursor position

        public MiniGame()
        {
            Content.RootDirectory = "Content";
            graphics = new GraphicsDeviceManager(this);
#if DEBUG
            graphics.IsFullScreen = false;
            graphics.PreferredBackBufferWidth = 640;
            graphics.PreferredBackBufferHeight = 480;
#else
            graphics.IsFullScreen = (Config.Instance.settings["Video.FullScreen"].ToLower() == "true");
            if (Config.Instance.settings["Video.ScreenWidth"] == "0")
            {
                if (graphics.IsFullScreen)
                {
                    graphics.PreferredBackBufferWidth = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width;
                    graphics.PreferredBackBufferHeight = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height;
                }
                else
                {
                    graphics.PreferredBackBufferWidth = 640;
                    graphics.PreferredBackBufferHeight = 480;
                }
            }
            else
            {
                graphics.PreferredBackBufferWidth = Convert.ToInt32(Config.Instance.settings["Video.ScreenWidth"]);
                graphics.PreferredBackBufferHeight = Convert.ToInt32(Config.Instance.settings["Video.ScreenHeight"]);
            }
#endif
            graphics.SynchronizeWithVerticalRetrace = (Config.Instance.settings["Video.VSync"].ToLower() == "true");

            IsFixedTimeStep = false;

            LoadPlugins();

            players = new List<IAI>();
/*            //Here you have to create a list of active players. Somehow :)
            if (plugins.Count == 0)
                throw new Exception("EPIC FAIL! No plugins found");
            else
            {
                players.Add(Activator.CreateInstance(plugins[0].GetType()) as IAI);
                players.Add(Activator.CreateInstance(plugins[0].GetType()) as IAI);
            }*/
            core = new Core(graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight, Content, graphics);
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
#if DEBUG
            if (IsKeyReleased(Keys.Q))
                System.Diagnostics.Debugger.Break(); //entering debug mode
#endif
            if (playingNow)
            {
                if (IsKeyReleased(Keys.Escape))
                {
                    playingNow = false;
                    cursorPosition = 0;
                }
                if (IsKeyReleased(Keys.PageUp) || IsKeyReleased(Keys.S))
                {
                    Core.Timing.TimeSpeed *= 2;
                    if (Core.Timing.TimeSpeed > 128)
                        Core.Timing.TimeSpeed = 128;
                }
                if (IsKeyReleased(Keys.PageDown) || IsKeyReleased(Keys.X))
                {
                    Core.Timing.TimeSpeed /= 2;
                    if (Core.Timing.TimeSpeed < 0.0625f)
                        Core.Timing.TimeSpeed = 0.0625f;
                }
                if (IsKeyReleased(Keys.Pause) || IsKeyReleased(Keys.Space))
                    Core.Timing.Paused = !Core.Timing.Paused;
            }
            else
            {
                if (IsKeyReleased(Keys.Escape))
                    this.Exit();
                if (plugins.Count > 0)
                {
                    if (IsKeyReleased(Keys.Down))
                    {
                        cursorPosition++;
                        if (cursorPosition >= plugins.Count)
                            cursorPosition = plugins.Count - 1;
                    }
                    if (IsKeyReleased(Keys.Up))
                    {
                        cursorPosition--;
                        if (cursorPosition < 0)
                            cursorPosition = 0;
                    }
                    if (IsKeyReleased(Keys.Space))
                    {
                        if(players.Count<2)
                            players.Add(Activator.CreateInstance(plugins[cursorPosition].GetType()) as IAI);
                    }
                    if (IsKeyReleased(Keys.Delete))
                        players.Clear();
                }
            }
            if (IsKeyReleased(Keys.Enter))
            {
                //Start a new game
                if (players.Count >= 2)
                {
                    core.Reset(players);
                    playingNow = true;
                }
            }
            oldState = newState;

            if (playingNow)
            {
                if (newState.IsKeyDown(Keys.End) || newState.IsKeyDown(Keys.Z))
                    Core.CameraPosition.Z += ((float)gameTime.ElapsedRealTime.TotalSeconds) * 4000.0f;
                if (newState.IsKeyDown(Keys.Home) || newState.IsKeyDown(Keys.A))
                {
                    Core.CameraPosition.Z -= ((float)gameTime.ElapsedRealTime.TotalSeconds) * 4000.0f;
                    if (Core.CameraPosition.Z < 200.0f)
                        Core.CameraPosition.Z = 200.0f;
                }
                if (newState.IsKeyDown(Keys.Left))
                    Core.CameraPosition.X -= ((float)gameTime.ElapsedRealTime.TotalSeconds) * 2500.0f;
                if (newState.IsKeyDown(Keys.Right))
                    Core.CameraPosition.X += ((float)gameTime.ElapsedRealTime.TotalSeconds) * 2500.0f;
                if (newState.IsKeyDown(Keys.Up))
                    Core.CameraPosition.Y += ((float)gameTime.ElapsedRealTime.TotalSeconds) * 2500.0f;
                if (newState.IsKeyDown(Keys.Down))
                    Core.CameraPosition.Y -= ((float)gameTime.ElapsedRealTime.TotalSeconds) * 2500.0f;

#if FAlLSE
                if (Mouse.GetState().X > Core.viewer.screenWidth - 30 )
                    Core.CameraPosition.X -= ((float)gameTime.ElapsedRealTime.TotalSeconds) * 500.0f;
                if (Mouse.GetState().X < 30 || newState.IsKeyDown(Keys.Left))
                    Core.CameraPosition.X += ((float)gameTime.ElapsedRealTime.TotalSeconds) * 500.0f;
                if (Mouse.GetState().Y > Core.viewer.screenHeight - 30)
                    Core.CameraPosition.Y += ((float)gameTime.ElapsedRealTime.TotalSeconds) * 500.0f;
                if (Mouse.GetState().Y < 30 )
                    Core.CameraPosition.Y -= ((float)gameTime.ElapsedRealTime.TotalSeconds) * 500.0f;
#endif
                Core.Timing.Update();
                while (Core.Timing.DeltaTimeGlobal > 0)
                {
                    core.Update();
                    Core.Timing.DeltaTimeGlobal -= Core.Timing.DeltaTime;
                }
            }
            base.Update(gameTime);
        }

        void DrawMenu()
        {
            graphics.GraphicsDevice.Clear(Color.CornflowerBlue);
            if (plugins.Count == 0)
                Core.viewer.DrawText("No plugins found! Please put some plugins into the game folder.", new Vector2(100, 100), 0, Color.Red);
            else
            {
                Core.viewer.DrawText("Available plugins:", new Vector2(60, 20), 0, Color.Yellow);
                for (int i = 0; i < plugins.Count; i++)
                {
                    if (cursorPosition == i)
                        Core.viewer.DrawText(">", new Vector2(20, 60 + i * 20), 0, Color.White);
                    Core.viewer.DrawText(plugins[i].Author, new Vector2(60, 60 + i * 20), 0, Color.White);
                    Core.viewer.DrawText(plugins[i].Description, new Vector2(250, 60 + i * 20), 0, Color.Gray);
                }
                Core.viewer.DrawText("Hint: Use arrows to select AI, [Space] to add AI to player list,", new Vector2(60, 340), 0, Color.LightGoldenrodYellow);
                Core.viewer.DrawText("and [Del] to clear list. [Enter] starts the game.", new Vector2(60, 360), 0, Color.LightGoldenrodYellow);
                Core.viewer.DrawText("Selected players:", new Vector2(60, 400), 0, Color.Yellow);
                for (int i = 0; i < players.Count; i++)
                {
                    Core.viewer.DrawText(players[i].Author, new Vector2(60, 440 + i * 20), 0, new Color(Core.Viewer.TeamColors[i]));
                    Core.viewer.DrawText(players[i].Description, new Vector2(250, 440 + i * 20), 0, Color.Gray);
                }
            }
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            if (playingNow)
                core.Draw();
            else
                DrawMenu();
            base.Draw(gameTime);
        }
    }
}
