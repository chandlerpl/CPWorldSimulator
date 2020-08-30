#pragma once
#include "NoiseGen.h"
namespace CPWS::WorldGenerator::Test::Noise {
	public ref class SimplexNoise : NoiseGen
	{
	public:
		SimplexNoise(unsigned int seed, double persistence, double scale, bool useCuda) : NoiseGen(seed, persistence, scale, useCuda) {}

		virtual double Noise(array<double>^ vals) override;

		virtual double Octave(int iterations, array<double>^ vals) override;

		virtual array<double, 2>^ NoiseMap(int iterations, array<int>^ vals) override;

		virtual array<double, 2>^ NoiseMapNotAsync(int iterations, array<int>^ vals) override;

	protected:
		virtual double UnmanagedNoise(int dimensions, double vals[]) override;

	private:
		void setup(int dimensions);
	};
}

