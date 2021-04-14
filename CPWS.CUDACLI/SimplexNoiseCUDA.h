#pragma once

namespace CPWS
{
    namespace WorldGenerator 
    {
        namespace CUDA
        {
            namespace Noise
            {
                public ref class SimplexNoiseCUDA abstract sealed
                {
                public:
                    static array<float, 2>^ NoiseMap(float Scale, float Persistence, int iterations, float d1, float d2, float d3);
                    static array<float, 2>^ NoiseMap(float Scale, float Persistence, int iterations, float d1, float d2, float d3, int gpuid);
                private:

                };
            }
        }

    }
}