using CP.Common.Maths;
using System;
using System.Collections.Generic;
using System.Text;

namespace CPWS.WorldGenerator.PoissonDisc
{
    public class PoissonDisc
    {
        public Vector3D position;
        public double radius;
        public double radiusSquared;
        List<PoissonDisc> neighbours = new List<PoissonDisc>(0);

        public PoissonDisc(Vector3D position, double radius)
        {
            this.position = position;
            this.radius = radius;
            this.radiusSquared = radius * radius;
        }

        public void AddNeighbour(PoissonDisc neighbour)
        {
            neighbours.Add(neighbour);
        }

        public List<PoissonDisc> GetNeighbours()
        {
            return neighbours;
        }

        public int GetNeighbourCount()
        {
            return neighbours.Count;
        }
    }
}
