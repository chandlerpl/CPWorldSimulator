using CP.Common.Maths;
using CP.Common.Random;
using System;
using System.Collections.Generic;
using System.Text;

namespace CPWS.WorldGenerator.PoissonDisc
{
    public class PoissonDiscSampling
    {
        public List<PoissonDisc> points = new List<PoissonDisc>();
        public double radius = 1.0;
        public uint seed = 24363;

        public PoissonDiscSampling(double radius, uint seed)
        {
            this.radius = radius;
            this.seed = seed;
        }
        
        public void Sample3D(Vector3D regionSize)
        {
            RandomHash hash = new RandomHash(seed);
            double cellSize = radius / 1.41421356237;

            int[,,] grid = new int[(int)Math.Ceiling(regionSize.X / cellSize), (int)Math.Ceiling(regionSize.Y / cellSize), (int)Math.Ceiling(regionSize.Z / cellSize)];
            List<Vector3D> spawnPoints = new List<Vector3D>
            {
                regionSize / 2
            };
            points.Add(new PoissonDisc(spawnPoints[0], radius));
            grid[(int)(spawnPoints[0].X / cellSize), (int)(spawnPoints[0].Y / cellSize), (int)(spawnPoints[0].Z / cellSize)] = points.Count;

            int currVal = 1;
            while(spawnPoints.Count > 0)
            {
                int spawnIndex = hash.Next(0, spawnPoints.Count, currVal++);
                Vector3D spawnCentre = spawnPoints[spawnIndex];

                bool candidateAccepted = false;
                for (int i = 0; i < 30; i++)
                {
                    double angle = hash.NextDouble(0, 1, (int)(spawnCentre.X * 100) + i, (int)(spawnCentre.Y * 100) + i, (int)(spawnCentre.Z * 100) + i) * Math.PI * 2;
                    double angle2 = hash.NextDouble(0, 1, (int)(spawnCentre.X * 100) + i + 1, (int)(spawnCentre.Y * 100) + i + 1, (int)(spawnCentre.Z * 100) + i + 1) * Math.PI * 2;
                    double x = Math.Cos(angle) * Math.Cos(angle2);
                    double z = Math.Sin(angle) * Math.Cos(angle2);
                    double y = Math.Sin(angle2);

                    Vector3D dir = new Vector3D(x, y, z);
                    Vector3D candidate = spawnCentre + (dir * hash.NextDouble(radius, 2 * radius, (int)(spawnCentre.X * 1000) + i, (int)(spawnCentre.Y * 1000) + i, (int)(spawnCentre.Z * 1000) + i));
                    if (IsValid3D(candidate, regionSize, cellSize, grid))
                    {
                        points.Add(new PoissonDisc(candidate, radius));
                        spawnPoints.Add(candidate);
                        grid[(int)(candidate.X / cellSize), (int)(candidate.Y / cellSize), (int)(candidate.Z / cellSize)] = points.Count;
                        candidateAccepted = true;
                        break;
                    }
                }
                if(!candidateAccepted)
                    spawnPoints.RemoveAt(spawnIndex);
            }
        }
        bool IsValid3D(Vector3D candidate, Vector3D sampleRegionSize, double cellSize, int[,,] grid)
        {
            if (candidate.X >= 0 && candidate.X < sampleRegionSize.X && candidate.Y >= 0 && candidate.Y < sampleRegionSize.Y && candidate.Z >= 0 && candidate.Z < sampleRegionSize.Z)
            {
                int cellX = (int)(candidate.X / cellSize);
                int cellY = (int)(candidate.Y / cellSize);
                int cellZ = (int)(candidate.Z / cellSize);
                int searchStartX = Math.Max(0, cellX - 2);
                int searchEndX = Math.Min(cellX + 2, grid.GetLength(0) - 1);
                int searchStartY = Math.Max(0, cellY - 2);
                int searchEndY = Math.Min(cellY + 2, grid.GetLength(1) - 1);
                int searchStartZ = Math.Max(0, cellZ - 2);
                int searchEndZ = Math.Min(cellZ + 2, grid.GetLength(2) - 1);

                for (int x = searchStartX; x <= searchEndX; x++)
                {
                    for (int y = searchStartY; y <= searchEndY; y++)
                    {
                        for (int z = searchStartZ; z <= searchEndZ; z++)
                        {
                            int pointIndex = grid[x, y, z] - 1;
                            if (pointIndex != -1)
                            {
                                PoissonDisc disc = points[pointIndex];
                                double sqrDst = (candidate - disc.position).SqrMagnitude;
                                if (sqrDst < disc.radiusSquared)
                                {
                                    return false;
                                }
                            }
                        }
                    }
                }
                return true;
            }
            return false;
        }

        public void Sample2D(Vector3D regionSize)
        {
            RandomHash hash = new RandomHash(seed);
            double cellSize = radius / 1.41421356237;

            int[,] grid = new int[(int)Math.Ceiling(regionSize.X / cellSize), (int)Math.Ceiling(regionSize.Y / cellSize)];
            List<Vector3D> spawnPoints = new List<Vector3D>
            {
                regionSize / 2
            };
            points.Add(new PoissonDisc(spawnPoints[0], radius));
            grid[(int)(spawnPoints[0].X / cellSize), (int)(spawnPoints[0].Y / cellSize)] = points.Count;

            int currVal = 1;
            while (spawnPoints.Count > 0)
            {
                int spawnIndex = hash.Next(0, spawnPoints.Count, currVal++);
                Vector3D spawnCentre = spawnPoints[spawnIndex];

                bool candidateAccepted = false;
                for (int i = 0; i < 30; i++)
                {
                    double angle = hash.NextDouble(0, 1, (int)(spawnCentre.X * 100) + i, (int)(spawnCentre.Y * 100) + i, i) * Math.PI * 2;
                    double x = Math.Cos(angle);
                    double y = Math.Sin(angle);

                    Vector3D dir = new Vector3D(x, y, regionSize.Z);
                    Vector3D candidate = spawnCentre + (dir * hash.NextDouble(radius, 2 * radius, (int)(spawnCentre.X * 1000) + i, (int)(spawnCentre.Y * 1000) + i, i));
                    if (IsValid2D(candidate, regionSize, cellSize, grid))
                    {
                        points.Add(new PoissonDisc(candidate, radius));
                        spawnPoints.Add(candidate);
                        grid[(int)(candidate.X / cellSize), (int)(candidate.Y / cellSize)] = points.Count;
                        candidateAccepted = true;
                        break;
                    }
                }
                if (!candidateAccepted)
                    spawnPoints.RemoveAt(spawnIndex);
            }
        }
        bool IsValid2D(Vector3D candidate, Vector3D sampleRegionSize, double cellSize, int[,] grid)
        {
            if (candidate.X >= 0 && candidate.X < sampleRegionSize.X && candidate.Y >= 0 && candidate.Y < sampleRegionSize.Y)
            {
                int cellX = (int)(candidate.X / cellSize);
                int cellY = (int)(candidate.Y / cellSize);
                int searchStartX = Math.Max(0, cellX - 2);
                int searchEndX = Math.Min(cellX + 2, grid.GetLength(0) - 1);
                int searchStartY = Math.Max(0, cellY - 2);
                int searchEndY = Math.Min(cellY + 2, grid.GetLength(1) - 1);

                for (int x = searchStartX; x <= searchEndX; x++)
                {
                    for (int y = searchStartY; y <= searchEndY; y++)
                    {
                        int pointIndex = grid[x, y] - 1;
                        if (pointIndex != -1)
                        {
                            PoissonDisc disc = points[pointIndex];
                            double sqrDst = (candidate - disc.position).SqrMagnitude;
                            if (sqrDst < disc.radiusSquared)
                            {
                                return false;
                            }
                        }
                    }
                }
                return true;
            }
            return false;
        }
    }
}
