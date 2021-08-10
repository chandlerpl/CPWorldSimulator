using CP.Common.Maths;
using System;
using System.Collections.Generic;
using System.Text;

namespace CPWS.WorldGenerator.ConvexHull
{
    public class ConvexHull3D
    {
        private List<ConvexHullFace> faces = new List<ConvexHullFace>();
        private List<ConvexHullEdge> edges = new List<ConvexHullEdge>();
        private List<Vector3D> processedPoints = new List<Vector3D>();
        private Dictionary<int, ConvexHullEdge> edgeMap = new Dictionary<int, ConvexHullEdge>();

        public List<Vector3D> ConstructHull(List<Vector3D> points) 
        {
            if (!BuildFirstHull(points))
                return null;

            foreach(Vector3D point in points)
            {
                IncrementHull(point);
                Cleanup();
            }

            return ExtractExteriorPoints();
        }

        public bool BuildFirstHull(List<Vector3D> points)
        {
            int pointCount = points.Count;
            if (pointCount <= 3)
            {
                return false;
            }

            int i = 2;
            while (Colinear(points[i], points[i - 1], points[i - 2]))
            {
                if (i++ == pointCount - 1)
                {
                    return false;
                }
            }

            ConvexHullFace face = new ConvexHullFace(points[i], points[i - 1], points[i - 2]);

            int j = i;
            while (VolumeSign(face, points[j]) == 0)
            {
                if (j++ == pointCount - 1)
                {
                    return false;
                }
            }

            Vector3D pointA = points[i];
            Vector3D pointB = points[i - 1];
            Vector3D pointC = points[i - 2];
            Vector3D pointD = points[j];

            processedPoints.Add(pointA);
            processedPoints.Add(pointB);
            processedPoints.Add(pointC);
            processedPoints.Add(pointD);

            AddOneFace(pointA, pointB, pointC, pointD);
            AddOneFace(pointA, pointB, pointD, pointC);
            AddOneFace(pointA, pointC, pointD, pointB);
            AddOneFace(pointB, pointC, pointD, pointA);

            return true;
        }
    
        void Cleanup()
        {
            for(int i = 0; i < edges.Count; ++i)
            {
                ConvexHullEdge edge = edges[i];
                if (edge.remove)
                {
                    Vector3D point1 = edge.pointA;
                    Vector3D point2 = edge.pointB;

                    int keyToEvict = Key2Edge(point1, point2);
                    edgeMap.Remove(keyToEvict);
                    edges.RemoveAt(i--);
                }


            }

            for (int i = 0; i < faces.Count; ++i)
            {
                ConvexHullFace face = faces[i];

                if (face.visible) faces.RemoveAt(i--);
            }
         }

        List<Vector3D> ExtractExteriorPoints()
        {
            List<Vector3D> points = new List<Vector3D>();
            foreach (ConvexHullFace face in faces)
            {
                    points.Add(face.pointA);
                    points.Add(face.pointB);
                    points.Add(face.pointC);
            }

            return points;
        }

        void IncrementHull(Vector3D pt)
        {
            bool vis = false;
            foreach(ConvexHullFace face in faces)
            {
                if(VolumeSign(face, pt) < 0)
                {
                    face.visible = vis = true;
                }
            }
            if (!vis) return;

            for(int i = 0; i < edges.Count; ++i)
            {
                ConvexHullEdge edge = edges[i];
                ConvexHullFace face1 = edge.adjface1;
                ConvexHullFace face2 = edge.adjface2;

                if(face1 == null || face2 == null)
                {
                    continue;
                }
                else if (face1.visible && face2.visible)
                {
                    edge.remove = true;
                }
                else if (face1.visible || face2.visible)
                {
                    if (face1.visible)
                    {
                        ConvexHullFace faceT = face2;
                        face2 = face1;
                        face1 = faceT;
                    }

                    Vector3D innerPoint = FindInnerPoint(face2, edge);
                    edge.Erase(face2);
                    AddOneFace(edge.pointA, edge.pointB, pt, innerPoint);
                }


            }
        }

        Vector3D FindInnerPoint(ConvexHullFace f, ConvexHullEdge e)
        {
            if (!f.pointA.Equals(e.pointA) && !f.pointA.Equals(e.pointB))
                return f.pointA;
            if (!f.pointB.Equals(e.pointA) && !f.pointB.Equals(e.pointB))
                return f.pointB;
            if (!f.pointC.Equals(e.pointA) && !f.pointC.Equals(e.pointB))
                return f.pointC;

            return null;
        }

        bool Colinear(Vector3D a, Vector3D b, Vector3D c)
        {
            return ((c.Z - a.Z) * (b.Y - a.Y) -
                (b.Z - a.Z) * (c.Y - a.Y)) == 0 &&
                 ((b.Z - a.Z) * (c.X - a.X) -
                  (b.X - a.X) * (c.Z - a.Z)) == 0 &&
                 ((b.X - a.X) * (c.Y - a.Y) -
                  (b.Y - a.Y) * (c.X - a.X)) == 0;
        }

        int VolumeSign(ConvexHullFace face, Vector3D p)
        {
            double vol;
            double ax, ay, az, bx, by, bz, cx, cy, cz;
            ax = face.pointA.X - p.X;
            ay = face.pointA.Y - p.Y;
            az = face.pointA.Z - p.Z;
            bx = face.pointB.X - p.X;
            by = face.pointB.Y - p.Y;
            bz = face.pointB.Z - p.Z;
            cx = face.pointC.X - p.X;
            cy = face.pointC.Y - p.Y;
            cz = face.pointC.Z - p.Z;
            vol = ax * (by * cz - bz * cy) +
                ay * (bz * cx - bx * cz) +
                az * (bx * cy - by * cx);
            if (vol == 0) return 0;

            return vol < 0 ? -1 : 1;
        }

        void AddOneFace(Vector3D pointA, Vector3D pointB, Vector3D pointC, Vector3D innerPoint)
        {
            faces.Add(new ConvexHullFace(pointA, pointB, pointC));
            ConvexHullFace face = faces[faces.Count - 1];

            if (VolumeSign(face, innerPoint) < 0) face.Reverse();

            CreateEdge(pointA, pointB, face);
            CreateEdge(pointA, pointC, face);
            CreateEdge(pointB, pointC, face);
        }

        private void CreateEdge(Vector3D pointA, Vector3D pointB, ConvexHullFace face)
        {
            int key = Key2Edge(pointA, pointB);

            if(!edgeMap.ContainsKey(key))
            {
                ConvexHullEdge edge = new ConvexHullEdge(pointA, pointB);
                edges.Add(edge);
                edgeMap.Add(key, edge);

            }

            edgeMap[key].LinkAdjFace(face);
        }

        private int Key2Edge(Vector3D pointA, Vector3D pointB)
        {
            return pointA.ToString().GetHashCode() ^ pointB.ToString().GetHashCode();
        }
    }
}
