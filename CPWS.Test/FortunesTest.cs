using CP.Common.Commands;
using CP.Common.Maths;
using CPWS.WorldGenerator.CUDA.Noise;
using CPWS.WorldGenerator.Noise;
using CPWS.WorldGenerator.PoissonDisc;
using CPWS.WorldGenerator.Voronoi.BowyerAlgorithm;
using CPWS.WorldGenerator.Voronoi.BowyerAlgorithm.Structures;
using CPWS.WorldGenerator.Voronoi.FortunesAlgorithm;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CPWS.Test
{
    class FortunesTest : TimedCommand
    {
        public override bool Init()
        {
            Name = "fortunes";
            Desc = "Generates a DelaunayTriangulation using Fortunes algorithm.";
            Aliases = new List<string>() { "fortunes" };
            ProperUse = "fortunes";

            return true;
        }

        FortunesAlgorithm fortune;
        public override void PrepareTest(params string[] args)
        {
            Random rand = new Random();
            fortune = new FortunesAlgorithm(rand.Next(0, 999999999));
            fortune.UseDelaunay = true;
            if (args.Length > 0)
            {
                fortune.PointCount = int.Parse(args[0]);
            }
        }

        public override Task RunTest()
        {
            fortune.Generate();

            return Task.CompletedTask;
        }
    }
}
