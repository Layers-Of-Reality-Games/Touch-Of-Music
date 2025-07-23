using UnityEngine;

public class PhysicsHandController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform trackedHand; // The actual hand tracking transform
    [SerializeField] private Rigidbody physicsHandRb; // Rigidbody on this physics hand

    [Header("Physics Settings")]
    [SerializeField] private float followStrength = 50f; // Force multiplier
    [SerializeField] private float followDamping = 10f; // Velocity damping
    [SerializeField] private float maxVelocity = 10f; // Cap velocity to prevent tunneling

    [Header("Rotation Settings")]
    [SerializeField] private float rotationStrength = 50f;
    [SerializeField] private float maxAngularVelocity = 10f;
    [SerializeField] private bool freezeRotationOnCollision = true;
    [SerializeField] private float collisionRotationDamping = 0.1f;

    private bool isColliding = false;
    private int collisionCount = 0;

    private void Start()
    {
        if (physicsHandRb == null)
            physicsHandRb = GetComponent<Rigidbody>();

        // Configure rigidbody
        physicsHandRb.useGravity = false;
        physicsHandRb.linearDamping = followDamping;
        physicsHandRb.angularDamping = followDamping;
        physicsHandRb.maxLinearVelocity = maxVelocity;
        physicsHandRb.maxAngularVelocity = maxAngularVelocity;
        physicsHandRb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        physicsHandRb.interpolation = RigidbodyInterpolation.Interpolate;
    }

    private void FixedUpdate()
    {
        if (trackedHand == null) return;

        // Position following with spring-damper physics
        Vector3 positionDifference = trackedHand.position - physicsHandRb.position;
        Vector3 targetVelocity = positionDifference * followStrength;

        // Apply velocity change instead of setting directly
        Vector3 velocityChange = targetVelocity - physicsHandRb.linearVelocity;
        physicsHandRb.AddForce(velocityChange, ForceMode.VelocityChange);

        // Rotation following (reduced during collision)
        float currentRotationStrength = isColliding && freezeRotationOnCollision ?
            rotationStrength * collisionRotationDamping : rotationStrength;

        Quaternion rotationDifference = trackedHand.rotation * Quaternion.Inverse(physicsHandRb.rotation);
        rotationDifference.ToAngleAxis(out float angle, out Vector3 axis);

        // Convert angle to -180 to 180 range
        if (angle > 180f) angle -= 360f;

        // Apply torque to match rotation (with deadzone to prevent jitter)
        if (Mathf.Abs(angle) > 0.1f)
        {
            Vector3 targetAngularVelocity = axis * (angle * Mathf.Deg2Rad) * currentRotationStrength;
            Vector3 angularVelocityChange = targetAngularVelocity - physicsHandRb.angularVelocity;

            // Clamp the angular velocity change to prevent wild spins
            angularVelocityChange = Vector3.ClampMagnitude(angularVelocityChange, maxAngularVelocity);

            physicsHandRb.AddTorque(angularVelocityChange, ForceMode.VelocityChange);
        }

        // Extra damping during collision
        if (isColliding)
        {
            physicsHandRb.angularVelocity *= 0.9f;
        }
    }

    // Optional: Sync visual hand to physics hand position when collision occurs
    public void SyncVisualToPhysics(Transform visualHand, float blendAmount = 0.5f)
    {
        visualHand.position = Vector3.Lerp(trackedHand.position, physicsHandRb.position, blendAmount);
        visualHand.rotation = Quaternion.Slerp(trackedHand.rotation, physicsHandRb.rotation, blendAmount);
    }

    // Collision detection
    private void OnCollisionEnter(Collision collision)
    {
        collisionCount++;
        isColliding = collisionCount > 0;
    }

    private void OnCollisionExit(Collision collision)
    {
        collisionCount--;
        isColliding = collisionCount > 0;
    }
}