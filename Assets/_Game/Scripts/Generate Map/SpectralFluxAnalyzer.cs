using System;
using System.Collections.Generic;
using UnityEngine;

public class SpectralFluxInfo
{
	public SpectralFluxInfo(float[] spectrum, float time, float adjCoeff)
    {
		this.spectrum = spectrum;
		this.time = time;

		this.adjustmentCoeff = adjCoeff;
    }

	public float[] spectrum;
	public float time;
	public float adjustmentCoeff;

	public float spectralFlux;
	public float threshold;
	public float prunedSpectralFlux;
	public bool isPeak;
}

public class SpectralFluxAnalyzer
{
	int numSamples = 1024;

	// Sensitivity multiplier to scale the average threshold.
	// In this case, if a rectified spectral flux sample is > 1.5 times the average, it is a peak
	float thresholdMultiplier = 3f;  // default 3f

	// Number of samples to average in our window
	int thresholdWindowSize = 30; // default 30

	float adjustFactor = 0.0001f; // default 0.0001f

	public List<SpectralFluxInfo> spectralFluxSamples;

	float[] curSpectrum;
	float[] prevSpectrum;

	public SpectralFluxAnalyzer()
	{
		//spectralFluxSamples = new List<SpectralFluxInfo>();

		curSpectrum = new float[numSamples];
		prevSpectrum = new float[numSamples];
	}

	public void SetCurSpectrum(float[] spectrum)
	{
		curSpectrum.CopyTo(prevSpectrum, 0);
		spectrum.CopyTo(curSpectrum, 0);
	}

	public void AnalyzeSpectrum(List<SpectralFluxInfo> data,List<CircleDetail> listCircle) //float[] spectrum, float time)
	{
		spectralFluxSamples = data;

		for (int i = 0; i < spectralFluxSamples.Count; i++)
		{
			// Set spectrum
			SetCurSpectrum(spectralFluxSamples[i].spectrum);

			// Get current spectral flux from spectrum
			spectralFluxSamples[i].spectralFlux = CalculateRectifiedSpectralFlux();

			// Get Flux threshold of time window surrounding index to process
			spectralFluxSamples[i].threshold = GetFluxThreshold(i);

			// Only keep amp amount above threshold to allow peak filtering
			spectralFluxSamples[i].prunedSpectralFlux = GetPrunedSpectralFlux(i);

			// Now that we are processed at n, n-1 has neighbors (n-2, n) to determine peak
			int indexToDetectPeak = i - 1;

			bool curPeak = indexToDetectPeak > 0 && IsPeak(indexToDetectPeak);

			if (curPeak)
			{
				spectralFluxSamples[indexToDetectPeak].isPeak = true;
				listCircle.Add(new CircleDetail(320, 240, spectralFluxSamples[indexToDetectPeak].time));
			}
		}
		PositionCalculation.CalculatePosition(listCircle);
	}

	float CalculateRectifiedSpectralFlux()
	{
		float sum = 0f;

		// Aggregate positive changes in spectrum data
		for (int i = 0; i < numSamples; i++)
		{
			sum += Mathf.Max(0f, curSpectrum[i] - prevSpectrum[i]);
		}
		return sum;
	}

	float GetFluxThreshold(int spectralFluxIndex)
	{
		// How many samples in the past and future we include in our average
		int windowStartIndex = Mathf.Max(0, spectralFluxIndex - thresholdWindowSize / 2);
		int windowEndIndex = Mathf.Min(spectralFluxSamples.Count - 1, spectralFluxIndex + thresholdWindowSize / 2);

		// Add up our spectral flux over the window
		float sum = 0f;
		for (int i = windowStartIndex; i < windowEndIndex; i++)
		{
			sum += spectralFluxSamples[i].spectralFlux;
		}

		// Return the average multiplied by our sensitivity multiplier
		float avg = sum / (windowEndIndex - windowStartIndex);

        var coeff = 1 + MathF.Log(spectralFluxSamples[spectralFluxIndex].adjustmentCoeff, adjustFactor);

		return avg * thresholdMultiplier * coeff;
	}

	float GetPrunedSpectralFlux(int spectralFluxIndex)
	{
		return Mathf.Max(0f, spectralFluxSamples[spectralFluxIndex].spectralFlux - spectralFluxSamples[spectralFluxIndex].threshold);
	}

	bool IsPeak(int spectralFluxIndex)
	{
		if (spectralFluxSamples[spectralFluxIndex].prunedSpectralFlux > spectralFluxSamples[spectralFluxIndex + 1].prunedSpectralFlux &&
			spectralFluxSamples[spectralFluxIndex].prunedSpectralFlux > spectralFluxSamples[spectralFluxIndex - 1].prunedSpectralFlux)
		{
			return true;
		}
		else
		{
			return false;
		}
	}

	void LogSample(int indexToLog)
	{
		int windowStart = Mathf.Max(0, indexToLog - thresholdWindowSize / 2);
		int windowEnd = Mathf.Min(spectralFluxSamples.Count - 1, indexToLog + thresholdWindowSize / 2);
		Debug.Log(string.Format(
			"Peak detected at song time {0} with pruned flux of {1} ({2} over thresh of {3}).\n" +
			"Thresh calculated on time window of {4}-{5} ({6} seconds) containing {7} samples.",
			spectralFluxSamples[indexToLog].time,
			spectralFluxSamples[indexToLog].prunedSpectralFlux,
			spectralFluxSamples[indexToLog].spectralFlux,
			spectralFluxSamples[indexToLog].threshold,
			spectralFluxSamples[windowStart].time,
			spectralFluxSamples[windowEnd].time,
			spectralFluxSamples[windowEnd].time - spectralFluxSamples[windowStart].time,
			windowEnd - windowStart
		));
	}
}