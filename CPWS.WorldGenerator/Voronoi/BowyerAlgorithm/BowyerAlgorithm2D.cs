using CP.Common.Maths;
using CP.Common.Random;
using CPWS.WorldGenerator.Voronoi.BowyerAlgorithm.Structures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CPWS.WorldGenerator.Voronoi.BowyerAlgorithm
{
    public class BowyerAlgorithm2D
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

        public BowyerAlgorithm2D(int seed)
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
            List<Triangle> triangulation = new List<Triangle>(Sites.Count * 4 - (Sites.Count - 1));
            Triangle superTriangle = new Triangle(new Vector3D(MaxX / 2, MaxY * 2), new Vector3D(-MaxX, -MaxY), new Vector3D(MaxX * 2, -MaxY), false);

            triangulation.Add(superTriangle);

            foreach (Vector3D position in Sites)
            {
                List<Edge> tris = new List<Edge>();

                for (int i = 0; i < triangulation.Count; ++i)
                {
                    Triangle triangle = triangulation[i];
                    Vector3D center = triangle.GetCircumcenter();
                    double radius = triangle.GetCircumradius();
                    
                    if (position.WithinSqrd(center, radius))
                    {
                        if (!tris.Contains(triangle.EdgeAB))
                        {
                            if (triangle.EdgeAB.PointA != position && triangle.EdgeAB.PointB != position)
                                tris.Add(triangle.EdgeAB);
                        }
                        else
                        {
                            tris.Remove(triangle.EdgeAB);
                        }
                        if (!tris.Contains(triangle.EdgeBC))
                        {
                            if (triangle.EdgeBC.PointA != position && triangle.EdgeBC.PointB != position)
                                tris.Add(triangle.EdgeBC);
                        }
                        else
                        {
                            tris.Remove(triangle.EdgeBC);
                        }
                        if (!tris.Contains(triangle.EdgeCA))
                        {
                            if (triangle.EdgeCA.PointA != position && triangle.EdgeCA.PointB != position)
                                tris.Add(triangle.EdgeCA);
                        }
                        else
                        {
                            tris.Remove(triangle.EdgeCA);
                        }

                        if (connectedTriangles.ContainsKey(triangle.PointA))
                        {
                            connectedTriangles[triangle.PointA].Remove(triangle);
                        }
                        if (connectedTriangles.ContainsKey(triangle.PointB))
                        {
                            connectedTriangles[triangle.PointB].Remove(triangle);
                        }
                        if (connectedTriangles.ContainsKey(triangle.PointC))
                        {
                            connectedTriangles[triangle.PointC].Remove(triangle);
                        }

                        triangulation.RemoveAt(i--);
                    }
                }

                foreach (Edge tri in tris)
                {
                    Triangle triangle = new Triangle(position, tri.PointA, tri.PointB, false);
                    AddConnectedTriangles(triangle);

                    triangulation.Add(triangle);
                }
            }

            if (removeSuperTriangle)
            {
                for (int i = 0; i < triangulation.Count; ++i)
                {
                    Triangle triangle = triangulation[i];
                    if (superTriangle.Points.Contains(triangle.PointA))
                    {
                        triangulation.RemoveAt(i--);
                    }
                    else if (superTriangle.Points.Contains(triangle.PointB))
                    {
                        triangulation.RemoveAt(i--);
                    }
                    else if (superTriangle.Points.Contains(triangle.PointC))
                    {
                        triangulation.RemoveAt(i--);
                    }
                }
            }

            delaunay = triangulation;
            return triangulation;
        }

        private void AddConnectedTriangles(Triangle triangle)
        {
            foreach(Vector3D pos in triangle.Points)
            {
                if (connectedTriangles.ContainsKey(pos))
                    connectedTriangles[pos].Add(triangle);
                else
                    connectedTriangles.Add(pos, new List<Triangle>() { triangle });
            }
        }

        public List<VoronoiCell> GenerateVoronoi()
        {
            foreach(Vector3D site in Sites)
            {
                VoronoiCell cell = new VoronoiCell(site);

                foreach(Triangle triangle in connectedTriangles[site])
                {
                    Vector3D point = triangle.GetCircumcenter().Clone();

                   /* if (point.X > MaxX)
                        point.X = MaxX;
                    else if (point.X < 0)
                        point.X = 0;

                    if (point.Y > MaxY)
                        point.Y = MaxY;
                    else if (point.Y < 0)
                        point.Y = 0;*/

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
