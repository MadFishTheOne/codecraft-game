using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CoreNamespace
{
    public class TimingClass
    {
        public const float SpeedsMultiplier = 10;
        /// <summary>
        /// in milliseconds
        /// </summary>
        long prevTime, startTime;
        float nowTime, deltaTime, timeSpeed;
        bool paused;
        public float TimeSpeed
        {
            get
            {
                return timeSpeed;
            }
            set
            {
                timeSpeed = value;
            }
        }
        public bool Paused
        {
            get
            {
                return paused;
            }
            set
            {
                paused = value;
            }
        }
        public TimingClass()
        {
            startTime = prevTime = Environment.TickCount;
            nowTime = deltaTime = 0;
            timeSpeed = 1;
        }
        public void Update()
        {
            if (Environment.TickCount - startTime > 1000 * 6) { }
            long currTime = Environment.TickCount;
            if (!paused)
            {
                deltaTime = (currTime - prevTime) * 0.001f;
                if (deltaTime > 0.5f)
                    deltaTime = 0.015f;
                deltaTime *= timeSpeed;
                nowTime += deltaTime;
            }
            else
                deltaTime = 0.0f;
            prevTime = currTime;
        }
        public const float maxDeltaTime = 0.05f;
        public float DeltaTime
        {
            get
            {
                return Math.Min(deltaTime, maxDeltaTime);
            }
        }
        public float DeltaTimeGlobal
        {
            get
            {
                return deltaTime;
            }
            set
            {
                deltaTime = value;
            }
        }
        public float NowTime
        {
            get
            {
                return nowTime;
            }
        }
    }
}
