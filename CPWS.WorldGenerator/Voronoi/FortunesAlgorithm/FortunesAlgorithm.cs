using System;
using System.Collections.Generic;
using System.Linq;
using CPWS.WorldGenerator.Voronoi.FortunesAlgorithm.Event;
using CPWS.WorldGenerator.Voronoi.FortunesAlgorithm.Maths;
using CPWS.WorldGenerator.Voronoi.FortunesAlgorithm.Structure;
using CPWS.WorldGenerator.Voronoi.FortunesAlgorithm.Structure.Points;
using CPWS.WorldGenerator.Noise;
using System.Dynamic;
using System.Diagnostics;

namespace CPWS.WorldGenerator.Voronoi.FortunesAlgorithm
{
    public class FortunesAlgorithm
    {
        private Random rand;
        private int seed;

        public List<FSite> Sites { get; private set; }
        public LinkedList<VoronoiEdge> Edges { get; private set; }
        public List<Tuple<Point, Point>> Delaunay { get; private set; }

        public int Seed { get => seed; set { seed = value; rand = value == 0 ? new Random() : new Random(seed); } }
        public int PointCount { get; set; }

        private double minX = 0;
        private double minY = 0;
        private double maxX = 1920;
        private double maxY = 1080;

        public double MinX { get => minX; set { minX = value; } }
        public double MinY { get => minY; set { minY = value; } }

        public double MaxX { get => maxX; set { maxX = value; } }
        public double MaxY { get => maxY; set {  maxY = value; } }

        public bool UseDelaunay { get; set; } = false;
        public bool UseNoisyEdges { get; set; } = false;
        public int NoisyEdgesNo { get; set; } = 9;

        public FortunesAlgorithm(int seed = 0, int pointCount = 250, double minX = 0, double minY = 0, double maxX = 1920, double maxY = 1080, bool useDelaunay = false, int noisyEdges = 0)
        {
            Seed = seed;
            PointCount = pointCount;

            MaxX = maxX;
            MaxY = maxY;
            MinX = minX;
            MinY = minY;

            UseDelaunay = useDelaunay;
            UseNoisyEdges = noisyEdges > 0;
            NoisyEdgesNo = noisyEdges;
        }

        public void Generate(List<FSite> sites = null)
        {
            Init(sites ?? GeneratePoints());
        }

        private void Init(List<FSite> points)
        {
            Sites = points;

            Edges = new LinkedList<VoronoiEdge>();
            GenerateVoronoi();

            if (UseDelaunay)
            {
                Delaunay = new List<Tuple<Point, Point>>();
                GenerateDelaunay();
            }

            if (UseNoisyEdges)
                GenerateNoisyEdges(NoisyEdgesNo);
        }

        private List<FSite> GeneratePoints()
        {
            List<FSite> sites = new List<FSite>();

            for (var i = 0; i < PointCount; i++)
            {
                sites.Add(new FSite(
                    rand.Next((int)(0), (int)maxX),
                    rand.Next((int)(0), (int)maxY)));
            }

            //uniq the points
            sites.Sort((p1, p2) =>
            {
                if (p1.X.ApproxEqual(p2.X))
                {
                    if (p1.Y.ApproxEqual(p2.Y))
                        return 0;
                    if (p1.Y < p2.Y)
                        return -1;
                    return 1;
                }
                if (p1.X < p2.X)
                    return -1;
                return 1;
            });

            var unique = new List<FSite>(sites.Count / 2);
            var last = sites.First();
            unique.Add(last);
            for (var index = 1; index < sites.Count; index++)
            {
                var point = sites[index];
                if (!last.X.ApproxEqual(point.X) ||
                    !last.Y.ApproxEqual(point.Y))
                {
                    unique.Add(point);
                    last = point;
                }
            }
            sites = unique;

            return sites;
        }

        private void GenerateDelaunay()
        {
            var processed = new HashSet<FSite>();
            Delaunay.Clear();
            foreach (var site in Sites)
            {
                foreach (var neighbor in site.Neighbours)
                {
                    if (!processed.Contains(neighbor))
                    {
                        Delaunay.Add(
                            new Tuple<Point, Point>(
                                new Point(site.X, site.Y),
                                new Point(neighbor.X, neighbor.Y)
                            ));
                    }
                }
                processed.Add(site);
            }
        }

        private void GenerateNoisyEdges(int noisyEdgesNo)
        {
            SimplexNoise noise = new SimplexNoise((uint)seed, 0.05, 0.5);

            double[,] noiseVals = noise.NoiseMapNotAsync(4, noisyEdgesNo * Edges.Count);
            int noiseVal = 0;

            double offset = 1d / (noisyEdgesNo + 2);

            foreach (VoronoiEdge edge in Edges)
            {
                edge.NoisyEdges = new NoisyPoint[noisyEdgesNo];

                Point startend = (edge.StartPoint + edge.EndPoint) * .5;
                double angle = Math.Atan2(edge.EndPoint.Y - edge.StartPoint.Y, edge.EndPoint.X - edge.StartPoint.X) / Math.PI;

                for (int i = 0; i < edge.NoisyEdges.Length; i++)
                {
                    Point p = edge.StartPoint + ((edge.EndPoint - edge.StartPoint) * ((i + 1) * offset));

                    NoisyPoint np = new NoisyPoint(p.X, p.Y)
                    {
                        XNoise = (noiseVals[0, noiseVal] * 10) * angle,
                        YNoise = (noiseVals[0, noiseVal] * 10) * angle
                    };

                    edge.NoisyEdges[i] = np;
                    noiseVal++;
                }
            }
        }

        private void GenerateVoronoi()
        {
            var eventQueue = new MinHeap<FortuneEvent>(5 * Sites.Count);
            foreach (var s in Sites)
            {
                eventQueue.Insert(new FortuneSiteEvent(s));
            }
            //init tree
            var beachLine = new BeachLine();
            var edges = new LinkedList<VoronoiEdge>();
            var deleted = new HashSet<FortuneCircleEvent>();

            //init edge list
            while (eventQueue.Count != 0)
            {
                var fEvent = eventQueue.Pop();
                if (fEvent is FortuneSiteEvent)
                    beachLine.AddBeachSection((FortuneSiteEvent)fEvent, eventQueue, deleted, edges);
                else
                {
                    if (deleted.Contains((FortuneCircleEvent)fEvent))
                    {
                        deleted.Remove((FortuneCircleEvent)fEvent);
                    }
                    else
                    {
                        beachLine.RemoveBeachSection((FortuneCircleEvent)fEvent, eventQueue, deleted, edges);
                    }
                }
            }

            Point topLeft = new Point(minX, minY);
            Point topRight = new Point(maxX, minY);
            Point bottomLeft = new Point(minX, maxY);
            Point bottomRight = new Point(maxX, maxY);

            VoronoiEdge topLeftEdge = null;
            double TLEdgeDistance = double.PositiveInfinity;
            VoronoiEdge topRightEdge = null;
            double TREdgeDistance = double.PositiveInfinity;
            VoronoiEdge bottomLeftEdge = null;
            double BLEdgeDistance = double.PositiveInfinity;
            VoronoiEdge bottomRightEdge = null;
            double BREdgeDistance = double.PositiveInfinity;

            foreach (VoronoiEdge edge in edges)
            {
                double Dist = Point.Distance(topLeft, edge.StartPoint);
                if (Dist < TLEdgeDistance)
                {
                    topLeftEdge = edge;
                    TLEdgeDistance = Dist;
                    continue;
                }
                Dist = Point.Distance(topRight, edge.StartPoint);
                if (Dist < TREdgeDistance)
                {
                    topRightEdge = edge;
                    TREdgeDistance = Dist;
                    continue;
                }
                Dist = Point.Distance(bottomLeft, edge.StartPoint);
                if (Dist < BLEdgeDistance)
                {
                    bottomLeftEdge = edge;
                    BLEdgeDistance = Dist;
                    continue;
                }
                Dist = Point.Distance(bottomRight, edge.StartPoint);
                if (Dist < BREdgeDistance)
                {
                    bottomRightEdge = edge;
                    BREdgeDistance = Dist;
                    continue;
                }
            }

            if (topLeftEdge != null)
            {
                Point end = topLeftEdge.EndPoint;
                topLeftEdge.EndPoint = topLeft;
                if (topLeftEdge.Neighbor != null)
                    topLeftEdge.Neighbor.StartPoint = topLeft;
                //edges.AddLast(new VoronoiEdge(topLeft, topLeftEdge.Left, topLeftEdge.Right, end));
            }
            else
            {
                Trace.WriteLine("Top Left was null!");
            }


            if (topRightEdge != null)
            {
                Point end = topRightEdge.EndPoint;
                topRightEdge.EndPoint = topRight;
                if (topRightEdge.Neighbor != null)
                    topRightEdge.Neighbor.StartPoint = topRight;
                //edges.AddLast(new VoronoiEdge(topRight, topRightEdge.Left, topRightEdge.Right, end));
            }
            else
            {
                Trace.WriteLine("Top Right was null!");
            }

            if (bottomLeftEdge != null)
            {
                Point end = bottomLeftEdge.EndPoint;
                bottomLeftEdge.EndPoint = bottomLeft;
                if (bottomLeftEdge.Neighbor != null)
                    bottomLeftEdge.Neighbor.StartPoint = bottomLeft;
                //edges.AddLast(new VoronoiEdge(bottomLeft, bottomLeftEdge.Left, bottomLeftEdge.Right, end));
            }
            else
            {
                Trace.WriteLine("Bottom Left was null!");
            }

            if (bottomRightEdge != null)
            {
                Point end = bottomRightEdge.EndPoint;
                bottomRightEdge.EndPoint = bottomRight;
                if (bottomRightEdge.Neighbor != null)
                    bottomRightEdge.Neighbor.StartPoint = bottomRight;
                //edges.AddLast(new VoronoiEdge(bottomRight, bottomRightEdge.Left, bottomRightEdge.Right, end));
            }
            else
            {
                Trace.WriteLine("Bottom Right was null!");
            }

            //clip edges
            var edgeNode = edges.First;
            while (edgeNode != null)
            {
                var edge = edgeNode.Value;
                var next = edgeNode.Next;

                var valid = ClipEdge(edge, minX, minY, maxX, maxY);
                if (!valid)
                    edges.Remove(edgeNode);

                if(valid)
                {
                    edgeNode.Value.Left.AddEdge(edgeNode.Value);
                    edgeNode.Value.Right.AddEdge(edgeNode.Value);
                }

                //advance
                edgeNode = next;
            }

            this.Edges = edges;
        }

        public void Relax(int iterations)
        {
            for (int i = 0; i < iterations; i++)
            {
                List<FSite> newPoints = new List<FSite>();

                // Go thourgh all sites
                foreach (FSite site in Sites)
                {
                    // Loop all corners of the site to calculate the centroid
                    List<Point> region = site.Points;
                    if (region.Count < 1)
                    {
                        continue;
                    }

                    Point centroid = new Point();
                    double signedArea = 0;
                    double x0 = 0;
                    double y0 = 0;
                    double x1 = 0;
                    double y1 = 0;
                    double a = 0;
                    // For all vertices except last
                    for (int j = 0; j < region.Count - 1; j++)
                    {
                        x0 = region[j].X;
                        y0 = region[j].Y;
                        x1 = region[j + 1].X;
                        y1 = region[j + 1].Y;
                        a = x0 * y1 - x1 * y0;
                        signedArea += a;
                        centroid.X += (x0 + x1) * a;
                        centroid.Y += (y0 + y1) * a;
                    }
                    // Do last vertex
                    x0 = region[region.Count - 1].X;
                    y0 = region[region.Count - 1].Y;
                    x1 = region[0].X;
                    y1 = region[0].Y;
                    a = x0 * y1 - x1 * y0;
                    signedArea += a;
                    centroid.X += (x0 + x1) * a;
                    centroid.Y += (y0 + y1) * a;

                    signedArea *= 0.5f;
                    centroid.X /= (6 * signedArea);
                    centroid.Y /= (6 * signedArea);
                    // Move site to the centroid of its Voronoi cell
                    newPoints.Add(new FSite(centroid));
                }

                Init(newPoints);
            }
        }

        private static bool ClipEdge(VoronoiEdge edge, double minX, double minY, double maxX, double maxY)
        {
            var accept = false;

            //if its a ray
            if (edge.EndPoint == null)
            {
                accept = ClipRay(edge, minX, minY, maxX, maxY);
            }
            else
            {
                //Cohen–Sutherland
                var start = ComputeOutCode(edge.StartPoint.X, edge.StartPoint.Y, minX, minY, maxX, maxY);
                var end = ComputeOutCode(edge.EndPoint.X, edge.EndPoint.Y, minX, minY, maxX, maxY);

                while (true)
                {
                    if ((start | end) == 0)
                    {
                        accept = true;
                        break;
                    }
                    if ((start & end) != 0)
                    {
                        break;
                    }

                    double x = -1, y = -1;
                    var outcode = start != 0 ? start : end;

                    if ((outcode & 0x8) != 0) // top
                    {
                        x = edge.StartPoint.X + (edge.EndPoint.X - edge.StartPoint.X) * (maxY - edge.StartPoint.Y) / (edge.EndPoint.Y - edge.StartPoint.Y);
                        y = maxY;
                    }
                    else if ((outcode & 0x4) != 0) // bottom
                    {
                        x = edge.StartPoint.X + (edge.EndPoint.X - edge.StartPoint.X) * (minY - edge.StartPoint.Y) / (edge.EndPoint.Y - edge.StartPoint.Y);
                        y = minY;
                    }
                    else if ((outcode & 0x2) != 0) //right
                    {
                        y = edge.StartPoint.Y + (edge.EndPoint.Y - edge.StartPoint.Y) * (maxX - edge.StartPoint.X) / (edge.EndPoint.X - edge.StartPoint.X);
                        x = maxX;
                    }
                    else if ((outcode & 0x1) != 0) //left
                    {
                        y = edge.StartPoint.Y + (edge.EndPoint.Y - edge.StartPoint.Y) * (minX - edge.StartPoint.X) / (edge.EndPoint.X - edge.StartPoint.X);
                        x = minX;
                    }

                    if (outcode == start)
                    {
                        edge.StartPoint = new Point(x, y);
                        start = ComputeOutCode(x, y, minX, minY, maxX, maxY);
                    }
                    else
                    {
                        edge.EndPoint = new Point(x, y);
                        end = ComputeOutCode(x, y, minX, minY, maxX, maxY);
                    }
                }
            }
            //if we have a neighbor
            if (edge.Neighbor != null)
            {
                //check it
                var valid = ClipEdge(edge.Neighbor, minX, minY, maxX, maxY);
                //both are valid
                if (accept && valid)
                {
                    edge.StartPoint = edge.Neighbor.EndPoint;
                }
                //this edge isn't valid, but the neighbor is
                //flip and set
                if (!accept && valid)
                {
                    edge.StartPoint = edge.Neighbor.EndPoint;
                    edge.EndPoint = edge.Neighbor.StartPoint;
                    accept = true;
                }
            }
            return accept;
        }

        private static int ComputeOutCode(double x, double y, double minX, double minY, double maxX, double maxY)
        {
            int code = 0;
            if (x.ApproxEqual(minX) || x.ApproxEqual(maxX))
            { }
            else if (x < minX)
                code |= 0x1;
            else if (x > maxX)
                code |= 0x2;

            if (y.ApproxEqual(minY) || x.ApproxEqual(maxY))
            { }
            else if (y < minY)
                code |= 0x4;
            else if (y > maxY)
                code |= 0x8;
            return code;
        }

        private static bool ClipRay(VoronoiEdge edge, double minX, double minY, double maxX, double maxY)
        {
            var start = edge.StartPoint;
            //horizontal ray
            if (edge.SlopeRise.ApproxEqual(0))
            {
                if (!Within(start.Y, minY, maxY))
                    return false;
                if (edge.SlopeRun > 0 && start.X > maxX)
                    return false;
                if (edge.SlopeRun < 0 && start.X < minX)
                    return false;
                if (Within(start.X, minX, maxX))
                {
                    if (edge.SlopeRun > 0)
                        edge.EndPoint = new Point(maxX, start.Y);
                    else
                        edge.EndPoint = new Point(minX, start.Y);
                }
                else
                {
                    if (edge.SlopeRun > 0)
                    {
                        edge.StartPoint = new Point(minX, start.Y);
                        edge.EndPoint = new Point(maxX, start.Y);
                    }
                    else
                    {
                        edge.StartPoint = new Point(maxX, start.Y);
                        edge.EndPoint = new Point(minX, start.Y);
                    }
                }
                return true;
            }
            //vertical ray
            if (edge.SlopeRun.ApproxEqual(0))
            {
                if (start.X < minX || start.X > maxX)
                    return false;
                if (edge.SlopeRise > 0 && start.Y > maxY)
                    return false;
                if (edge.SlopeRise < 0 && start.Y < minY)
                    return false;
                if (Within(start.Y, minY, maxY))
                {
                    if (edge.SlopeRise > 0)
                        edge.EndPoint = new Point(start.X, maxY);
                    else
                        edge.EndPoint = new Point(start.X, minY);
                }
                else
                {
                    if (edge.SlopeRise > 0)
                    {
                        edge.StartPoint = new Point(start.X, minY);
                        edge.EndPoint = new Point(start.X, maxY);
                    }
                    else
                    {
                        edge.StartPoint = new Point(start.X, maxY);
                        edge.EndPoint = new Point(start.X, minY);
                    }
                }
                return true;
            }

            //works for outside
            //Debug.Assert(edge.Slope != null, "edge.Slope != null");
            //Debug.Assert(edge.Intercept != null, "edge.Intercept != null");
            var topX = new Point(CalcX(edge.Slope.Value, maxY, edge.Intercept.Value), maxY);
            var bottomX = new Point(CalcX(edge.Slope.Value, minY, edge.Intercept.Value), minY);
            var leftY = new Point(minX, CalcY(edge.Slope.Value, minX, edge.Intercept.Value));
            var rightY = new Point(maxX, CalcY(edge.Slope.Value, maxX, edge.Intercept.Value));

            //reject intersections not within bounds
            var candidates = new List<Point>();
            if (Within(topX.X, minX, maxX))
                candidates.Add(topX);
            if (Within(bottomX.X, minX, maxX))
                candidates.Add(bottomX);
            if (Within(leftY.Y, minY, maxY))
                candidates.Add(leftY);
            if (Within(rightY.Y, minY, maxY))
                candidates.Add(rightY);

            //reject candidates which don't align with the slope
            for (var i = candidates.Count - 1; i > -1; i--)
            {
                var candidate = candidates[i];
                //grab vector representing the edge
                var ax = candidate.X - start.X;
                var ay = candidate.Y - start.Y;
                if (edge.SlopeRun * ax + edge.SlopeRise * ay < 0)
                    candidates.RemoveAt(i);
            }

            //if there are two candidates we are outside the closer one is start
            //the further one is the end
            if (candidates.Count == 2)
            {
                var ax = candidates[0].X - start.X;
                var ay = candidates[0].Y - start.Y;
                var bx = candidates[1].X - start.X;
                var by = candidates[1].Y - start.Y;
                if (ax * ax + ay * ay > bx * bx + by * by)
                {
                    edge.StartPoint = candidates[1];
                    edge.EndPoint = candidates[0];
                }
                else
                {
                    edge.StartPoint = candidates[0];
                    edge.EndPoint = candidates[1];
                }
            }

            //if there is one candidate we are inside
            if (candidates.Count == 1)
                edge.EndPoint = candidates[0];

            //there were no candidates
            return edge.EndPoint != null;
        }

        private static bool Within(double x, double a, double b)
        {
            return x.ApproxGreaterThanOrEqualTo(a) && x.ApproxLessThanOrEqualTo(b);
        }

        private static double CalcY(double m, double x, double b)
        {
            return m * x + b;
        }

        private static double CalcX(double m, double y, double b)
        {
            return (y - b) / m;
        }
    }
}