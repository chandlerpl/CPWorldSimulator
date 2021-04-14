#include "SimplexNoise.h"
#include <math.h>
#include <vector>

namespace CPWS::WorldGenerator::Test::Noise {
    static int primeList[4]{ 1619, 31337, 6971, 1013 };

    std::vector<int> ivvals;
    std::vector<double> vvals;
    std::vector<double> xvals;
    std::vector<double> ranks;
    double* values;

    double G;
    double F;

    void SimplexNoise::setup(int dimensions) {
        vvals = std::vector<double>(dimensions);
        ivvals = std::vector<int>(dimensions);
        xvals = std::vector<double>(dimensions);
        ranks = std::vector<double>(dimensions);
        values = new double[dimensions];

        G = ((dimensions + 1) - sqrt(dimensions + 1)) / ((dimensions + 1) * dimensions);
        F = (sqrt(dimensions + 1) - 1) / dimensions;
    }

    array<double, 2>^ SimplexNoise::NoiseMap(int iterations, array<int>^ vals) {
        return nullptr;
    }

    array<double, 2>^ SimplexNoise::NoiseMapNotAsync(int iterations, array<int>^ vals) {
        array<double, 2>^ buffer = gcnew array<double, 2>(vals[1], vals[0]);

        int dimensions = vals->Length;
        setup(dimensions);

        array<double>^ elements = gcnew array<double>(3);
        elements[2] = 0;

        for (int y = 0; y < vals[1]; ++y)
        {
            elements[1] = y;
            for (int x = 0; x < vals[0]; ++x)
            {
                elements[0] = x;
                buffer[y, x] = Octave(iterations, elements);
            }
        }

        delete[] values;

        return buffer;
    }

    double SimplexNoise::Noise(array<double>^ vals) {
        int dimensions = vals->Length;
        setup(dimensions);

        for (int j = 0; j < dimensions; ++j)
            values[j] = vals[j];

        return UnmanagedNoise(dimensions, values);
    }

    double SimplexNoise::Octave(int iterations, array<double>^ vals) {

        double maxAmp = 0;
        double amp = 1;
        double freq = scale;
        double noise = 0;

        int len = vals->Length;
        for (int i = 0; i < iterations; ++i)
        {
            for (int j = 0; j < len; ++j)
                values[j] = vals[j] * freq;

            noise += UnmanagedNoise(len, values) * amp;
            maxAmp += amp;
            amp *= persistence;
            freq *= 2;
        }
        
        noise /= maxAmp;

        return noise;
    }

    double SimplexNoise::UnmanagedNoise(int dimensions, double vals[])
    {
        double s = 0;
        for (int i = 0; i < dimensions; ++i)
            s += vals[i];
        s *= F;

        double t = 0;
        for (int i = 0; i < dimensions; ++i)
        {
            vvals[i] = 0;
            xvals[i] = 0;
            ranks[i] = 0;
            ivvals[i] = (int)(vals[i] + s);
            t += ivvals[i];
        }
        t *= G;

        for (int i = dimensions - 1; i >= 0; --i)
        {
            xvals[i] = vals[i] - (ivvals[i] - t);
            for (int j = i + 1; j < dimensions; ++j)
                if (xvals[i] > xvals[j]) ranks[i]++; else ranks[j]++;
        }
        double n = 0;
        int temp = dimensions - 1;

        for (int i = 0; i < dimensions + 1; ++i)
        {
            t = 0.6;
            unsigned int hash = getSeed();

            for (int j = 0; j < dimensions; ++j)
            {
                int ival = 0;
                if (i > 0) ival = (i == dimensions ? 1 : (ranks[j] >= temp ? 1 : 0));
                double vval = vvals[j] = i == 0 ? xvals[j] : xvals[j] - ival + i * G;

                t -= vval * vval;

                hash ^= (unsigned int)(primeList[j % 4] * (ivvals[j] + ival));
            }
            if (i > 0) temp--;
            if (t >= 0)
            {
                hash = hash * hash * hash * 60493;
                hash = (hash >> 13) ^ hash;

                hash &= 15;

                double result = 0.0;
                int current = 1;

                for (int j = dimensions - 1; j > -1; --j)
                {
                    result += (hash & current) == 0 ? -vvals[j] : vvals[j];
                    current *= 2;
                }

                n += (t * t) * t * t * result;
            }
        }

        return 32.0 * n;
    }
}
