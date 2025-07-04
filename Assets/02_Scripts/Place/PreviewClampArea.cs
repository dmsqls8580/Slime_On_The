using UnityEngine;
using UnityEngine.Tilemaps;

public class PreviewClampArea : MonoBehaviour
{
    private Tilemap tilemap;

    private Vector3Int centerCell;
    private int clampHalfSize = 2; // 5x5의 절반 (중심을 포함해서 좌우 2칸씩)

    public void Initialize(Tilemap _tilemap)
    {
        tilemap = _tilemap;
    }

    // 플레이어 위치 기준으로 중심 셀 위치 설정
    public void UpdateCenter(Vector3 playerWorldPosition)
    {
        centerCell = tilemap.WorldToCell(playerWorldPosition);
    }

    // 마우스 위치를 받아서 클램프된 셀 위치 반환
    public Vector3Int GetClampedCell(Vector3 mouseWorldPosition)
    {
        Vector3Int mouseCell = tilemap.WorldToCell(mouseWorldPosition);

        int clampedX = Mathf.Clamp(mouseCell.x, centerCell.x - clampHalfSize, centerCell.x + clampHalfSize);
        int clampedY = Mathf.Clamp(mouseCell.y, centerCell.y - clampHalfSize, centerCell.y + clampHalfSize);

        return new Vector3Int(clampedX, clampedY, 0);
    }
}
