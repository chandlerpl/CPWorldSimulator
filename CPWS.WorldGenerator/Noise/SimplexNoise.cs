using CP.Common.Utilities;
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

        public void Setup(int dimensions)
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

            if(len == 3)
            {
                tasks = new Task[yLen / 8];
                for (int y = 0, index = 0; y < yLen; y += 8, index++)
                {
                    int yCopy = y;
                    tasks[index] = Task.Run(() =>
                    {
                        Vector<float> yin = new Vector<float>(new float[] { yCopy, yCopy + 1, yCopy + 2, yCopy + 3, yCopy + 4, yCopy + 5, yCopy + 6, yCopy + 7 });

                        for (int x = 0; x < xLen; ++x)
                        {
                            Vector<float> result = FractalFBM(iterations, new Vector<float>(x), yin, new Vector<float>(vals[2]));
                            buffer[yCopy, x] = result[0];
                            buffer[yCopy + 1, x] = result[1];
                            buffer[yCopy + 2, x] = result[2];
                            buffer[yCopy + 3, x] = result[3];
                            buffer[yCopy + 4, x] = result[4];
                            buffer[yCopy + 5, x] = result[5];
                            buffer[yCopy + 6, x] = result[6];
                            buffer[yCopy + 7, x] = result[7];
                        }
                    });
                }
            } else
            {
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
                            switch (type)
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

            }
            await Task.WhenAll(tasks).ConfigureAwait(false);

            return buffer;
        }

        public async Task<float[,]> NoiseMap(int iterations, FractalType type, bool t, params int[] vals)
        {
            int len = vals.Length;
            int yLen = (len < 2 || vals[1] == 0) ? 1 : vals[1];
            int xLen = vals[0];
            Task[] tasks = new Task[yLen];

            var buffer = new float[yLen, xLen];
            Setup(len);

            if (len == 3 && t)
            {
                tasks = new Task[yLen / 8];
                for (int y = 0, index = 0; y < yLen; y += 8, index++)
                {
                    int yCopy = y;
                    tasks[index] = Task.Run(() =>
                    {
                        Vector<float> yin = new Vector<float>(new float[] { yCopy, yCopy + 1, yCopy + 2, yCopy + 3, yCopy + 4, yCopy + 5, yCopy + 6, yCopy + 7 });

                        for (int x = 0; x < xLen; ++x)
                        {
                            Vector<float> result = FractalFBM(iterations, new Vector<float>(x), yin, new Vector<float>(vals[2]));
                            buffer[yCopy, x] = result[0];
                            buffer[yCopy + 1, x] = result[1];
                            buffer[yCopy + 2, x] = result[2];
                            buffer[yCopy + 3, x] = result[3];
                            buffer[yCopy + 4, x] = result[4];
                            buffer[yCopy + 5, x] = result[5];
                            buffer[yCopy + 6, x] = result[6];
                            buffer[yCopy + 7, x] = result[7];
                        }
                    });
                }
            }
            else
            {
                for (int y = 0; y < yLen; ++y)
                {
                    int yCopy = y;
                    tasks[y] = Task.Run(() =>
                    {
                        for (int x = 0; x < xLen; ++x)
                        {
                            buffer[yCopy, x] = FractalFBM(iterations, x, yCopy, vals[2]);
                        }
                    });
                }

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

        public Vector<float> FractalFBM(int iterations, Vector<float> xin, Vector<float> yin, Vector<float> zin)
        {
            Vector<float> maxAmp = Vector<float>.Zero;
            Vector<float> amp = Vector<float>.One;
            Vector<float> freq = new Vector<float>((float)Scale);
            Vector<float> noise = Vector<float>.Zero;

            for (int i = 0; i < iterations; ++i)
            {
                noise += Noise(xin * freq, yin * freq, zin * freq) * amp;
                maxAmp += amp;
                amp *= (float)Persistence;
                freq *= 2;
            }

            noise /= maxAmp;

            return noise;
        }

        private static Vector<int> FastFloor(Vector<float> vals)
        {
#if NET5_0_OR_GREATER
            return Vector.ConvertToInt32(Vector.Floor(vals));
#else
            Vector<int> xi = Vector.ConvertToInt32(vals);

            return Vector.ConditionalSelect(Vector.LessThan(vals, Vector.ConvertToSingle(xi)), xi - Vector<int>.One, xi);
#endif
        }

        public Vector<float> Noise(Vector<float> xin, Vector<float> yin, Vector<float> zin)
        {
            Vector<float> g = new Vector<float>((float)G);

            Vector<float> s = (xin + yin + zin) * (float)F;
            Vector<int> i = FastFloor(xin + s);
            Vector<int> j = FastFloor(yin + s);
            Vector<int> k = FastFloor(zin + s);
            Vector<float> i0 = Vector.ConvertToSingle(i);
            Vector<float> j0 = Vector.ConvertToSingle(j);
            Vector<float> k0 = Vector.ConvertToSingle(k);
            Vector<float> t = (i0 + j0 + k0) * g;
            Vector<float> x0 = xin - (i0 - t);
            Vector<float> y0 = yin - (j0 - t);
            Vector<float> z0 = zin - (k0 - t);

            Vector<int> xranks = Vector.GreaterThan(x0, y0) + Vector.GreaterThan(x0, z0);
            Vector<int> yranks = Vector.GreaterThan(y0, z0) + Vector.LessThanOrEqual(x0, y0);
            Vector<int> zranks = Vector.LessThanOrEqual(y0, z0) + Vector.LessThanOrEqual(x0, z0);

            // Round 1
            t = new Vector<float>(0.6f) - (x0 * x0) - (y0 * y0) - (z0 * z0);
            Vector<float> n = Vector.ConditionalSelect(Vector.GreaterThanOrEqual(t, Vector<float>.Zero), grad(i, j, k, x0, y0, z0, t), Vector<float>.Zero);

            // Round 2
            Vector<int> i1 = Vector.ConditionalSelect(Vector.LessThanOrEqual(xranks, new Vector<int>(-2)), Vector<int>.One, Vector<int>.Zero);
            Vector<int> j1 = Vector.ConditionalSelect(Vector.LessThanOrEqual(yranks, new Vector<int>(-2)), Vector<int>.One, Vector<int>.Zero);
            Vector<int> k1 = Vector.ConditionalSelect(Vector.LessThanOrEqual(zranks, new Vector<int>(-2)), Vector<int>.One, Vector<int>.Zero);
            Vector<float> x1 = x0 - Vector.ConvertToSingle(i1) + g;
            Vector<float> y1 = y0 - Vector.ConvertToSingle(j1) + g;
            Vector<float> z1 = z0 - Vector.ConvertToSingle(k1) + g;

            t = new Vector<float>(0.6f) - (x1 * x1) - (y1 * y1) - (z1 * z1);
            n += Vector.ConditionalSelect(Vector.GreaterThanOrEqual(t, Vector<float>.Zero), grad(i + i1, j + j1, k + k1, x1, y1, z1, t), Vector<float>.Zero);

            // Round 3
            i1 = Vector.ConditionalSelect(Vector.LessThanOrEqual(xranks, -Vector<int>.One), Vector<int>.One, Vector<int>.Zero);
            j1 = Vector.ConditionalSelect(Vector.LessThanOrEqual(yranks, -Vector<int>.One), Vector<int>.One, Vector<int>.Zero);
            k1 = Vector.ConditionalSelect(Vector.LessThanOrEqual(zranks, -Vector<int>.One), Vector<int>.One, Vector<int>.Zero);
            x1 = x0 - Vector.ConvertToSingle(i1) + 2.0f * g;
            y1 = y0 - Vector.ConvertToSingle(j1) + 2.0f * g;
            z1 = z0 - Vector.ConvertToSingle(k1) + 2.0f * g;

            t = new Vector<float>(0.6f) - (x1 * x1) - (y1 * y1) - (z1 * z1);
            n += Vector.ConditionalSelect(Vector.GreaterThanOrEqual(t, Vector<float>.Zero), grad(i + i1, j + j1, k + k1, x1, y1, z1, t), Vector<float>.Zero);

            // Round 4
            Vector<float> g3 = 3.0f * g;
            x1 = x0 - Vector<float>.One + g3;
            y1 = y0 - Vector<float>.One + g3;
            z1 = z0 - Vector<float>.One + g3;

            t = new Vector<float>(0.6f) - (x1 * x1) - (y1 * y1) - (z1 * z1);
            n += Vector.ConditionalSelect(Vector.GreaterThanOrEqual(t, Vector<float>.Zero), grad(i + Vector<int>.One, j + Vector<int>.One, k + Vector<int>.One, x1, y1, z1, t), Vector<float>.Zero);

            return n * 32.0f;
        }

        private Vector<float> grad(Vector<int> i, Vector<int> j, Vector<int> k, Vector<float> x, Vector<float> y, Vector<float> z, Vector<float> t)
        {
            Vector<uint> hash = new Vector<uint>(Seed);
            hash ^= (Vector<uint>)(1619 * i);
            hash ^= (Vector<uint>)(31337 * j);
            hash ^= (Vector<uint>)(6971 * k);

            hash = hash * hash * hash * 60493;
            hash = (hash / new Vector<uint>(32768)) ^ hash;

            t *= t;
            return t * t * (Vector.ConditionalSelect(Vector.Equals((Vector<int>)(hash & Vector<uint>.One), Vector<int>.Zero), -z, z) + Vector.ConditionalSelect(Vector.Equals((Vector<int>)(hash & new Vector<uint>(2)), Vector<int>.Zero), -y, y) + Vector.ConditionalSelect(Vector.Equals((Vector<int>)(hash & new Vector<uint>(4)), Vector<int>.Zero), -x, x));
        }

        public Vector<float> Noise(Vector<float> xin, Vector<float> yin)
        {
            Vector<float> g = new Vector<float>((float)G);

            Vector<float> s = (xin + yin) * (float)F;
            Vector<int> i = FastFloor(xin + s);
            Vector<int> j = FastFloor(yin + s);
            Vector<float> i0 = Vector.ConvertToSingle(i);
            Vector<float> j0 = Vector.ConvertToSingle(j);
            Vector<float> t = (i0 + j0) * g;
            Vector<float> x0 = xin - (i0 - t);
            Vector<float> y0 = yin - (j0 - t);

            Vector<int> xranks = Vector.GreaterThan(x0, y0);
            Vector<int> yranks = Vector.LessThanOrEqual(x0, y0);

            // Round 1
            t = new Vector<float>(0.6f) - (x0 * x0) - (y0 * y0);
            Vector<float> n = Vector.ConditionalSelect(Vector.GreaterThanOrEqual(t, Vector<float>.Zero), grad(i, j, x0, y0, t), Vector<float>.Zero);

            // Round 2
            Vector<int> i1 = Vector.ConditionalSelect(Vector.LessThanOrEqual(xranks, new Vector<int>(-1)), Vector<int>.One, Vector<int>.Zero);
            Vector<int> j1 = Vector.ConditionalSelect(Vector.LessThanOrEqual(yranks, new Vector<int>(-1)), Vector<int>.One, Vector<int>.Zero);
            Vector<float> x1 = x0 - Vector.ConvertToSingle(i1) + g;
            Vector<float> y1 = y0 - Vector.ConvertToSingle(j1) + g;

            t = new Vector<float>(0.6f) - (x1 * x1) - (y1 * y1);
            n += Vector.ConditionalSelect(Vector.GreaterThanOrEqual(t, Vector<float>.Zero), grad(i + i1, j + j1, x1, y1, t), Vector<float>.Zero);

            // Round 4
            Vector<float> g3 = 3.0f * g;
            x1 = x0 - Vector<float>.One + g3;
            y1 = y0 - Vector<float>.One + g3;

            t = new Vector<float>(0.6f) - (x1 * x1) - (y1 * y1);
            n += Vector.ConditionalSelect(Vector.GreaterThanOrEqual(t, Vector<float>.Zero), grad(i + Vector<int>.One, j + Vector<int>.One, x1, y1, t), Vector<float>.Zero);

            return n * 32.0f;
        }

        private Vector<float> grad(Vector<int> i, Vector<int> j, Vector<float> x, Vector<float> y, Vector<float> t)
        {
            Vector<uint> hash = new Vector<uint>(Seed);
            hash ^= (Vector<uint>)(1619 * i);
            hash ^= (Vector<uint>)(31337 * j);

            hash = hash * hash * hash * 60493;
            hash = (hash / new Vector<uint>(32768)) ^ hash;

            t *= t;
            return t * t * (Vector.ConditionalSelect(Vector.Equals((Vector<int>)(hash & Vector<uint>.One), Vector<int>.Zero), -y, y) + Vector.ConditionalSelect(Vector.Equals((Vector<int>)(hash & new Vector<uint>(2)), Vector<int>.Zero), -x, x));
        }


        public Vector<float> Noise(Vector<float> xin, Vector<float> yin, Vector<float> zin, Vector<float> win)
        {
            Vector<float> g = new Vector<float>((float)G);

            Vector<float> s = (xin + yin + zin + win) * (float)F;
            Vector<int> i = FastFloor(xin + s);
            Vector<int> j = FastFloor(yin + s);
            Vector<int> k = FastFloor(zin + s);
            Vector<int> l = FastFloor(win + s);

            Vector<float> i0 = Vector.ConvertToSingle(i);
            Vector<float> j0 = Vector.ConvertToSingle(j);
            Vector<float> k0 = Vector.ConvertToSingle(k);
            Vector<float> l0 = Vector.ConvertToSingle(l);

            Vector<float> t = (i0 + j0 + k0 + l0) * g;

            Vector<float> x0 = xin - (i0 - t);
            Vector<float> y0 = yin - (j0 - t);
            Vector<float> z0 = zin - (k0 - t);
            Vector<float> w0 = win - (l0 - t);

            Vector<int> xranks = Vector.GreaterThan(x0, y0) + Vector.GreaterThan(x0, z0) + Vector.GreaterThan(x0, w0);
            Vector<int> yranks = Vector.GreaterThan(y0, z0) + Vector.LessThanOrEqual(x0, y0) + Vector.GreaterThan(y0, w0);
            Vector<int> zranks = Vector.LessThanOrEqual(y0, z0) + Vector.LessThanOrEqual(x0, z0) + Vector.GreaterThan(z0, w0);
            Vector<int> wranks = Vector.LessThanOrEqual(y0, w0) + Vector.LessThanOrEqual(x0, w0) + Vector.LessThanOrEqual(z0, w0);

            // Round 1
            t = new Vector<float>(0.6f) - (x0 * x0) - (y0 * y0) - (z0 * z0) - (w0 * w0);
            Vector<float> n = Vector.ConditionalSelect(Vector.GreaterThanOrEqual(t, Vector<float>.Zero), grad(i, j, k, l, x0, y0, z0, w0, t), Vector<float>.Zero);

            // Round 2
            Vector<int> r = new Vector<int>(-3);
            Vector<int> i1 = Vector.ConditionalSelect(Vector.LessThanOrEqual(xranks, r), Vector<int>.One, Vector<int>.Zero);
            Vector<int> j1 = Vector.ConditionalSelect(Vector.LessThanOrEqual(yranks, r), Vector<int>.One, Vector<int>.Zero);
            Vector<int> k1 = Vector.ConditionalSelect(Vector.LessThanOrEqual(zranks, r), Vector<int>.One, Vector<int>.Zero);
            Vector<int> l1 = Vector.ConditionalSelect(Vector.LessThanOrEqual(wranks, r), Vector<int>.One, Vector<int>.Zero);
            Vector<float> x1 = x0 - Vector.ConvertToSingle(i1) + g;
            Vector<float> y1 = y0 - Vector.ConvertToSingle(j1) + g;
            Vector<float> z1 = z0 - Vector.ConvertToSingle(k1) + g;
            Vector<float> w1 = w0 - Vector.ConvertToSingle(l1) + g;

            t = new Vector<float>(0.6f) - (x1 * x1) - (y1 * y1) - (z1 * z1) - (w1 * w1);
            n += Vector.ConditionalSelect(Vector.GreaterThanOrEqual(t, Vector<float>.Zero), grad(i + i1, j + j1, k + k1, l + l1, x1, y1, z1, w1, t), Vector<float>.Zero);

            // Round 3
            r = new Vector<int>(-2);
            i1 = Vector.ConditionalSelect(Vector.LessThanOrEqual(xranks, r), Vector<int>.One, Vector<int>.Zero);
            j1 = Vector.ConditionalSelect(Vector.LessThanOrEqual(yranks, r), Vector<int>.One, Vector<int>.Zero);
            k1 = Vector.ConditionalSelect(Vector.LessThanOrEqual(zranks, r), Vector<int>.One, Vector<int>.Zero);
            l1 = Vector.ConditionalSelect(Vector.LessThanOrEqual(wranks, r), Vector<int>.One, Vector<int>.Zero);
            Vector<float> g2 = 2.0f * g;
            x1 = x0 - Vector.ConvertToSingle(i1) + g2;
            y1 = y0 - Vector.ConvertToSingle(j1) + g2;
            z1 = z0 - Vector.ConvertToSingle(k1) + g2;
            w1 = w0 - Vector.ConvertToSingle(l1) + g2;

            t = new Vector<float>(0.6f) - (x1 * x1) - (y1 * y1) - (z1 * z1) - (w1 * w1);
            n += Vector.ConditionalSelect(Vector.GreaterThanOrEqual(t, Vector<float>.Zero), grad(i + i1, j + j1, k + k1, l + l1, x1, y1, z1, w1, t), Vector<float>.Zero);

            // Round 4
            r = new Vector<int>(-1);
            i1 = Vector.ConditionalSelect(Vector.LessThanOrEqual(xranks, r), Vector<int>.One, Vector<int>.Zero);
            j1 = Vector.ConditionalSelect(Vector.LessThanOrEqual(yranks, r), Vector<int>.One, Vector<int>.Zero);
            k1 = Vector.ConditionalSelect(Vector.LessThanOrEqual(zranks, r), Vector<int>.One, Vector<int>.Zero);
            l1 = Vector.ConditionalSelect(Vector.LessThanOrEqual(wranks, r), Vector<int>.One, Vector<int>.Zero);
            g2 = 3.0f * g;
            x1 = x0 - Vector.ConvertToSingle(i1) + g2;
            y1 = y0 - Vector.ConvertToSingle(j1) + g2;
            z1 = z0 - Vector.ConvertToSingle(k1) + g2;
            w1 = w0 - Vector.ConvertToSingle(l1) + g2;

            t = new Vector<float>(0.6f) - (x1 * x1) - (y1 * y1) - (z1 * z1) - (w1 * w1);
            n += Vector.ConditionalSelect(Vector.GreaterThanOrEqual(t, Vector<float>.Zero), grad(i + i1, j + j1, k + k1, l + l1, x1, y1, z1, w1, t), Vector<float>.Zero);

            // Round 5
            g2 = 4.0f * g;
            x1 = x0 - Vector<float>.One + g2;
            y1 = y0 - Vector<float>.One + g2;
            z1 = z0 - Vector<float>.One + g2;
            w1 = w0 - Vector<float>.One + g2;

            t = new Vector<float>(0.6f) - (x1 * x1) - (y1 * y1) - (z1 * z1) - (w1 * w1);
            n += Vector.ConditionalSelect(Vector.GreaterThanOrEqual(t, Vector<float>.Zero), grad(i + Vector<int>.One, j + Vector<int>.One, k + Vector<int>.One, l + Vector<int>.One, x1, y1, z1, w1, t), Vector<float>.Zero);

            return n * 32.0f;
        }

        private Vector<float> grad(Vector<int> i, Vector<int> j, Vector<int> k, Vector<int> l, Vector<float> x, Vector<float> y, Vector<float> z, Vector<float> w, Vector<float> t)
        {
            Vector<uint> hash = new Vector<uint>(Seed);
            hash ^= (Vector<uint>)(1619 * i);
            hash ^= (Vector<uint>)(31337 * j);
            hash ^= (Vector<uint>)(6971 * k);
            hash ^= (Vector<uint>)(1013 * l);

            hash = hash * hash * hash * 60493;
            hash = (hash / new Vector<uint>(32768)) ^ hash;

            t *= t;
            return t * t * (Vector.ConditionalSelect(Vector.Equals((Vector<int>)(hash & Vector<uint>.One), Vector<int>.Zero), -w, w) + Vector.ConditionalSelect(Vector.Equals((Vector<int>)(hash & new Vector<uint>(2)), Vector<int>.Zero), -z, z) + Vector.ConditionalSelect(Vector.Equals((Vector<int>)(hash & new Vector<uint>(4)), Vector<int>.Zero), -y, y) + Vector.ConditionalSelect(Vector.Equals((Vector<int>)(hash & new Vector<uint>(8)), Vector<int>.Zero), -x, x));
        }
    }
}
