using UnityEngine;

public class ExampleIPlaceable : MonoBehaviour, IPlaceable
{
    [SerializeField] private PlaceableInfo placeableInfo;
    public PlaceableInfo PlaceableInfo => placeableInfo;
}
