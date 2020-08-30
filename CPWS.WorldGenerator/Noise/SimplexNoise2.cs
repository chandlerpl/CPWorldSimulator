using CP.Common.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace CPWS.WorldGenerator.Noise
{
    public class SimplexNoise2 : NoiseGen
    {
        public SimplexNoise2(uint seed, double scale, double persistence, bool useCuda = true) : base(seed)
        {
            UseCuda = useCuda;

            Scale = scale;
            Persistence = persistence;
        }

        public override async Task<double[,]> NoiseMap(int iterations, params int[] vals)
        {
            Task[] tasks = new Task[vals[1]];

            var buffer = new double[vals[1], vals[0]];

            for (int y = 0; y < vals[1]; y++)
            {
                int yCopy = y;
                tasks[y] = Task.Factory.StartNew(() =>
                {
                    for (int x = 0; x < vals[0]; x++)
                    {
                        buffer[yCopy, x] = Octave(iterations, x, yCopy, 0);
                    }
                });
            }

            await Task.WhenAll(tasks);
            

            return buffer;
        }

        public override double[,] NoiseMapNotAsync(int iterations, params int[] vals)
        {
            var buffer = new double[vals[1], vals[0]];

            for (int y = 0; y < vals[1]; y++)
            {
                for (int x = 0; x < vals[0]; x++)
                {
                    buffer[y, x] = Octave(iterations, x, y, 0);
                }
            }

            return buffer;
        }

        public override double Octave(int iterations, params double[] vals)
        {
            double maxAmp = 0;
            double amp = 1;
            double freq = Scale;
            double noise = 0;

            for (int i = 0; i < iterations; ++i)
            {
                double[] nVals = new double[vals.Length];
                for (int j = 0; j < vals.Length; j++)
                    nVals[j] = vals[j] * freq;

                noise += Noise(nVals) * amp;
                maxAmp += amp;
                amp *= Persistence;
                freq *= 2;
            }

            noise /= maxAmp;

            return noise;
        }

        public override double Noise(params double[] vals)
        {
            int dimensions = vals.Length;

            double G = ((dimensions + 1) - Math.Sqrt(dimensions + 1)) / ((dimensions + 1) * dimensions);

            double s = 0;
            foreach (double v in vals)
                s += v;
            s *= (Math.Sqrt(dimensions + 1) - 1) / dimensions;

            int[] ivvals = new int[dimensions];

            double t = 0;
            for (int i = 0; i < dimensions; i++)
            {
                ivvals[i] = (int)(vals[i] + s);
                t += ivvals[i];
            }
            t *= G;

            double[] xvals = new double[dimensions];
            double[] ranks = new double[dimensions];

            for (int i = dimensions - 1; i >= 0; i--)
            {
                xvals[i] = vals[i] - (ivvals[i] - t);
                for (int j = i + 1; j < dimensions; j++)
                    if (xvals[i] > xvals[j]) ranks[i]++; else ranks[j]++;
            }
            double n = 0;
            int temp = dimensions - 1;
            double[] vvals = new double[dimensions];

            for (int i = 0; i < dimensions + 1; i++)
            {
                t = 0.6;
                uint hash = Seed;

                for (int j = 0; j < dimensions; j++)
                {
                    int ival = 0;
                    if (i > 0) ival = (i == dimensions ? 1 : (ranks[j] >= temp ? 1 : 0));
                    double vval = vvals[j] = i == 0 ? xvals[j] : xvals[j] - ival + i * G;

                    t -= vval * vval;

                    hash ^= (uint)(primeList[j % 4] * (ivvals[j] + ival));
                }
                if (i > 0) temp--;
                if (t >= 0)
                {
                    hash = hash * hash * hash * 60493;
                    hash = (hash >> 13) ^ hash;

                    hash &= 15;

                    double result = 0.0;
                    int current = 1;

                    for (int j = dimensions - 1; j > -1; j--)
                    {
                        result += (hash & current) == 0 ? -vvals[j] : vvals[j];
                        current *= 2;
                    }

                    n += (t * t) * t * t * result;
                }
            }

            return 32.0 * n;
        }

        private static readonly int[] primeList = new int[] { 1619, 31337, 6971, 1013 };
    }
}
