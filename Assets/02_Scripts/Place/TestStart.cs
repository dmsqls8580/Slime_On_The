using UnityEngine;

public class TestStart : MonoBehaviour
{
    [SerializeField] private ExampleIPlaceable testItem;
    [SerializeField] private PlaceMode placeMode;

    private void Start()
    {
        placeMode.SetActiveTruePlaceMode(testItem.PlaceableInfo);
    }
}
