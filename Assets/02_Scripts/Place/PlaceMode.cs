using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class PlaceMode : MonoBehaviour
{
    [SerializeField] private LayerMask layerMask;
    [SerializeField] private GameObject previewTilePrefab;
    [SerializeField] private ClampArea clampArea;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private Tilemap tilemap;

    private GameObject normalPrefab;
    private GameObject previewInstance;
    private Vector2Int size;

    private List<PreviewTile> previewTiles = new List<PreviewTile>();

    private bool canPlace = false;
    private Vector3 mouseWorldPos = Vector3.zero;
    private Vector3Int baseCell = Vector3Int.zero;

    private void Update()
    {
        clampArea.UpdateCenter(playerTransform.position);
        mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        baseCell = clampArea.GetClampedCell(mouseWorldPos);
        UpdatePreview();
        if (Input.GetKeyDown(KeyCode.B) && canPlace)
            Place();
    }

    private void UpdatePreview()
    {
        Vector3Int bottomLeftCell = baseCell - new Vector3Int(Mathf.FloorToInt((size.x - 1) / 2f), 0, 0);
        if (size.x % 2 == 0 && mouseWorldPos.x < tilemap.GetCellCenterWorld(baseCell).x)
            bottomLeftCell -= new Vector3Int(Mathf.FloorToInt(tilemap.cellSize.x), 0, 0);

        float previewOffsetX = (size.x - 1) * tilemap.cellSize.x / 2f;
        Vector3 previewWorldPos = tilemap.GetCellCenterWorld(bottomLeftCell) + new Vector3(previewOffsetX, -0.5f, 0);
        previewInstance.transform.position = previewWorldPos;

        canPlace = true;

        for (int i = 0; i < previewTiles.Count; i++)
        {
            int x = i % size.x;
            int y = i / size.x;
            Vector3Int tileCell = new Vector3Int(bottomLeftCell.x + x, bottomLeftCell.y + y, 0);

            previewTiles[i].transform.position = tilemap.GetCellCenterWorld(tileCell);

            bool isTilePlaceable = CheckPlacable(previewTiles[i].transform.position);
            previewTiles[i].SetValid(isTilePlaceable);
            if (!isTilePlaceable)
                canPlace = false;
        }
    }

    private bool CheckPlacable(Vector3 _worldPos)
    {
        Collider2D[] colliders = Physics2D.OverlapBoxAll(_worldPos, tilemap.cellSize * 0.9f, 0f, layerMask);
        return colliders.Length == 0;
    }

    private void Place()
    {
        Instantiate(normalPrefab, previewInstance.transform.position, Quaternion.identity);
    }

    public void SetActiveTruePlaceMode(PlaceableInfo _placeableInfo)
    {
        Initialize(_placeableInfo);
        gameObject.SetActive(true);
    }

    private void Initialize(PlaceableInfo _placeableInfo)
    {
        normalPrefab = _placeableInfo.normalPrefab;
        previewInstance = Instantiate(_placeableInfo.previewPrefab, transform);
        size = _placeableInfo.size;
        clampArea.Initialize(tilemap);

        for (int i = 0; i < size.x * size.y; i++)
        {
            GameObject tileInstance = Instantiate(previewTilePrefab, transform);
            previewTiles.Add(tileInstance.GetComponent<PreviewTile>());
        }
    }

    public void SetActiveFalsePlaceMode()
    {
        if (previewInstance != null)
            Destroy(previewInstance);
        foreach (PreviewTile tile in previewTiles)
            Destroy(tile.gameObject);
        previewTiles.Clear();
        gameObject.SetActive(false);
    }
}
