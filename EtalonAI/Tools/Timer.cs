using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MiniGameInterfaces;
namespace AINamespace
{
    
    /// <summary>
    /// used to tick in given intervals
    /// </summary>
    class Timer
    {
        static Random rand=new Random();
        float deltaTime;
        
        float prevTickTime;
        bool timeElapsed;
        IGame game;

        /// <summary>
        /// creates a new timer
        /// </summary>
        /// <param name="DeltaTime">time intervals to tick</param>
        /// <param name="Game">game</param>
        public Timer(float DeltaTime,IGame Game)
        {
            deltaTime = DeltaTime;
            game = Game;
            prevTickTime = game.Time - deltaTime * (float)rand.NextDouble();
            
        }
        /// <summary>
        /// elapses time;
        /// must be called every game loop
        /// </summary>
        public void Update()
        {
            timeElapsed =game.Time- prevTickTime >= deltaTime;
            float t = game.Time - prevTickTime;
        }
        /// <summary>
        /// true if deltaTime was elapsed since last call of Reset()
        /// </summary>
        public bool TimeElapsed
        {
            get { return timeElapsed; }
        }
        /// <summary>
        /// resets timer;
        /// must be called after every tick
        /// </summary>
        public void Reset()
        {
            prevTickTime = game.Time;
        }
        /// <summary>
        /// elapses time
        /// </summary>
        public void ExeedDeltaTime()
        {
            prevTickTime = game.Time - deltaTime * 1.1f;
            Update();
        }
    }
}
