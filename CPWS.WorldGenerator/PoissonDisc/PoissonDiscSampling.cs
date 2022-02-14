using CP.Common.Maths;
using CP.Common.Random;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace CPWS.WorldGenerator.PoissonDisc
{
    public class PoissonDiscSampling
    {

        private static readonly double pi = 2 * Math.PI;

        public double radius = 1.0;
        public uint seed = 24363;
        public int k = 4;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="radius">Distance between each point</param>
        /// <param name="seed">The seed for the random attribute</param>
        public PoissonDiscSampling(double radius, uint seed)
        {
            this.radius = radius;
            this.seed = seed;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="radius">Distance between each point</param>
        /// <param name="seed">The seed for the random attribute</param>
        /// <param name="k">The number of attempts this algorithm runs to find a new point per point (default: 5)</param>
        public PoissonDiscSampling(double radius, uint seed, int k)
        {
            this.radius = radius;
            this.seed = seed;
            this.k = k;
        }

        public List<PoissonDisc> Sample3D(Vector3D regionSize, bool createNeighbours = false)
        {
            List<PoissonDisc> points = new List<PoissonDisc>();

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
            while (spawnPoints.Count > 0)
            {
                int spawnIndex = hash.Next(0, spawnPoints.Count, currVal++);
                Vector3D spawnCentre = spawnPoints[spawnIndex];

                double seed = hash.NextDouble(0, 1, currVal++);
                double seed2 = hash.NextDouble(0, 1, currVal++);
                double r = radius + 0.0000001;

                bool candidateAccepted = false;
                for (int j = 0; j < k; j++)
                {
                    double theta = pi * (seed + 1.0 * j / k);
                    double theta2 = pi * (seed2 + 1.0 * j / k);

                    double x = spawnCentre.X + r * Math.Cos(theta) * Math.Cos(theta2);
                    double y = spawnCentre.Y + r * Math.Sin(theta) * Math.Cos(theta2);
                    double z = spawnCentre.Z + r * Math.Sin(theta2);

                    Vector3D candidate = new Vector3D(x, y, z);

                    if (IsValid3D(candidate, regionSize, cellSize, grid, points))
                    {
                        points.Add(new PoissonDisc(candidate, radius));
                        spawnPoints.Add(candidate);
                        grid[(int)(candidate.X / cellSize), (int)(candidate.Y / cellSize), (int)(candidate.Z / cellSize)] = points.Count;
                        candidateAccepted = true;
                        break;
                    }
                }
                if (!candidateAccepted)
                    spawnPoints.RemoveAt(spawnIndex);
            }

            if (createNeighbours)
                return CreateNeighbourList3D(points, grid, cellSize);

            return points;
        }
        bool IsValid3D(Vector3D candidate, Vector3D sampleRegionSize, double cellSize, int[,,] grid, List<PoissonDisc> points)
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

        List<PoissonDisc> CreateNeighbourList3D(List<PoissonDisc> points, int[,,] grid, double cellSize)
        {
            List<PoissonDisc> newPoints = new List<PoissonDisc>();

            for (int x = 0; x < grid.GetLength(0); x++)
            {
                for (int y = 0; y < grid.GetLength(1); y++)
                {
                    for (int z = 0; z < grid.GetLength(2); z++)
                    {
                        int pointIndex = grid[x, y, z] - 1;
                        if (pointIndex != -1)
                        {
                            PoissonDisc disc = points[pointIndex];
                            newPoints.Add(disc);

                            int searchStartX = Math.Max(0, x - 2);
                            int searchEndX = Math.Min(x + 2, grid.GetLength(0) - 1);
                            int searchStartY = Math.Max(0, y - 2);
                            int searchEndY = Math.Min(y + 2, grid.GetLength(1) - 1);
                            int searchStartZ = Math.Max(0, z - 2);
                            int searchEndZ = Math.Min(z + 2, grid.GetLength(2) - 1);

                            for (int x1 = searchStartX; x1 <= searchEndX; x1++)
                            {
                                for (int y1 = searchStartY; y1 <= searchEndY; y1++)
                                {
                                    for (int z1 = searchStartZ; z1 <= searchEndZ; z1++)
                                    {
                                        int pointIndex2 = grid[x1, y1, z1] - 1;
                                        if (pointIndex2 != -1 && pointIndex2 != pointIndex)
                                        {
                                            PoissonDisc disc2 = points[pointIndex2];

                                            double sqrDst = (disc2.position - disc.position).SqrMagnitude;
                                            if (sqrDst < disc.radiusSquared * 2)
                                            {
                                                disc.AddNeighbour(disc2);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return newPoints;
        }

        public List<PoissonDisc> Sample2D(Vector3D regionSize, bool createNeighbours = false, Vector3D centerPos = null, Func<Vector3D, float> calcRadius = null)
        {
            List<PoissonDisc> points = new List<PoissonDisc>();

            RandomHash hash = new RandomHash(seed);
            double cellSize = (radius * 2) / 1.41421356237; // Minimum distance between cells divided by sqrt(2)

            int[,] grid = new int[(int)Math.Ceiling(regionSize.X / cellSize), (int)Math.Ceiling(regionSize.Y / cellSize)];
            List<PoissonDisc> spawnPoints = new List<PoissonDisc>
        {
            new PoissonDisc(centerPos ?? regionSize / 2, radius)
        };

            points.Add(spawnPoints[0]);
            grid[(int)(spawnPoints[0].position.X / cellSize), (int)(spawnPoints[0].position.Y / cellSize)] = points.Count;

            int currVal = 0;
            while (spawnPoints.Count > 0)
            {
                int spawnIndex = hash.Next(0, spawnPoints.Count, ++currVal);
                PoissonDisc spawnCentre = spawnPoints[spawnIndex];

                bool candidateAccepted = false;
                double seed = hash.NextDouble(0, 1, currVal);
                double nextRadius = calcRadius == null ? radius : calcRadius(spawnCentre.position);
                double distance = spawnCentre.radius + nextRadius;
                double r = distance + 0.0000001;

                for (int j = 0; j < k; j++)
                {
                    double theta = pi * (seed + 1.0 * j / k);

                    double x = spawnCentre.position.X + r * Math.Cos(theta);
                    double y = spawnCentre.position.Y + r * Math.Sin(theta);

                    Vector3D candidate = new Vector3D(x, y, 0);
                    if (IsValid2D(candidate, nextRadius, regionSize, cellSize, grid, points))
                    {
                        if (distance > maxRadius)
                        {
                            maxRadius = distance;
                            searchZone = (int)Math.Ceiling(distance / cellSize);
                        }
                        PoissonDisc disc = new PoissonDisc(candidate, nextRadius);
                        points.Add(disc);
                        spawnPoints.Add(disc);
                        grid[(int)(candidate.X / cellSize), (int)(candidate.Y / cellSize)] = points.Count;
                        candidateAccepted = true;
                        break;
                    }
                }
                if (!candidateAccepted)
                    spawnPoints.RemoveAt(spawnIndex);
            }

            if (createNeighbours)
                return CreateNeighbourList2D(points, grid, cellSize);

            return points;
        }

        double maxRadius = 0f;
        int searchZone = 2;
        bool IsValid2D(Vector3D candidate, double radius, Vector3D sampleRegionSize, double cellSize, int[,] grid, List<PoissonDisc> points)
        {
            if (candidate.X >= 0 && candidate.X < sampleRegionSize.X && candidate.Y >= 0 && candidate.Y < sampleRegionSize.Y)
            {
                int cellX = (int)(candidate.X / cellSize);
                int cellY = (int)(candidate.Y / cellSize);
                int searchStartX = Math.Max(0, cellX - searchZone);
                int searchEndX = Math.Min(cellX + searchZone, grid.GetLength(0) - 1);
                int searchStartY = Math.Max(0, cellY - searchZone);
                int searchEndY = Math.Min(cellY + searchZone, grid.GetLength(1) - 1);

                for (int x = searchStartX; x <= searchEndX; x++)
                {
                    for (int y = searchStartY; y <= searchEndY; y++)
                    {
                        int pointIndex = grid[x, y] - 1;
                        if (pointIndex != -1)
                        {
                            PoissonDisc disc = points[pointIndex];
                            double sqrDst = (candidate - disc.position).SqrMagnitude;
                            double r = disc.radius + radius;
                            if (sqrDst < r * r)
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

        List<PoissonDisc> CreateNeighbourList2D(List<PoissonDisc> points, int[,] grid, double cellSize)
        {
            List<PoissonDisc> newPoints = new List<PoissonDisc>();

            for (int x = 0; x < grid.GetLength(0); x++)
            {
                for (int y = 0; y < grid.GetLength(1); y++)
                {
                    int pointIndex = grid[x, y] - 1;
                    if (pointIndex != -1)
                    {
                        PoissonDisc disc = points[pointIndex];
                        newPoints.Add(disc);

                        int searchStartX = Math.Max(0, x - 2);
                        int searchEndX = Math.Min(x + 2, grid.GetLength(0) - 1);
                        int searchStartY = Math.Max(0, y - 2);
                        int searchEndY = Math.Min(y + 2, grid.GetLength(1) - 1);

                        for (int x1 = searchStartX; x1 <= searchEndX; x1++)
                        {
                            for (int y1 = searchStartY; y1 <= searchEndY; y1++)
                            {
                                int pointIndex2 = grid[x1, y1] - 1;
                                if (pointIndex2 != -1 && pointIndex2 != pointIndex)
                                {
                                    PoissonDisc disc2 = points[pointIndex2];

                                    double sqrDst = (disc2.position - disc.position).SqrMagnitude;
                                    if (sqrDst < disc.radiusSquared * 2)
                                    {
                                        disc.AddNeighbour(disc2);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return newPoints;
        }
    }
}