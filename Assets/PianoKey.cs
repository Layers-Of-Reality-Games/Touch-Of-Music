using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Filtering;
using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;

public class PianoKey : MonoBehaviour
{
    [SerializeField] private BaseString baseString;
    [SerializeField] private XRSimpleInteractable interactable;
    [SerializeField] private XRPokeFollowAffordance pokeFollowAffordance;
    [SerializeField] private XRPokeFilter pokeFilter;

    [Header("Velocity Settings")]
    [SerializeField] private float velocitySmoothTime = 0.05f;
    [SerializeField] private float minVelocity = 0.01f;
    [SerializeField] private float maxVelocity = 0.5f;
    [SerializeField] private float velocityScaleFactor = 1.0f;
    [SerializeField] private AnimationCurve velocityCurve = AnimationCurve.Linear(0, 0, 1, 1);

    
    [Header("PLAY")]
    [SerializeField] private bool playplz = false;
    
    private string noteName;
    private PianoStringData stringData;

    // Velocity tracking
    private Vector3 previousPosition;
    private Vector3 currentVelocity;
    private Vector3 smoothedVelocity;
    private Vector3 velocitySmoothing;
    private bool isBeingPoked = false;
    private float maxVelocityDuringPoke = 0f;

    private void Awake()
    {
        if (baseString == null) baseString = GetComponentInChildren<BaseString>();
        if (interactable == null) interactable = GetComponent<XRSimpleInteractable>();
        if (pokeFollowAffordance == null) pokeFollowAffordance = GetComponent<XRPokeFollowAffordance>();
        if (pokeFilter == null) pokeFilter = GetComponent<XRPokeFilter>();
    }

    private void Start()
    {
        StartCoroutine(SetupXREvents());
        previousPosition = transform.position;
    }

    private IEnumerator SetupXREvents()
    {
        yield return null;

        if (interactable != null)
        {
            interactable.selectEntered.RemoveAllListeners();
            interactable.selectExited.RemoveAllListeners();
            interactable.hoverEntered.RemoveAllListeners();
            interactable.hoverExited.RemoveAllListeners();

            // Use hover events for poke detection
            interactable.hoverEntered.AddListener(OnHoverEntered);
            interactable.hoverExited.AddListener(OnHoverExited);
            interactable.selectEntered.AddListener(OnSelectEntered);
            interactable.selectExited.AddListener(OnSelectExited);
        }
        else
        {
            Debug.LogError($"No XRSimpleInteractable found on {name}");
        }
    }

    private void Update()
    {
        if (playplz)
        {
            PlayNote();

        }
        if (isBeingPoked && pokeFollowAffordance != null)
        {
            Vector3 pokePosition = pokeFollowAffordance.pokeFollowTransform.position;

            currentVelocity = (pokePosition - previousPosition) / Time.deltaTime;
            previousPosition = pokePosition;

            smoothedVelocity = Vector3.SmoothDamp(smoothedVelocity, currentVelocity, ref velocitySmoothing, velocitySmoothTime);

            float velocityMagnitude = smoothedVelocity.magnitude * velocityScaleFactor;
            if (velocityMagnitude > maxVelocityDuringPoke)
            {
                maxVelocityDuringPoke = velocityMagnitude;
            }
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

    private void OnHoverEntered(HoverEnterEventArgs args)
    {
        isBeingPoked = true;
        maxVelocityDuringPoke = 0f;
        previousPosition = pokeFollowAffordance != null ? pokeFollowAffordance.pokeFollowTransform.position : transform.position;
    }

    private void OnHoverExited(HoverExitEventArgs args)
    {
        isBeingPoked = false;
    }

    private void OnSelectEntered(SelectEnterEventArgs args)
    {
        Debug.Log($"Select entered: {noteName} with max velocity: {maxVelocityDuringPoke}");
        PlayNoteWithVelocity(maxVelocityDuringPoke);
    }

    private void OnSelectExited(SelectExitEventArgs args)
    {
        StopNote();
    }

    public void PlayNote()
    {
        PlayNoteWithVelocity(0.5f);
    }

    public void PlayNoteWithVelocity(float velocity)
    {
        float normalizedVelocity = Mathf.InverseLerp(minVelocity, maxVelocity, velocity);
        normalizedVelocity = Mathf.Clamp01(normalizedVelocity);

        float intensity = velocityCurve.Evaluate(normalizedVelocity);

        Debug.Log($"Playing note: {noteName} with intensity: {intensity:F2} (velocity: {velocity:F3})");

        if (baseString != null)
            baseString.PinchWithIntensity(intensity);
        else
            Debug.LogError($"Cannot play note {noteName} - BaseString is null!");
    }

    public void StopNote()
    {
        if (baseString != null) baseString.StopPinch();
    }
}