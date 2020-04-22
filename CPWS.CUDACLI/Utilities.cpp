/* 
 * Copyright (C) Pope Games, Inc - All Rights Reserved
 * Unauthorized copying of this file, via any medium is strictly prohibited
 * Proprietary and confidential
 * Author: Chandler Pope-Lewis <c.popelewis@gmail.com>
 */
#include "Utilities.h"
#include "cuda_runtime.h"

CP::CommonCUDA::GpuCap^ CP::CommonCUDA::CUDAUtilities::GetCapabilities()
{
    CP::CommonCUDA::GpuCap^ gpu = gcnew CP::CommonCUDA::GpuCap();
    gpu->QueryFailed = false;
    gpu->StrongestDeviceId = -1;
    gpu->ComputeCapabilityMajor = -1;
    gpu->ComputeCapabilityMinor = -1;

    int count = 0;
    cudaError_t error_id = cudaGetDeviceCount(&count);
    gpu->DeviceCount = count;

    if (error_id != cudaSuccess)
    {
        gpu->QueryFailed = true;
        gpu->DeviceCount = 0;
        return gpu;
    }

    if (gpu->DeviceCount == 0)
        return gpu; // "There are no available device(s) that support CUDA

    // Find best device
    for (int dev = 0; dev < gpu->DeviceCount; ++dev)
    {
        cudaDeviceProp deviceProp;
        cudaGetDeviceProperties(&deviceProp, dev);
        if (deviceProp.major > gpu->ComputeCapabilityMajor)
        {
            gpu->StrongestDeviceId = dev;
            gpu->ComputeCapabilityMajor = deviceProp.major;
            gpu->ComputeCapabilityMinor = 0;
        }
        if (deviceProp.minor > gpu->ComputeCapabilityMinor)
        {
            gpu->StrongestDeviceId = dev;
            gpu->ComputeCapabilityMinor = deviceProp.minor;
        }
    }
    return gpu;
}
