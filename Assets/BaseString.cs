using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class BaseString : MonoBehaviour
{
    private const int sampleCount = 256;

    [Header("String properties")]
    [SerializeField] private float length = 0.65f;
    [SerializeField] private float linearDensity = 0.00738f;
    [SerializeField] private float tension = 86f;
    [SerializeField] private int harmonicsCount = 3;

    [Header("Pinch properties")]
    [SerializeField] private float pinchPosition = 0.15f;
    [SerializeField] private float pinchIntensity = 2f;

    [Header("Display")]
    [SerializeField] private LineRenderer lineRendererPrefab;
    [SerializeField] private TextMeshProUGUI harmonicTextPrefab;
    [SerializeField] private Canvas canvas;

    private readonly float dampingCoefficient = 0.5f;

    private AudioSource audioSource;

    private float currentTime;
    private float excitationIntensity;

    private LineRenderer[] harmonicLines;
    private float[] harmonicsFrequencies;
    private double[] harmonicsProportions;
    private TextMeshProUGUI[] harmonicText;
    private bool isPlaying;
    private int sampleRate;
    public int HarmonicsCount => harmonicsCount;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        sampleRate = AudioSettings.outputSampleRate;

        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;
        audioSource.loop = true;

        audioSource.clip = AudioClip.Create("StringSound", sampleRate * 2, 1, sampleRate, true, OnAudioRead);

        harmonicsFrequencies = new float[harmonicsCount];
        harmonicsProportions = new double[harmonicsCount];

        SetStringProperties(length, linearDensity, tension);
    }

    private void OnAudioFilterRead(float[] data, int channels)
    {
        if (!isPlaying)
        {
            // Fill with silence when not playing
            for (var i = 0; i < data.Length; i++) data[i] = 0f;
            return;
        }

        float timeStep = 1f / sampleRate;

        for (var i = 0; i < data.Length; i += channels)
        {
            float sample = GenerateSoundSample(currentTime);
            currentTime += timeStep;

            for (var channel = 0; channel < channels; channel++) data[i + channel] = sample;
        }
    }

    private void OnAudioRead(float[] data)
    {
        if (!isPlaying)
        {
            for (var i = 0; i < data.Length; i++) data[i] = 0f;
            return;
        }

        float timeStep = 1f / sampleRate;

        for (var i = 0; i < data.Length; i++)
        {
            float sample = GenerateSoundSample(currentTime);
            currentTime += timeStep;
            data[i] = sample;
        }
    }

    public void SetStringProperties(float stringLength, float density, float stringTension)
    {
        length = stringLength;
        linearDensity = density;
        tension = stringTension;

        for (var i = 0; i < harmonicsCount; i++)
        {
            harmonicsFrequencies[i] = (float) ((i + 1) / (2 * length) * Math.Sqrt(tension / linearDensity));
            harmonicsProportions[i] = 0;
        }
    }

    public void SetStringPropertiesFromData(PianoStringData data)
    {
        SetStringProperties(data.length, data.linearDensity, data.tension);
    }

    public void Pinch()
    {
        Debug.Log($"Pinch called - Frequency: {harmonicsFrequencies[0]:F2} Hz");

        currentTime = 0f;
        excitationIntensity = pinchIntensity;
        isPlaying = true;

        for (var i = 0; i < harmonicsCount; i++)
            harmonicsProportions[i] = Math.Abs(Math.Sin((i + 1) * Mathf.PI * pinchPosition));
        if (!audioSource.isPlaying) audioSource.Play();
    }

    public void StopPinch()
    {
        Debug.Log("StopPinch called");
        StartCoroutine(StopAfterDecay());
    }

    private IEnumerator StopAfterDecay()
    {
        yield return new WaitForSeconds(3f);
        isPlaying = false;
        if (audioSource.isPlaying && !isPlaying) audioSource.Stop();
    }

    private float GenerateSoundSample(float time)
    {
        float sample = 0;

        for (var k = 0; k < harmonicsCount; k++)
            if (harmonicsFrequencies[k] > 0) // Only process valid frequencies
            {
                float harmonicWave = Mathf.Sin(2 * Mathf.PI * harmonicsFrequencies[k] * time);
                float dampingFactor = Mathf.Exp(-dampingCoefficient * (k + 1) * time);

                sample += (float) (harmonicWave * harmonicsProportions[k] * excitationIntensity * dampingFactor);
            }

        sample *= 0.3f;

        return Mathf.Clamp(sample, -1f, 1f);
    }

    public void OnChangeHarmonicsCount(int count)
    {
        if (count >= 10) return;

        harmonicsCount = count;
        Array.Resize(ref harmonicsFrequencies, harmonicsCount);
        Array.Resize(ref harmonicsProportions, harmonicsCount);
    }

    public string GetNoteNameFromFrequency(float frequency)
    {
        const float referenceFrequency = 440.0f;

        string[] noteNames = { "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B" };
        float numberOfHalfSteps = 12 * Mathf.Log(frequency / referenceFrequency, 2);
        int roundedHalfSteps = Mathf.RoundToInt(numberOfHalfSteps);
        int octave = 4 + (roundedHalfSteps + 9) / 12;

        int noteIndex = (roundedHalfSteps + 9) % 12;
        if (noteIndex < 0) noteIndex += 12;

        string noteName = noteNames[noteIndex];

        float cents = 100 * (numberOfHalfSteps - roundedHalfSteps);
        string centsString = cents != 0 ? $" ({cents:+0.0;-0.0} cents)" : "";

        return $"{noteName}{octave}{centsString}";
    }
}