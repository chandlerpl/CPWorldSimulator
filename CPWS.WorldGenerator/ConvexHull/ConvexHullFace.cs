using CP.Common.Maths;
using System;
using System.Collections.Generic;
using System.Text;

namespace CPWS.WorldGenerator.ConvexHull
{
    public class ConvexHullFace
    {
        public Vector3D pointA;
        public Vector3D pointB;
        public Vector3D pointC;
        public bool visible = false;

        public ConvexHullFace(Vector3D pointA, Vector3D pointB, Vector3D pointC)
        {
            this.pointA = pointA;
            this.pointB = pointB;
            this.pointC = pointC;
        }

        public void Reverse()
        {
            Vector3D n = pointC;
            pointC = pointA;
            pointA = n;
        }
    }
}
