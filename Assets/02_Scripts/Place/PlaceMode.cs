using UnityEngine;

public class PlaceMode : MonoBehaviour
{
    [SerializeField] private ObjectPreview previewPrefab;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private TileManager tileManager;

    private ObjectPreview preview;
    private GameObject objectToPlacePrefab;
    private PlaceType currentPlaceType;

    Vector3Int lastGridPos;
    bool lastCanPlace;

    private void Awake()
    {
        preview = Instantiate(previewPrefab);
        preview.Hide();
    }

    public void SetBuildInfo(GameObject prefab, PlaceType type)
    {
        objectToPlacePrefab = prefab;
        currentPlaceType = type;
    }

    private void OnEnable()
    {
        preview.Show();
    }

    private void OnDisable()
    {
        preview.Hide();
    }

    private void Update()
    {
        Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        Vector3Int gridPos = tileManager.GetGridPosition(mouseWorldPos);
        bool canPlace = tileManager.CanPlaceAt(gridPos, currentPlaceType);

        // 상태가 바뀌었을 때만 업데이트
        if (canPlace != lastCanPlace || gridPos != lastGridPos)
        {
            if (canPlace)
            {
                preview.SetPosition(gridPos);
                preview.Show();
            }
            else
            {
                preview.Hide();
            }

            lastGridPos = gridPos;
            lastCanPlace = canPlace;
        }

        if (canPlace && Input.GetMouseButtonDown(0))
        {
            Place(gridPos);
            gameObject.SetActive(false);
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            gameObject.SetActive(false);
        }
    }

    private void Place(Vector3Int gridPos)
    {
        Vector3 worldPos = tileManager.GetWorldPosition(gridPos);
        GameObject obj = Instantiate(objectToPlacePrefab, worldPos, Quaternion.identity);
        tileManager.SetPlacedObject(gridPos, obj);
    }
}
