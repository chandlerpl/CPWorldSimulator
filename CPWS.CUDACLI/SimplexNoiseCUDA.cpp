/*#include "../CPWS.CUDACLI/SimplexNoiseCUDA.h"
using namespace std;

extern "C" double* Noise3DCuda(short* source, int x, int y, int z, double scale, double persistence, int iterations, int gpuid);


array<double, 2>^ CPWS::CUDA::Noise::SimplexNoiseCUDA::NoiseMap(array<short>^ Source, double Scale, double Persistence, int iterations, double x, double y, double z)
{
    return CPWS::CUDA::Noise::SimplexNoiseCUDA::NoiseMap(Source, Scale, Persistence, iterations, x, y, z, 0);
}

array<double, 2>^ CPWS::CUDA::Noise::SimplexNoiseCUDA::NoiseMap(array<short>^ Source, double Scale, double Persistence, int iterations, double x, double y, double z, int gpuid)
{
    array<double, 2>^ result = gcnew array<double, 2>(y, x);

    pin_ptr<short> UnmanagedSource = &Source[0];

    double* results = Noise3DCuda(UnmanagedSource, x, y, z, Scale, Persistence, iterations, gpuid);

    for (int y1 = 0; y1 < y; y1++)
    {
        for (int x1 = 0; x1 < x; x1++)
        {
            int x2 = y1 * x;
            result[y1, x1] = results[x2 + x1];
        }
    }

    delete[] results;
    delete[] UnmanagedSource;

    return result;
}*/