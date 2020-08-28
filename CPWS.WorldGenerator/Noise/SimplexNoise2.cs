using CP.Common.Utilities;
using System;
using System.Collections.Generic;
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
            throw new NotImplementedException();
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
            return vals.Length switch
            {
                //1 => Noise1D(vals[0]),
                //2 => Noise2D(vals[0], vals[1]),
                3 => NoiseTest(vals[0], vals[1], vals[2]),
                //4 => Noise4D(vals[0], vals[1], vals[2], vals[3]),
                _ => throw new NotImplementedException(),
            };
        }

        private static readonly int[] primeList = new int[] { 1619, 31337, 6971, 1013 };

        private double GradCoord3D(int[] ints, double[] doubles)
        {
            int dimensions = ints.Length;
            uint hash = Seed;

            for (int i = 0; i < ints.Length; i++)
            {
                hash ^= (uint)(primeList[i%4] * ints[i]);
            }

            hash = hash * hash * hash * 60493;
            hash = (hash >> 13) ^ hash;

            hash &= 15;

            double result = 0.0;
            int current = 1;

            for(int i = dimensions - 1; i > -1; i--)
            {
                result += (hash & current) == 0 ? -doubles[i] : doubles[i];
                current *= 2;
            }

            return result;
        }

        public double NoiseTest(params double[] vals)
        {
            int dimensions = vals.Length;
            double G = GValues(dimensions);

            double s = 0;
            foreach (double v in vals)
                s += v;
            s *= FValues(dimensions);

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

            for (int i = dimensions-1; i >= 0; i--)
            {
                xvals[i] = vals[i] - (ivvals[i] - t);
                for (int j = i + 1; j < dimensions; j++)
                    if (xvals[i] > xvals[j]) ranks[i]++; else ranks[j]++;
            }

            double n = 0;
            int temp = dimensions - 1;

            for (int i = 0; i < dimensions + 1; i++)
            {
                double[] vvals = new double[dimensions];
                int[] p = new int[dimensions];

                t = 0.6;

                for (int j = 0; j < dimensions; j++)
                {
                    int ival = 0;
                    if (i > 0) ival = (i == dimensions ? 1 : (ranks[j] >= temp ? 1 : 0));
                    vvals[j] = i == 0 ? xvals[j] : xvals[j] - ival + i * G;

                    t -= vvals[j] * vvals[j];

                    p[j] = ivvals[j] + ival;
                }
                if (i > 0) temp--;

                if (t >= 0)
                {
                    n += (t * t) * t * t * GradCoord3D(p, vvals);
                }
            }

            return 32.0 * n;
        }
        
        /*public double Noise3D(double xin, double yin, double zin)
        {
            double G = GValues(3);

            double s = (xin + yin + zin) * FValues(3);
            int i = (int)(xin + s);
            int j = (int)(yin + s);
            int k = (int)(zin + s);

            double t = (i + j + k) * G;

            double x0 = xin - (i - t);
            double y0 = yin - (j - t);
            double z0 = yin - (k - t);

            int xrank = 0;
            int yrank = 0;
            int zrank = 0;

            if (x0 > y0) xrank++; else yrank++;
            if (x0 > z0) xrank++; else zrank++;
            if (y0 > z0) yrank++; else zrank++;

            double n = 0;

            int i1 = xrank >= 2 ? 1 : 0;
            int j1 = yrank >= 2 ? 1 : 0;
            int k1 = zrank >= 2 ? 1 : 0;

            int i2 = xrank >= 1 ? 1 : 0;
            int j2 = yrank >= 1 ? 1 : 0;
            int k2 = zrank >= 1 ? 1 : 0;

            double x1 = x0 - i1 + G;
            double y1 = y0 - j1 + G;
            double z1 = z0 - k1 + G;

            double x2 = x0 - i2 + 2.0 * G;
            double y2 = y0 - j2 + 2.0 * G;
            double z2 = z0 - k2 + 2.0 * G;

            double x3 = x0 - 1 + 3.0 * G;
            double y3 = y0 - 1 + 3.0 * G;
            double z3 = z0 - 1 + 3.0 * G;

            t = 0.6 - x0 * x0 - y0 * y0 - z0 * z0;
            if (t >= 0)
            {
                t *= t;
                n += t * t * dot(grad4[gi0], x0, y0, z0);
            }
            t = 0.6 - x1 * x1 - y1 * y1 - z1 * z1;
            if (t >= 0)
            {
                t *= t;
                n += t * t * dot(grad4[gi1], x1, y1, z1);
            }
            t = 0.6 - x2 * x2 - y2 * y2 - z2 * z2;
            if (t >= 0)
            {
                t *= t;
                n += t * t * dot(grad4[gi2], x2, y2, z2);
            }
            t = 0.6 - x3 * x3 - y3 * y3 - z3 * z3;
            if (t >= 0)
            {
                t *= t;
                n += t * t * dot(grad4[gi3], x3, y3, z3);
            }


        }*/

        double FValues(double dim)
        {
            return (Math.Sqrt(dim + 1) - 1) / dim;
        }

        double GValues(double dim)
        {
            return ((dim + 1) - Math.Sqrt(dim + 1)) / ((dim + 1) * dim);
        }
    }
}
