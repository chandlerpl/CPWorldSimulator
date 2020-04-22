#pragma unmanaged
#include "cuda_runtime.h"
#include "device_launch_parameters.h"

#include <cmath>
#include <stdio.h>
#include "GpuTimer.h"

using namespace std;
extern "C" double* Noise3DCuda(short* source, int x, int y, int z, double scale, double persistence, int iterations, int gpuid);

static int* grad3 = new int[36]{ 
    1,1,0,
    -1,1,0,
    1,-1,0,
    -1,-1,0,
    1,0,1,
    -1,0,1,
    1,0,-1,
    -1,0,-1,
    0,1,1,
    0,-1,1,
    0,1,-1,
    0,-1,-1 
};

__device__ double FValues(double dim)
{
    return (sqrt(dim + 1) - 1) / dim;
}

__device__ double GValues(double dim)
{
    return ((dim + 1) - sqrt(dim + 1)) / ((dim + 1) * dim);
}

__device__ double Noise3DDevice(short* source, int *grad, double* dimX, double* dimY, double* dimZ)
{
    double n0, n1, n2, n3;
    double G = GValues(3);

    double s = (*dimX + *dimY + *dimZ) * FValues(3);

    int i = (int)floor(*dimX + s);
    int j = (int)floor(*dimY + s);
    int k = (int)floor(*dimZ + s);
    double t = (i + j + k) * G;

    double X0 = i - t;
    double Y0 = j - t;
    double Z0 = k - t;

    double x0 = *dimX - X0;
    double y0 = *dimY - Y0;
    double z0 = *dimZ - Z0;

    int i1, j1, k1;
    int i2, j2, k2;
    if (x0 >= y0)
    {
        if (y0 >= z0) { i1 = 1; j1 = 0; k1 = 0; i2 = 1; j2 = 1; k2 = 0; }
        else if (x0 >= z0) { i1 = 1; j1 = 0; k1 = 0; i2 = 1; j2 = 0; k2 = 1; }
        else { i1 = 0; j1 = 0; k1 = 1; i2 = 1; j2 = 0; k2 = 1; }
    }
    else
    { // x0<y0
        if (y0 < z0) { i1 = 0; j1 = 0; k1 = 1; i2 = 0; j2 = 1; k2 = 1; }
        else if (x0 < z0) { i1 = 0; j1 = 1; k1 = 0; i2 = 0; j2 = 1; k2 = 1; }
        else { i1 = 0; j1 = 1; k1 = 0; i2 = 1; j2 = 1; k2 = 0; }
    }

    double x1 = x0 - i1 + G;
    double y1 = y0 - j1 + G;
    double z1 = z0 - k1 + G;
    double x2 = x0 - i2 + 2.0 * G;
    double y2 = y0 - j2 + 2.0 * G;
    double z2 = z0 - k2 + 2.0 * G;
    double x3 = x0 - 1.0 + 3.0 * G;
    double y3 = y0 - 1.0 + 3.0 * G;
    double z3 = z0 - 1.0 + 3.0 * G;

    int ii = i & 255;
    int jj = j & 255;
    int kk = k & 255;

    int gi0 = (source[ii + source[jj + source[kk]]] % 12) * 3;
    int gi1 = (source[ii + i1 + source[jj + j1 + source[kk + k1]]] % 12) * 3;
    int gi2 = (source[ii + i2 + source[jj + j2 + source[kk + k2]]] % 12) * 3;
    int gi3 = (source[ii + 1 + source[jj + 1 + source[kk + 1]]] % 12) * 3;

    double t0 = 0.6 - x0 * x0 - y0 * y0 - z0 * z0;
    if (t0 < 0) n0 = 0.0;
    else
    {
        t0 *= t0;
        n0 = t0 * t0 * (grad[gi0] * x0 + grad[gi0 + 1] * y0 + grad[gi0 + 2] * z0);
    }
    double t1 = 0.6 - x1 * x1 - y1 * y1 - z1 * z1;
    if (t1 < 0) n1 = 0.0;
    else
    {
        t1 *= t1;
        n1 = t1 * t1 * (grad[gi1] * x1 + grad[gi1 + 1] * y1 + grad[gi1 + 2] * z1);
    }
    double t2 = 0.6 - x2 * x2 - y2 * y2 - z2 * z2;
    if (t2 < 0) n2 = 0.0;
    else
    {
        t2 *= t2;
        n2 = t2 * t2 * (grad[gi2] * x2 + grad[gi2 + 1] * y2 + grad[gi2 + 2] * z2);
    }
    double t3 = 0.6 - x3 * x3 - y3 * y3 - z3 * z3;
    if (t3 < 0) n3 = 0.0;
    else
    {
        t3 *= t3;
        n3 = t3 * t3 * (grad[gi3] * x3 + grad[gi3 + 1] * y3 + grad[gi3 + 2] * z3);
    }

    return 32.0 * (n0 + n1 + n2 + n3);
}

__global__ void Noise3DOctaveKernel(short* source, int* grad, double* scale, double* Persistence, int* height, int* width, int* dimZ, int* iterations, double* results)
{
    int x = blockIdx.x * blockDim.x + threadIdx.x;
    int y = blockIdx.y * blockDim.y + threadIdx.y;

    double maxAmp = 0;
    double cAmp = 1;
    double freq = *scale;
    double noise = 0;
    
    for (int i = 0; i < *iterations; i++)
    {
        double nX = x * freq;
        double nY = y * freq;
        double nZ = *dimZ * freq;

        noise += Noise3DDevice(source, grad, &nX, &nY, &nZ) * cAmp;
        maxAmp += cAmp;
        cAmp *= *Persistence;
        freq *= 2;
    }

    results[y * *width + x] = noise / maxAmp;
}

void cudaAllocate(void* dst, const void* src, size_t count, cudaMemcpyKind kind) {
    if (cudaMalloc((void**)dst, count) != cudaSuccess) {
        fprintf(stderr, "cudaMalloc failed!");
        throw - 1;
    }
    if (cudaMemcpy(dst, src, count, kind) != cudaSuccess) {
        fprintf(stderr, "cudaMemcpy failed!");
        throw - 1;
    }
}

extern "C" double* Noise3DCuda(short* source, int x, int y, int z, double scale, double persistence, int iterations, int gpuid)
{
    cudaError_t cudaStatus;

    dim3 dimBlock(8, 8, 1);
    dim3 dimGrid(x / dimBlock.x, y / dimBlock.y, 1);

    double* dev_Scale = 0;
    double* dev_Persistence = 0;
    int* dev_Iterations = 0;
    int* dev_dimZ = 0;
    short* dev_Source = 0;
    int* dev_Grad = 0;
    double* dev_Results = 0;
    int* dev_Height = 0;
    int* dev_Width = 0;
    double* results = (double*)malloc((x * y) * sizeof(double));;

    cudaStatus = cudaSetDevice(gpuid);
    if (cudaStatus != cudaSuccess) {
        fprintf(stderr, "Setting CUDA Device failed!");
        goto Error;
    }
    
    /*try {
        cudaAllocate(dev_Source, source, sizeof(short) * 512, cudaMemcpyHostToDevice);
    }
    catch (int e) {
        goto Error;
    }*/
    cudaStatus = cudaMalloc((void**)&dev_Source, sizeof(short) * 512);
    if (cudaStatus != cudaSuccess) {
        fprintf(stderr, "cudaMalloc failed!");
        goto Error;
    }
    cudaStatus = cudaMemcpy(dev_Source, source,  sizeof(short) * 512, cudaMemcpyHostToDevice);
    if (cudaStatus != cudaSuccess) {
        fprintf(stderr, "cudaMemcpy failed!");
        goto Error;
    }


    cudaStatus = cudaMalloc((void**)&dev_Grad, sizeof(int) * 36);
    if (cudaStatus != cudaSuccess) {
        fprintf(stderr, "cudaMalloc failed!");
        goto Error;
    }
    cudaStatus = cudaMalloc((void**)&dev_Scale, sizeof(double));
    if (cudaStatus != cudaSuccess) {
        fprintf(stderr, "cudaMalloc failed!");
        goto Error;
    }
    cudaStatus = cudaMalloc((void**)&dev_Persistence, sizeof(double));
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

    cudaStatus = cudaMemcpy(dev_Grad, grad3, sizeof(int) * 36, cudaMemcpyHostToDevice);
    if (cudaStatus != cudaSuccess) {
        fprintf(stderr, "cudaMemcpy failed!");
        goto Error;
    }

    cudaStatus = cudaMemcpy(dev_Scale, &scale, sizeof(double), cudaMemcpyHostToDevice);
    if (cudaStatus != cudaSuccess) {
        fprintf(stderr, "cudaMemcpy failed!");
        goto Error;
    }
    cudaStatus = cudaMemcpy(dev_Persistence, &persistence, sizeof(double), cudaMemcpyHostToDevice);
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

    cudaStatus = cudaMalloc((void**)&dev_Results, (x * y) * sizeof(double));
    if (cudaStatus != cudaSuccess) {
        fprintf(stderr, "cudaMalloc failed!");
        goto Error;
    }

    Noise3DOctaveKernel << <dimGrid, dimBlock, 0 >> > (dev_Source, dev_Grad, dev_Scale, dev_Persistence, dev_Height, dev_Width, dev_dimZ, dev_Iterations, dev_Results);

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
    cudaStatus = cudaMemcpy(results, dev_Results, (x * y) * sizeof(double), cudaMemcpyDeviceToHost);
    if (cudaStatus != cudaSuccess) {
        fprintf(stderr, "cudaMemcpy failed!");
        goto Error;
    }

Error:
    cudaFree(dev_Iterations);
    cudaFree(dev_Persistence);
    cudaFree(dev_Scale);
    cudaFree(dev_dimZ);
    cudaFree(dev_Source);
    cudaFree(dev_Width);
    cudaFree(dev_Height);
    cudaFree(dev_Grad);
    cudaFree(dev_Results);

    return results;
}
#pragma managed