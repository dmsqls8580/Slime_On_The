using UnityEngine;

public class PlaceModeStartButton : MonoBehaviour
{
    [SerializeField] private PlaceableObjectInfo info;
    [SerializeField] private PlaceMode placeMode;

    public void OnClickPlace()
    {
        placeMode.StartPlacement(info);
    }
}
