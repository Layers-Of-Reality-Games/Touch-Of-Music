using UnityEngine;

public class PianoKeys : MonoBehaviour
{
    [SerializeField] private int keyCount = 88;
    [SerializeField] private PianoKey whiteKeyPrefab;
    [SerializeField] private PianoKey blackKeyPrefab;

    private void Start()
    {
        GeneratePianoKeys();
    }

    private void GeneratePianoKeys()
    {
        string[] noteNames = { "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B" };

        float whiteKeyWidth = 0.027f;
        float blackKeyWidth = 0.018f;

        for (int i = 0; i < keyCount; i++)
        {
            int noteIndex = (i + 9) % 12;
            int octave = (i + 9) / 12;
            string noteName = noteNames[noteIndex] + octave;
            bool isBlackKey = noteNames[noteIndex].Contains("#");
            if (isBlackKey) continue;

            PianoKey keyPrefab = isBlackKey ? blackKeyPrefab : whiteKeyPrefab;
            PianoKey key = Instantiate(keyPrefab, transform);
            key.name = noteName;
            key.Initialize(noteName);

            float xPos = CalculateKeyPosition(i, whiteKeyWidth);
            float yPos = isBlackKey ? 0.01f : 0f; // Black keys slightly raised
            float zPos = isBlackKey ? -0.01f : 0f; // Black keys slightly forward

            key.transform.localPosition = new Vector3(xPos, yPos, zPos);
        }
    }

    private static float CalculateKeyPosition(int keyIndex, float whiteKeyWidth)
    {
        // Count white keys before this one
        int whiteKeysBeforeThis = 0;

        for (int i = 0; i < keyIndex; i++)
        {
            int noteIndex = (i + 9) % 12;
            if (noteIndex is 0 or 2 or 4 or 5 or 7 or 9 or 11)
            {
                whiteKeysBeforeThis++;
            }
        }

        int currentNoteIndex = (keyIndex + 9) % 12;
        bool isBlackKey = !(currentNoteIndex is 0 or 2 or 4 or 5 or 7 or 9 or 11);

        if (!isBlackKey)
        {
            return whiteKeysBeforeThis * whiteKeyWidth;
        }

        float basePosition = whiteKeysBeforeThis * whiteKeyWidth;

        switch (currentNoteIndex)
        {
            case 1:  // C#
            case 6:  // F#
                return basePosition + whiteKeyWidth * 0.65f;
            case 3:  // D#
            case 8:  // G#
            case 10: // A#
                return basePosition + whiteKeyWidth * 0.35f;
            default:
                return basePosition;
        }
    }
}