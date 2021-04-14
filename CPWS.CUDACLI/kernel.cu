#pragma unmanaged
#include "cuda_runtime.h"
#include "device_launch_parameters.h"

#include <cmath>
#include <stdio.h>

using namespace std;
extern "C" float* Noise3DCuda(int x, int y, int z, float scale, float persistence, int iterations, int gpuid);

__device__ float FValues(float dim)
{
    if (dim == 3) return 0.3333333333333333f;
    return (sqrt(dim + 1) - 1) / dim;
}

__device__ float GValues(float dim)
{
    if (dim == 3) return 0.16666666666666666f;
    return ((dim + 1) - sqrt(dim + 1)) / ((dim + 1) * dim);
}

__device__ float Noise3DDevice(float* vals)
{
    float s = 0;
    float dimensions = 3;

    float vvals[3];
    float xvals[3];
    float ranks[3];
    int ivvals[3];

    for (int i = 0; i < dimensions; ++i)
    {
        s += vals[i];
    }
    s *= FValues(dimensions);

    float t = 0;
    for (int i = 0; i < dimensions; ++i)
    {
        vvals[i] = 0;
        xvals[i] = 0;
        ranks[i] = 0;
        t += ivvals[i] = (int)(vals[i] + s);
    }
    float G = 0;
    t *= G = GValues(dimensions);

    for (int i = dimensions - 1; i >= 0; --i)
    {
        xvals[i] = vals[i] - (ivvals[i] - t);
        for (int j = i + 1; j < dimensions; ++j)
            if (xvals[i] > xvals[j]) ranks[i]++; else ranks[j]++;
    }
    float n = 0;
    int temp = dimensions - 1;

    for (int i = 0; i < dimensions + 1; ++i)
    {
        t = 0.6;
        unsigned int hash = 98743247568;

        for (int j = 0; j < dimensions; ++j)
        {
            int ival = 0;
            if (i > 0) ival = (i == dimensions ? 1 : (ranks[j] >= temp ? 1 : 0));
            float vval = vvals[j] = i == 0 ? xvals[j] : xvals[j] - ival + i * G;

            t -= vval * vval;

            hash ^= (unsigned int)(1619 * (ivvals[j] + ival));
        }
        if (i > 0) temp--;
        if (t >= 0)
        {
            hash = hash * hash * hash * 60493;
            hash = (hash >> 13) ^ hash;

            hash &= 15;

            float result = 0.0;
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

__global__ void Noise3DOctaveKernel(float* scale, float* Persistence, int* height, int* width, int* dimZ, int* iterations, float* results)
{
    int x = blockIdx.x * blockDim.x + threadIdx.x;
    int y = blockIdx.y * blockDim.y + threadIdx.y;

    float maxAmp = 0;
    float cAmp = 1;
    float freq = *scale;
    float noise = 0;
    
    for (int i = 0; i < *iterations; i++)
    {
        float nX = x * freq;
        float nY = y * freq;
        float nZ = *dimZ * freq;

        float vvals[3] = { nX, nY, nZ };
        noise += Noise3DDevice(vvals) * cAmp;
        maxAmp += cAmp;
        cAmp *= *Persistence;
        freq *= 2;
    }

    results[y * *width + x] = noise / maxAmp;
}

extern "C" float* Noise3DCuda(int x, int y, int z, float scale, float persistence, int iterations, int gpuid)
{
    cudaError_t cudaStatus;

    dim3 dimBlock(8, 8, 1);
    dim3 dimGrid(x / dimBlock.x, y / dimBlock.y, 1);

    float* dev_Scale = 0;
    float* dev_Persistence = 0;
    int* dev_Iterations = 0;
    int* dev_dimZ = 0;
    float* dev_Results = 0;
    int* dev_Height = 0;
    int* dev_Width = 0;
    float* results = (float*)malloc((x * y) * sizeof(float));;

    cudaStatus = cudaSetDevice(gpuid);
    if (cudaStatus != cudaSuccess) {
        fprintf(stderr, "Setting CUDA Device failed!");
        goto Error;
    }

    cudaStatus = cudaMalloc((void**)&dev_Scale, sizeof(float));
    if (cudaStatus != cudaSuccess) {
        fprintf(stderr, "cudaMalloc failed!");
        goto Error;
    }
    cudaStatus = cudaMalloc((void**)&dev_Persistence, sizeof(float));
    if (cudaStatus != cudaSuccess) {
        fprintf(stderr, "cudaMalloc failed!");
        goto Error;
    }
    cudaStatus = cudaMalloc((void**)&dev_Iterations, sizeof(int));
    if (cudaStatus != cudaSuccess) {
        fprintf(stderr, "cudaMalloc failed!");
        goto Error;
    }
    cudaStatus = cudaMalloc((void**)&dev_dimZ, sizeof(int));
    if (cudaStatus != cudaSuccess) {
        fprintf(stderr, "cudaMalloc failed!");
        goto Error;
    }
    cudaStatus = cudaMalloc((void**)&dev_Height, sizeof(int));
    if (cudaStatus != cudaSuccess) {
        fprintf(stderr, "cudaMalloc failed!");
        goto Error;
    }
    cudaStatus = cudaMalloc((void**)&dev_Width, sizeof(int));
    if (cudaStatus != cudaSuccess) {
        fprintf(stderr, "cudaMalloc failed!");
        goto Error;
    }

    cudaStatus = cudaMemcpy(dev_Scale, &scale, sizeof(float), cudaMemcpyHostToDevice);
    if (cudaStatus != cudaSuccess) {
        fprintf(stderr, "cudaMemcpy failed!");
        goto Error;
    }
    cudaStatus = cudaMemcpy(dev_Persistence, &persistence, sizeof(float), cudaMemcpyHostToDevice);
    if (cudaStatus != cudaSuccess) {
        fprintf(stderr, "cudaMemcpy failed!");
        goto Error;
    }
    cudaStatus = cudaMemcpy(dev_dimZ, &z, sizeof(int), cudaMemcpyHostToDevice);
    if (cudaStatus != cudaSuccess) {
        fprintf(stderr, "cudaMemcpy failed!");
        goto Error;
    }
    cudaStatus = cudaMemcpy(dev_Height, &y, sizeof(int), cudaMemcpyHostToDevice);
    if (cudaStatus != cudaSuccess) {
        fprintf(stderr, "cudaMemcpy failed!");
        goto Error;
    }
    cudaStatus = cudaMemcpy(dev_Width, &x, sizeof(int), cudaMemcpyHostToDevice);
    if (cudaStatus != cudaSuccess) {
        fprintf(stderr, "cudaMemcpy failed!");
        goto Error;
    }
    cudaStatus = cudaMemcpy(dev_Iterations, &iterations, sizeof(int), cudaMemcpyHostToDevice);
    if (cudaStatus != cudaSuccess) {
        fprintf(stderr, "cudaMemcpy failed!");
        goto Error;
    }

    cudaStatus = cudaMalloc((void**)&dev_Results, (x * y) * sizeof(float));
    if (cudaStatus != cudaSuccess) {
        fprintf(stderr, "cudaMalloc failed!");
        goto Error;
    }

    Noise3DOctaveKernel <<<dimGrid, dimBlock, 0 >>> (dev_Scale, dev_Persistence, dev_Height, dev_Width, dev_dimZ, dev_Iterations, dev_Results);

    // Check for any errors launching the kernel
    cudaStatus = cudaGetLastError();
    if (cudaStatus != cudaSuccess) {
        fprintf(stderr, "Noise3DOctaveKernel launch failed: %s\n", cudaGetErrorString(cudaStatus));
        goto Error;
    }

    // cudaDeviceSynchronize waits for the kernel to finish, and returns
    // any errors encountered during the launch.
    cudaStatus = cudaDeviceSynchronize();
    if (cudaStatus != cudaSuccess) {
        fprintf(stderr, "cudaDeviceSynchronize returned error code %d after launching addKernel!\n", cudaStatus);
        goto Error;
    }

    // Copy output vector from GPU buffer to host memory.
    cudaStatus = cudaMemcpy(results, dev_Results, (x * y) * sizeof(float), cudaMemcpyDeviceToHost);
    if (cudaStatus != cudaSuccess) {
        fprintf(stderr, "cudaMemcpy failed!");
        goto Error;
    }

Error:
    cudaFree(dev_Iterations);
    cudaFree(dev_Persistence);
    cudaFree(dev_Scale);
    cudaFree(dev_dimZ);
    cudaFree(dev_Width);
    cudaFree(dev_Height);
    cudaFree(dev_Results);

    return results;
}
#pragma managed