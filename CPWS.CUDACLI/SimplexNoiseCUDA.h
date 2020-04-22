#pragma once

namespace CPWS
{
    namespace CUDA
    {
        namespace Noise
        {
            public ref class SimplexNoiseCUDA abstract sealed
            {
            public:
                static array<double, 2>^ NoiseMap(array<short>^ Source, double Scale, double Persistence, int iterations, double d1, double d2, double d3);
                static array<double, 2>^ NoiseMap(array<short>^ Source, double Scale, double Persistence, int iterations, double d1, double d2, double d3, int gpuid);
            private:

            };
        }
    }
}