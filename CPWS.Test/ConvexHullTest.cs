using CP.Common.Commands;
using CP.Common.Maths;
using CPWS.WorldGenerator.ConvexHull;
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
    class ConvexHullTest : Command
    {
        public override bool Init()
        {
            Name = "ConvexHull";
            Desc = "Generates a ConvexHull Sample";
            Aliases = new List<string>() { "convexhull" };
            ProperUse = "convexhull [samples (default: 10)] [radius (default: 10)]";

            return true;
        }

        public override bool Execute(object obj, List<string> args)
        {
            int samples = args.Count > 1 ? int.Parse(args[1]) : 10;
            double radius = args.Count > 2 ? double.Parse(args[2]) : 1;

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

            for (int s = 0; s < samples; s++)
            {
                List<Vector3D> points = new List<Vector3D>();
                List<PoissonDisc> tPoints = PoissonDiscSampling.Sample3D(84357, radius, new Vector3D(5, 5, 5), 4, false);

                foreach (PoissonDisc disc in tPoints)
                {
                    points.Add(new Vector3D(disc.position.X, disc.position.Y, disc.position.Z));
                }

                ConvexHull3D hull = new ConvexHull3D();
                Console.WriteLine(points.Count);
                watch.Restart();
                points = hull.ConstructHull(points);
                watch.Stop();
                Console.WriteLine(points.Count);
                times[s] = watch.Elapsed.TotalMilliseconds;
            }
            Console.WriteLine(samples + ": avg => " + times.Average() + " | best => " + times.Min() + " | worst => " + times.Max());
        }
    }
}
