using CP.Common.Maths;
using CP.Common.Random;
using CPWS.WorldGenerator.Voronoi.BowyerAlgorithm.Structures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CPWS.WorldGenerator.Voronoi.BowyerAlgorithm
{
    public class BowyerAlgorithm2DOriginal
    {
        private RandomHash rand = new RandomHash();
        private int seed;

        public int Seed { get => seed; set { seed = value; rand = value == 0 ? new RandomHash() : new RandomHash(seed); } }
        public int PointCount { get; set; } = 50;
        public double MaxX { get; set; } = 1024;
        public double MaxY { get; set; } = 1024;

        public List<Vector3D> Sites { get; private set; }

        private List<Triangle> delaunay;
        public List<Triangle> Delaunay { get => delaunay == null ? Generate() : delaunay; }

        public List<VoronoiCell> Voronoi { get; private set; } = new List<VoronoiCell>();
        private Dictionary<Vector3D, List<Triangle>> connectedTriangles = new Dictionary<Vector3D, List<Triangle>>();

        public BowyerAlgorithm2DOriginal(int seed)
        {
            Seed = seed;
        }

        private List<Vector3D> GeneratePoints()
        {
            Sites = new List<Vector3D>();

            int randPos = 0;
            for (var i = 0; i < PointCount; i++)
            {
                Sites.Add(new Vector3D(
                    rand.Next(0, (int)MaxX, randPos++),
                    rand.Next(0, (int)MaxY, randPos++)));
            }

            //uniq the points
            Sites.Sort((p1, p2) =>
            {
                if (p1.X.ApproxEqual(p2.X))
                {
                    if (p1.Y.ApproxEqual(p2.Y))
                    {
                        return 0;
                    }
                    if (p1.Y < p2.Y)
                    {
                        return -1;
                    }
                    return 1;
                }
                if (p1.X < p2.X)
                {
                    return -1;
                }
                return 1;
            });

            var unique = new List<Vector3D>(Sites.Count / 2);
            var last = Sites.First();
            unique.Add(last);
            for (var index = 1; index < Sites.Count; index++)
            {
                var point = Sites[index];
                if (!last.X.ApproxEqual(point.X) || !last.Y.ApproxEqual(point.Y))
                {
                    unique.Add(point);
                    last = point;
                }
            }
            Sites = unique;

            return Sites;
        }

        public List<Triangle> Generate(List<Vector3D> sites = null, bool removeSuperTriangle = false)
        {
            if (sites == null) GeneratePoints();
            else Sites = sites;
            List<Triangle> triangulation = new List<Triangle>();
            Triangle superTriangle = new Triangle(new Vector3D(MaxX / 2, MaxY * 2), new Vector3D(-MaxX, -MaxY), new Vector3D(MaxX * 2, -MaxY), true);

            triangulation.Add(superTriangle);

            foreach (Vector3D position in Sites)
            {
                List<Triangle> badTriangles = new List<Triangle>();

                foreach (Triangle triangle in triangulation)
                {
                    Vector3D center = triangle.GetCircumcenter();
                    double radius = triangle.GetCircumradius();

                    if (position.Within(center, radius))
                    {
                        badTriangles.Add(triangle);
                    }
                }
                List<Edge> tris = new List<Edge>();
                foreach (Triangle triangle in badTriangles)
                {
                    tris.AddRange(triangle.Edges);

                    foreach (Vector3D p in triangle.Points)
                    {
                        if (connectedTriangles.ContainsKey(p))
                        {
                            connectedTriangles[p].Remove(triangle);
                        }
                    }

                    triangulation.Remove(triangle);
                }

                List<Edge> polygon = tris.GroupBy(o => o).Where(o => o.Count() == 1).Select(o => o.First()).ToList();
                foreach (Edge tri in polygon.Where(possibleEdge => possibleEdge.PointA != position && possibleEdge.PointB != position))
                {
                    Triangle triangle = new Triangle(position, tri.PointA, tri.PointB, true);
                    AddConnectedTriangles(triangle);

                    triangulation.Add(triangle);
                }
            }
            if (removeSuperTriangle)
            {
                List<Triangle> badTriangles = new List<Triangle>();

                foreach (Triangle triangle in triangulation)
                {
                    foreach (Vector3D pos in triangle.Points)
                    {
                        if (superTriangle.Points.Contains(pos))
                        {
                            badTriangles.Add(triangle);
                            break;
                        }
                    }
                }
                foreach (Triangle triangle in badTriangles)
                {
                    foreach (Vector3D p in triangle.Points)
                    {
                        if (connectedTriangles.ContainsKey(p))
                        {
                            //connectedTriangles[p].Remove(triangle);
                        }
                    }

                    triangulation.Remove(triangle);
                }
            }

            delaunay = triangulation;
            return triangulation;
        }

        private void AddConnectedTriangles(Triangle triangle)
        {
            foreach (Vector3D pos in triangle.Points)
            {
                if (connectedTriangles.ContainsKey(pos))
                    connectedTriangles[pos].Add(triangle);
                else
                    connectedTriangles.Add(pos, new List<Triangle>() { triangle });
            }
        }

        public List<VoronoiCell> GenerateVoronoi()
        {
            foreach (Vector3D site in Sites)
            {
                VoronoiCell cell = new VoronoiCell(site);

                foreach (Triangle triangle in connectedTriangles[site])
                {
                    Vector3D point = triangle.GetCircumcenter().Clone();

                    if (point.X > MaxX)
                        point.X = MaxX;
                    else if (point.X < 0)
                        point.X = 0;

                    if (point.Y > MaxY)
                        point.Y = MaxY;
                    else if (point.Y < 0)
                        point.Y = 0;

                    cell.Points.Add(point);
                }
                cell.SortPoints();
                for (int i = 0; i < cell.Points.Count - 1; ++i)
                {
                    cell.Edges.Add(new Edge(cell.Points[i], cell.Points[i + 1]));
                }
                cell.Edges.Add(new Edge(cell.Points[cell.Points.Count - 1], cell.Points[0]));

                Voronoi.Add(cell);
            }

            return Voronoi;
        }
    }
}
