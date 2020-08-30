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
    class GValuesTest : CPCommand
    {
        public override bool Init()
        {
            Name = "gvalues";
            Desc = "";
            Aliases = new List<string>() { "gvalues" };
            ProperUse = "gvalues";

            return true;
        }

        public override bool Execute(object obj, List<string> args)
        {
            int samples = args.Count > 1 ? int.Parse(args[1]) : 1000;
            runTest(samples);

            return true;
        }

        private async void runTest(int samples)
        {
            double[] times = new double[samples];
            double[] times2 = new double[samples];

            Stopwatch watch = new Stopwatch();
            Random rand = new Random();

            double i = 0;
            for (int s = 0; s < samples; s++)
            {
                int ran = rand.Next(0, 100);
                double ran2 = ran;
                watch.Restart();
                i = ((ran2 + 1) - Math.Sqrt(ran2 + 1)) / ((ran2 + 1) * ran2);
                watch.Stop();
                times[s] = watch.Elapsed.TotalMilliseconds;

                watch.Restart();
                //SimplexNoise2.GValues(ran);
                watch.Stop();
                times2[s] = watch.Elapsed.TotalMilliseconds;
            }

            Console.WriteLine("Processor:" + Environment.ProcessorCount);
            ThreadPool.GetMinThreads(out int minThread, out int comp);
            Console.WriteLine("MinThread:" + minThread);
            ThreadPool.GetMinThreads(out int maxThread, out int comp1);
            Console.WriteLine("MaxThread:" + maxThread);
            Console.WriteLine("64bit:" + Environment.Is64BitProcess + Environment.NewLine);
            
            Console.WriteLine("GValues: avg => " + times2.Average() + " | best => " + times2.Min() + " | worst => " + times2.Max());
            Console.WriteLine("GValues2: avg => " + times.Average() + " | best => " + times.Min() + " | worst => " + times.Max());

            Console.WriteLine("Finished!");
        }
    }
}
