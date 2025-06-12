using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class Hand : MonoBehaviour
{
    [SerializeField] private List<Transform> leftHandPokeTransforms;
    [SerializeField] private List<Transform> rightHandPokeTransforms;

    public XRPokeInteractor pokeInteractorPrefab;

    private void Start()
    {
        foreach (Transform pokeTransform in leftHandPokeTransforms)
        {
            XRPokeInteractor interactor = Instantiate(pokeInteractorPrefab);
            interactor.attachTransform = pokeTransform;
        }

        foreach (Transform pokeTransform in rightHandPokeTransforms)
        {
            XRPokeInteractor interactor = Instantiate(pokeInteractorPrefab);
            interactor.attachTransform = pokeTransform;
        }
    }
}