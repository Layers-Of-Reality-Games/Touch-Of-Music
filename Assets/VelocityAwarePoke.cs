using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class VelocityAwarePoke : MonoBehaviour
{
    private XRPokeInteractor pokeInteractor;
    private InputDevice device;

    [Header("Velocity Settings")]
    [SerializeField] private float minPokeDepth = 0.003f;
    [SerializeField] private float maxPokeDepth = 0.05f;
    [SerializeField] private float velocityThreshold = 2f;

    void Start()
    {
        pokeInteractor = GetComponent<XRPokeInteractor>();

        if (transform.name.Contains("Left") || transform.parent.name.Contains("Left"))
        {
            device = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
        }
        else
        {
            device = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
        }
    }

    void Update()
    {
        if (!device.TryGetFeatureValue(CommonUsages.deviceVelocity, out Vector3 velocity)) return;

        float speed = velocity.magnitude;
        float normalizedSpeed = Mathf.Clamp01(speed / velocityThreshold);
        pokeInteractor.pokeDepth = Mathf.Lerp(minPokeDepth, maxPokeDepth, normalizedSpeed);
    }
}