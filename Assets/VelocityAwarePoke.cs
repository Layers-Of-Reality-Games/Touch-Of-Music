using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class VelocityAwarePoke : MonoBehaviour
{
    XRPokeInteractor pokeInteractor;
    private InputDevice _device;

    [Header("Velocity Settings")]
    [SerializeField] private float minPokeDepth = 0.01f;
    [SerializeField] private float maxPokeDepth = 0.05f;
    [SerializeField] private float velocityThreshold = 2f;

    void Start()
    {
        pokeInteractor = GetComponent<XRPokeInteractor>();

        if (transform.name.Contains("Left") || transform.parent.name.Contains("Left"))
        {
            _device = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
        }
        else
        {
            _device = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
        }
    }

    void Update()
    {
        Vector3 velocity;
        if (_device.TryGetFeatureValue(CommonUsages.deviceVelocity, out velocity))
        {
            float speed = velocity.magnitude;
            float normalizedSpeed = Mathf.Clamp01(speed / velocityThreshold);
            pokeInteractor.pokeDepth = Mathf.Lerp(minPokeDepth, maxPokeDepth, normalizedSpeed);
        }
    }
}