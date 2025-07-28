using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class WaterRingApplier : MonoBehaviour
{
    [Header("설정")]
    public Tilemap groundTilemap;
    public TileBase waterTile;
    public float waterRingRadius = 280f;

    /// <summary>
    /// 유효한 육지 타일 중, 중심에서 일정 거리 이상 떨어진 타일은 waterTile로 덮어씌움
    /// </summary>
    public void Apply(List<Vector3Int> validTiles, Dictionary<Vector3Int, int> tileToRegion)
    {
        foreach (var pos in validTiles)
        {
            float dist = Vector3.Distance((Vector3)pos, Vector3.zero);
            if (!tileToRegion.ContainsKey(pos) || dist > waterRingRadius)
            {
                groundTilemap.SetTile(pos, waterTile);
            }
        }
    }
}
