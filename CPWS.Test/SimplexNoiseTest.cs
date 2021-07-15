using CP.Common.Commands;
using CPWS.WorldGenerator.CUDA.Noise;
using CPWS.WorldGenerator.Noise;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
            noise = new SimplexNoise((uint)rand.Next(0, 999999999), 0.5, 0.005, false);

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
            _ = await noise.NoiseMap(permutations, FractalType.FBM, dims);
        }
    }
}
