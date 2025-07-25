using System;
using UnityEngine;

[Serializable]
public class PianoStringData
{
    public string noteName;
    public float frequency;
    public float length;
    public float linearDensity;
    public float tension;
    public int stringCount;

    public PianoStringData(string note, float freq, float len, float density, float tens, int count = 1)
    {
        noteName = note;
        frequency = freq;
        length = len;
        linearDensity = density;
        tension = tens;
        stringCount = count;
    }
}

public class PianoStringCalculator
{
    public static PianoStringData CalculateStringProperties(string noteName)
    {
        float frequency = GetFrequencyFromNoteName(noteName);

        float linearDensity, tension;
        int stringCount;

        if (frequency < 261.63f) // Graves (A0-B3)
        {
            linearDensity = Mathf.Lerp(0.08f, 0.02f, (frequency - 27.5f) / (261.63f - 27.5f));
            tension = Mathf.Lerp(700f, 900f, (frequency - 27.5f) / (261.63f - 27.5f));
            stringCount = 1;
        }
        else if (frequency < 1046.5f) // Médiums (C4-B5)
        {
            linearDensity = Mathf.Lerp(0.015f, 0.005f, (frequency - 261.63f) / (1046.5f - 261.63f));
            tension = Mathf.Lerp(800f, 1000f, (frequency - 261.63f) / (1046.5f - 261.63f));
            stringCount = frequency < 523.25f ? 2 : 3; // 2 cordes jusqu'à C5, puis 3
        }
        else // Aigus (C6-C8)
        {
            linearDensity = Mathf.Lerp(0.008f, 0.003f, (frequency - 1046.5f) / (4186f - 1046.5f));
            tension = Mathf.Lerp(900f, 1200f, (frequency - 1046.5f) / (4186f - 1046.5f));
            stringCount = 3;
        }

        // L = (1/(2*f)) * sqrt(T/μ)
        var length = (float) (1.0 / (2.0 * frequency) * Math.Sqrt(tension / linearDensity));

        return new PianoStringData(noteName, frequency, length, linearDensity, tension, stringCount);
    }

    private static float GetFrequencyFromNoteName(string noteName)
    {
        string noteBase = noteName.Substring(0, noteName.Length - 1);
        int octave = int.Parse(noteName.Substring(noteName.Length - 1));

        // Tableau des fréquences de base (octave 4)
        string[] noteNames = { "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B" };
        float[] baseFrequencies =
        {
            261.63f, 277.18f, 293.66f, 311.13f, 329.63f, 349.23f, 369.99f, 392.00f, 415.30f, 440.00f, 466.16f, 493.88f
        };

        int noteIndex = Array.IndexOf(noteNames, noteBase);
        if (noteIndex == -1) return 440f; // Par défaut A4

        float baseFrequency = baseFrequencies[noteIndex];
        float frequency = baseFrequency * Mathf.Pow(2f, octave - 4);

        return frequency;
    }
}