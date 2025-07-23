using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class PianoKeySurfaceConstraint : MonoBehaviour
{
    [Header("Key Settings")]
    [SerializeField] private float keyPressDepth = 0.01f; // How far the key can be pressed
    [SerializeField] private float surfaceOffset = 0.001f; // Small offset to prevent z-fighting
    [SerializeField] private float constraintStrength = 15f; // How strongly to pull the finger to surface
    [SerializeField] private float releaseDistance = 0.05f; // Distance to release the constraint

    [Header("Components")]
    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable interactable;
    [SerializeField] private Collider keyCollider;
    private Transform keyTransform;
    private Vector3 keyStartPosition;

    // Track active interactors
    private Dictionary<UnityEngine.XR.Interaction.Toolkit.Interactors.IXRInteractor, ConstraintInfo> activeConstraints = new Dictionary<UnityEngine.XR.Interaction.Toolkit.Interactors.IXRInteractor, ConstraintInfo>();

    private class ConstraintInfo
    {
        public Transform interactorTransform;
        public Vector3 localContactPoint;
        public bool isConstrained;
    }

    void Start()
    {
        // Get components
        interactable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable>();
        keyTransform = transform;
        keyStartPosition = keyTransform.localPosition;

        // Subscribe to interaction events
        interactable.hoverEntered.AddListener(OnHoverEntered);
        interactable.hoverExited.AddListener(OnHoverExited);
        interactable.selectEntered.AddListener(OnSelectEntered);
        interactable.selectExited.AddListener(OnSelectExited);
    }

    void OnDestroy()
    {
        // Unsubscribe from events
        if (interactable != null)
        {
            interactable.hoverEntered.RemoveListener(OnHoverEntered);
            interactable.hoverExited.RemoveListener(OnHoverExited);
            interactable.selectEntered.RemoveListener(OnSelectEntered);
            interactable.selectExited.RemoveListener(OnSelectExited);
        }
    }

    void OnHoverEntered(HoverEnterEventArgs args)
    {
        var interactor = args.interactorObject;
        if (interactor.transform != null && !activeConstraints.ContainsKey(interactor))
        {
            activeConstraints[interactor] = new ConstraintInfo
            {
                interactorTransform = interactor.transform,
                isConstrained = false
            };
        }
    }

    void OnHoverExited(HoverExitEventArgs args)
    {
        activeConstraints.Remove(args.interactorObject);
    }

    void OnSelectEntered(SelectEnterEventArgs args)
    {
        var interactor = args.interactorObject;
        if (activeConstraints.ContainsKey(interactor))
        {
            activeConstraints[interactor].isConstrained = true;
        }
    }

    void OnSelectExited(SelectExitEventArgs args)
    {
        var interactor = args.interactorObject;
        if (activeConstraints.ContainsKey(interactor))
        {
            activeConstraints[interactor].isConstrained = false;
        }
    }

    void FixedUpdate()
    {
        foreach (var kvp in activeConstraints)
        {
            var constraint = kvp.Value;
            if (constraint.interactorTransform == null) continue;

            // Get the closest point on the key collider to the interactor
            Vector3 closestPoint = keyCollider.ClosestPoint(constraint.interactorTransform.position);

            // Calculate distance to surface
            float distanceToSurface = Vector3.Distance(constraint.interactorTransform.position, closestPoint);

            // Apply constraint if close enough or already constrained
            if (constraint.isConstrained || distanceToSurface < releaseDistance)
            {
                // Calculate the target position (on the surface with offset)
                Vector3 surfaceNormal = (constraint.interactorTransform.position - closestPoint).normalized;
                Vector3 targetPosition = closestPoint + surfaceNormal * surfaceOffset;

                // Apply constraint force
                if (constraint.interactorTransform.TryGetComponent<Rigidbody>(out Rigidbody rb))
                {
                    // Use physics-based constraint
                    Vector3 force = (targetPosition - constraint.interactorTransform.position) * constraintStrength;
                    rb.AddForce(force, ForceMode.VelocityChange);

                    // Dampen velocity perpendicular to the key surface
                    Vector3 velocity = rb.linearVelocity;
                    float normalVelocity = Vector3.Dot(velocity, surfaceNormal);
                    if (normalVelocity > 0) // Moving away from surface
                    {
                        rb.linearVelocity -= surfaceNormal * normalVelocity * 0.8f;
                    }
                }
                else
                {
                    // Direct position constraint (less ideal but works without rigidbody)
                    constraint.interactorTransform.position = Vector3.Lerp(
                        constraint.interactorTransform.position,
                        targetPosition,
                        Time.fixedDeltaTime * constraintStrength
                    );
                }
            }
        }
    }

    // Optional: Add haptic feedback
    public void TriggerHapticFeedback(UnityEngine.XR.Interaction.Toolkit.Interactors.IXRInteractor interactor, float amplitude = 0.1f, float duration = 0.1f)
    {
        if (interactor is UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInputInteractor controllerInteractor)
        {
            controllerInteractor.SendHapticImpulse(amplitude, duration);
        }
    }
}