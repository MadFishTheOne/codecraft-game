using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AINamespace
{
    class AngleClass
    {
        public const float pi = 3.1415926f;
        public static float Normalize(float angle)
        {
            while (angle < -pi)
                angle += pi*2;
            while (angle > pi)
                angle -= pi*2;
            return angle;
        }
        public static float Distance(float angle1, float angle2)
        {
            float prevDist = Math.Abs(Normalize(angle1 - angle2));
            return (float)Math.Min(prevDist, Math.Abs(Normalize(pi*2 - prevDist)));
        }
        public static float Difference(float angle1, float angle2)
        {
            return Normalize(angle1 - angle2);         
        }
        public static bool RotateCCWToAngle(float thisAngle,float aimedAngle, out float aimIsNearDecrementing)//,out bool Equals)
        {
            float angleDist = AngleClass.Distance(aimedAngle, thisAngle);
            
            if (angleDist < pi / 180f * 30)
            {
                aimIsNearDecrementing = angleDist / (pi / 180f * 30);            
            }
            else aimIsNearDecrementing = 1;
            return AngleClass.Difference(thisAngle, aimedAngle) < 0;            
        }

        public static float Add(float angle1, float angle2)
        {
            return Normalize(angle1 + angle2);
        }
    }
}
