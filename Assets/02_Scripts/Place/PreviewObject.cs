using System.Collections.Generic;
using UnityEngine;

public class PreviewObject : MonoBehaviour
{
    private PlaceableObjectInfo info;
    private TileManager tileManager;
    private List<SpriteRenderer> renderers = new List<SpriteRenderer>();
    // 좌측 하단 그리드 좌표.
    private Vector3Int currentBasePosition;
    private Vector3 currentPreviewPosition;

    private bool isPlaceable = false;

    public void Init(PlaceableObjectInfo info, TileManager tileManager)
    {
        this.info = info;
        this.tileManager = tileManager;

        renderers.AddRange(GetComponentsInChildren<SpriteRenderer>());
    }

    public void UpdatePreview(Vector3 mouseWorldPos)
    {
        Vector3Int basePos = tileManager.GetGridPosition(mouseWorldPos);
        currentBasePosition = basePos;

        Vector3 worldPos = tileManager.GetWorldPosition(basePos);

        // Bottom Center 기준.
        Vector3 offset = new Vector3((info.size.x - 1) / 2f, -0.5f, 0f);

        currentPreviewPosition = worldPos + offset;

        isPlaceable = CheckCanPlace(basePos);
        UpdateColor(isPlaceable);

        transform.position = currentPreviewPosition;
    }

    private bool CheckCanPlace(Vector3Int basePos)
    {
        for (int x = 0; x < info.size.x; x++)
        {
            for (int y = 0; y < info.size.y; y++)
            {
                Vector3Int offset = new Vector3Int(basePos.x + x, basePos.y + y, basePos.z);
                if (!tileManager.CanPlaceAt(offset, info.placeType))
                    return false;
            }
        }
        return true;
    }

    private void UpdateColor(bool canPlace)
    {
        Color color = canPlace ? new Color(0, 1, 0, 0.5f) : new Color(1, 0, 0, 0.5f);
        foreach (var renderer in renderers)
        {
            renderer.color = color;
        }
    }

    public bool IsPlaceable() => isPlaceable;
    public Vector3Int GetBasePosition() => currentBasePosition;
    public Vector3 GetPreviewPosition() => currentPreviewPosition;
}
