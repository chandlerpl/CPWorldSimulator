#include "../CPWS.CUDACLI/SimplexNoiseCUDA.h"
using namespace std;

extern "C" float* Noise3DCuda(int x, int y, int z, float scale, float persistence, int iterations, int gpuid);

array<float, 2>^ CPWS::WorldGenerator::CUDA::Noise::SimplexNoiseCUDA::NoiseMap(float Scale, float Persistence, int iterations, float x, float y, float z)
{
    return CPWS::WorldGenerator::CUDA::Noise::SimplexNoiseCUDA::NoiseMap(Scale, Persistence, iterations, x, y, z, 0);
}

array<float, 2>^ CPWS::WorldGenerator::CUDA::Noise::SimplexNoiseCUDA::NoiseMap(float Scale, float Persistence, int iterations, float x, float y, float z, int gpuid)
{
    array<float, 2>^ result = gcnew array<float, 2>(y, x);

    float* results = Noise3DCuda(x, y, z, Scale, Persistence, iterations, gpuid);

    for (int y1 = 0; y1 < y; y1++)
    {
        for (int x1 = 0; x1 < x; x1++)
        {
            int x2 = y1 * x;
            result[y1, x1] = results[x2 + x1];
        }
    }

    delete[] results;

    return result;
}