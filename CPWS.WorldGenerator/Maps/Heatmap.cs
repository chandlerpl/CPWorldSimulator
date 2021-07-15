using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using CPWS.WorldGenerator.Noise;

namespace CPWS.WorldGenerator.Maps
{
    public static class Heatmap
    {
        public static async Task<double[,]> GenerateHeatmap(NoiseGen noise, int yin, int xin, double[,] heightMap = null)
        {
            Task[] tasks = new Task[yin];
            double[,] heatmap = new double[yin, xin];

            for (int y = 0; y < yin; y++)
            {
                double gradient = GenerateGradient((double)y / yin);
                int yCopy = y;

                tasks[y] = Task.Factory.StartNew(() =>
                {
                    for (int x = 0; x < xin; x++)
                    {
                        heatmap[yCopy, x] = Lerp(noise.FractalFBM(4, 2, x, yCopy), gradient, .85) * (heightMap != null ? 1 - ((heightMap[yCopy, x] + 1) / 2) : 1);
                    }
                });
            }
            await Task.WhenAll(tasks);

            return heatmap;
        }

        public static double Lerp(double a, double b, double t)
        {
            if (t <= 0)
                return a;
            else if (t >= 1)
                return b;

            if (a > b)
            {
                t = 1 - t;
                double temp = a;
                a = b;
                b = temp;
            }

            return a + (b - a) * t;
        }

        private static double GenerateGradient(params double[] vals)
        {
            double t = 1;

            foreach (double val in vals)
                t *= (1 - Math.Pow((-1 + 2 * val), 2));
            //t += 1 - 2 * Math.Abs(val - 0.5);

            return t;
        }
    }
}
