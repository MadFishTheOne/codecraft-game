using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
namespace AINamespace
{
    /// <summary>
    /// class for simplifying working with angles
    /// </summary>
    public class AngleClass
    {
        /// <summary>
        /// pi
        /// </summary>
        public const float pi = 3.1415926f;
        /// <summary>
        /// normalizes an angle. Returns value in interval [-pi...pi]
        /// </summary>
        /// <param name="angle">angle to normalize</param>
        /// <returns>normalized angle</returns>
        public static float Normalize(float angle)
        {
            while (angle < -pi)
                angle += pi * 2;
            while (angle > pi)
                angle -= pi * 2;
            return angle;
        }
        /// <summary>
        /// calculates absolute distance between two angles
        /// </summary>
        /// <param name="angle1">first angle</param>
        /// <param name="angle2">second angle</param>
        /// <returns>angle distance, in radians</returns>
        public static float Distance(float angle1, float angle2)
        {
            float prevDist = Math.Abs(Normalize(angle1 - angle2));
            return (float)Math.Min(prevDist, Math.Abs(Normalize(pi * 2 - prevDist)));
        }
        /// <summary>
        /// calculates angles difference
        /// </summary>
        /// <param name="angle1">first angle</param>
        /// <param name="angle2">second angle</param>
        /// <returns>distance between angles</returns>
        public static float Difference(float angle1, float angle2)
        {
            return Normalize(angle1 - angle2);
        }
        //public static bool RotateCCWToAngle(float thisAngle,float aimedAngle, out float aimIsNearDecrementing)//,out bool Equals)
        //{
        //    float angleDist = AngleClass.Distance(aimedAngle, thisAngle);

        //    if (angleDist < pi / 180f * 30)
        //    {
        //        aimIsNearDecrementing = angleDist / (pi / 180f * 30);            
        //    }
        //    else aimIsNearDecrementing = 1;
        //    return AngleClass.Difference(thisAngle, aimedAngle) < 0;            
        //}
        /// <summary>
        /// adds two angles
        /// </summary>
        /// <param name="angle1">first angle</param>
        /// <param name="angle2">second angle</param>
        /// <returns>angle sum</returns>
        public static float Add(float angle1, float angle2)
        {
            return Normalize(angle1 + angle2);
        }
    }
}
