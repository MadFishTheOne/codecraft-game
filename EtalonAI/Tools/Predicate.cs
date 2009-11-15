using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AINamespace
{
    public abstract class Predicate
    {
        Timer timer;
        bool value;
        public static  implicit  operator bool (Predicate p)
        {
           return p.value;
        }

        public void Update()
        {
            timer.Update();
            if (timer.TimeElapsed)
            {
                RecalculateValue();
                timer.Reset();
            }
        }
        public abstract void RecalculateValue();
        

    }
}
