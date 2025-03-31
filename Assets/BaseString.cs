using System;
using UnityEngine;

public class BaseString : MonoBehaviour
{
    [Header("String properties")]
    [SerializeField] private float length = 0.65f;
    [SerializeField] private float linearDensity = 0.00738f;
    [SerializeField] private float tension = 86f;

    [Header("Pinch properties")]
    [SerializeField] private float pinchPosition = 0.15f;
    [SerializeField] private float pinchIntensity = 2f;

    [Header("Display")]
    // [SerializeField] private LineRenderer waveformLine;
    [SerializeField] private LineRenderer[] harmonicLines; // Taille = nombre d'harmoniques

    private readonly float dampingCoefficient = 0.5f;
    private readonly float[] harmonicsFrequencies = new float[3];
    private readonly double[] harmonicsProportions = new double[3];

    private AudioSource audioSource;

    private float currentTime;
    private float excitationIntensity;
    private int sampleRate;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        sampleRate = AudioSettings.outputSampleRate;

        audioSource.playOnAwake = true;
        audioSource.spatialBlend = 0f;
        // audioSource.loop = true;
        audioSource.clip = AudioClip.Create("StringSound", sampleRate, 1, sampleRate, false);
        audioSource.Play();
    }

    private void Update()
    {
        UpdateWaveformDisplay();
    }

    private void OnAudioFilterRead(float[] data, int channels)
    {
        float timeStep = 1f / sampleRate;

        for (var i = 0; i < data.Length; i += channels)
        {
            float sample = GenerateSoundSample(currentTime);
            currentTime += timeStep;

            for (var channel = 0; channel < channels; channel++) data[i + channel] = sample;
        }
    }

    private void UpdateWaveformDisplay()
    {
        const int sampleCount = 256;

        for (var h = 0; h < harmonicsProportions.Length; h++)
        {
            harmonicLines[h].positionCount = sampleCount;

            for (var i = 0; i < sampleCount; i++)
            {
                float t = i / (float) sampleCount * 0.1f;

                float harmonicWave = Mathf.Sin(2 * Mathf.PI * harmonicsFrequencies[h] * t / 20f); // Ralenti
                float dampingFactor = Mathf.Exp(-dampingCoefficient * h * currentTime);
                var amplitude = (float) (harmonicWave * harmonicsProportions[h] * excitationIntensity * dampingFactor);

                var position = new Vector3(i / (float) sampleCount * 10f, amplitude * 3f, 0);
                harmonicLines[h].SetPosition(i, position);
            }
        }
    }

    public void Pinch()
    {
        currentTime = 0f;
        excitationIntensity = pinchIntensity;

        harmonicsProportions[0] = Math.Abs(Math.Sin(1 * Mathf.PI * pinchPosition / length));
        harmonicsProportions[1] = Math.Abs(Math.Sin(2 * Mathf.PI * pinchPosition / length));
        harmonicsProportions[2] = Math.Abs(Math.Sin(3 * Mathf.PI * pinchPosition / length));

        harmonicsFrequencies[0] = (float) (1 / (2 * length) * Math.Sqrt(tension / linearDensity));
        harmonicsFrequencies[1] = (float) (2 / (2 * length) * Math.Sqrt(tension / linearDensity));
        harmonicsFrequencies[2] = (float) (3 / (2 * length) * Math.Sqrt(tension / linearDensity));
    }

    private float GenerateSoundSample(float time)
    {
        float sample = 0;

        for (var k = 0; k < harmonicsProportions.Length; k++)
        {
            float harmonicWave = Mathf.Sin(2 * Mathf.PI * harmonicsFrequencies[k] * time);
            float dampingFactor = Mathf.Exp(-dampingCoefficient * k * time);

            sample += (float) (harmonicWave * harmonicsProportions[k] * excitationIntensity * dampingFactor);
        }

        return Mathf.Clamp(sample, -1f, 1f);
    }
}