using UnityEngine;

public class ExampleIPlaceable : MonoBehaviour, IPlaceable
{
    [SerializeField] private PlaceableData placeableData;
    public PlaceableData PlaceableData => placeableData;
}
