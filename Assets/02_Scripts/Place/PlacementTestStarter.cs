using UnityEngine;

public class PlacementTestStarter : MonoBehaviour
{
    [SerializeField] private PlaceMode placeMode;
    [SerializeField] private GameObject installablePrefab;
    [SerializeField] private GameObject previewPrefab;

    public void TestStartPlacement()
    {
        PlaceableObjectInfo testInfo = new PlaceableObjectInfo
        {
            id = "TestBuilding",
            installablePrefab = installablePrefab,
            previewPrefab = previewPrefab,
            size = new Vector2Int(2, 2),
            placeType = PlaceType.Building
        };

        placeMode.StartPlacement(testInfo);
    }
}
