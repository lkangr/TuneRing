using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpectralFluxInfo
{
	public SpectralFluxInfo(float[] spectrum, float time, double[] sampleChunk)
    {
		this.spectrum = spectrum;
		this.time = time;
		this.sampleChunk = sampleChunk;
    }

	public float[] spectrum;
	public float time;
	public double[] sampleChunk;

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
	float thresholdMultiplier = 3f;  //1.5f

	// Number of samples to average in our window
	int thresholdWindowSize = 50; //50

	public List<SpectralFluxInfo> spectralFluxSamples;

	float[] curSpectrum;
	float[] prevSpectrum;

	double[] sampleSpectrum;

	public SpectralFluxAnalyzer()
	{
		//spectralFluxSamples = new List<SpectralFluxInfo>();

		curSpectrum = new float[numSamples];
		prevSpectrum = new float[numSamples];

		sampleSpectrum = new double[numSamples];
	}

	public void SetCurSpectrum(float[] spectrum, double[] sample)
	{
		curSpectrum.CopyTo(prevSpectrum, 0);
		spectrum.CopyTo(curSpectrum, 0);
		sample.CopyTo(sampleSpectrum, 0);
	}

	public List<CircleDetail> AnalyzeSpectrum(List<SpectralFluxInfo> data) //float[] spectrum, float time)
	{
		spectralFluxSamples = data;

		List<CircleDetail> listCircle = new List<CircleDetail>();

		for (int i = 0; i < spectralFluxSamples.Count; i++)
		{
			// Set spectrum
			SetCurSpectrum(spectralFluxSamples[i].spectrum, spectralFluxSamples[i].sampleChunk);

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
				listCircle.Add(new CircleDetail(300, 220, spectralFluxSamples[indexToDetectPeak].time));
			}
		}
		return listCircle;
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

		double sumb = 0f;
		foreach (var a in sampleSpectrum) sumb += (1 - a);
		var avgb = (sumb / sampleSpectrum.Length) / 2 + 1;

		// Return the average multiplied by our sensitivity multiplier
		float avg = sum / (windowEndIndex - windowStartIndex);
		return avg * thresholdMultiplier * (float)avgb;
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