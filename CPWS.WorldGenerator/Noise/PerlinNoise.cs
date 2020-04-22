using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CPWS.WorldGenerator.Noise
{
    class PerlinNoise : NoiseGen
    {
        public PerlinNoise(uint seed) : base(seed)
        {
            throw new NotImplementedException("Perlin Noise function not yet implemented.");
        }

        public override double Noise(params double[] vals)
        {
            throw new NotImplementedException();
        }

        public override Task<double[,]> NoiseMap(int iterations, params int[] vals)
        {
            throw new NotImplementedException();
        }

        public override double[,] NoiseMapNotAsync(int iterations, params int[] vals)
        {
            throw new NotImplementedException();
        }

        public override double Octave(int iterations, params double[] vals)
        {
            throw new NotImplementedException();
        }
    }
}
