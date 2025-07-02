using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class PianoKey : MonoBehaviour
{
    [SerializeField] private BaseString baseString;
    [SerializeField] private XRSimpleInteractable interactable;

    private string noteName;
    private PianoStringData stringData;

    private void Awake()
    {
        // Try to find components if not assigned
        if (baseString == null) baseString = GetComponentInChildren<BaseString>();

        if (interactable == null) interactable = GetComponent<XRSimpleInteractable>();
    }

    private void Start()
    {
        StartCoroutine(SetupXREvents());
    }

    private IEnumerator SetupXREvents()
    {
        yield return null;

        if (interactable != null)
        {
            interactable.selectEntered.RemoveAllListeners();
            interactable.selectExited.RemoveAllListeners();

            interactable.selectEntered.AddListener(OnSelectEntered);
            interactable.selectExited.AddListener(OnSelectExited);

            Debug.Log($"XR events configured for {noteName}");
        }
        else
        {
            Debug.LogError($"No XRSimpleInteractable found on {name}");
        }
    }

    public void Initialize(string note)
    {
        noteName = note;
        stringData = PianoStringCalculator.CalculateStringProperties(noteName);

        if (baseString != null)
            baseString.SetStringPropertiesFromData(stringData);
        else
            Debug.LogWarning($"BaseString is null on {noteName}");
    }

    private void OnSelectEntered(SelectEnterEventArgs args)
    {
        Debug.Log($"Select entered: {noteName}");
        PlayNote();
    }

    private void OnSelectExited(SelectExitEventArgs args)
    {
        Debug.Log($"Select exited: {noteName}");
        StopNote();
    }

    public void PlayNote()
    {
        Debug.Log($"Playing note: {noteName}");
        if (baseString != null)
            baseString.Pinch();
        else
            Debug.LogError($"Cannot play note {noteName} - BaseString is null!");
    }

    public void StopNote()
    {
        Debug.Log($"Stopping note: {noteName}");
        if (baseString != null) baseString.StopPinch();
    }
}