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
    class CellTest : TimedCommand
    {
        public override bool Init()
        {
            Name = "cellTest";
            Desc = "Generates a 3D DelaunayTriangulation using Bowyer algorithm.";
            Aliases = new List<string>() { "cell" };
            ProperUse = "cell";

            return true;
        }

        DelaunayCell cell;
        DelaunayCell cell2;
        public override void PrepareTest(params string[] args)
        {
            cell = new DelaunayCell();
            cell.Points.Add(new Vector3D(10 / 2, 10 * 3, 10 / 2));
            cell.Points.Add(new Vector3D(-(10 * 2), -10, -(10 * 2)));
            cell.Points.Add(new Vector3D(10 * 3, -10, -(10 * 2)));
            cell.Points.Add(new Vector3D(10 / 2, -10, 10 * 4));

            cell2 = new DelaunayCell();
            cell2.Points.Add(new Vector3D(10 / 2, 10 * 3, 10 / 2));
            cell2.Points.Add(new Vector3D(-(10 * 2), -10, -(10 * 2)));
            cell2.Points.Add(new Vector3D(10 * 3, -10, -(10 * 2)));
            cell2.Points.Add(new Vector3D(10 / 2, -10, 10 * 4));
        }

        public override Task RunTest()
        {
            _ = cell.GetCentroid();
            //bowyer.GenerateVoronoi();

            return Task.CompletedTask;
        }
    }
}
