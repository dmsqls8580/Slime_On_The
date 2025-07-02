using UnityEngine;

public class PlaceMode : MonoBehaviour
{
    [SerializeField] private TileManager tileManager;

    private PlaceableObjectInfo currentInfo;
    private GameObject previewInstance;
    private PreviewObject previewObject;

    private Camera mainCamera;

    private bool isPlacing = false;

    private void Awake()
    {
        mainCamera = Camera.main;
        gameObject.SetActive(false);
    }

    private void Update()
    {
        if (!isPlacing) return;

        Vector3 mouseWorld = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = 0;

        previewObject.UpdatePreview(mouseWorld);

        if (Input.GetMouseButtonDown(0) && previewObject.IsPlaceable())
        {
            PlaceObject();
            ExitPlacementMode();
        }

        // ESC∑Œ √Îº“
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ExitPlacementMode();
        }
    }

    public void StartPlacement(PlaceableObjectInfo info)
    {
        currentInfo = info;
        previewInstance = Instantiate(info.previewPrefab);
        previewObject = previewInstance.GetComponent<PreviewObject>();
        previewObject.Init(info, tileManager);

        isPlacing = true;
        gameObject.SetActive(true);
    }

    private void PlaceObject()
    {
        Vector3Int basePos = previewObject.GetBasePosition();
        Vector3 previewPos = previewObject.GetPreviewPosition();

        GameObject placed = Instantiate(currentInfo.installablePrefab, previewPos, Quaternion.identity);

        for (int x = 0; x < currentInfo.size.x; x++)
        {
            for (int y = 0; y < currentInfo.size.y; y++)
            {
                Vector3Int tilePos = new Vector3Int(basePos.x + x, basePos.y + y, basePos.z);
                tileManager.SetPlacedObject(tilePos, placed);
            }
        }
    }

    private void ExitPlacementMode()
    {
        if (previewInstance != null)
            Destroy(previewInstance);

        isPlacing = false;
        gameObject.SetActive(false);
    }
}
