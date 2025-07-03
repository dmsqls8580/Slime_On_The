using UnityEngine;
using UnityEngine.Tilemaps;

public class PlaceMode : MonoBehaviour
{
    [SerializeField] private Tilemap tilemap;

    private GameObject normalPrefab;
    private GameObject previewPrefab;
    private GameObject previewPrefabInstance;
    private ObjectPreview objectPreview;

    private void Update()
    {
        objectPreview.UpdatePreview();

        if (Input.GetKeyDown(KeyCode.B) && objectPreview.CanPlace)
        {
            Place();
        }
    }

    private void Place()
    {
        Instantiate(normalPrefab, previewPrefabInstance.transform.position, Quaternion.identity);
    }

    // 아이템 사용 시 호출 필요.
    // UIQuickSlot에 PlaceMode.cs 참조시키고 placeMode.SetActiveTruePlaceMode(아이템정보);.
    public void SetActiveTruePlaceMode(PlaceableInfo _placeableInfo)
    {
        Initialize(_placeableInfo);
        gameObject.SetActive(true);
    }

    private void Initialize(PlaceableInfo _placeableInfo)
    {
        normalPrefab = _placeableInfo.normalPrefab;
        previewPrefab = _placeableInfo.previewPrefab;
        previewPrefabInstance = Instantiate(previewPrefab);
        objectPreview = previewPrefabInstance.GetComponent<ObjectPreview>();
        objectPreview.Initialize(tilemap);
    }

    // 아이템 사용 끝나면 호출 필요.
    public void SetActiveFalsePlaceMode()
    {
        if (previewPrefabInstance != null)
            Destroy(previewPrefabInstance);
        gameObject.SetActive(false);
    }
}
