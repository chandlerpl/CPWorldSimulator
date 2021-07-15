using CP.Common.Maths;
using System;
using System.Collections.Generic;
using System.Text;

namespace CPWS.WorldGenerator.Voronoi.BowyerAlgorithm.Structures
{
    public class VoronoiCell
    {
        public Vector3D Site { get; private set; }
        public List<Vector3D> Points { get; private set; } = new List<Vector3D>();
        public List<Edge> Edges { get; private set; } = new List<Edge>();
        public VoronoiCell(Vector3D site)
        {
            Site = site;
        }

        public void SortPoints()
        {
            Points.Sort(new Comparison<Vector3D>(SortCornersClockwise));
        }

        public int SortCornersClockwise(Vector3D A, Vector3D B)
        {
            var atanA = Math.Atan2(A.Y - Site.Y, A.X - Site.X);
            var atanB = Math.Atan2(B.Y - Site.Y, B.X - Site.X);

            if (atanA < atanB) return -1;
            else if (atanA > atanB) return 1;
            return 0;
        }
    }
}
