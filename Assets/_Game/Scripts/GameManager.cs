using DG.Tweening;
using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using DSPLib;
using Vector2 = UnityEngine.Vector2;
using System.Threading;
using TMPro;

public class GameManager : MonoBehaviour
{
    [Header("Background")]
    public BackGround bg;
    public Image maskIngame;
    public Sprite imageTemp;

    [Header("Audio")]
    public AudioSource audioSource;

    [Header("Map")]
    public DefaultAsset mapFile;

    [Header("Hit Circle")]
    public Transform hitCircleContainer;
    public GameObject hitCirclePrefabs;

    [Header("Preprocess Song")]
    public GameObject maskProcess;
    public TextMeshProUGUI textProcess;

    [Header("------------------------------------")]
    [SerializeField] private Vector2 resolution;
    public static Vector2 Resolution { get { return GameBroker.Ins.GameManager.resolution; } }

    private float screenScale;
    private Vector2 screenOffset;

    public List<CircleDetail> listCircle = new List<CircleDetail>();
    private int currCircle;

    private bool preIngame = false;
    private float timePreIngame;
    private bool inGame = false;

    private bool autoMode = false;

    //process song
    private float[] multiChannelSamples;
    private int numTotalSamples;
    private int numChannels;
    private float clipLength;
    private int sampleRate;
    private SpectralFluxAnalyzer preProcessedSpectralFluxAnalyzer;
    private bool ispreprocess = false;
    private bool changeProcessState = false;

    void Start()
    {
        var canvas = GameObject.FindGameObjectWithTag("MasterCanvas").GetComponent<RectTransform>();

        if (((canvas.sizeDelta.x - 200)/ (canvas.sizeDelta.y - 200)) > (resolution.x / resolution.y))
            screenScale = (canvas.sizeDelta.y - 200) / resolution.y;
        else screenScale = (canvas.sizeDelta.x - 200) / resolution.x;

        screenOffset = new Vector2((canvas.sizeDelta.x - resolution.x * screenScale) / 2, (canvas.sizeDelta.y - resolution.y * screenScale) / 2);
        MainView.Show();
    }

    void Update()
    {
        if (inGame)
        {
            if (Input.GetKeyDown(KeyCode.BackQuote))
            {
                GameBroker.EndGame();
            }

            if (!audioSource.isPlaying)
            {
                GameBroker.EndGame();
            }
        }

        if (changeProcessState)
        {
            changeProcessState = false;
            SetMaskProcess();
        }
    }

    public void Play(bool autoMode)
    {
        InGameView.Show();

        this.autoMode = autoMode;

        if (!ispreprocess)
        {
            listCircle.Clear();
            var map = new StreamReader(AssetDatabase.GetAssetPath(mapFile)).ReadToEnd().Split("\n");
            foreach (var line in map)
            {
                var data = line.Split(",");
                listCircle.Add(new CircleDetail(float.Parse(data[0]), float.Parse(data[1]), float.Parse(data[2]) / 1000));
            }
        }

        currCircle = 0;

        preIngame = true;
        timePreIngame = -2f;
        DOVirtual.DelayedCall(2.5f, () => maskIngame.DOFade(0.8f, 1f).OnComplete(() =>
        {
            DOTween.To(() => timePreIngame, x => timePreIngame = x, 0f, 2f).SetEase(Ease.Linear).OnComplete(() =>
            {
                preIngame = false;
                audioSource.Play();
                inGame = true;
            });
        }));
    }

    public void EndGame()
    {
        audioSource.Stop();

        inGame = false;

        for (int i = 0; i < hitCircleContainer.childCount; i++)
        {
            Destroy(hitCircleContainer.GetChild(i).gameObject);
        }

        maskIngame.color = new Color(0, 0, 0, 0);

        MainView.Show();
    }

    private void FixedUpdate()
    {
        if (listCircle.Count > currCircle)
        {
            var circle = listCircle[currCircle];
            if ((preIngame && timePreIngame >= circle.time - 1f) ||
                (inGame && audioSource.time >= circle.time - 1f))
            {
                var obj = Instantiate(hitCirclePrefabs, hitCircleContainer).GetComponent<HitCircle>();
                obj.SetPositionAndLayer(circle.posX * screenScale + screenOffset.x, circle.posY * screenScale + screenOffset.y, circle.color, circle.numDis, -currCircle, autoMode);
                if (autoMode && currCircle == 0)
                {
                    GameBroker.CursorTo(0);
                }
                currCircle++;
            }
        }
    }

#region generate map
    public void SetMaskProcess()
    {
        maskProcess.SetActive(!maskProcess.activeSelf);
        if (maskProcess.activeSelf)
        {
            textProcess.DOFade(1f, 1f).SetEase(Ease.Linear).SetLoops(-1, LoopType.Yoyo);
        }
        else
        {
            textProcess.DOKill();
            textProcess.DOFade(0f, 0f);
        }
    }

    public float getTimeFromIndex(int index)
    {
        return ((1f / (float)this.sampleRate) * index);
    }

    public void ProcessSong()
    {
        changeProcessState = true;

        preProcessedSpectralFluxAnalyzer = new SpectralFluxAnalyzer();
        listCircle.Clear();
        ispreprocess = true;

        multiChannelSamples = new float[audioSource.clip.samples * audioSource.clip.channels];
        numChannels = audioSource.clip.channels;
        numTotalSamples = audioSource.clip.samples;
        clipLength = audioSource.clip.length;

        // We are not evaluating the audio as it is being played by Unity, so we need the clip's sampling rate
        sampleRate = audioSource.clip.frequency;

        audioSource.clip.GetData(multiChannelSamples, 0);
        Debug.Log("GetData done");

        Thread bgThread = new Thread(getFullSpectrumThreaded);

        Debug.Log("Starting Background Thread");
        bgThread.Start();
    }

    public void getFullSpectrumThreaded()
    {
        try
        {
            // We only need to retain the samples for combined channels over the time domain
            float[] preProcessedSamples = new float[this.numTotalSamples];

            int numProcessed = 0;
            float combinedChannelAverage = 0f;
            for (int i = 0; i < multiChannelSamples.Length; i++)
            {
                    combinedChannelAverage += multiChannelSamples[i];

                // Each time we have processed all channels samples for a point in time, we will store the average of the channels combined
                if ((i + 1) % this.numChannels == 0)
                {
                    preProcessedSamples[numProcessed] = combinedChannelAverage / this.numChannels;
                    numProcessed++;
                    combinedChannelAverage = 0f;
                }
            }

            Debug.Log("Combine Channels done");
            Debug.Log(preProcessedSamples.Length);

            // Once we have our audio sample data prepared, we can execute an FFT to return the spectrum data over the time domain
            int spectrumSampleSize = 1024;
            int iterations = preProcessedSamples.Length / spectrumSampleSize;

            FFT fft = new FFT();
            fft.Initialize((UInt32)spectrumSampleSize);

            Debug.Log(string.Format("Processing {0} time domain samples for FFT", iterations));
            double[] sampleChunk = new double[spectrumSampleSize];
            List<SpectralFluxInfo> spectralFluxSamples = new List<SpectralFluxInfo>();
            for (int i = 0; i < iterations; i++)
            {
                // Grab the current 1024 chunk of audio sample data
                Array.Copy(preProcessedSamples, i * spectrumSampleSize, sampleChunk, 0, spectrumSampleSize);

                // Apply our chosen FFT Window
                double[] windowCoefs = DSP.Window.Coefficients(DSP.Window.Type.Hanning, (uint)spectrumSampleSize);
                double[] scaledSpectrumChunk = DSP.Math.Multiply(sampleChunk, windowCoefs);
                double scaleFactor = DSP.Window.ScaleFactor.Signal(windowCoefs);

                // Perform the FFT and convert output (complex numbers) to Magnitude
                Complex[] fftSpectrum = fft.Execute(scaledSpectrumChunk);
                double[] scaledFFTSpectrum = DSPLib.DSP.ConvertComplex.ToMagnitude(fftSpectrum);
                scaledFFTSpectrum = DSP.Math.Multiply(scaledFFTSpectrum, scaleFactor);

                // These 1024 magnitude values correspond (roughly) to a single point in the audio timeline
                float curSongTime = getTimeFromIndex(i) * spectrumSampleSize;

                float sum = 0f;
                foreach (var a in sampleChunk) sum += MathF.Abs((float)a);
                var adjustmentCoeff = Mathf.Abs(sum / sampleChunk.Length);

                // Send our magnitude data off to our Spectral Flux Analyzer to be analyzed for peaks
                spectralFluxSamples.Add(new SpectralFluxInfo(Array.ConvertAll(scaledFFTSpectrum, x => (float)x), curSongTime, adjustmentCoeff));
            }

            preProcessedSpectralFluxAnalyzer.AnalyzeSpectrum(spectralFluxSamples, listCircle);
            Debug.Log("Spectrum Analysis done");
            Debug.Log("Background Thread Completed");

            changeProcessState = true;

        }
        catch (Exception e)
        {
            // Catch exceptions here since the background thread won't always surface the exception to the main thread
            Debug.Log(e.ToString());
        }
    }
}
#endregion
