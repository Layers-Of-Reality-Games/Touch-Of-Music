using UnityEngine;

public class PianoKeys : MonoBehaviour
{
    [SerializeField] private int keyCount = 88;
    [SerializeField] private PianoKey keyPrefab;

    private void Start()
    {
        GeneratePianoKeys();
    }

    private void GeneratePianoKeys()
    {
        string[] noteNames = { "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B" };

        for (var i = 0; i < keyCount; i++)
        {
            int noteIndex = (i + 9) % 12;
            bool isBlackKey = noteNames[noteIndex].Contains("#");
            if (isBlackKey) continue;
            int octave = (i + 9) / 12;
            string noteName = noteNames[noteIndex] + octave;

            PianoKey key = Instantiate(keyPrefab, transform);
            key.name = noteName;
            key.Initialize(noteName);

            float xPos = CalculateKeyPosition(i, isBlackKey);
            key.transform.localPosition = new Vector3(xPos, isBlackKey ? 0.01f : 0f, 0);
        }
    }

    private float CalculateKeyPosition(int keyIndex, bool isBlackKey)
    {
        var whiteKeyWidth = 0.027f;
        var whiteKeyCount = 0;

        for (var i = 0; i <= keyIndex; i++)
        {
            string[] noteNames = { "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B" };
            int noteIndex = (i + 9) % 12;
            if (!noteNames[noteIndex].Contains("#"))
                whiteKeyCount++;
        }

        return whiteKeyCount * whiteKeyWidth;
    }
}