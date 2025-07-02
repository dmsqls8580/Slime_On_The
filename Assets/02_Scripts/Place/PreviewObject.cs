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

    public void Init(PlaceableObjectInfo _info, TileManager _tileManager)
    {
        info = _info;
        tileManager = _tileManager;

        renderers.AddRange(GetComponentsInChildren<SpriteRenderer>());
    }

    public void UpdatePreview(Vector3 _mouseWorldPos)
    {
        Vector3Int basePos = tileManager.GetGridPosition(_mouseWorldPos);
        currentBasePosition = basePos;

        Vector3 worldPos = tileManager.GetWorldPosition(basePos);

        Vector3 offset;

        bool isRotated = Mathf.RoundToInt(transform.rotation.eulerAngles.y) % 180 != 0;

        if (!isRotated)
        {
            isPlaceable = CheckCanPlace(info.size.x, info.size.y, basePos);
            offset = new Vector3((info.size.x - 1) / 2f, -0.5f, 0f);
        }
        else
        {
            isPlaceable = CheckCanPlace(info.size.y, info.size.x, basePos);
            offset = new Vector3((info.size.y - 1) / 2f, -0.5f, 0f);
        }

        UpdateColor(isPlaceable);

        currentPreviewPosition = worldPos + offset;

        transform.position = currentPreviewPosition;
    }

    private bool CheckCanPlace(int _x, int _y, Vector3Int _basePos)
    {
        for (int x = 0; x < _x; x++)
        {
            for (int y = 0; y < _y; y++)
            {
                Vector3Int offset = new Vector3Int(_basePos.x + x, _basePos.y + y, _basePos.z);
                if (!tileManager.CanPlaceAt(offset, info.placeType))
                    return false;
            }
        }
        return true;
    }

    private void UpdateColor(bool _canPlace)
    {
        Color color = _canPlace ? new Color(0, 1, 0, 0.5f) : new Color(1, 0, 0, 0.5f);
        foreach (var renderer in renderers)
        {
            renderer.color = color;
        }
    }

    public void SetRotation()
    {
        transform.rotation *= Quaternion.Euler(0, 90, 0);
    }

    public bool IsPlaceable() => isPlaceable;
    public Vector3Int GetBasePosition() => currentBasePosition;
    public Vector3 GetPreviewPosition() => currentPreviewPosition;
    public Quaternion GetPreviewRotation() => transform.rotation;
}
