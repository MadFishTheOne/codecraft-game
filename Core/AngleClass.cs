using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace CoreNamespace
{
    class AngleClass
    {
        public static float Normalize(float angle)
        {
            while (angle < -MathHelper.Pi)
                angle += MathHelper.TwoPi;
            while (angle > MathHelper.Pi)
                angle -= MathHelper.TwoPi;
            return angle;
        }
        public static float Distance(float angle1, float angle2)
        {
            float prevDist = Math.Abs(Normalize(angle1 - angle2));
            return (float)Math.Min(prevDist, Math.Abs(Normalize(MathHelper.TwoPi - prevDist)));
        }
        public static float Difference(float angle1, float angle2)
        {
            return Normalize(angle1 - angle2);
            //float prevDist = angle1 - angle2;
            //if (Math.Abs(prevDist) < MathHelper.Pi) return prevDist;
            //return (float)Math.Min(prevDist, Math.Abs(Normalize(MathHelper.TwoPi - prevDist)));
        }
    }
}
