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
    public class Timer
    {
        static Random rand=new Random();
        float deltaTime;
        
        float prevTickTime;
        //bool timeElapsed;
        

        /// <summary>
        /// creates a new timer
        /// </summary>
        /// <param name="DeltaTime">time intervals to tick</param>        
        public Timer(float DeltaTime)
        {
            deltaTime = DeltaTime;            
            prevTickTime = AI.game.Time - deltaTime * (float)rand.NextDouble();
            
        }        
        /// <summary>
        /// true if deltaTime was elapsed since last call of Reset()
        /// </summary>
        public bool TimeElapsed
        {
            get { return AI.game.Time - prevTickTime >= deltaTime; }
        }
        /// <summary>
        /// resets timer;
        /// must be called after every tick
        /// </summary>
        public void Reset()
        {
            prevTickTime = AI.game.Time;
        }
        /// <summary>
        /// elapses time
        /// </summary>
        public void ExeedDeltaTime()
        {
            prevTickTime = AI.game.Time - deltaTime * 1.1f;            
        }
    }
}
