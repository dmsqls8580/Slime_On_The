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
    }

    private void Start()
    {
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
            ExitPlaceMode();
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            previewObject.SetRotation();
        }

        // ESC로 취소
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ExitPlaceMode();
        }
    }

    public void StartPlacement(PlaceableObjectInfo _info)
    {
        currentInfo = _info;
        previewInstance = Instantiate(_info.previewPrefab);
        previewObject = previewInstance.GetComponent<PreviewObject>();
        previewObject.Init(_info, tileManager);

        isPlacing = true;
        gameObject.SetActive(true);
    }

    private void PlaceObject()
    {
        Vector3 previewPos = previewObject.GetPreviewPosition();
        Quaternion previewRot = previewObject.GetPreviewRotation();

        GameObject installedPrefab = Instantiate(currentInfo.installablePrefab, previewPos, previewRot);

        // 90도 또는 270도일 때는 회전된 상태로 판단.
        bool isRotated = Mathf.RoundToInt(previewRot.eulerAngles.y) % 180 != 0;

        if (isRotated)
        {
            CheckCanPlace(currentInfo.size.y, currentInfo.size.x, installedPrefab);
        }
        else
        {
            CheckCanPlace(currentInfo.size.x, currentInfo.size.y, installedPrefab);
        }
    }

    private void CheckCanPlace(int _x, int _y, GameObject _installedPrefab)
    {
        Vector3Int basePos = previewObject.GetBasePosition();

        for (int x = 0; x < _x; x++)
        {
            for (int y = 0; y < _y; y++)
            {
                Vector3Int tilePos = new Vector3Int(basePos.x + x, basePos.y + y, basePos.z);
                tileManager.SetPlacedObject(tilePos, _installedPrefab);
            }
        }
    }

    private void ExitPlaceMode()
    {
        if (previewInstance != null)
            Destroy(previewInstance);

        isPlacing = false;
        gameObject.SetActive(false);
    }
}
