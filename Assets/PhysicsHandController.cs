using UnityEngine;

public class PhysicsHandController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform indexTip;

    private Transform testTransform;

    private void Start()
    {
        testTransform = indexTip;
    }

    private void Update()
    {
        indexTip.position = testTransform.position;
    }
}