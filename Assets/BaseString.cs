using System;
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
    private int sampleRate;
    public int HarmonicsCount => harmonicsCount;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        sampleRate = AudioSettings.outputSampleRate;

        audioSource.playOnAwake = true;
        audioSource.spatialBlend = 0f;
        audioSource.clip = AudioClip.Create("StringSound", sampleRate, 1, sampleRate, false);
        audioSource.Play();

        harmonicsFrequencies = new float[harmonicsCount];
        harmonicsProportions = new double[harmonicsCount];

        // RegenerateHarmonics();
    }

    // private void Update()
    // {
    //     UpdateWaveformDisplay();
    // }

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

    private void RegenerateHarmonics()
    {
        if (harmonicLines != null)
            for (var i = 0; i < harmonicLines.Length; i++)
                Destroy(harmonicLines[i].gameObject);
        harmonicLines = new LineRenderer[harmonicsCount];

        for (var i = 0; i < harmonicsCount; i++)
        {
            harmonicLines[i] = Instantiate(lineRendererPrefab, transform);
            harmonicLines[i].positionCount = 256;
            harmonicLines[i].startWidth = 0.2f;
            harmonicLines[i].endWidth = 0.2f;
            harmonicLines[i].material.color = Color.Lerp(Color.red, Color.blue, (float) i / harmonicsCount);
        }

        RegenerateText();
    }

    private void RegenerateText()
    {
        if (harmonicText != null)
            for (var i = 0; i < harmonicText.Length; i++)
                Destroy(harmonicText[i].gameObject);
        harmonicText = new TextMeshProUGUI[harmonicsCount];

        for (var i = 0; i < harmonicsCount; i++)
        {
            harmonicText[i] = Instantiate(harmonicTextPrefab, canvas.transform);
            harmonicText[i].text =
                $"Harmonic {i + 1} : {GetNoteNameFromFrequency(harmonicsFrequencies[i])} - {harmonicsFrequencies[i]:F2} Hz";
            harmonicText[i].color = Color.Lerp(Color.red, Color.blue, (float) i / harmonicsCount);
            harmonicText[i].fontSize = 10;
            harmonicText[i].transform.position = new Vector3(100, 100 + i * 30f, 0);
        }
    }

    private void UpdateWaveformDisplay()
    {
        for (var h = 0; h < harmonicsCount; h++)
        for (var i = 0; i < sampleCount; i++)
        {
            float x = i / (float) (sampleCount - 1);

            float harmonicShape = Mathf.Sin((h + 1) * Mathf.PI * x);
            float oscillation = Mathf.Sin(2 * Mathf.PI * harmonicsFrequencies[h] * currentTime / 70f);
            float dampingFactor = Mathf.Exp(-dampingCoefficient * (h + 1) * currentTime);
            var amplitude = (float) (harmonicShape
                                     * oscillation
                                     * harmonicsProportions[h]
                                     * excitationIntensity
                                     * dampingFactor);
            var position = new Vector3(x * 10f, amplitude * 3f, 0);
            harmonicLines[h].SetPosition(i, position);
        }
    }

    public void Pinch()
    {
        currentTime = 0f;
        excitationIntensity = pinchIntensity;

        for (var i = 0; i < harmonicsCount; i++)
        {
            harmonicsProportions[i] = Math.Abs(Math.Sin((i + 1) * Mathf.PI * pinchPosition / length));
            harmonicsFrequencies[i] = (float) ((i + 1) / (2 * length) * Math.Sqrt(tension / linearDensity));
        }

        // RegenerateText();
    }

    private float GenerateSoundSample(float time)
    {
        float sample = 0;

        for (var k = 0; k < harmonicsCount; k++)
        {
            float harmonicWave = Mathf.Sin(2 * Mathf.PI * harmonicsFrequencies[k] * time);
            float dampingFactor = Mathf.Exp(-dampingCoefficient * (k + 1) * time);

            sample += (float) (harmonicWave * harmonicsProportions[k] * excitationIntensity * dampingFactor);
        }

        return Mathf.Clamp(sample, -1f, 1f);
    }

    public void OnChangeHarmonicsCount(int count)
    {
        if (count >= 10) return;

        harmonicsCount = count;
        Array.Resize(ref harmonicsFrequencies, harmonicsCount);
        Array.Resize(ref harmonicsProportions, harmonicsCount);

        // RegenerateHarmonics();
    }

    public string GetNoteNameFromFrequency(float frequency)
    {
        const float referenceFrequency = 440.0f;

        string[] noteNames = { "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B" };
        float numberOfHalfSteps = 12 * Mathf.Log(frequency / referenceFrequency, 2);
        int roundedHalfSteps = Mathf.RoundToInt(numberOfHalfSteps);
        int octave = 3 + (roundedHalfSteps + 9) / 12;

        int noteIndex = (roundedHalfSteps + 9) % 12;
        if (noteIndex < 0) noteIndex += 12;

        string noteName = noteNames[noteIndex];

        float cents = 100 * (numberOfHalfSteps - roundedHalfSteps);
        string centsString = cents != 0 ? $" ({cents:+0.0;-0.0} cents)" : "";

        return $"{noteName}{octave}{centsString}";
    }
}