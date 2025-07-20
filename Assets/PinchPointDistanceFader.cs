using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;


public class PinchPointDistanceFader : MonoBehaviour
{
    [Header("References")]
    public UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInteractor nearFarInteractor;
    public Renderer pinchRenderer;

    [Header("Distance Settings")]
    public float fadeStartDistance = 0.3f;
    public float fadeEndDistance = 0.05f;

    [Header("Shader Settings")]
    public string transparencyProperty = "_Transparency";

    private MaterialPropertyBlock propertyBlock;
    private int transparencyID;

    private void Start()
    {
        propertyBlock = new MaterialPropertyBlock();
        transparencyID = Shader.PropertyToID(transparencyProperty);
    }

    private void Update()
    {
        var transparency = 0f;

        if (nearFarInteractor && nearFarInteractor.hasHover)
        {
            var nearestDistance = float.MaxValue;

            foreach (IXRHoverInteractable interactable in nearFarInteractor.interactablesHovered)
            {
                float distance = Vector3.Distance(
                    transform.position,
                    interactable.transform.position
                );
                nearestDistance = Mathf.Min(nearestDistance, distance);
            }

            transparency = Mathf.InverseLerp(fadeStartDistance, fadeEndDistance, nearestDistance);
        }

        pinchRenderer.GetPropertyBlock(propertyBlock);
        propertyBlock.SetFloat(transparencyID, transparency);
        pinchRenderer.SetPropertyBlock(propertyBlock);

        pinchRenderer.GetPropertyBlock(propertyBlock);
        float actualValue = propertyBlock.GetFloat(transparencyID);
        Debug.Log($"Set: {transparency}, Actual: {actualValue}");

        // Also check material value
        float matValue = pinchRenderer.sharedMaterial.GetFloat(transparencyProperty);
        Debug.Log($"Material value: {matValue}");
    }
}