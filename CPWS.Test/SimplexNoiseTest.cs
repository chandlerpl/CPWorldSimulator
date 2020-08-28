using CP.Common.Commands;
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
    class SimplexNoiseTest : CPCommand
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

            runTest(samples, perms, dims);
            
            return true;
        }

        private async void runTest(int samples, int permutations, params int[] dims)
        {
            double[] times = new double[samples];

            Stopwatch watch = new Stopwatch();
            Random rand = new Random();

            for (int s = 0; s < samples; s++)
            {
                SimplexNoise2 noise = new SimplexNoise2((uint)rand.Next(0, 999999999), 0.005, 0.5, false);
                watch.Restart();
                Task t = noise.NoiseMap(permutations, dims);

                await Task.WhenAll(t);
                watch.Stop();
                times[s] = watch.Elapsed.TotalMilliseconds;
            }

            Console.WriteLine("Processor:" + Environment.ProcessorCount);
            ThreadPool.GetMinThreads(out int minThread, out int comp);
            Console.WriteLine("MinThread:" + minThread);
            ThreadPool.GetMinThreads(out int maxThread, out int comp1);
            Console.WriteLine("MaxThread:" + maxThread);
            Console.WriteLine("64bit:" + Environment.Is64BitProcess + Environment.NewLine);

            var res = (dims[0] + "x" + dims[1]);

            Console.WriteLine(res + ": avg => " + times.Average() + " | best => " + times.Min() + " | worst => " + times.Max());

            Console.WriteLine("Finished!");
        }
    }
}
