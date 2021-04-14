using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using CP.Common.Utilities;

namespace CPWS.WorldGenerator.Noise
{
    public abstract class NoiseGen
    {
        protected uint Offset { get; } = 1455382547;

        private uint seed;
        private double persistence;
        private double scale;
        private bool useCuda;

        public virtual uint Seed { get => seed; protected set => seed = value; }
        public virtual double Persistence { get => persistence; set => persistence = value; }
        public virtual double Scale { get => scale; set => scale = value; }
        public virtual bool UseCuda 
        { 
            get 
            {
                if (!useCuda)
                    return false;

                if (!GPUUtilities.CheckCUDACapability())
                    useCuda = false;

                return useCuda;
            } set => useCuda = value; 
        }

        public virtual int CudaId { get => GPUUtilities.GetStrongestCudaGpu(); }

        public NoiseGen(uint seed)
        {
            Seed = seed;
        }

        public abstract double Noise(int dimensions, params double[] vals);

        public abstract double Octave(int iterations, int dimensions, params double[] vals);

        public abstract Task<double[,]> NoiseMap(int iterations, params int[] vals);

        public abstract double[,] NoiseMapNotAsync(int iterations, params int[] vals);
    }
}
