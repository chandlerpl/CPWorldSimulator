using CPWS.WorldGenerator.Noise;
using CPWS.WorldGenerator.PoissonDisc;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CPWS.Test
{
    class PoissonDiscTest : CP.Common.Commands.Command
    {
        public override bool Init()
        {
            Name = "PoissonDisc";
            Desc = "Generates a Poisson Disc Sample";
            Aliases = new List<string>() { "poissondisc" };
            ProperUse = "poissondisc [samples (default: 10)] [radius (default: 10)]";

            return true;
        }

        public override bool Execute(object obj, List<string> args)
        {
            int samples = args.Count > 1 ? int.Parse(args[1]) : 10;
            double radius = args.Count > 2 ? double.Parse(args[2]) : 0.5;

            runTest(samples, radius);

            return true;
        }

        private void runTest(int samples, double radius)
        {
            double[] times = new double[samples];

            Stopwatch watch = new Stopwatch();
            Random rand = new Random();

            Console.WriteLine("Cores: " + Environment.ProcessorCount);
            Console.WriteLine("64bit:" + Environment.Is64BitProcess + Environment.NewLine);

            int count = 0;
            for (int s = 0; s < samples; s++)
            {
                watch.Restart();
                List<PoissonDisc> discs = PoissonDiscSampling.Sample2D(13235, radius, new CP.Common.Maths.Vector3D(100, 100, 100), 8, false);
                watch.Stop();
                count = discs.Count;
                times[s] = watch.Elapsed.TotalMilliseconds;
            }
            Console.WriteLine(samples + ": avg => " + times.Average() + " | best => " + times.Min() + " | worst => " + times.Max() + " | count => " + count);
        }
    }
}
