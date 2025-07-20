using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DebugDisplay : MonoBehaviour
{
    private Dictionary<string, string> debugLogs = new();
    public TextMeshProUGUI display;
    public Camera camera;

    private void Update()
    {
        Vector3 direction = transform.position - camera.transform.position;
        direction.y = 0; // Keep upright
        transform.rotation = Quaternion.LookRotation(direction);
    }

    private void OnEnable()
    {
        Application.logMessageReceived += HandleLog;
    }

    private void OnDisable()
    {
        Application.logMessageReceived -= HandleLog;
    }

    void HandleLog(string logString, string stackTrace, LogType type)
    {
        if (type == LogType.Log)
        {
            string[] splitString = logString.Split(char.Parse(":"));
            string debugKey = splitString.Length > 1 ? splitString[0].Trim() : "General";
            string debugValue = splitString.Length > 1 ? string.Join(":", splitString, 1, splitString.Length - 1).Trim() : logString.Trim();

            debugLogs[debugKey] = debugValue;
        }

        var displayText = "";
        foreach (KeyValuePair<string, string> log in debugLogs)
        {
            if (log.Value == "")
            {
                displayText += $"{log.Key}\n";
            }
            else
            {
                displayText += $"{log.Key}: {log.Value}\n";
            }
        }

        display.text = displayText;
    }
}
