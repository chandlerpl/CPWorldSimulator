#if !RELEASENOGPU
using CPWS.CUDA;

namespace CP.Common.Utilities
{
    public static class GPUUtilities
    {
        static GpuCap LatestGPUQuery;

        public static bool CheckCUDACapability()
        {
            GpuCap gpu = CUDAUtilities.GetCapabilities();
            LatestGPUQuery = gpu;
            if (gpu.DeviceCount > 0 && !gpu.QueryFailed)
            {
                return true;
            }

            return false;
        }

        public static int GetStrongestCudaGpu()
        {
            return LatestGPUQuery.StrongestDeviceId;
        }
    }
}
#endif