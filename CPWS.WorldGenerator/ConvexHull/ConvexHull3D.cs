using CP.Common.Maths;
using System;
using System.Collections.Generic;
using System.Text;

namespace CPWS.WorldGenerator.ConvexHull
{
    public class ConvexHull3D
    {
        public List<Vector3D> processedPoints = new List<Vector3D>();

        public void ConstructHull(ref List<Vector3D> points) 
        {
            if (!BuildFirstHull(points))
                return;

            foreach(Vector3D point in points)
            {
                IncrementHull(point);
                Cleanup();
            }

            ExtractExteriorPoints();
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
            while (VolumeSign(face, points[j]))
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

            AddOnceFace(pointA, pointB, pointC, pointD);
            AddOnceFace(pointA, pointB, pointD, pointC);
            AddOnceFace(pointA, pointC, pointD, pointB);
            AddOnceFace(pointB, pointC, pointD, pointA);

            return true;
        }

        Vector3D FindInnerPoint(ConvexHullFace f, ConvexHullEdge e)
        {
            for(int i = 0; i < 3; ++i)
            {
                
            }

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

        }
    }
}
