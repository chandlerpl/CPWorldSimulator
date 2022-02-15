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

        public static List<PoissonDisc> Sample3D(uint seed, double minRadius, Vector3D regionSize, int k = 4, bool createNeighbours = false, Vector3D centerPos = null, Func<Vector3D, float> calcRadius = null)
        {
            List<PoissonDisc> points = new List<PoissonDisc>();

            RandomHash hash = new RandomHash(seed);
            double cellSize = minRadius * 2 / 1.41421356237;
            double maxRadius = 0f;
            int searchZone = 2;

            int[,,] grid = new int[(int)Math.Ceiling(regionSize.X / cellSize), (int)Math.Ceiling(regionSize.Y / cellSize), (int)Math.Ceiling(regionSize.Z / cellSize)];
            List<PoissonDisc> spawnPoints = new List<PoissonDisc>
            {
            new PoissonDisc(centerPos ?? regionSize / 2, minRadius)
            };

            points.Add(spawnPoints[0]);
            grid[(int)(spawnPoints[0].position.X / cellSize), (int)(spawnPoints[0].position.Y / cellSize), (int)(spawnPoints[0].position.Z / cellSize)] = points.Count;

            int currVal = 1;
            while (spawnPoints.Count > 0)
            {
                int spawnIndex = hash.Next(0, spawnPoints.Count, currVal++);
                PoissonDisc spawnCentre = spawnPoints[spawnIndex];

                double randRot = hash.NextDouble(0, 1, currVal++);
                double randRot2 = hash.NextDouble(0, 1, currVal++);
                double nextRadius = calcRadius == null ? minRadius : calcRadius(spawnCentre.position);
                double distance = spawnCentre.radius + nextRadius;
                double r = distance + 0.0000001;

                bool candidateAccepted = false;
                for (int j = 0; j < k; j++)
                {
                    double theta = pi * (randRot + 1.0 * j / k);
                    double theta2 = pi * (randRot2 + 1.0 * j / k);

                    double x = spawnCentre.position.X + r * Math.Cos(theta) * Math.Cos(theta2);
                    double y = spawnCentre.position.Y + r * Math.Sin(theta) * Math.Cos(theta2);
                    double z = spawnCentre.position.Z + r * Math.Sin(theta2);

                    Vector3D candidate = new Vector3D(x, y, z);

                    if (IsValid3D(candidate, nextRadius, searchZone, regionSize, cellSize, grid, points))
                    {
                        if (distance > maxRadius)
                        {
                            maxRadius = distance;
                            searchZone = (int)Math.Ceiling(distance / cellSize);
                        }
                        PoissonDisc disc = new PoissonDisc(candidate, minRadius);
                        points.Add(disc);
                        spawnPoints.Add(disc);
                        grid[(int)(candidate.X / cellSize), (int)(candidate.Y / cellSize), (int)(candidate.Z / cellSize)] = points.Count;
                        candidateAccepted = true;
                        break;
                    }
                }
                if (!candidateAccepted)
                    spawnPoints.RemoveAt(spawnIndex);
            }

            if (createNeighbours)
                return CreateNeighbourList3D(points, grid, searchZone);

            return points;
        }
        private static bool IsValid3D(Vector3D candidate, double radius, int searchZone, Vector3D sampleRegionSize, double cellSize, int[,,] grid, List<PoissonDisc> points)
        {
            if (candidate.X >= 0 && candidate.X < sampleRegionSize.X && candidate.Y >= 0 && candidate.Y < sampleRegionSize.Y && candidate.Z >= 0 && candidate.Z < sampleRegionSize.Z)
            {
                int cellX = (int)(candidate.X / cellSize);
                int cellY = (int)(candidate.Y / cellSize);
                int cellZ = (int)(candidate.Z / cellSize);
                int searchStartX = Math.Max(0, cellX - searchZone);
                int searchEndX = Math.Min(cellX + searchZone, grid.GetLength(0) - 1);
                int searchStartY = Math.Max(0, cellY - searchZone);
                int searchEndY = Math.Min(cellY + searchZone, grid.GetLength(1) - 1);
                int searchStartZ = Math.Max(0, cellZ - searchZone);
                int searchEndZ = Math.Min(cellZ + searchZone, grid.GetLength(2) - 1);

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
                                double r = disc.radius + radius;
                                if (sqrDst < r * r)
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

        private static List<PoissonDisc> CreateNeighbourList3D(List<PoissonDisc> points, int[,,] grid, int searchZone)
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

                            int searchStartX = Math.Max(0, x - searchZone);
                            int searchEndX = Math.Min(x + searchZone, grid.GetLength(0) - 1);
                            int searchStartY = Math.Max(0, y - searchZone);
                            int searchEndY = Math.Min(y + searchZone, grid.GetLength(1) - 1);
                            int searchStartZ = Math.Max(0, z - searchZone);
                            int searchEndZ = Math.Min(z + searchZone, grid.GetLength(2) - 1);

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

        public static List<PoissonDisc> Sample2D(uint seed, double minRadius, Vector3D regionSize, int k = 4, bool createNeighbours = false, Vector3D centerPos = null, Func<Vector3D, float> calcRadius = null)
        {
            List<PoissonDisc> points = new List<PoissonDisc>();
            double maxRadius = 0f;
            int searchZone = 2;

            RandomHash hash = new RandomHash(seed);
            double cellSize = minRadius * 2 / 1.41421356237; // Minimum distance between cells divided by sqrt(2)

            int[,] grid = new int[(int)Math.Ceiling(regionSize.X / cellSize), (int)Math.Ceiling(regionSize.Y / cellSize)];
            List<PoissonDisc> spawnPoints = new List<PoissonDisc>
            {
                new PoissonDisc(centerPos ?? regionSize / 2, minRadius)
            };

            points.Add(spawnPoints[0]);
            grid[(int)(spawnPoints[0].position.X / cellSize), (int)(spawnPoints[0].position.Y / cellSize)] = points.Count;

            int currVal = 0;
            while (spawnPoints.Count > 0)
            {
                int spawnIndex = hash.Next(0, spawnPoints.Count, ++currVal);
                PoissonDisc spawnCentre = spawnPoints[spawnIndex];

                bool candidateAccepted = false;
                double randRot = hash.NextDouble(0, 1, currVal);
                double nextRadius = calcRadius == null ? minRadius : calcRadius(spawnCentre.position);
                double distance = spawnCentre.radius + nextRadius;
                double r = distance + 0.0000001;

                for (int j = 0; j < k; j++)
                {
                    double theta = pi * (randRot + 1.0 * j / k);

                    double x = spawnCentre.position.X + r * Math.Cos(theta);
                    double y = spawnCentre.position.Y + r * Math.Sin(theta);

                    Vector3D candidate = new Vector3D(x, y, 0);
                    if (IsValid2D(candidate, nextRadius, searchZone, regionSize, cellSize, grid, points))
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
                return CreateNeighbourList2D(points, grid, searchZone);

            return points;
        }

        private static bool IsValid2D(Vector3D candidate, double radius, int searchZone, Vector3D sampleRegionSize, double cellSize, int[,] grid, List<PoissonDisc> points)
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

        private static List<PoissonDisc> CreateNeighbourList2D(List<PoissonDisc> points, int[,] grid, int searchZone)
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

                        int searchStartX = Math.Max(0, x - searchZone);
                        int searchEndX = Math.Min(x + searchZone, grid.GetLength(0) - 1);
                        int searchStartY = Math.Max(0, y - searchZone);
                        int searchEndY = Math.Min(y + searchZone, grid.GetLength(1) - 1);

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