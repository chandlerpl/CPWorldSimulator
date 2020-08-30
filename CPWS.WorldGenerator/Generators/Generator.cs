using System;
using System.Collections.Generic;
using System.Text;
using CPWS.WorldGenerator.Noise;
using CPWS.WorldGenerator.Voronoi.FortunesAlgorithm;

namespace CPWS.WorldGenerator.Generators
{
    public enum NoiseType
    {
        SIMPLEX,
        PERLIN,
        CUSTOM
    }

    public abstract class Generator
    {
        public uint Seed { get; private set; }

        public NoiseGen noiseGen;
        public FortunesAlgorithm voronoiGen;

        public Generator(uint seed, NoiseType noiseType)
        {
            Seed = seed;
            
            switch(noiseType)
            {
                case NoiseType.SIMPLEX:
                    noiseGen = new SimplexNoise2(seed, 0.05, 0.5, false);
                    break;
                case NoiseType.PERLIN:
                    //noiseGen = new PerlinNoise(seed);
                    break;
                case NoiseType.CUSTOM:
                    //noiseGen = new CustomNoise(seed);
                    break;
            }

            Generate();
        }

        public abstract void Generate();
    }
}
