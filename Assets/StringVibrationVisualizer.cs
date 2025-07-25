using System.Collections.Generic;
using UnityEngine;

public class StringVibrationVisualizer : MonoBehaviour
{
    [Header("Visualization Settings")]
    [SerializeField] private int pointsPerString = 100;
    [SerializeField] private float visualizationLength = 0.5f;
    [SerializeField] private float visualizationHeight = 0.1f;
    [SerializeField] private float visualizationXOffset = 0.05f; // Changed from Y offset
    [SerializeField] private float timeSlowFactor = 0.01f;
    [SerializeField] private bool showHarmonics = true;
    [SerializeField] private bool showSum = true;
    [SerializeField] private bool verticalOrientation = true; // New option

    [Header("Line Renderer Settings")]
    [SerializeField] private float sumLineWidth = 0.005f;
    [SerializeField] private float harmonicLineWidth = 0.002f;
    [SerializeField] private float fadeOutTime = 2f;
    [SerializeField] private bool visualizationEnabled = true;

    [Header("Colors")]
    [SerializeField] private Color sumColor = Color.white;
    [SerializeField] private Color[] harmonicColors = new Color[]
    {
        new Color(1f, 0.3f, 0.3f, 0.7f), // Red
        new Color(0.3f, 1f, 0.3f, 0.7f), // Green
        new Color(0.3f, 0.3f, 1f, 0.7f), // Blue
        new Color(1f, 1f, 0.3f, 0.7f),   // Yellow
        new Color(1f, 0.3f, 1f, 0.7f),   // Magenta
        new Color(0.3f, 1f, 1f, 0.7f),   // Cyan
        new Color(1f, 0.6f, 0.3f, 0.7f), // Orange
        new Color(0.6f, 0.3f, 1f, 0.7f)  // Purple
    };

    private BaseString baseString;
    private LineRenderer sumLineRenderer;
    private List<LineRenderer> harmonicLineRenderers = new List<LineRenderer>();
    private float visualTime = 0f;
    private float currentAlpha = 0f;
    private bool isPlaying = false;

    private float[] harmonicsFrequencies;
    private float[] harmonicsAmplitudes;
    private float stringLength;


    // public static bool VisualizationEnabled
    // {
    //     get => s_visualizationEnabled;
    //     set
    //     {
    //         s_visualizationEnabled = value;
    //         if (!value)
    //         {
    //             var allVisualizers = FindObjectsOfType<StringVibrationVisualizer>();
    //             foreach (var visualizer in allVisualizers)
    //             {
    //                 visualizer.StopVisualization();
    //             }
    //         }
    //     }
    // }

    private void Awake()
    {
        baseString = GetComponent<BaseString>();
        if (baseString == null)
        {
            Debug.LogError("StringVibrationVisualizer requires BaseString component!");
            enabled = false;
            return;
        }
    }

    public void Initialize(LineRenderer providedLineRenderer = null)
    {
        if (providedLineRenderer != null)
        {
            sumLineRenderer = providedLineRenderer;
        }
        else
        {
            GameObject sumObj = new GameObject("String Vibration Sum");
            sumObj.transform.SetParent(transform);

            sumObj.transform.localPosition = Vector3.up * visualizationXOffset;

            sumLineRenderer = sumObj.AddComponent<LineRenderer>();
        }

        SetupLineRenderer(sumLineRenderer, sumColor, sumLineWidth);

        // Create line renderers for each harmonic
        int harmonicsCount = baseString.HarmonicsCount;

        // Clear any existing harmonic renderers
        foreach (var lr in harmonicLineRenderers)
        {
            if (lr != null) Destroy(lr.gameObject);
        }
        harmonicLineRenderers.Clear();

        for (int i = 0; i < harmonicsCount; i++)
        {
            GameObject harmonicObj = new GameObject($"Harmonic {i + 1}");
            harmonicObj.transform.SetParent(transform);

            // Position offset based on orientation
            if (verticalOrientation)
                harmonicObj.transform.localPosition = Vector3.right * visualizationXOffset;
            else
                harmonicObj.transform.localPosition = Vector3.up * visualizationXOffset;

            LineRenderer lr = harmonicObj.AddComponent<LineRenderer>();
            Color color = harmonicColors[i % harmonicColors.Length];
            SetupLineRenderer(lr, color, harmonicLineWidth);
            harmonicLineRenderers.Add(lr);

            lr.gameObject.SetActive(showHarmonics);
        }

        // Initialize arrays
        harmonicsFrequencies = new float[harmonicsCount];
        harmonicsAmplitudes = new float[harmonicsCount];
    }

    private void SetupLineRenderer(LineRenderer lr, Color color, float width)
    {
        lr.positionCount = pointsPerString;
        lr.startWidth = width;
        lr.endWidth = width;
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.startColor = color;
        lr.endColor = color;
        lr.useWorldSpace = false;
        lr.alignment = LineAlignment.View;
        lr.enabled = false;
    }

    public void StartVisualization(float[] frequencies, float[] amplitudes, float length)
    {
        if (!visualizationEnabled)
            return;

        harmonicsFrequencies = frequencies;
        harmonicsAmplitudes = amplitudes;
        stringLength = length;

        visualTime = 0f;
        currentAlpha = 1f;
        isPlaying = true;

        // Enable line renderers
        if (sumLineRenderer != null)
        {
            sumLineRenderer.enabled = showSum;
        }

        for (int i = 0; i < harmonicLineRenderers.Count; i++)
        {
            if (harmonicLineRenderers[i] != null)
            {
                harmonicLineRenderers[i].enabled = showHarmonics && i < harmonicsFrequencies.Length;
            }
        }

        UpdateAlpha();

        // Force an immediate update to set initial positions
        UpdateStringVisualization();
    }

    public void StopVisualization()
    {
        isPlaying = false;
        currentAlpha = 0f;
        UpdateAlpha();

        if (sumLineRenderer != null)
            sumLineRenderer.enabled = false;

        foreach (var lr in harmonicLineRenderers)
        {
            if (lr != null)
                lr.enabled = false;
        }
    }

    private void Update()
    {
        if (!visualizationEnabled)
        {
            if (isPlaying)
            {
                StopVisualization();
            }

            return;
        }

        if (!isPlaying && currentAlpha <= 0)
        {
            return;
        }

        // Update visualization time (slowed down)
        if (isPlaying)
        {
            visualTime += Time.deltaTime * timeSlowFactor;
        }
        else
        {
            // Fade out
            currentAlpha = Mathf.Max(0, currentAlpha - Time.deltaTime / fadeOutTime);
            UpdateAlpha();

            // Disable when fully faded
            if (currentAlpha <= 0)
            {
                if (sumLineRenderer != null)
                    sumLineRenderer.enabled = false;

                foreach (var lr in harmonicLineRenderers)
                {
                    if (lr != null)
                        lr.enabled = false;
                }
            }
        }

        // Update string shape
        UpdateStringVisualization();
    }

    private void UpdateStringVisualization()
    {
        // Safety check
        if (harmonicsFrequencies == null || harmonicsAmplitudes == null)
            return;

        Vector3[] sumPositions = new Vector3[pointsPerString];
        List<Vector3[]> harmonicPositions = new List<Vector3[]>();

        // Initialize harmonic position arrays
        for (int h = 0; h < harmonicsFrequencies.Length; h++)
        {
            harmonicPositions.Add(new Vector3[pointsPerString]);
        }

        // Calculate positions for each point along the string
        for (int i = 0; i < pointsPerString; i++)
        {
            float x = (float)i / (pointsPerString - 1);
            float stringPos = x * visualizationLength - visualizationLength * 0.5f;

            float sumY = 0f;

            // Calculate each harmonic's contribution
            for (int h = 0; h < harmonicsFrequencies.Length; h++)
            {
                if (harmonicsAmplitudes[h] > 0.001f && harmonicsFrequencies[h] > 20f)
                {
                    // Standing wave formula: A * sin(kx) * cos(ωt)
                    float n = h + 1; // Harmonic number
                    float k = n * Mathf.PI / stringLength;
                    float omega = 2f * Mathf.PI * harmonicsFrequencies[h];

                    // Apply time-based damping
                    float damping = Mathf.Exp(-0.5f * (h + 1) * visualTime);
                    float amplitude = harmonicsAmplitudes[h] * damping * visualizationHeight;

                    // Standing wave displacement
                    float y = amplitude * Mathf.Sin(k * x * stringLength) * Mathf.Cos(omega * visualTime);

                    sumY += y;

                    // Store individual harmonic position
                    if (showHarmonics && h < harmonicLineRenderers.Count)
                    {
                        // Create position based on orientation
                        if (verticalOrientation)
                            harmonicPositions[h][i] = new Vector3(y, stringPos, 0);
                        else
                            harmonicPositions[h][i] = new Vector3(stringPos, y, 0);
                    }
                }
            }

            // Set sum position based on orientation
            if (verticalOrientation)
                sumPositions[i] = new Vector3(sumY, stringPos, 0);
            else
                sumPositions[i] = new Vector3(stringPos, sumY, 0);
        }

        if (showSum && sumLineRenderer != null)
        {
            sumLineRenderer.SetPositions(sumPositions);
        }

        if (showHarmonics)
        {
            for (int h = 0; h < Mathf.Min(harmonicLineRenderers.Count, harmonicsFrequencies.Length); h++)
            {
                if (harmonicsAmplitudes[h] > 0.001f && harmonicLineRenderers[h] != null)
                {
                    harmonicLineRenderers[h].SetPositions(harmonicPositions[h]);
                }
            }
        }
    }

    private void UpdateAlpha()
    {
        // Update sum line alpha
        if (sumLineRenderer != null)
        {
            Color sumCol = sumLineRenderer.startColor;
            sumCol.a = currentAlpha;
            sumLineRenderer.startColor = sumCol;
            sumLineRenderer.endColor = sumCol;
        }

        // Update harmonic lines alpha
        for (int i = 0; i < harmonicLineRenderers.Count; i++)
        {
            if (harmonicLineRenderers[i] != null)
            {
                Color harmCol = harmonicColors[i % harmonicColors.Length];
                harmCol.a = currentAlpha * 0.7f; // Keep harmonics slightly transparent
                harmonicLineRenderers[i].startColor = harmCol;
                harmonicLineRenderers[i].endColor = harmCol;
            }
        }
    }
}