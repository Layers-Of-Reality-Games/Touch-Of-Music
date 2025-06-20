using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class Hand : MonoBehaviour
{
    [SerializeField] private List<Transform> fingerPokeTransforms;

    public XRPokeInteractor pokeInteractorPrefab;

    private void Start()
    {
        foreach (Transform pokeTransform in fingerPokeTransforms)
        {
            XRPokeInteractor interactor = Instantiate(pokeInteractorPrefab);
            interactor.attachTransform = pokeTransform;
        }
    }
}