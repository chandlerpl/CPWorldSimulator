using CP.Common.Maths;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CPWS.WorldGenerator.Voronoi.BowyerAlgorithm.Structures
{
    public class Triangle
    {
        private Vector3D pointA;
        private Vector3D pointB;
        private Vector3D pointC;

        public Vector3D PointA
        {
            get => pointA;
            set
            {
                pointA = value;
                edgeCA = null;
                edgeAB = null;
                edges = null;
                points = null;
            }
        }
        public Vector3D PointB
        {
            get => pointB;
            set
            {
                pointB = value;
                edgeAB = null;
                edgeBC = null;
                edges = null;
                points = null;
            }
        }
        public Vector3D PointC
        {
            get => pointC; 
            set
            {
                pointC = value;
                edgeCA = null;
                edgeBC = null;
                edges = null;
                points = null;
            }
        }
        

        private Edge edgeAB;
        private Edge edgeBC;
        private Edge edgeCA;
        public Edge EdgeAB => edgeAB ?? (edgeAB = new Edge(PointA, PointB));
        public Edge EdgeBC => edgeBC ?? (edgeBC = new Edge(PointB, PointC));
        public Edge EdgeCA => edgeCA ?? (edgeCA = new Edge(PointC, PointA));

        private List<Edge> edges = null;
        public List<Edge> Edges { get
            {
                if(edges == null)
                {
                    edges = new List<Edge>
                    {
                        EdgeAB,
                        EdgeBC,
                        EdgeCA
                    };
                }

                return edges;
            }
        }
        private List<Vector3D> points = null;
        public List<Vector3D> Points
        {
            get
            {
                if (points == null)
                {
                    points = new List<Vector3D>
                    {
                        PointA,
                        PointB,
                        PointC
                    };
                }

                return points;
            }
        }

        public Triangle() { }

        public Triangle(Vector3D pointA, Vector3D pointB, Vector3D pointC, bool checkNormal)
        {
            PointA = pointA;
            PointB = pointB;
            PointC = pointC;

            if(checkNormal && !CheckNormal())
            {
                PointB = pointA;
                PointA = pointB;
            }
        }

        public void Reset()
        {
            PointA = null;
            PointB = null;
            PointC = null;
            circumcenter = null;
            circumradius = 0;
            circumradiusSqrt = 0;
            centroid = null;
        }

        private bool CheckNormal()
        {
            double dot = GetNormal().Dot(PointA);

            return Math.Sign(dot) > 0;
        }


        public bool IsSharingEdge(Triangle triangle)
        {
            return Points.Where(p => triangle.Points.Contains(p)).Count() == 2;
        }

        public Vector3D GetNormal()
        {
            return (PointB - PointA).Cross(PointC - PointA);
        }

        public int GetSideOfPoint(Vector3D point)
        {
            double dot = GetNormal().Dot(point - PointA);

            return Math.Sign(dot);
        }

        public override bool Equals(object obj)
        {
            if (obj is Triangle tri)
            {
                bool e1 = EdgeAB.Equals(tri.EdgeAB) || EdgeAB.Equals(tri.EdgeBC) || EdgeAB.Equals(tri.EdgeCA);
                bool e2 = EdgeBC.Equals(tri.EdgeAB) || EdgeBC.Equals(tri.EdgeBC) || EdgeBC.Equals(tri.EdgeCA);
                bool e3 = EdgeCA.Equals(tri.EdgeAB) || EdgeCA.Equals(tri.EdgeBC) || EdgeCA.Equals(tri.EdgeCA);

                return e1 && e2 && e3;
            }

            return base.Equals(obj);
        }

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

                double determinant = row1.X * (row2.Y) - row2.X * (row1.Y);
                double volume = determinant / 6f;
                double twelveVolume = 1f / (volume * 12);

                double x = Points[0].X + twelveVolume * ((row2.Y) * sqLength1 - (row1.Y) * sqLength2);
                double y = Points[0].Y + twelveVolume * (-(row2.X) * sqLength1 + (row1.X) * sqLength2);
               
                circumcenter = new Vector3D(x, y, 0);
                circumradius = (Points[0] - circumcenter).SqrMagnitude;
            }

            return circumcenter;
        }

        public double GetCircumradius()
        {
            return circumradius;
        }

        double circumradiusSqrt = 0;
        public double GetCircumradiusSqrt()
        {
            if(circumradiusSqrt == 0)
                circumradiusSqrt = Math.Sqrt(circumradiusSqrt);

            return circumradiusSqrt;
        }

        Vector3D centroid = null;
        public Vector3D GetCentroid()
        {
            if (centroid == null)
            {
                centroid = new Vector3D(0, 0);

                foreach (Vector3D p in Points)
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
    }
}
