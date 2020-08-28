using CPWS.WorldGenerator.Voronoi.FortunesAlgorithm.Structure.Points;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPWS.WorldGenerator.Voronoi.FortunesAlgorithm.Structure
{
    public class FSite
    {
        public Point Position { get; private set; }

        public double X => Position.X;
        public double Y => Position.Y;

        public List<FSite> Neighbours { get; private set; }

        public List<VoronoiEdge> Cell { get; private set; }

        private List<Point> _points;

        public List<Point> Points
        {
            get
            {
                if (_points == null)
                {
                    _points = new List<Point>();

                    foreach (var edge in Cell)
                    {
                        _points.Add(edge.StartPoint);

                        if (edge.NoisyEdges != null && edge.NoisyEdges.Length > 0)
                        {
                            foreach (var nEdge in edge.NoisyEdges)
                            {
                                _points.Add(nEdge);
                            }
                        }

                        _points.Add(edge.EndPoint);
                    }
                    _points.Sort(new Comparison<Point>(SortCornersClockwise));
                }

                return _points;
            }
        }

        public FSite(Point position)
        {
            Position = position;

            Neighbours = new List<FSite>();
            Cell = new List<VoronoiEdge>();
        }
        public FSite(double x, double y)
        {
            Position = new Point(x, y);

            Neighbours = new List<FSite>();
            Cell = new List<VoronoiEdge>();
        }

        public bool Contains(Point testPoint)
        {
            bool result = false;
            int j = Points.Count - 1;
            for (int i = 0; i < Points.Count; i++)
            {
                if (Points[i].Y < testPoint.Y && Points[j].Y >= testPoint.Y || Points[j].Y < testPoint.Y && Points[i].Y >= testPoint.Y)
                {
                    if (Points[i].X + ((testPoint.Y - Points[i].Y) / (Points[j].Y - Points[i].Y) * (Points[j].X - Points[i].X)) < testPoint.X)
                    {
                        result = !result;
                    }
                }
                j = i;
            }
            return result;
        }

        public int SortCornersClockwise(Point A, Point B)
        {
            var atanA = Math.Atan2(A.Y - Y, A.X - X);
            var atanB = Math.Atan2(B.Y - Y, B.X - X);

            if (atanA < atanB) return -1;
            else if (atanA > atanB) return 1;
            return 0;
        }

        internal void AddEdge(VoronoiEdge value)
        {
            if (value.StartPoint == null || value.EndPoint == null
                || double.IsNaN(value.StartPoint.X) || double.IsNaN(value.StartPoint.Y)
                || double.IsNaN(value.EndPoint.X) || double.IsNaN(value.EndPoint.Y))
            {
                return;
            }

            Cell.Add(value);
            _points = null;
        }
    }
}
