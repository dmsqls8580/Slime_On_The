using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

[System.Serializable]
public class BossSpawnerEntry
{
    public string bossName;
    public BiomeType biome;
    public GameObject prefab;
}

public class BossSpawnerPlacer : MonoBehaviour
{
    [Header("보스 설정")]
    public List<BossSpawnerEntry> bossEntries;

    [Header("타일맵")]
    public Tilemap groundTilemap;
    public Tilemap roadTilemap;

    [Header("부모 오브젝트")]
    public Transform spawnerParent;

    [Header("기타 설정")]
    public int seed = 12345;
    public float minSpacing = 80f;
    public float safeDistanceFromPlayer = 100f;

    private System.Random prng;

    public void Place(Dictionary<Vector3Int, int> tileToRegionMap, Dictionary<int, BiomeType> regionBiomes)
    {
        prng = new System.Random(seed);
        UnityEngine.Random.InitState(seed);

        Dictionary<BiomeType, List<Vector3Int>> biomeTiles = new();
        foreach (var kvp in tileToRegionMap)
        {
            if (!regionBiomes.TryGetValue(kvp.Value, out var biome)) continue;
            if (!biomeTiles.ContainsKey(biome))
                biomeTiles[biome] = new();
            biomeTiles[biome].Add(kvp.Key);
        }

        foreach (var entry in bossEntries)
        {
            if (!biomeTiles.TryGetValue(entry.biome, out var candidateTiles) || candidateTiles.Count == 0) continue;

            bool placed = false;

            for (int tries = 0; tries < 100; tries++)
            {
                Vector3Int randomCell = candidateTiles[prng.Next(candidateTiles.Count)];
                Vector3 worldPos = groundTilemap.GetCellCenterWorld(randomCell);
                worldPos.x = Mathf.Round(worldPos.x);
                worldPos.y = Mathf.Round(worldPos.y);

                // 플레이어 위치(0,0,0)로부터 안전 거리 확보
                if (Vector3.Distance(worldPos, Vector3.zero) < safeDistanceFromPlayer)
                    continue;

                if (!CanPlaceWithoutOverlap(entry.prefab, worldPos)) continue;

                GameObject instance = Instantiate(entry.prefab, worldPos, Quaternion.identity);
                if (spawnerParent != null) instance.transform.SetParent(spawnerParent);

                placed = true;
                break;
            }

            if (!placed)
                Debug.LogWarning($"[BossSpawnerPlacer] {entry.bossName} 배치 실패 (biome: {entry.biome})");
        }
    }

    private bool CanPlaceWithoutOverlap(GameObject prefab, Vector3 worldPos)
    {
        GameObject temp = Instantiate(prefab, worldPos, Quaternion.identity);
        temp.hideFlags = HideFlags.HideAndDontSave;

        var tilemap = temp.GetComponentInChildren<Tilemap>();
        var tileCollider = temp.GetComponentInChildren<TilemapCollider2D>();

        bool overlaps = false;

        if (tilemap != null && tileCollider != null)
        {
            BoundsInt bounds = tilemap.cellBounds;
            foreach (var pos in bounds.allPositionsWithin)
            {
                if (!tilemap.HasTile(pos)) continue;

                Vector3Int roadCheckCell = roadTilemap.WorldToCell(tilemap.CellToWorld(pos) + (worldPos - tilemap.transform.position));
                if (roadTilemap.HasTile(roadCheckCell))
                {
                    overlaps = true;
                    break;
                }
            }
        }

        DestroyImmediate(temp);
        return !overlaps;
    }
}
