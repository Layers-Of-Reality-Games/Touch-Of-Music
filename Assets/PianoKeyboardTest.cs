// PianoKeyboardTester.cs
using System.Collections.Generic;
using UnityEngine;

public class PianoKeyboardTester : MonoBehaviour
{
    [Header("Piano Keys Mapping")]
    [SerializeField] private List<PianoKeyMapping> keyMappings = new List<PianoKeyMapping>();
    
    [Header("Settings")]
    [SerializeField] private float testVelocity = 0.7f;
    [SerializeField] private bool enableTesting = true;
    
    [System.Serializable]
    public class PianoKeyMapping
    {
        public KeyCode keyboardKey;
        public PianoKey pianoKey;
        public string noteName;
        [HideInInspector] public bool isPressed;
    }
    
    private void Start()
    {
        // Auto-find piano keys if not assigned
        if (keyMappings.Count == 0)
        {
            AutoSetupKeyMappings();
        }
    }
    
    private void Update()
    {
        if (!enableTesting) return;
        
        foreach (var mapping in keyMappings)
        {
            if (mapping.pianoKey == null) continue;
            
            // Key pressed
            if (Input.GetKeyDown(mapping.keyboardKey))
            {
                mapping.isPressed = true;
                mapping.pianoKey.PlayNote();
                Debug.Log($"Playing {mapping.noteName} with keyboard key {mapping.keyboardKey}");
            }
            
            // Key released
            if (Input.GetKeyUp(mapping.keyboardKey) && mapping.isPressed)
            {
                mapping.isPressed = false;
                mapping.pianoKey.StopNote();
                Debug.Log($"Stopping {mapping.noteName}");
            }
        }
        
        // Additional controls
        if (Input.GetKeyDown(KeyCode.Space))
        {
            // Stop all notes
            StopAllNotes();
        }
        
        // Velocity control
        if (Input.GetKey(KeyCode.LeftShift))
        {
            testVelocity = 1.0f; // Forte
        }
        else if (Input.GetKey(KeyCode.LeftControl))
        {
            testVelocity = 0.3f; // Piano
        }
        else
        {
            testVelocity = 0.7f; // Mezzo-forte
        }
    }
    
    private void AutoSetupKeyMappings()
    {
        PianoKey[] pianoKeys = FindObjectsOfType<PianoKey>();
        
        // Standard piano keyboard mapping (like a DAW)
        KeyCode[] keys = {
            KeyCode.Q,     // C
            KeyCode.Z,     // C#
            KeyCode.S,     // D
            KeyCode.E,     // D#
            KeyCode.D,     // E
            KeyCode.F,     // F
            KeyCode.T,     // F#
            KeyCode.G,     // G
            KeyCode.Y,     // G#
            KeyCode.H,     // A
            KeyCode.U,     // A#
            KeyCode.J,     // B
            KeyCode.K,     // C (octave up)
            KeyCode.O,     // C#
            KeyCode.L,     // D
            KeyCode.P,     // D#
            KeyCode.M,     // E
        };
        
        for (int i = 0; i < Mathf.Min(pianoKeys.Length, keys.Length); i++)
        {
            keyMappings.Add(new PianoKeyMapping
            {
                keyboardKey = keys[i],
                pianoKey = pianoKeys[i],
                noteName = pianoKeys[i].name,
                isPressed = false
            });
        }
    }
    
    public void StopAllNotes()
    {
        foreach (var mapping in keyMappings)
        {
            if (mapping.pianoKey != null && mapping.isPressed)
            {
                mapping.pianoKey.StopNote();
                mapping.isPressed = false;
            }
        }
        Debug.Log("All notes stopped");
    }
    
    private void OnGUI()
    {
        if (!enableTesting) return;
        
        GUILayout.BeginArea(new Rect(10, 10, 300, 200));
        GUILayout.Label("Piano Keyboard Tester");
        GUILayout.Label($"Current Velocity: {testVelocity:F1}");
        GUILayout.Label("Hold Shift for forte (loud)");
        GUILayout.Label("Hold Ctrl for piano (soft)");
        GUILayout.Label("Space to stop all notes");
        GUILayout.Label("Keys: A-S-D-F-G-H-J-K-L...");
        GUILayout.EndArea();
    }
}
