using namespace CP::CommonCUDA;

namespace CPWS::WorldGenerator::Test::Noise {
    public ref class NoiseGen abstract
    {
    private:
        unsigned int seed;
        bool useCuda;

    public:
        double persistence;
        double scale;

        NoiseGen(unsigned int seed, double persistence, double scale, bool useCuda) : seed(seed), persistence(persistence), scale(scale), useCuda(useCuda) { }

        virtual double Noise(array<double>^ vals) = 0;

        virtual double Octave(int iterations, array<double>^ vals) = 0;

        virtual array<double, 2>^ NoiseMap(int iterations, array<int>^ vals) = 0;

        virtual array<double, 2>^ NoiseMapNotAsync(int iterations, array<int>^ vals) = 0;

        unsigned int getSeed() { return seed; }

        bool UseCuda()
        {
            if (!useCuda)
                return false;

            if (!CUDAUtilities::GetCapabilities())
                useCuda = false;

            return useCuda;
        }

        void setUseCuda(bool val) { useCuda = val; }
    protected:
        void setSeed(unsigned int newSeed) { seed = newSeed; }

        virtual double UnmanagedNoise(int dimensions, double vals[]) = 0;
    };
}


