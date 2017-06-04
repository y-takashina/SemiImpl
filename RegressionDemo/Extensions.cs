using System;
using System.Collections.Generic;

namespace RegressionDemo
{
    public static class Extensions
    {
        public static double NextGaussian(this Random random)
        {
            double v1, v2, r;
            do
            {
                v1 = random.NextDouble() - 0.5;
                v2 = random.NextDouble() - 0.5;
                r = v1 * v1 + v2 * v2;
            } while (r >= 0.25 || r == 0);
            double f = Math.Sqrt(-2 * Math.Log(r * 4) / r);
            return v1 * f;
        }

    }

}
