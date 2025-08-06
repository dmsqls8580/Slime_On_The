using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PlaceMode : MonoBehaviour
{
    [Header("감지할 레이어")]
    [SerializeField] private LayerMask layerMask;

    [Header("Clamp Area")]
    [SerializeField] private ClampArea clampArea;
    [SerializeField] private Transform playerTransform;

    [Header("프리뷰 타일 프리팹")]
    [SerializeField] private GameObject previewTilePrefab;

    [Header("타일맵")]
    [SerializeField] private Tilemap tilemap;

    [Header("스크립트 참조")]
    [SerializeField] private PlacedObjectManager placedObjectManager;

    private GameObject objectPrefab;
    private GameObject prefabInstance;
    private ItemSO itemSO;
    private int quickSlotIndex;
    
    private Vector2Int size;

    private List<PreviewTile> previewTiles = new();

    private bool canPlace = false;
    private Vector3 mouseWorldPos = Vector3.zero;
    private Vector3Int baseCell = Vector3Int.zero;

    private InventoryManager inventoryManager;

    private void Awake()
    {
        inventoryManager = InventoryManager.Instance;
        if (tilemap == null)
        {
            tilemap = FindObjectsOfType<Tilemap>().FirstOrDefault(t => t.gameObject.name == "Ground");
        }
        if (playerTransform == null)
        {
            playerTransform = FindObjectsOfType<Transform>().FirstOrDefault(t => t.gameObject.name == "Player");
        }
        gameObject.SetActive(false);
    }

    private void Update()
    {
        clampArea.UpdateCenter(playerTransform.position);
        mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        baseCell = clampArea.GetClampedCell(mouseWorldPos);
        UpdatePreview();
    }

    private void UpdatePreview()
    {
        Vector3Int bottomLeftCell = baseCell - new Vector3Int(Mathf.FloorToInt((size.x - 1) / 2f), 0, 0);
        if (size.x % 2 == 0 && mouseWorldPos.x < tilemap.GetCellCenterWorld(baseCell).x)
            bottomLeftCell -= new Vector3Int(Mathf.FloorToInt(tilemap.cellSize.x), 0, 0);

        float previewOffsetX = (size.x - 1) * tilemap.cellSize.x / 2f;
        Vector3 previewWorldPos = tilemap.GetCellCenterWorld(bottomLeftCell) + new Vector3(previewOffsetX, 0f, 0f);
        prefabInstance.transform.position = previewWorldPos;

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

    public void Place()
    {
        if (!canPlace) return;
        GameObject placedObject = Instantiate(objectPrefab, prefabInstance.transform.position, Quaternion.identity);
        SetObject(placedObject);
        inventoryManager.RemoveItem(quickSlotIndex, 1);
        SetActiveFalsePlaceMode();
    }

    private void SetObject(GameObject _placedObject)
    {
        Collider2D collider2D = _placedObject.GetComponent<Collider2D>();
        if (collider2D != null)
            collider2D.enabled = true;

        SpriteRenderer spriteRenderer = _placedObject.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            Color color = spriteRenderer.color;
            color.a = 1f;
            spriteRenderer.color = color;
        }

        ModifierObject modifier = _placedObject.GetComponent<ModifierObject>();
        if (modifier != null)
            modifier.enabled = true;

        if (_placedObject.TryGetComponent(out PlacedObject placedObjectComponent))
        {
            placedObjectManager.AddPlacedObject(placedObjectComponent);
        }
    }

    public void SetActiveTruePlaceMode(ItemSO _itemSO, int _quickSlotIndex)
    {
        gameObject.SetActive(true);
        Initialize(_itemSO);
        quickSlotIndex = _quickSlotIndex;
    }

    private void Initialize(ItemSO _itemSO)
    {
        itemSO = _itemSO;
        objectPrefab = itemSO.placeableData.objectPrefab;
        mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        prefabInstance = Instantiate(objectPrefab, mouseWorldPos, Quaternion.identity);
        size = itemSO.placeableData.size;
        clampArea.Initialize(tilemap);

        for (int i = 0; i < size.x * size.y; i++)
        {
            GameObject tileInstance = Instantiate(previewTilePrefab, mouseWorldPos, Quaternion.identity);
            previewTiles.Add(tileInstance.GetComponent<PreviewTile>());
        }
    }

    public void SetActiveFalsePlaceMode()
    {
        if (prefabInstance != null)
            Destroy(prefabInstance);
        foreach (PreviewTile tile in previewTiles)
            Destroy(tile.gameObject);
        previewTiles.Clear();
        canPlace = false;
        gameObject.SetActive(false);
    }
}
