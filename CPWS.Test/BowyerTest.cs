using CP.Common.Commands;
using CP.Common.Maths;
using CPWS.WorldGenerator.CUDA.Noise;
using CPWS.WorldGenerator.Noise;
using CPWS.WorldGenerator.PoissonDisc;
using CPWS.WorldGenerator.Voronoi.BowyerAlgorithm;
using CPWS.WorldGenerator.Voronoi.BowyerAlgorithm.Structures;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CPWS.Test
{
    class BowyerTest : TimedCommand
    {
        public override bool Init()
        {
            Name = "bowyer";
            Desc = "Generates a DelaunayTriangulation using Bowyer algorithm.";
            Aliases = new List<string>() { "bowyer" };
            ProperUse = "bowyer";

            return true;
        }

        BowyerAlgorithm2D bowyer;
        public override void PrepareTest(params string[] args)
        {
            Random rand = new Random();
            bowyer = new BowyerAlgorithm2D(rand.Next(0, 999999999));
            if(args.Length > 0)
            {
                bowyer.PointCount = int.Parse(args[0]);
            }
        }

        public override Task RunTest()
        {
            bowyer.Generate();
            //bowyer.GenerateVoronoi();

            return Task.CompletedTask;
        }
    }
}
