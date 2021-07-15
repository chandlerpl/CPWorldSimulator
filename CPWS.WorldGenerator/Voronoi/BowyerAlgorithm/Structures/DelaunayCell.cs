using CP.Common.Maths;
using System;
using System.Collections.Generic;
using System.Text;

namespace CPWS.WorldGenerator.Voronoi.BowyerAlgorithm.Structures
{
    public class DelaunayCell
    {
        public List<Vector3D> Points { get; set; } = new List<Vector3D>();

        private List<Edge> edges;
        private List<Triangle> faces;
        public List<Edge> Edges { 
            get
            {
                if (edges == null)
                {
                    edges = new List<Edge>();
                    for (int i = 0; i < Points.Count; ++i)
                        for (int j = i + 1; j < Points.Count; ++j)
                            edges.Add(new Edge(Points[i], Points[j]));

                }

                return edges;
            }
        }
        public List<Triangle> Faces
        {
            get
            {
                if(faces == null)
                {
                    faces = new List<Triangle>();
                    int count = Points.Count;
                    for (int i = 0; i < count; ++i)
                        for (int j = i + 1; j < count; ++j)
                        {
                            for (int k = j + 1; k < count; ++k)
                            {
                                Triangle face = new Triangle(Points[i], Points[j], Points[k], false);
                                if(face.GetSideOfPoint(GetCentroid()) > 0)
                                {
                                    face.PointA = Points[j];
                                    face.PointB = Points[i];
                                }
                                if (!faces.Contains(face))
                                {
                                    faces.Add(face);
                                }
                            }
                        }
                }

                return faces;
            }
        }

        private List<Vector3D> uniquePoints = null;
        public List<Vector3D> UniquePoints
        {
            get
            {
                if(uniquePoints == null)
                {
                    uniquePoints = new List<Vector3D>();

                    foreach (Triangle face in Faces)
                    {
                        if (!uniquePoints.Contains(face.PointA))
                            uniquePoints.Add(face.PointA);
                        if (!uniquePoints.Contains(face.PointB))
                            uniquePoints.Add(face.PointB);
                        if (!uniquePoints.Contains(face.PointC))
                            uniquePoints.Add(face.PointC);
                    }
                }

                return uniquePoints;
            }
        }

        public List<DelaunayCell> Neighbours
        {
            get
            {
                List<DelaunayCell> neighbours = new List<DelaunayCell>();
                List<DelaunayCell> connected = new List<DelaunayCell>();
                foreach(Vector3D point in UniquePoints)
                {
                    //connected.AddRange(point.ConnectedTriangles);
                }

                foreach (DelaunayCell triangle in connected)
                {
                    if (IsSharingFace(triangle))
                    {
                        neighbours.Add(triangle);
                    }
                }

                return neighbours;
            }
        }

        public void Reset()
        {
            Points = new List<Vector3D>();
            edges = null;
            faces = null;
            circumcenter = null;
            circumradius = 0;
            circumradiusSqrt = 0;
            uniquePoints = null;
            centroid = null;
        }

        public Vector3D VoronoiCenter { get; set; }
        public List<Edge> VoronoiCellEdges { get; private set; } = new List<Edge>();

        Vector3D circumcenter = null;
        double circumradius;
        public Vector3D GetCircumcenter()
        {
            if (circumcenter == null)
            {
                Vector3D row1 = Points[1] - Points[0];
                double sqLength1 = row1.SqrMagnitude;
                Vector3D row2 = Points[2] - Points[0];
                double sqLength2 = row2.SqrMagnitude;
                Vector3D row3 = Points[3] - Points[0];
                double sqLength3 = row3.SqrMagnitude;

                double determinant = row1.X * (row2.Y * row3.Z - row3.Y * row2.Z) - row2.X * (row1.Y * row3.Z - row3.Y * row1.Z) + row3.X * (row1.Y * row2.Z - row2.Y * row1.Z);
                double volume = determinant / 6f;
                double twelveVolume = 1f / (volume * 12);

                double x = Points[0].X + twelveVolume * ((row2.Y * row3.Z - row3.Y * row2.Z) * sqLength1 - (row1.Y * row3.Z - row3.Y * row1.Z) * sqLength2 + (row1.Y * row2.Z - row2.Y * row1.Z) * sqLength3);
                double y = Points[0].Y + twelveVolume * (-(row2.X * row3.Z - row3.X * row2.Z) * sqLength1 + (row1.X * row3.Z - row3.X * row1.Z) * sqLength2 - (row1.X * row2.Z - row2.X * row1.Z) * sqLength3);
                double z = Points[0].Z + twelveVolume * ((row2.X * row3.Y - row3.X * row2.Y) * sqLength1 - (row1.X * row3.Y - row3.X * row1.Y) * sqLength2 + (row1.X * row2.Y - row2.X * row1.Y) * sqLength3);

                circumcenter = new Vector3D(x, y, z);
                circumradius = (Points[0] - circumcenter).SqrMagnitude;
            }

            return circumcenter;
        }

        Vector3D centroid = null;
        public Vector3D GetCentroid()
        {
            if(centroid == null)
            {
                centroid = new Vector3D(0, 0);

                foreach(Vector3D p in Points)
                {
                    centroid.X += p.X;
                    centroid.Y += p.Y;
                    centroid.Z += p.Z;
                }

                centroid.X /= Points.Count;
                centroid.Y /= Points.Count;
                centroid.Z /= Points.Count;
            } 
            
            return centroid;
        }

        public bool IsPointInCell(Vector3D point)
        {
            foreach (Triangle tri in Faces)
            {
                Console.WriteLine(point + ": " + tri.GetSideOfPoint(point));
                if(tri.GetSideOfPoint(point) > 0)
                {
                    return false;
                }
            }

            return true;
        }

        internal void SetCircumcenter(Vector3D circumcenter)
        {
            this.circumcenter = circumcenter;
            this.circumradius = (Points[0] - circumcenter).SqrMagnitude;
        }

        public bool IsSharingFace(DelaunayCell cell)
        {
            foreach(Triangle face in Faces)
            {
                if(cell.Faces.Contains(face))
                {
                    return true;
                }
            }

            return false;
        }

        public override bool Equals(object obj)
        {
            if (obj is DelaunayCell cell)
            {
                foreach(Vector3D point in Points)
                {
                    bool match = false;
                    foreach (Vector3D cellPoint in cell.Points)
                    {
                        if (point.Equals(cellPoint))
                            match = true;
                    }

                    if (!match)
                        return false;
                }

                return true;
            }

            return base.Equals(obj);
        }
        double circumradiusSqrt;
        public double GetCircumradiusSqrt()
        {
            if (circumradiusSqrt == 0)
                circumradiusSqrt = Math.Sqrt(circumradius);

            return circumradiusSqrt;
        }

        public double GetCircumradius()
        {
            return circumradius;
        }
    }
}
