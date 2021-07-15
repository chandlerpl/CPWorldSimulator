using CP.Common.Maths;
using System;
using System.Collections.Generic;
using System.Text;

namespace CPWS.WorldGenerator.Voronoi.BowyerAlgorithm.Structures
{
    public class Edge
    {
        public Vector3D PointA { get; set; }
        public Vector3D PointB { get; set; }

        public Edge(Vector3D pointA, Vector3D pointB)
        {
            PointA = pointA;
            PointB = pointB;
        }

        public double Length()
        {
            return (PointA - PointB).Length();
        }

        public double SqrMagnitude { get { return (PointA - PointB).SqrMagnitude; } }

        public override bool Equals(object obj)
        {
            if(obj is Edge edge)
            {
                return (edge.PointA.ApproxEquals(PointA) && edge.PointB.ApproxEquals(PointB)) || (edge.PointA.ApproxEquals(PointB) && edge.PointB.ApproxEquals(PointA));
            }
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return PointA.GetHashCode() + PointB.GetHashCode();
        }
    }
}
