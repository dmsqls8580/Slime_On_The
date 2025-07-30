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

        // 2. 바이옴마다 구조물 배치
        foreach (var structureData in specialStructures)
        {
            if (!biomeTiles.TryGetValue(structureData.biome, out var candidates)) continue;

            int targetCount = prng.Next(structureData.minCount, structureData.maxCount + 1);
            List<Vector3> placed = new();

            // 중심 타일 계산
            Vector3 center = Vector3.zero;
            foreach (var tile in candidates)
                center += (Vector3)tile;
            center /= candidates.Count;

            // 중심에 가까운 순서로 정렬
            var sortedTiles = candidates.OrderBy(t => Vector3.Distance((Vector3)t, center)).ToList();

            int sortedIndex = 0;

            for (int i = 0; i < targetCount; i++)
            {
                bool placedSuccessfully = false;

                // 중심 가까운 타일 순서대로 최대 50번 시도
                for (int tries = 0; tries < 50 && sortedIndex < sortedTiles.Count; sortedIndex++)
                {
                    Vector3Int cellPos = sortedTiles[sortedIndex];
                    tries++;

                    if (!tileToRegionMap.ContainsKey(cellPos)) continue;
                    if (roadTilemap.HasTile(cellPos)) continue;

                    int regionId = tileToRegionMap[cellPos];
                    if (!regionBiomes.TryGetValue(regionId, out var biome)) continue;
                    if (biome != structureData.biome) continue;

                    Vector3 worldPos = groundTilemap.CellToWorld(cellPos) + new Vector3(0.5f, 0.5f, 0);

                    // 거리/충돌 검사
                    if (placed.Any(p => Vector3.Distance(p, worldPos) < structureData.minSpacing)) continue;
                    if (Physics2D.OverlapCircle(worldPos, 1f) != null) continue;

                    GameObject prefab = structureData.prefabs[prng.Next(structureData.prefabs.Count)];
                    GameObject instance = Instantiate(prefab, worldPos, Quaternion.identity);
                    if (structureParent != null)
                        instance.transform.SetParent(structureParent.transform);

                    placed.Add(worldPos);
                    placedSuccessfully = true;
                    break;
                }

                if (!placedSuccessfully)
                {
                    Debug.LogWarning($"[{structureData.biome}] 구조물 배치 실패 (중심 타일 주변 공간 부족)");
                }
            }
        }
    }
}
