/* 
 * Copyright (C) Pope Games, Inc - All Rights Reserved
 * Unauthorized copying of this file, via any medium is strictly prohibited
 * Proprietary and confidential
 * Author: Chandler Pope-Lewis <c.popelewis@gmail.com>
 */
#pragma once

namespace CP
{
	namespace CommonCUDA
	{
		public ref struct GpuCap
		{
			bool QueryFailed;           // True on error
			int  DeviceCount;           // Number of CUDA devices found 
			int  StrongestDeviceId;     // ID of best CUDA device
			int  ComputeCapabilityMajor; // Major compute capability (of best device)
			int  ComputeCapabilityMinor; // Minor compute capability
		};

		public ref class CUDAUtilities abstract sealed
		{
		public:
			static CP::CommonCUDA::GpuCap^ GetCapabilities();
		private:

		};
	}
}