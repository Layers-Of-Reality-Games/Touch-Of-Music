using UnityEngine;

public class PianoKeys : MonoBehaviour
{
    [SerializeField] private int keyCount = 88;
    [SerializeField] private PianoKey whiteKeyPrefab;
    [SerializeField] private PianoKey blackKeyPrefab;
    [SerializeField] private int octaveOffset = 0;

    private void Start()
    {
        GeneratePianoKeys();
    }

    private void GeneratePianoKeys()
    {
        string[] noteNames = { "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B" };

        float whiteKeyWidth = 0.027f;
        float blackKeyWidth = 0.018f;
        float blackKeyHeight = 0.005f;
        float blackKeyDepth = 0.02f;

        for (int i = 0; i < keyCount; i++)
        {
            int noteIndex = (i + 9) % 12;
            int octave = ((i + 9) / 12) + octaveOffset;
            string noteName = noteNames[noteIndex] + octave;
            bool isBlackKey = noteNames[noteIndex].Contains("#");

            if (isBlackKey) continue;

            PianoKey key = Instantiate(whiteKeyPrefab, transform);
            key.name = noteName;
            key.Initialize(noteName);

            float xPos = CalculateKeyPosition(i, whiteKeyWidth);
            key.transform.localPosition = new Vector3(xPos, 0f, 0f);
        }

        for (int i = 0; i < keyCount; i++)
        {
            int noteIndex = (i + 9) % 12;
            int octave = ((i + 9) / 12) + octaveOffset;
            string noteName = noteNames[noteIndex] + octave;
            bool isBlackKey = noteNames[noteIndex].Contains("#");

            if (!isBlackKey) continue;

            PianoKey key = Instantiate(blackKeyPrefab, transform);
            key.name = noteName;
            key.Initialize(noteName);

            float xPos = CalculateKeyPosition(i, whiteKeyWidth);
            key.transform.localPosition = new Vector3(xPos, blackKeyHeight, blackKeyDepth);

            key.transform.localScale = new Vector3(blackKeyWidth / whiteKeyWidth, 1f, 0.6f);
        }
    }

    private static float CalculateKeyPosition(int keyIndex, float whiteKeyWidth)
    {
        int whiteKeyIndex = 0;

        for (int i = 0; i <= keyIndex; i++)
        {
            int noteIndex = (i + 9) % 12;
            // White keys are C, D, E, F, G, A, B (indices 0, 2, 4, 5, 7, 9, 11)
            if (noteIndex is 0 or 2 or 4 or 5 or 7 or 9 or 11)
            {
                if (i < keyIndex)
                    whiteKeyIndex++;
            }
        }

        int currentNoteIndex = (keyIndex + 9) % 12;
        bool isBlackKey = !(currentNoteIndex is 0 or 2 or 4 or 5 or 7 or 9 or 11);

        if (!isBlackKey)
        {
            return whiteKeyIndex * whiteKeyWidth;
        }

        float leftWhiteKeyPosition = whiteKeyIndex * whiteKeyWidth;

        switch (currentNoteIndex)
        {
            case 1:  // C#
            case 6:  // F#
                return leftWhiteKeyPosition - whiteKeyWidth * 0.55f;
            case 3:  // D#
            case 8:  // G#
            case 10: // A#
                return leftWhiteKeyPosition - whiteKeyWidth * 0.45f;
            default:
                return leftWhiteKeyPosition;
        }
    }
}