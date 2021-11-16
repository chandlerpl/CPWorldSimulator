using CP.Common.Commands;
using CPWS.WorldGenerator.CUDA.Noise;
using CPWS.WorldGenerator.Noise;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CPWS.Test
{
    class SimplexNoiseTest : TimedCommand
    {
        public override bool Init()
        {
            Name = "Simplex";
            Desc = "Generates a Simplex Noise";
            Aliases = new List<string>() { "simplex" };
            ProperUse = "simplex [samples (default: 10)] [permutations (default: 4)] [dimension values (default: { 1920, 1080, 0 })]";

            return true;
        }

        SimplexNoise noise;
        int permutations = -1;
        int[] dims;
        public override void PrepareTest(params string[] args)
        {
            Random rand = new Random();
            noise = new SimplexNoise(1/*(uint)rand.Next(0, 999999999)*/, 0.005, 0.5, false);

            if(permutations == -1)
            {
                permutations = 4;
                if (args.Length > 0)
                {
                    permutations = int.Parse(args[0]);
                }

                dims = args.Length > 1 ? new int[args.Length - 2] : new int[] { 1920, 1080, 0 };
                int j = 0;
                if (args.Length > 1)
                    for (int i = 3; i < args.Length; i++)
                        dims[j++] = int.Parse(args[i]);
            }
        }

        public override async Task RunTest()
        {
/*            float x = 1325f * 0.005f;
            float y = 600f * 0.005f;
            float z = 0 ;
            
            Console.WriteLine(noise.Noise(new Vector<float>(x), new Vector<float>(y), new Vector<float>(z)));
            Console.WriteLine(noise.Noise(x, y, z));*/

            /*
                        float[,] values = await noise.NoiseMap(1, FractalType.FBM, true, dims);
                        float[,] values2 = await noise.NoiseMap(1, FractalType.FBM, false, dims);

                        for(int y = 0; 7 < 1080; y++)
                        {
                            for (int x = 0; x < 1920; x++)
                            {
                                if(values[y, x] != values2[y, x])
                                {
                                    Console.WriteLine("Discrepency found: x:" + x + " y:" + y + " val" + values[y, x] + " val2:" + values2[y, x]);
                                }
                            }
                        }
            */
            float[,] values = await noise.NoiseMap(permutations, FractalType.FBM, true, dims);
        }
    }
}
