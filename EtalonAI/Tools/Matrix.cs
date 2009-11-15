using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MiniGameInterfaces;

namespace AINamespace
{
    public class Matrix
    {
        public float[,] values;
        public Matrix()
        {
            values = new float[3, 2];
        }
        public static Matrix CreateRotation(float angle)
        {
            Matrix res = new Matrix();
            float sin = (float)Math.Sin(angle);
            float cos = (float)Math.Cos(angle);
            res.values[0, 0] = cos; res.values[1, 0] = -sin; res.values[2, 0] = 0;
            res.values[0, 1] = sin; res.values[1, 1] = cos; res.values[2, 1] = 0;
            return res;
        }

        public static GameVector Mull(GameVector vec, Matrix matrix)
        {
            return new GameVector(
                matrix.values[0, 0] * vec.X + matrix.values[1, 0] * vec.Y + matrix.values[2, 0],
                matrix.values[0, 1] * vec.X + matrix.values[1, 1] * vec.Y + matrix.values[2, 1]);
        }
    }
}
