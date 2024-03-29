﻿using CP.Common.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace CPWS.WorldGenerator.Noise
{
    public enum FractalType { FBM, Billow, Rigid };

    public class SimplexNoise : NoiseGen
    {
        private static readonly double[] GValues = new [] { 0.2928932188134524, 0.21132486540518713, 0.16666666666666666, 0.1381966011250105 };
        private static readonly double[] FValues = new [] { 0.41421356237309515, 0.3660254037844386, 0.3333333333333333, 0.30901699437494745 };
        private static readonly int[] primeList = new [] { 1619, 31337, 6971, 1013 };

        public class SimplexThread
        {
            public int dimensions { get; private set; }

            public double[] values { get; private set; }
            public int[] ivvals { get; private set; }
            public double[] vvals { get; private set; }
            public double[] xvals { get; private set; }
            public double[] ranks { get; private set; }
            public double[] nVals { get; private set; }

            public SimplexThread(int dimensions)
            {
                this.dimensions = dimensions;

                values = new double[dimensions];
                ivvals = new int[dimensions];
                vvals = new double[dimensions];
                xvals = new double[dimensions];
                ranks = new double[dimensions];
                nVals = new double[dimensions];
            }
        }

        double G;
        double F;

        public SimplexNoise(uint seed, double scale, double persistence, bool useCuda = true) : base(seed)
        {
            UseCuda = useCuda;

            Scale = scale;
            Persistence = persistence;
        }

        private void Setup(int dimensions)
        {
            if (dimensions - 1 < 4)
            {
                G = GValues[dimensions - 1];
                F = FValues[dimensions - 1];
            } else
            {
                double sqrt = Math.Sqrt(dimensions + 1);
                G = ((dimensions + 1) - sqrt) / ((dimensions + 1) * dimensions);
                F = (sqrt - 1) / dimensions;
            }
        }

        public override async Task<double[,]> NoiseMap(int iterations, FractalType type, params int[] vals)
        {
            int len = vals.Length;
            int yLen = (len < 2 || vals[1] == 0) ? 1 : vals[1];
            int xLen = vals[0];
            Task[] tasks = new Task[yLen];

            var buffer = new double[yLen, xLen];
            Setup(len);

            for (int y = 0; y < yLen; ++y)
            {
                int yCopy = y;
                tasks[y] = Task.Run(() =>
                {
                    SimplexThread st = new SimplexThread(len);
                    if (len > 1)
                        st.values[1] = yCopy;

                    for (int x = 0; x < xLen; ++x)
                    {
                        st.values[0] = x;
                        switch(type)
                        {
                            case FractalType.FBM:
                                buffer[yCopy, x] = FractalFBM(iterations, len, st, st.values);
                                break;
                            case FractalType.Billow:
                                buffer[yCopy, x] = FractalBillow(iterations, len, st, st.values);
                                break;
                            case FractalType.Rigid:
                                buffer[yCopy, x] = FractalRigid(iterations, len, st, st.values);
                                break;
                        }
                    }
                });
            }

            await Task.WhenAll(tasks).ConfigureAwait(false);

            return buffer;
        }

        public override double[,] NoiseMapNotAsync(int iterations, params int[] vals)
        {
            int len = vals.Length;
            int yLen = (len < 2 || vals[1] == 0) ? 1 : vals[1];
            int xLen = vals[0];
            var buffer = new double[yLen, xLen];

            Setup(len);
            SimplexThread st = new SimplexThread(len);
            for (int y = 0; y < yLen; ++y)
            {
                if (len > 1)
                {
                    st.values[1] = y;
                }
                for (int x = 0; x < xLen; ++x)
                {
                    st.values[0] = x;
                    buffer[y, x] = FractalFBM(iterations, len, st, st.values);
                }
            }

            return buffer;
        }

        public override double FractalFBM(int iterations, int dimensions, params double[] vals) => FractalFBM(iterations, dimensions, new SimplexThread(dimensions), vals);

        public override double FractalBillow(int iterations, int dimensions, params double[] vals) => FractalBillow(iterations, dimensions, new SimplexThread(dimensions), vals);
        public override double FractalRigid(int iterations, int dimensions, params double[] vals) => FractalRigid(iterations, dimensions, new SimplexThread(dimensions), vals);

        private double FractalFBM(int iterations, int dimensions, SimplexThread st, params double[] vals)
        {
            double maxAmp = 0;
            double amp = 1;
            double freq = Scale;
            double noise = 0;

            for (int i = 0; i < iterations; ++i)
            {
                for (int j = 0; j < vals.Length; ++j)
                {
                    st.nVals[j] = vals[j] * freq;
                }

                noise += Noise(dimensions, st, st.nVals) * amp;
                maxAmp += amp;
                amp *= Persistence;
                freq *= 2;
            }

            noise /= maxAmp;

            return noise;
        }

        private double FractalBillow(int iterations, int dimensions, SimplexThread st, params double[] vals)
        {
            double maxAmp = 0;
            double amp = 1;
            double freq = Scale;
            double noise = 0;

            for (int i = 0; i < iterations; ++i)
            {
                for (int j = 0; j < vals.Length; ++j)
                {
                    st.nVals[j] = vals[j] * freq;
                }

                noise += (Math.Abs(Noise(dimensions, st, st.nVals)) * 2 - 1) * amp;
                maxAmp += amp;
                amp *= Persistence;
                freq *= 2;
            }

            noise /= maxAmp;

            return noise;
        }

        private double FractalRigid(int iterations, int dimensions, SimplexThread st, params double[] vals)
        {
            double maxAmp = 0;
            double amp = 1;
            double freq = Scale;
            double noise = 0;

            for (int i = 0; i < iterations; ++i)
            {
                for (int j = 0; j < vals.Length; ++j)
                {
                    st.nVals[j] = vals[j] * freq;
                }

                noise += (1 - Math.Abs(Noise(dimensions, st, st.nVals))) * amp;
                maxAmp += amp;
                amp *= Persistence;
                freq *= 2;
            }

            return noise / maxAmp;
        }

        public override double Noise(int dimensions, params double[] vals) => Noise(dimensions, new SimplexThread(dimensions), vals);

        private double Noise(int dimensions, SimplexThread st, params double[] vals)
        {
            double s = 0;

            foreach (double v in vals)
            {
                s += v;
            }
            s *= F;

            double t = 0;
            for (int i = 0; i < dimensions; ++i)
            {
                st.vvals[i] = 0;
                st.xvals[i] = 0;
                st.ranks[i] = 0;
                st.ivvals[i] = (int)(vals[i] + s);
                t += st.ivvals[i];
            }
            t *= G;

            for (int i = dimensions - 1; i >= 0; --i)
            {
                st.xvals[i] = vals[i] - (st.ivvals[i] - t);
                for (int j = i + 1; j < dimensions; ++j)
                {

                    if (st.xvals[i] > st.xvals[j]) 
                    { 
                        st.ranks[i]++; 
                    } else 
                    {
                        st.ranks[j]++;
                    }
                }
            }
            double n = 0;
            int temp = dimensions - 1;

            for (int i = 0; i < dimensions + 1; ++i)
            {
                t = 0.6;
                uint hash = Seed;

                for (int j = 0; j < dimensions; ++j)
                {
                    int ival = 0;
                    if (i > 0) 
                    { 
                        ival = (i == dimensions ? 1 : (st.ranks[j] >= temp ? 1 : 0)); 
                    }
                    double vval = st.vvals[j] = i == 0 ? st.xvals[j] : st.xvals[j] - ival + i * G;

                    t -= vval * vval;

                    hash ^= (uint)(primeList[j % 4] * (st.ivvals[j] + ival));
                }
                if (i > 0) 
                { 
                    temp--; 
                }
                if (t >= 0)
                {
                    hash = hash * hash * hash * 60493;
                    hash = (hash >> 13) ^ hash;

                    hash &= 15;

                    double result = 0.0;
                    int current = 1;

                    for (int j = dimensions - 1; j > -1; --j)
                    {
                        result += (hash & current) == 0 ? -st.vvals[j] : st.vvals[j];
                        current *= 2;
                    }

                    n += (t * t) * t * t * result;
                }
            }

            return 32.0 * n;
        }
    }
}
