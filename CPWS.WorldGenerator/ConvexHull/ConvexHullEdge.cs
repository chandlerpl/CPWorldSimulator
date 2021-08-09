using CP.Common.Maths;
using System;
using System.Collections.Generic;
using System.Text;

namespace CPWS.WorldGenerator.ConvexHull
{
    public class ConvexHullEdge
    {
        Vector3D pointA, pointB;
        ConvexHullFace adjface1, adjface2;

        public ConvexHullEdge(Vector3D pointA, Vector3D pointB)
        {
            this.pointA = pointA;
            this.pointB = pointB;
        }

        public void LinkAdjFace(ConvexHullFace face)
        {
            if (adjface1 != null && adjface2 != null)
            {
                return;
            }

            if(adjface1 == null)
            {
                adjface1 = face;
            } else
            {
                adjface2 = face;
            }
        }

        public void Erase(ConvexHullFace face)
        {
            if(adjface2 == face)
            {
                adjface2 = null;
            } else
            {
                adjface1 = null;
            }
        }
    }
}
