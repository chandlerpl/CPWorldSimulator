using CP.Common.Maths;
using CP.Common.Random;
using CPWS.WorldGenerator.Voronoi.BowyerAlgorithm.Structures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CPWS.WorldGenerator.Voronoi.BowyerAlgorithm
{
    public class BowyerAlgorithm3D
    {
        private RandomHash rand = new RandomHash();
        private int seed;

        public int Seed { get => seed; set { seed = value; rand = value == 0 ? new RandomHash() : new RandomHash(seed); } }
        public int PointCount { get; set; } = 50;
        public double MaxX { get; set; } = 1024;
        public double MaxY { get; set; } = 1024;
        public double MaxZ { get; set; } = 1024;

        public List<Vector3D> Sites { get; private set; }

        private List<DelaunayCell> delaunay;
        public List<DelaunayCell> Delaunay { get => delaunay == null ? Generate() : delaunay; }

        public List<VoronoiCell> Voronoi { get; private set; } = new List<VoronoiCell>();
        private Dictionary<Vector3D, List<DelaunayCell>> connectedTriangles = new Dictionary<Vector3D, List<DelaunayCell>>();

        public BowyerAlgorithm3D(int seed)
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
                    rand.Next(0, (int)MaxY, randPos++),
                    rand.Next(0, (int)MaxZ, randPos++)));
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

        public List<DelaunayCell> Generate(List<Vector3D> sites = null, bool removeSuperTriangle = false)
        {
            if (sites == null) GeneratePoints();
            else Sites = sites;
            List<DelaunayCell> triangulation = new List<DelaunayCell>();
            DelaunayCell superTriangle = new DelaunayCell();
            superTriangle.Points.Add(new Vector3D(MaxX / 2, MaxY * 3, MaxZ / 2));
            superTriangle.Points.Add(new Vector3D(-(MaxX * 2), -MaxY, -(MaxZ * 2)));
            superTriangle.Points.Add(new Vector3D(MaxX * 3, -MaxY, -(MaxZ * 2)));
            superTriangle.Points.Add(new Vector3D(MaxX / 2, -MaxY, MaxZ * 4));

            triangulation.Add(superTriangle);

            foreach (Vector3D position in Sites)
            {
                List<Triangle> tris = new List<Triangle>();

                for (int i = 0; i < triangulation.Count; ++i)
                {
                    DelaunayCell cell = triangulation[i];
                    Vector3D center = cell.GetCircumcenter();
                    double radius = cell.GetCircumradius();

                    if (position.WithinSqrd(center, radius))
                    {
                        foreach (Triangle face in cell.Faces)
                        {
                            if (!tris.Contains(face))
                            {
                                if (face.PointA != position && face.PointB != position && face.PointC != position)
                                    tris.Add(face);
                            }
                            else
                            {
                                tris.Remove(face);
                            }
                        }

                        foreach (Vector3D p in cell.Points)
                        {
                            if (connectedTriangles.ContainsKey(p))
                            {
                                connectedTriangles[p].Remove(cell);
                            }
                        }

                        triangulation.RemoveAt(i--);
                    }
                }

                foreach (Triangle tri in tris)
                {
                    DelaunayCell triangle = new DelaunayCell();
                    triangle.Points.Add(position);
                    triangle.Points.Add(tri.PointA);
                    triangle.Points.Add(tri.PointB);
                    triangle.Points.Add(tri.PointC);
                    AddConnectedTriangles(triangle);

                    triangulation.Add(triangle);
                }
            }
            if (removeSuperTriangle)
            {
                for(int i = 0; i < triangulation.Count; ++i)
                {
                    DelaunayCell triangle = triangulation[i];
                    foreach (Vector3D pos in triangle.Points)
                    {
                        if (superTriangle.Points.Contains(pos))
                        {
                            triangulation.RemoveAt(i--);
                            break;
                        }
                    }
                }
            }

            delaunay = triangulation;
            return triangulation;
        }

        private void AddConnectedTriangles(DelaunayCell triangle)
        {
            foreach(Vector3D pos in triangle.Points)
            {
                if (connectedTriangles.ContainsKey(pos))
                    connectedTriangles[pos].Add(triangle);
                else
                    connectedTriangles.Add(pos, new List<DelaunayCell>() { triangle });
            }
        }

        public List<VoronoiCell> GenerateVoronoi()
        {
            foreach(Vector3D site in Sites)
            {
                VoronoiCell cell = new VoronoiCell(site);

                foreach(DelaunayCell triangle in connectedTriangles[site])
                {
                    Vector3D point = triangle.GetCircumcenter().Clone();
                    /*
                    if (point.X > MaxX)
                        point.X = MaxX;
                    else if (point.X < 0)
                        point.X = 0;

                    if (point.Y > MaxY)
                        point.Y = MaxY;
                    else if (point.Y < 0)
                        point.Y = 0;

                    if (point.Z > MaxZ)
                        point.Z = MaxZ;
                    else if (point.Z < 0)
                        point.Z = 0;
                    */
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
