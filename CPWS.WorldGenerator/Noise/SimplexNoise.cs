using System;
using System.Linq;
using System.Threading.Tasks;
using CPWS.CUDA.Noise;
using CP.Common.Utilities;

namespace CPWS.WorldGenerator.Noise
{
    public class SimplexNoise : NoiseGen
    {
        private static int[][] grad3 =
        {
            new int[] {1,1,0},
            new int[] {-1,1,0},
            new int[] {1,-1,0},
            new int[] {-1,-1,0},
            new int[] {1,0,1},
            new int[] {-1,0,1},
            new int[] {1,0,-1},
            new int[] {-1,0,-1},
            new int[] {0,1,1},
            new int[] {0,-1,1},
            new int[] {0,1,-1},
            new int[] {0,-1,-1},
        };

        private static readonly int[][] grad4 =
        {
            new int[] {0,1,1,1},    new int[] {0,1,1,-1},   new int[] {0,1,-1,1},   new int[] {0,1,-1,-1},
            new int[] {0,-1,1,1},   new int[] {0,-1,1,-1},  new int[] {0,-1,-1,1},  new int[] {0,-1,-1,-1},
            new int[] {1,0,1,1},    new int[] {1,0,1,-1},   new int[] {1,0,-1,1},   new int[] {1,0,-1,-1},
            new int[] {-1,0,1,1},   new int[] {-1,0,1,-1},  new int[] {-1,0,-1,1},  new int[] {-1,0,-1,-1},
            new int[] {1,1,0,1},    new int[] {1,1,0,-1},   new int[] {1,-1,0,1},   new int[] {1,-1,0,-1},
            new int[] {-1,1,0,1},   new int[] {-1,1,0,-1},  new int[] {-1,-1,0,1},  new int[] {-1,-1,0,-1},
            new int[] {1,1,1,0},    new int[] {1,1,-1,0},   new int[] {1,-1,1,0},   new int[] {1,-1,-1,0},
            new int[] {-1,1,1,0},   new int[] {-1,1,-1,0},  new int[] {-1,-1,1,0},  new int[] {-1,-1,-1,0},
        };

        private static short[] source =
        {
            151,160,137,91,90,15, 131,13,201,95,96,53,194,233,7,225,140,36,103,30,69,142,8,99,37,240,21,10,23,
            190, 6,148,247,120,234,75,0,26,197,62,94,252,219,203,117,35,11,32,57,177,33,
            88,237,149,56,87,174,20,125,136,171,168, 68,175,74,165,71,134,139,48,27,166,
            77,146,158,231,83,111,229,122,60,211,133,230,220,105,92,41,55,46,245,40,244,
            102,143,54, 65,25,63,161, 1,216,80,73,209,76,132,187,208, 89,18,169,200,196,
            135,130,116,188,159,86,164,100,109,198,173,186, 3,64,52,217,226,250,124,123,
            5,202,38,147,118,126,255,82,85,212,207,206,59,227,47,16,58,17,182,189,28,42,
            223,183,170,213,119,248,152, 2,44,154,163, 70,221,153,101,155,167, 43,172,9,
            129,22,39,253, 19,98,108,110,79,113,224,232,178,185, 112,104,218,246,97,228,
            251,34,242,193,238,210,144,12,191,179,162,241, 81,51,145,235,249,14,239,107,
            49,192,214, 31,181,199,106,157,184, 84,204,176,115,121,50,45,127, 4,150,254,
            138,236,205,93,222,114,67,29,24,72,243,141,128,195,78,66,215,61,156,180
        };

        private static int[,] simplex = {
            {0,1,2,3},{0,1,3,2},{0,0,0,0},{0,2,3,1},{0,0,0,0},{0,0,0,0},{0,0,0,0},{1,2,3,0},
            {0,2,1,3},{0,0,0,0},{0,3,1,2},{0,3,2,1},{0,0,0,0},{0,0,0,0},{0,0,0,0},{1,3,2,0},
            {0,0,0,0},{0,0,0,0},{0,0,0,0},{0,0,0,0},{0,0,0,0},{0,0,0,0},{0,0,0,0},{0,0,0,0},
            {1,2,0,3},{0,0,0,0},{1,3,0,2},{0,0,0,0},{0,0,0,0},{0,0,0,0},{2,3,0,1},{2,3,1,0},
            {1,0,2,3},{1,0,3,2},{0,0,0,0},{0,0,0,0},{0,0,0,0},{2,0,3,1},{0,0,0,0},{2,1,3,0},
            {0,0,0,0},{0,0,0,0},{0,0,0,0},{0,0,0,0},{0,0,0,0},{0,0,0,0},{0,0,0,0},{0,0,0,0},
            {2,0,1,3},{0,0,0,0},{0,0,0,0},{0,0,0,0},{3,0,1,2},{3,0,2,1},{0,0,0,0},{3,1,2,0},
            {2,1,0,3},{0,0,0,0},{0,0,0,0},{0,0,0,0},{3,1,0,2},{0,0,0,0},{3,2,0,1},{3,2,1,0}
        };

        private short[] Perm;

        public SimplexNoise(uint seed, double scale, double persistence, bool useCuda = true) : base(seed)
        {
            UseCuda = useCuda;

            Scale = scale;
            Persistence = persistence;
            Seed = seed * offset;

            generatePermutations(Seed);
        }

        ~SimplexNoise()
        {

        }

        private void generatePermutations(uint seed)
        {
            Perm = new short[512];

            for (int i = 0; i < 512; i++)
            {
                int r = (int)((seed + 31) % (i + 1));
                if (r < 0)
                    r += (i + 1);

                Perm[i] = source[r & 255];
            }
        }


        double FValues(double dim)
        {
            return (Math.Sqrt(dim + 1) - 1) / dim;
        }

        double GValues(double dim)
        {
            return ((dim + 1) - Math.Sqrt(dim + 1)) / ((dim + 1) * dim);
        }

        public override double Noise(double[] vals)
        {
            switch (vals.Length)
            {
                case 1:
                    return Noise1D(vals[0]);
                case 2:
                    return Noise2D(vals[0], vals[1]);
                case 3:
                    return Noise3D(vals[0], vals[1], vals[2]);
                case 4:
                    return Noise4D(vals[0], vals[1], vals[2], vals[3]);
                default:
                    throw new NotImplementedException();
            }
        }

        public override double Octave(int iterations, double[] vals)
        {
            switch (vals.Length)
            {
                case 1:
                    return Octave1D(iterations, vals[0]);
                case 2:
                    return Octave2D(iterations, vals[0], vals[1]);
                case 3:
                    return Octave3D(iterations, vals[0], vals[1], vals[2]);
                case 4:
                    return Octave4D(iterations, vals[0], vals[1], vals[2], vals[3]);
                default:
                    throw new NotImplementedException();
            }
        }

        public override async Task<double[,]> NoiseMap(int iterations, int[] vals)
        {
            switch (vals.Length)
            {
                case 1:
                    return await NoiseMap1D(iterations, vals[0]);
                case 2:
                    return await NoiseMap2D(iterations, vals[0], vals[1]);
                case 3:
                    if(UseCuda)
                        return SimplexNoiseCUDA.NoiseMap(Perm, Scale, Persistence, iterations, vals[0], vals[1], vals[2], GPUUtilities.GetStrongestCudaGpu());
                    else
                        return await NoiseMap3D(iterations, vals[0], vals[1], vals[2]);
                case 4:
                    return await NoiseMap4D(iterations, vals[0], vals[1], vals[2], vals[3]);
                default:
                    throw new NotImplementedException();
            }
        }

        public override double[,] NoiseMapNotAsync(int iterations, params int[] vals)
        {
            switch (vals.Length)
            {
                case 1:
                    return NoiseMap1D(iterations, vals[0]).Result;
                case 3:
                    return NoiseMap3DNA(iterations, vals[0], vals[1], vals[2]);
                default:
                    throw new NotImplementedException();
            }
        }

        public async Task<double[,]> NoiseMap1D(int iterations, int length)
        {
            var buffer = new double[1, length];

            for (int y = 0; y < length; y++)
            {
                buffer[0, y] = Octave1D(iterations, y);
            }

            return buffer;
        }

        private double Octave1D(int iterations, double xin)
        {
            double maxAmp = 0;
            double amp = 1;
            double freq = Scale;
            double noise = 0;

            for (int i = 0; i < iterations; ++i)
            {
                noise += Noise1D(xin * freq) * amp;
                maxAmp += amp;
                amp *= Persistence;
                freq *= 2;
            }

            return noise / maxAmp;
        }

        private double Noise1D(double xin)
        {
            double n0, n1;

            double G = GValues(1);
            double s = xin * FValues(1);
            int i = (int)Math.Floor(xin + s);
            double t = i * G;
            double X0 = i - t;
            double x0 = xin - X0;

            int i1 = 1;

            double x1 = x0 - i1 + G;

            int ii = i & 255;
            int gi0 = Perm[ii] % 12;
            int gi1 = Perm[ii + i1] % 12;

            double t0 = 0.5 - x0 * x0;
            if (t0 < 0) n0 = 0.0;
            else
            {
                t0 *= t0;
                n0 = t0 * t0 * (grad3[gi0][0] * x0);
            }
            double t1 = 0.5 - x1 * x1;
            if (t1 < 0) n1 = 0.0;
            else
            {
                t1 *= t1;
                n1 = t1 * t1 * (grad3[gi1][0] * x1);
            }

            return 70.0 * (n0 + n1);
        }

        private async Task<double[,]> NoiseMap2D(int iterations, int width, int height)
        {
            Task[] tasks = new Task[height];

            var buffer = new double[height, width];

            for (int y = 0; y < height; y++)
            {
                int yCopy = y;
                tasks[y] = Task.Factory.StartNew(() =>
                {
                    for (int x = 0; x < width; x++)
                    {
                        buffer[yCopy, x] = Octave2D(iterations, x, yCopy);
                    }
                });
            }

            await Task.WhenAll(tasks);

            return buffer;
        }

        private double Octave2D(int iterations, double xin, double yin)
        {
            double maxAmp = 0;
            double amp = 1;
            double freq = Scale;
            double noise = 0;

            for (int i = 0; i < iterations; ++i)
            {
                noise += Noise2D(xin * freq, yin * freq) * amp;
                maxAmp += amp;
                amp *= Persistence;
                freq *= 2;
            }


            noise /= maxAmp;

            return noise;
        }

        private double Noise2D(double xin, double yin)
        {
            double n0, n1, n2;

            double G = GValues(2);
            double s = (xin + yin) * FValues(2); 
            int i = (int)Math.Floor(xin + s);
            int j = (int)Math.Floor(yin + s);
            double t = (i + j) * G;
            double X0 = i - t;
            double Y0 = j - t;
            double x0 = xin - X0;
            double y0 = yin - Y0;

            int i1, j1;
            if (x0 > y0) { i1 = 1; j1 = 0; }
            else { i1 = 0; j1 = 1; }

            double x1 = x0 - i1 + G;
            double y1 = y0 - j1 + G;
            double x2 = x0 - 1.0 + 2.0 * G;
            double y2 = y0 - 1.0 + 2.0 * G;

            int ii = i & 255;
            int jj = j & 255;
            int gi0 = Perm[ii + Perm[jj]] % 12;
            int gi1 = Perm[ii + i1 + Perm[jj + j1]] % 12;
            int gi2 = Perm[ii + 1 + Perm[jj + 1]] % 12;

            double t0 = 0.6 - x0 * x0 - y0 * y0;
            if (t0 < 0) n0 = 0.0;
            else
            {
                t0 *= t0;
                n0 = t0 * t0 * dot(grad3[gi0], x0, y0);
            }
            double t1 = 0.6 - x1 * x1 - y1 * y1;
            if (t1 < 0) n1 = 0.0;
            else
            {
                t1 *= t1;
                n1 = t1 * t1 * dot(grad3[gi1], x1, y1);
            }
            double t2 = 0.6 - x2 * x2 - y2 * y2;
            if (t2 < 0) n2 = 0.0;
            else
            {
                t2 *= t2;
                n2 = t2 * t2 * dot(grad3[gi2], x2, y2);
            }
            // Add contributions from each corner to get the final noise value.
            // The result is scaled to return values in the interval [-1,1].
            return 30.0 * (n0 + n1 + n2);
        }

        private async Task<double[,]> NoiseMap3D(int iterations, int width, int height, int layer)
        {
            Task[] tasks = new Task[height];

            var buffer = new double[height, width];

            for (int y = 0; y < height; y++)
            {
                int yCopy = y;
                tasks[y] = Task.Factory.StartNew(() =>
                {
                    for (int x = 0; x < width; x++)
                    {
                        buffer[yCopy, x] = Octave3D(iterations, x, yCopy, layer);
                    }
                });
            }

            await Task.WhenAll(tasks);

            return buffer;
        }

        private double[,] NoiseMap3DNA(int iterations, int width, int height, int layer)
        {
            var buffer = new double[height, width];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    buffer[y, x] = Octave3D(iterations, x, y, layer);
                }
            }

            return buffer;
        }

        private async Task<double[,]> NoiseMap3D2(int iterations, int width, int height, int layer)
        {
            Task[] tasks = new Task[height];

            var buffer = new double[height, width];

            for (int y = 0; y < height; y++)
            {
                int yCopy = y;
                tasks[y] = Task.Factory.StartNew(() =>
                {
                    for (int x = 0; x < width; x++)
                    {
                        double val = Octave3D(iterations, x, yCopy, layer);

                        if (val < 0)
                            val = -1;

                        buffer[yCopy, x] = val;
                    }
                });
            }

            await Task.WhenAll(tasks);

            return buffer;
        }

        private double Octave3D(int iterations, double xin, double yin, double zin)
        {
            double maxAmp = 0;
            double amp = 1;
            double freq = Scale;
            double noise = 0;

            for (int i = 0; i < iterations; ++i)
            {
                noise += Noise3D(xin * freq, yin * freq, zin * freq) * amp;
                maxAmp += amp;
                amp *= Persistence;
                freq *= 2;
            }

            noise /= maxAmp;

            return noise;
        }

        private double Noise3D(double xin, double yin, double zin)
        {
            double n0, n1, n2, n3;

            double G = GValues(3);
            double s = (xin + yin + zin) * FValues(3);
            int i = (int)Math.Floor(xin + s);
            int j = (int)Math.Floor(yin + s);
            int k = (int)Math.Floor(zin + s);
            double t = (i + j + k) * G;
            double X0 = i - t;
            double Y0 = j - t;
            double Z0 = k - t;
            double x0 = xin - X0;
            double y0 = yin - Y0;
            double z0 = zin - Z0;

            int i1, j1, k1;
            int i2, j2, k2;
            if (x0 >= y0)
            {
                if (y0 >= z0)
                { i1 = 1; j1 = 0; k1 = 0; i2 = 1; j2 = 1; k2 = 0; }
                else if (x0 >= z0) { i1 = 1; j1 = 0; k1 = 0; i2 = 1; j2 = 0; k2 = 1; }
                else { i1 = 0; j1 = 0; k1 = 1; i2 = 1; j2 = 0; k2 = 1; }
            }
            else
            { // x0<y0
                if (y0 < z0) { i1 = 0; j1 = 0; k1 = 1; i2 = 0; j2 = 1; k2 = 1; }
                else if (x0 < z0) { i1 = 0; j1 = 1; k1 = 0; i2 = 0; j2 = 1; k2 = 1; }
                else { i1 = 0; j1 = 1; k1 = 0; i2 = 1; j2 = 1; k2 = 0; }
            }

            double x1 = x0 - i1 + G;
            double y1 = y0 - j1 + G;
            double z1 = z0 - k1 + G;
            double x2 = x0 - i2 + 2.0 * G;
            double y2 = y0 - j2 + 2.0 * G;
            double z2 = z0 - k2 + 2.0 * G;
            double x3 = x0 - 1.0 + 3.0 * G;
            double y3 = y0 - 1.0 + 3.0 * G;
            double z3 = z0 - 1.0 + 3.0 * G;

            int ii = i & 255;
            int jj = j & 255;
            int kk = k & 255;

            int gi0 = (Perm[ii + Perm[jj + Perm[kk]]] % 12);
            int gi1 = (Perm[ii + i1 + Perm[jj + j1 + Perm[kk + k1]]] % 12);
            int gi2 = (Perm[ii + i2 + Perm[jj + j2 + Perm[kk + k2]]] % 12);
            int gi3 = (Perm[ii + 1 + Perm[jj + 1 + Perm[kk + 1]]] % 12);

            double t0 = 0.6 - x0 * x0 - y0 * y0 - z0 * z0;
            if (t0 < 0) n0 = 0.0;
            else
            {
                t0 *= t0;
                n0 = t0 * t0 * dot(grad3[gi0], new double[3] { x0, y0, z0 });
            }
            double t1 = 0.6 - x1 * x1 - y1 * y1 - z1 * z1;
            if (t1 < 0) n1 = 0.0;
            else
            {
                t1 *= t1;
                n1 = t1 * t1 * dot(grad3[gi1], new double[3] { x1, y1, z1 });
            }
            double t2 = 0.6 - x2 * x2 - y2 * y2 - z2 * z2;
            if (t2 < 0) n2 = 0.0;
            else
            {
                t2 *= t2;
                n2 = t2 * t2 * dot(grad3[gi2], new double[3] { x2, y2, z2 });
            }
            double t3 = 0.6 - x3 * x3 - y3 * y3 - z3 * z3;
            if (t3 < 0) n3 = 0.0;
            else
            {
                t3 *= t3;
                n3 = t3 * t3 * dot(grad3[gi3], new double[3] { x3, y3, z3 });
            }

            return 32.0 * (n0 + n1 + n2 + n3);
        }

        private double Noise3D2(params double[] dimensionValues)
        {
            int dimensions = dimensionValues.Length;
            double[] nth = new double[dimensions + 1];

            double F = (Math.Sqrt(dimensions + 1) - 1.0) / dimensions;
            double G = (dimensions + 1 - Math.Sqrt(dimensions + 1)) / ((dimensions + 1) * dimensions);

            double s = dimensionValues.Sum() * F;
            int[] floors = new int[dimensions];
            double[] bases = new double[dimensions];

            double t = 0;
            for (int i = 0; i < dimensions; i++)
            {
                int j = (int)Math.Floor(dimensionValues[i] + s);
                t += j;
                floors[i] = j;
            }
            t *= G;

            for (int i = 0; i < dimensions; i++)
            {
                bases[i] = dimensionValues[i] - (floors[i] - t);

                floors[i] = floors[i] & 255;
            }
            
            int c = 0;
            c += (bases[0] > bases[1]) ? 32 : 0;
            c += (bases[0] > bases[2]) ? 16 : 0;
            c += (bases[1] > bases[2]) ? 8 : 0;

            int[,] vals = new int[dimensions + 1, dimensions];

            vals[1, 0] = simplex[c, 0] >= 2 ? 1 : 0;
            vals[1, 1] = simplex[c, 1] >= 2 ? 1 : 0;
            vals[1, 2] = simplex[c, 2] >= 2 ? 1 : 0;
            // The number 2 in the "simplex" array is at the second largest coordinate.
            vals[2, 0] = simplex[c, 0] >= 1 ? 1 : 0;
            vals[2, 1] = simplex[c, 1] >= 1 ? 1 : 0;
            vals[2, 2] = simplex[c, 2] >= 1 ? 1 : 0;

            vals[dimensions, 0] = 1;
            vals[dimensions, 1] = 1;
            vals[dimensions, 2] = 1;
            
            for (int i = 0; i <= dimensions; i++)
            {
                double[] d = new double[dimensions];

                for(int j = 0; j < dimensions; j++)
                    d[j] = bases[j] - vals[i, j] + (i * G);

                int gi = Perm[floors[0] + vals[i, 0] + Perm[floors[1] + vals[i, 1] + Perm[floors[2] + vals[i, 2]]]] % 12;

                double tV = dot(0.5, d);

                if (tV < 0) nth[i] = 0.0;
                else
                {
                    tV *= tV;
                    nth[i] = tV * tV * dot(grad3[gi], d);
                }
            }

            return 80.0 * nth.Sum();
        }

        private async Task<double[,]> NoiseMap4D(int iterations, int width, int height, int layer, int d4)
        {
            Task[] tasks = new Task[height];

            var buffer = new double[height, width];

            for (int y = 0; y < height; y++)
            {
                int yCopy = y;
                tasks[y] = Task.Factory.StartNew(() =>
                {
                    for (int x = 0; x < width; x++)
                    {
                        buffer[yCopy, x] = Octave4D(iterations, x, yCopy, layer, d4);
                    }
                });
            }

            await Task.WhenAll(tasks);

            return buffer;
        }

        private double Octave4D(int iterations, double xin, double yin, double zin, double win)
        {
            double maxAmp = 0;
            double amp = 1;
            double freq = Scale;
            double noise = 0;

            for (int i = 0; i < iterations; ++i)
            {
                noise += Noise4D(xin * freq, yin * freq, zin * freq, win * freq) * amp;
                maxAmp += amp;
                amp *= Persistence;
                freq *= 2;
            }

            noise /= maxAmp;

            return noise;
        }

        private double Noise4D(double xin, double yin, double zin, double win)
        {
            double n0, n1, n2, n3, n4;

            double G = GValues(4);
            double s = (xin + yin + zin + win) * FValues(4);
            int i = (int)Math.Floor(xin + s);
            int j = (int)Math.Floor(yin + s);
            int k = (int)Math.Floor(zin + s);
            int l = (int)Math.Floor(win + s);
            double t = (i + j + k + l) * G;
            double X0 = i - t;
            double Y0 = j - t;
            double Z0 = k - t;
            double W0 = l - t;
            double x0 = xin - X0;
            double y0 = yin - Y0;
            double z0 = zin - Z0;
            double w0 = win - W0;

            int c1 = (x0 > y0) ? 32 : 0;
            int c2 = (x0 > z0) ? 16 : 0;
            int c3 = (y0 > z0) ? 8 : 0;
            int c4 = (x0 > w0) ? 4 : 0;
            int c5 = (y0 > w0) ? 2 : 0;
            int c6 = (z0 > w0) ? 1 : 0;

            int c = c1 + c2 + c3 + c4 + c5 + c6;
            int i1, j1, k1, l1;
            int i2, j2, k2, l2;
            int i3, j3, k3, l3;

            i1 = simplex[c, 0] >= 3 ? 1 : 0;
            j1 = simplex[c, 1] >= 3 ? 1 : 0;
            k1 = simplex[c, 2] >= 3 ? 1 : 0;
            l1 = simplex[c, 3] >= 3 ? 1 : 0;
            // The number 2 in the "simplex" array is at the second largest coordinate.
            i2 = simplex[c, 0] >= 2 ? 1 : 0;
            j2 = simplex[c, 1] >= 2 ? 1 : 0;
            k2 = simplex[c, 2] >= 2 ? 1 : 0;
            l2 = simplex[c, 3] >= 2 ? 1 : 0;

            i3 = simplex[c, 0] >= 1 ? 1 : 0;
            j3 = simplex[c, 1] >= 1 ? 1 : 0;
            k3 = simplex[c, 2] >= 1 ? 1 : 0;
            l3 = simplex[c, 3] >= 1 ? 1 : 0;

            double x1 = x0 - i1 + G;
            double y1 = y0 - j1 + G;
            double z1 = z0 - k1 + G;
            double w1 = w0 - l1 + G;
            double x2 = x0 - i2 + 2.0 * G;
            double y2 = y0 - j2 + 2.0 * G;
            double z2 = z0 - k2 + 2.0 * G;
            double w2 = w0 - l2 + 2.0 * G;
            double x3 = x0 - i3 + 3.0 * G;
            double y3 = y0 - j3 + 3.0 * G;
            double z3 = z0 - k3 + 3.0 * G;
            double w3 = w0 - l3 + 3.0 * G;
            double x4 = x0 - 1.0 + 4.0 * G;
            double y4 = y0 - 1.0 + 4.0 * G;
            double z4 = z0 - 1.0 + 4.0 * G;
            double w4 = w0 - 1.0 + 4.0 * G;

            int ii = i & 255;
            int jj = j & 255;
            int kk = k & 255;
            int ll = l & 255;
            int gi0 = Perm[ii + Perm[jj + Perm[kk + Perm[ll]]]] % 12;
            int gi1 = Perm[ii + i1 + Perm[jj + j1 + Perm[kk + k1 + Perm[ll + l1]]]] % 12;
            int gi2 = Perm[ii + i2 + Perm[jj + j2 + Perm[kk + k2 + Perm[ll + l2]]]] % 12;
            int gi3 = Perm[ii + i3 + Perm[jj + j3 + Perm[kk + k3 + Perm[ll + l3]]]] % 12;
            int gi4 = Perm[ii + 1 + Perm[jj + 1 + Perm[kk + 1 + Perm[ll + 1]]]] % 12;

            double t0 = 0.6 - x0 * x0 - y0 * y0 - z0 * z0 - w0 * w0;
            if (t0 < 0) n0 = 0.0;
            else
            {
                t0 *= t0;
                n0 = t0 * t0 * dot(grad4[gi0], x0, y0, z0, w0);
            }
            double t1 = 0.6 - x1 * x1 - y1 * y1 - z1 * z1 - w1 * w1;
            if (t1 < 0) n1 = 0.0;
            else
            {
                t1 *= t1;
                n1 = t1 * t1 * dot(grad4[gi1], x1, y1, z1, w1);
            }
            double t2 = 0.6 - x2 * x2 - y2 * y2 - z2 * z2 - w2 * w2;
            if (t2 < 0) n2 = 0.0;
            else
            {
                t2 *= t2;
                n2 = t2 * t2 * dot(grad4[gi2], x2, y2, z2, w2);
            }
            double t3 = 0.6 - x3 * x3 - y3 * y3 - z3 * z3 - w3 * w3;
            if (t3 < 0) n3 = 0.0;
            else
            {
                t3 *= t3;
                n3 = t3 * t3 * dot(grad4[gi3], x3, y3, z3, w3);
            }
            double t4 = 0.6 - x4 * x4 - y4 * y4 - z4 * z4 - w4 * w4;
            if (t4 < 0) n4 = 0.0;
            else
            {
                t4 *= t4;
                n4 = t4 * t4 * dot(grad4[gi4], x4, y4, z4, w4);
            }

            return 32.0 * (n0 + n1 + n2 + n3 + n4);
        }

        // This is an attempt to make an nth dimensional Noise function, where N is any number of dimensions.
        // Essentially a Noise function that can handle any number of dimensions.
        private double NoiseND(double[] dimensionValues)
        {
            int dimensions = dimensionValues.Length;
            double[] nth = new double[dimensions];

            double F = (Math.Sqrt(dimensions + 1) - 1.0) / dimensions;
            double G = (dimensions + 1 - Math.Sqrt(dimensions + 1)) / ((dimensions + 1) * dimensions);

            double s = dimensionValues.Sum() * F;

            double[] floors = new double[dimensions];

            double t = 0;
            for (int i = 0; i < dimensions; i++)
            {
                int j = (int)Math.Floor(dimensionValues[i] + s);
                t += j;
                floors[i] = j;
            }
            t *= G;

            double[,] vals = new double[dimensions+1, dimensions];
            double[,] simple = new double[dimensions-1, dimensions];
            for (int i = 0; i < dimensions; i++)
            {
                vals[0, i] = dimensionValues[i] - (floors[i] - t);
            }



            return nth.Sum();// * valueThatIdkYet;
        }

        private static double dot(int[] gradient, params double[] values)
        {
            double result = 0.0;
            for (int i = 0; i < values.Length; i++)
                result += gradient[i] * values[i];

            return result;
        }

        private static double dot(double weight, params double[] values)
        {
            for (int i = 0; i < values.Length; i++)
                weight -= values[i] * values[i];

            return weight;
        }
    }
}
