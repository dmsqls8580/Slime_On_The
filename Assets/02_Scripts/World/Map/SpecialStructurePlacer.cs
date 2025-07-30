using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

[System.Serializable]
public class SpecialStructureData
{
    public BiomeType biome;
    public List<GameObject> prefabs;
    public int minCount = 1;
    public int maxCount = 2;
    public float minSpacing = 10f;
}

public class SpecialStructurePlacer : MonoBehaviour
{
    [Header("구조물 설정")]
    public List<SpecialStructureData> specialStructures;

    [Header("타일맵")]
    public Tilemap groundTilemap;
    public Tilemap roadTilemap;

    [Header("부모 오브젝트 (Hierarchy 정리용)")]
    public GameObject structureParent;

    [Header("기본 설정")]
    public int seed = 12345;

    private System.Random prng;

    public void Place(Dictionary<Vector3Int, int> tileToRegionMap, Dictionary<int, BiomeType> regionBiomes)
    {
        prng = new System.Random(seed);
        UnityEngine.Random.InitState(seed);

        // 1. 바이옴별 타일 수집
        Dictionary<BiomeType, List<Vector3Int>> biomeTiles = new();
        foreach (var kvp in tileToRegionMap)
        {
            int regionId = kvp.Value;
            if (!regionBiomes.TryGetValue(regionId, out var biome)) continue;

            if (!biomeTiles.ContainsKey(biome)) biomeTiles[biome] = new();
            biomeTiles[biome].Add(kvp.Key);
        }

        // 2. 바이옴별 구조물 배치
        foreach (var structureData in specialStructures)
        {
            if (!biomeTiles.TryGetValue(structureData.biome, out var candidateTiles)) continue;

            int targetCount = prng.Next(structureData.minCount, structureData.maxCount + 1);
            List<Vector3> placedPositions = new();

            for (int i = 0; i < targetCount; i++)
            {
                int tries = 0;
                bool success = false;

                while (tries++ < 100)
                {
                    Vector3Int cellPos = candidateTiles[prng.Next(candidateTiles.Count)];
                    if (!tileToRegionMap.TryGetValue(cellPos, out int regionId)) continue;
                    if (!regionBiomes.TryGetValue(regionId, out var biome) || biome != structureData.biome) continue;

                    Vector3 worldPos = groundTilemap.CellToWorld(cellPos) + new Vector3(0.5f, 0.5f, 0);
                    if (placedPositions.Any(p => Vector3.Distance(p, worldPos) < structureData.minSpacing)) continue;

                    GameObject prefab = structureData.prefabs[prng.Next(structureData.prefabs.Count)];

                    // 바운딩 박스 검사
                    if (!CanPlaceWithoutOverlap(prefab, worldPos)) continue;

                    GameObject instance = Instantiate(prefab, worldPos, Quaternion.identity);
                    if (structureParent != null)
                        instance.transform.SetParent(structureParent.transform);

                    placedPositions.Add(worldPos);
                    success = true;
                    break;
                }

                if (!success)
                {
                    Debug.LogWarning($"[{structureData.biome}] 구조물 배치 실패 (충돌 또는 거리 조건 불만족)");
                }
            }
        }
    }

    /// <summary>
    /// 프리팹이 월드 위치 기준으로 길 타일과 겹치지 않는지 확인
    /// </summary>
    private bool CanPlaceWithoutOverlap(GameObject prefab, Vector3 worldPos)
    {
        if (prefab.TryGetComponent<Collider2D>(out var collider))
        {
            Vector3 size = collider.bounds.size;
            Bounds bounds = new(worldPos, size);

            // 길 타일 체크
            foreach (Vector3Int cell in GetOverlappingCells(bounds))
            {
                if (roadTilemap.HasTile(cell)) return false;
            }

            // 일반 충돌 체크
            if (Physics2D.OverlapBox(worldPos, size, 0f) != null)
                return false;
        }

        return true;
    }

    /// <summary>
    /// 바운드 안의 셀 좌표를 계산
    /// </summary>
    private IEnumerable<Vector3Int> GetOverlappingCells(Bounds bounds)
    {
        int minX = Mathf.FloorToInt(bounds.min.x);
        int maxX = Mathf.CeilToInt(bounds.max.x);
        int minY = Mathf.FloorToInt(bounds.min.y);
        int maxY = Mathf.CeilToInt(bounds.max.y);

        for (int x = minX; x <= maxX; x++)
        {
            for (int y = minY; y <= maxY; y++)
            {
                yield return groundTilemap.WorldToCell(new Vector3(x, y, 0));
            }
        }
    }
}
