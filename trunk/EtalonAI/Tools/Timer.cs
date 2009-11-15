using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MiniGameInterfaces;
namespace AINamespace
{
    //used to tick in given intervals
    class Timer//not tested
    {
        static Random rand=new Random();
        float deltaTime;
        
        float prevTickTime;
        bool timeElapsed;
        IGame game;
        public Timer(float DeltaTime,IGame Game)
        {
            deltaTime = DeltaTime;
            game = Game;
            prevTickTime = game.Time - deltaTime * (float)rand.NextDouble();
            
        }

        public void Update()
        {
            timeElapsed =game.Time- prevTickTime >= deltaTime;
            float t = game.Time - prevTickTime;
        }
        public bool TimeElapsed
        {
            get { return timeElapsed; }
        }
        public void Reset()
        {
            prevTickTime = game.Time;
        }
        public void ExeedDeltaTime()
        {
            prevTickTime = game.Time - deltaTime * 1.1f;
            Update();
        }
    }
}
