using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CP.Common.Random;

namespace CPWS.WorldGenerator.Noise
{
    public class CustomNoise : NoiseGen
    {
        public CustomNoise(uint seed) : base(seed)
        {
            hash = new RandomHash(Seed);
        }

        RandomHash hash;
        public bool Debug { get; set; }

        public override double Noise(params double[] vals)
        {
            int[] floors = new int[vals.Length];

            for (int i = 0; i < vals.Length; i++)
            {
                floors[i] = (int)Math.Floor(vals[i]);
                vals[i] -= floors[i];
            }

            return GetPoint(vals.Length - 1, floors, vals);
        }

        private double GetPoint(int index, int[] floors, double[] vals)
        {
            if(index == 0)
            {
                var hashVal1 = ((double)hash.GetHash(floors) / uint.MaxValue);
                int[] floors1 = (int[])floors.Clone();
                floors1[0] += 1;

                if (Debug)
                    Console.WriteLine(index + " | [{0}] | [{1}]", string.Join(", ", floors), string.Join(", ", floors1));

                var hashVal2 = ((double)hash.GetHash(floors1) / uint.MaxValue);
               
                return Lerp(hashVal1, hashVal2, vals[index]);
                return hashVal1 + (hashVal2 - hashVal1) * vals[0];
            }
            else
            {
                double r1 = GetPoint(index - 1, floors, vals);
                int[] floors1 = (int[])floors.Clone();
                floors1[index] += 1;
                
                if(Debug)
                    Console.WriteLine(index + " | [{0}] | [{1}]", string.Join(", ", floors), string.Join(", ", floors1));

                double r2 = GetPoint(index - 1, floors1, vals);
                return Lerp(r1, r2, vals[index]);
                return r1 + (r2 - r1) * vals[index];
            }
        }

        public override double Octave(int iterations, params double[] vals)
        {
            double maxAmp = 0;
            double amp = 1;
            double freq = Scale;
            double noise = 0;// Noise(vals.Select(r => r * freq).ToArray()) * amp;
            

            for (int i = 0; i < iterations; ++i)
            {
                noise += Noise(vals.Select(r => r * freq).ToArray()) * amp;
                //noise += simplex.Noise(vals[0], vals.Length > 1 ? vals[1] : 0);
                maxAmp += amp;
                amp *= Persistence;
                freq *= 2;
            }

            noise /= maxAmp;

            return noise;
        }

        public override Task<double[,]> NoiseMap(int iterations, params int[] vals)
        {
            throw new NotImplementedException();
        }

        public override double[,] NoiseMapNotAsync(int iterations, params int[] vals)
        {
            var buffer = new double[1, vals[0]];

            if (vals.Length == 1)
            {
                for (int x = 0; x < vals[0]; x++)
                {
                    buffer[0, x] = Octave(iterations, x);
                }
            } else
            {
                buffer = new double[vals[0], vals[1]];

                for (int y = 0; y < vals[0]; y++)
                {
                    for (int x = 0; x < vals[1]; x++)
                    {
                        double[] vals1 = vals.Select(t => (double)t).ToArray();
                        vals1[0] = x;
                        vals1[1] = y;
                        buffer[y, x] = Octave(iterations, vals1);
                    }
                }
            }

            return buffer;
        }

        static double Lerp(double a, double b, double t)
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
    }
}
