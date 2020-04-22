using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPWS.WorldGenerator.Voronoi.FortunesAlgorithm.Structure.Points
{
    public class NoisyPoint : Point
    {
        public NoisyPoint() { }
        
        public NoisyPoint(double x, double y)
        {
            X = x;
            Y = y;
        }

        public double XNoise { get; set; } = 0;
        public double YNoise { get; set; } = 0;

    }
}
