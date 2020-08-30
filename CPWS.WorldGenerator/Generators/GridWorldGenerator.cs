using System;
using System.Collections.Generic;
using System.Text;
using CPWS.WorldGenerator.Generators.Data;
using CPWS.WorldGenerator.Maps;
using CPWS.WorldGenerator.Voronoi.FortunesAlgorithm.Structure.Points;

namespace CPWS.WorldGenerator.Generators
{
    public class GridWorldGenerator : Generator
    {
        public GridWorldData Data;

        public GridWorldGenerator(uint seed, NoiseType noiseType, int x, int y) : base(seed, noiseType)
        {
            Data = new GridWorldData(x, y);
            voronoiGen = new Voronoi.FortunesAlgorithm.FortunesAlgorithm((int)seed, maxX: x, maxY: y);
            voronoiGen.Relax(4);
        }

        public override async void Generate()
        {
            List<Point> sites = voronoiGen.Sites.ConvertAll(a => { return new Point(a.X, a.Y); });
            //double[,] heightMap = await noiseGen.NoiseMap(4, Data.X, Data.Y);
            //Data.SetHeightData(heightMap);

            //Data.SetTemperatureData(await Heatmap.GenerateHeatmap(noiseGen, Data.Y, Data.X, heightMap));

            
        }
    }
}
