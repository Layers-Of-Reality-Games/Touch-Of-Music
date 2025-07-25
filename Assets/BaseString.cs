using System;
using UnityEngine;

public class BaseString : MonoBehaviour
{
    [Header("String properties")]
    [SerializeField] private float length = 0.65f;
    [SerializeField] private float linearDensity = 0.00738f;
    [SerializeField] private float tension = 86f;
    [SerializeField] private int harmonicsCount = 5;

    [Header("Pinch properties")]
    [SerializeField] private float pinchPosition = 0.15f;
    [SerializeField] private float pinchIntensity = 0.5f;
    [SerializeField] private float minIntensity = 0.1f;
    [SerializeField] private float maxIntensity = 1.0f;

    [Header("Audio Settings")]
    [SerializeField] private float masterVolume = 0.5f;
    [SerializeField] private float dampingCoefficient = 0.5f;

    [Header("Visualization Settings")]
    [SerializeField] private StringVibrationVisualizer visualizer;

    private AudioSource audioSource;
    private float[] harmonicsFrequencies;
    private float[] harmonicsAmplitudes;
    private float[] harmonicsPhases;
    private float amplitude;
    private float targetAmplitude;
    private int sampleRate;
    private float timeSincePinch;
    private float currentPinchIntensity;
    private bool visualizationActive = false;

    private const float FADE_SPEED = 50f;

    public int HarmonicsCount => harmonicsCount;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            Debug.LogError("No AudioSource component found!");
            return;
        }

        if (visualizer == null)
        {
            visualizer = GetComponent<StringVibrationVisualizer>();
        }

        sampleRate = AudioSettings.outputSampleRate;
        Debug.Log($"Sample rate: {sampleRate}");

        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;
        audioSource.loop = true;
        audioSource.volume = 1f;

        audioSource.clip = AudioClip.Create("StringSound", sampleRate * 2, 1, sampleRate, true, OnAudioRead);

        harmonicsFrequencies = new float[harmonicsCount];
        harmonicsAmplitudes = new float[harmonicsCount];
        harmonicsPhases = new float[harmonicsCount];

        SetStringProperties(length, linearDensity, tension);

        if (visualizer != null)
        {
            visualizer.Initialize();
        }
    }

    private void OnAudioRead(float[] data)
    {
        float deltaTime = 1f / sampleRate;

        for (int i = 0; i < data.Length; i++)
        {
            amplitude = Mathf.MoveTowards(amplitude, targetAmplitude, FADE_SPEED * deltaTime);

            float sample = 0f;

            if (amplitude > 0.001f)
            {
                timeSincePinch += deltaTime;

                for (int h = 0; h < harmonicsCount; h++)
                {
                    float freq = harmonicsFrequencies[h];

                    if (freq > 20f && freq < 20000f)
                    {
                        harmonicsPhases[h] += 2f * Mathf.PI * freq * deltaTime;

                        // Wrap phase to prevent numerical issues
                        while (harmonicsPhases[h] > 2f * Mathf.PI)
                            harmonicsPhases[h] -= 2f * Mathf.PI;

                        // Calculate time-based damping (exponential decay)
                        float damping = Mathf.Exp(-dampingCoefficient * (h + 1) * timeSincePinch);
                        float currentAmplitude = harmonicsAmplitudes[h] * damping * amplitude;

                        sample += Mathf.Sin(harmonicsPhases[h]) * currentAmplitude;
                    }
                }

                sample *= masterVolume * currentPinchIntensity;
            }

            data[i] = Mathf.Clamp(sample, -0.95f, 0.95f);
        }
    }

    public void SetStringProperties(float stringLength, float density, float stringTension)
    {
        length = stringLength;
        linearDensity = density;
        tension = stringTension;

        float fundamental = (float)(0.5f / length * Math.Sqrt(tension / linearDensity));
        Debug.Log($"Fundamental frequency: {fundamental} Hz");

        for (int i = 0; i < harmonicsCount; i++)
        {
            // Apply slight inharmonicity
            harmonicsFrequencies[i] = fundamental * (i + 1) * (1f + 0.0001f * (i + 1) * (i + 1));
            harmonicsAmplitudes[i] = 0f;
            harmonicsPhases[i] = 0f;
        }
    }

    public void SetStringPropertiesFromData(PianoStringData data)
    {
        SetStringProperties(data.length, data.linearDensity, data.tension);
    }

    public void Pinch()
    {
        PinchWithIntensity(pinchIntensity);
    }

    public void PinchWithIntensity(float intensity)
    {
        timeSincePinch = 0f;

        // Clamp intensity between min and max
        currentPinchIntensity = Mathf.Clamp(intensity, minIntensity, maxIntensity);

        // Calculate harmonic amplitudes based on pinch position and intensity
        for (int i = 0; i < harmonicsCount; i++)
        {
            float n = i + 1;

            // Physical string pinch formula with intensity modifier
            harmonicsAmplitudes[i] = 2f
                                     * currentPinchIntensity
                                     / (n * n * Mathf.PI * Mathf.PI)
                                     * Mathf.Sin(n * Mathf.PI * pinchPosition);

            // Velocity affects higher harmonics differently (harder strikes produce more high frequencies)
            float harmonicIntensityModifier = 1f + (currentPinchIntensity - 0.5f) * 0.3f * (n / harmonicsCount);
            harmonicsAmplitudes[i] *= harmonicIntensityModifier;
        }

        targetAmplitude = 1f;

        // Start visualization
        if (visualizer != null)
        {
            visualizer.StartVisualization(harmonicsFrequencies, harmonicsAmplitudes, length);
            visualizationActive = true;
        }

        if (!audioSource.isPlaying)
        {
            audioSource.Play();
        }
    }

    public void StopPinch()
    {
        targetAmplitude = 0f;
        visualizer.StopVisualization();
    }

    private void Update()
    {
        if (amplitude < 0.001f && targetAmplitude == 0f && audioSource.isPlaying)
        {
            audioSource.Stop();
            for (int i = 0; i < harmonicsCount; i++)
            {
                harmonicsPhases[i] = 0f;
            }
        }
    }
}