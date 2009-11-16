using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MiniGameInterfaces;

namespace AINamespace
{
    /// <summary>
    /// matrix class is used for hold transformations in game world
    /// </summary>
    public class Matrix
    {
        /// <summary>
        /// matrix components.
        /// float[3,2] size
        /// </summary>
        public float[,] values;
        /// <summary>
        /// create new matrix
        /// </summary>
        public Matrix()
        {
            values = new float[3, 2];
        }
        /// <summary>
        /// create rotation matrix
        /// </summary>
        /// <param name="angle">rotation angle</param>
        /// <returns>rotation matrix</returns>
        public static Matrix CreateRotation(float angle)
        {
            Matrix res = new Matrix();
            float sin = (float)Math.Sin(angle);
            float cos = (float)Math.Cos(angle);
            res.values[0, 0] = cos; res.values[1, 0] = -sin; res.values[2, 0] = 0;
            res.values[0, 1] = sin; res.values[1, 1] = cos; res.values[2, 1] = 0;
            return res;
        }
        /// <summary>
        /// mulls GameVector with specified matrix
        /// </summary>
        /// <param name="vec"></param>
        /// <param name="matrix"></param>
        /// <returns></returns>
        public static GameVector Mull(GameVector vec, Matrix matrix)
        {
            return new GameVector(
                matrix.values[0, 0] * vec.X + matrix.values[1, 0] * vec.Y + matrix.values[2, 0],
                matrix.values[0, 1] * vec.X + matrix.values[1, 1] * vec.Y + matrix.values[2, 1]);
        }
    }
}
