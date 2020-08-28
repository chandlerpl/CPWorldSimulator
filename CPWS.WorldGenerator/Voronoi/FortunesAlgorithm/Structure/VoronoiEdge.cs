using CPWS.WorldGenerator.Voronoi.FortunesAlgorithm.Maths;
using CPWS.WorldGenerator.Voronoi.FortunesAlgorithm.Structure.Points;

namespace CPWS.WorldGenerator.Voronoi.FortunesAlgorithm.Structure
{
    public class VoronoiEdge
    {
        public Point StartPoint { get; set; }
        public Point EndPoint { get; set; }

        public FSite Left { get; private set; }
        public FSite Right { get; private set; }

        public NoisyPoint[] NoisyEdges { get; set; }

        internal double SlopeRise { get; }
        internal double SlopeRun { get; }
        internal double? Slope { get; }
        internal double? Intercept { get; }

        public VoronoiEdge Neighbor { get; internal set; }

        public VoronoiEdge(Point start, FSite left, FSite right, Point end = null)
        {
            StartPoint = start;
            EndPoint = end;
            Left = left;
            Right = right;

            //for bounding box edges
            if (left == null || right == null)
                return;

            //from negative reciprocal of slope of line from left to right
            //ala m = (left.y -right.y / left.x - right.x)
            SlopeRise = left.X - right.X;
            SlopeRun = -(left.Y - right.Y);
            Intercept = null;

            if (SlopeRise.ApproxEqual(0) || SlopeRun.ApproxEqual(0)) return;
            Slope = SlopeRise / SlopeRun;
            Intercept = start.Y - Slope * start.X;
        }
    }
}