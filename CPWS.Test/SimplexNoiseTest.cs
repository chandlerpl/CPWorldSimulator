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
    class SimplexNoiseTest : Command
    {
        public override bool Init()
        {
            Name = "Simplex";
            Desc = "Generates a Simplex Noise";
            Aliases = new List<string>() { "simplex" };
            ProperUse = "simplex [samples (default: 10)] [permutations (default: 4)] [dimension values (default: { 1920, 1080, 0 })]";

            return true;
        }

        public override bool Execute(object obj, List<string> args)
        {
            int samples = args.Count > 1 ? int.Parse(args[1]) : 10;
            int perms = args.Count > 2 ? int.Parse(args[2]) : 4;

            int[] dims = args.Count > 3 ? new int[args.Count - 3] : new int[] { 1920, 1080, 0 };
            int j = 0;
            if (args.Count > 3)
                for (int i = 3; i < args.Count; i++)
                    dims[j++] = int.Parse(args[i]);

            runTest(samples, perms, dims).GetAwaiter().GetResult();

            return true;
        }

        private async Task runTest(int samples, int permutations, params int[] dims)
        {
            double[] times = new double[samples];

            Stopwatch watch = new Stopwatch();
            Random rand = new Random();

            Console.WriteLine("Cores: " + Environment.ProcessorCount);
            Console.WriteLine("64bit:" + Environment.Is64BitProcess + Environment.NewLine);

            var res = (dims[0] + "x" + dims[1]);

            for (int s = 0; s < samples; s++)
            {
                Console.WriteLine("Test");
                SimplexNoise noise = new SimplexNoise((uint)rand.Next(0, 999999999), 0.5, 0.005, false);
                watch.Restart();
                _ = await noise.NoiseMap(4, dims);
                //_ = SimplexNoiseCUDA.NoiseMap(0.5f, 0.005f, 4, 40000, 40000, 0);
                watch.Stop();
                times[s] = watch.Elapsed.TotalMilliseconds;
            }
            Console.WriteLine(res + ": avg => " + times.Average() + " | best => " + times.Min() + " | worst => " + times.Max());
        }
    }
}
