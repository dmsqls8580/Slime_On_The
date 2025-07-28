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

    [Header("기본 설정")]
    public int seed = 12345;

    private System.Random prng;

    public void Place(Dictionary<Vector3Int, int> tileToRegionMap, Dictionary<int, BiomeType> regionBiomes)
    {
        prng = new System.Random(seed);
        UnityEngine.Random.InitState(seed);

        // 1. 바이옴별 타일 모으기
        Dictionary<BiomeType, List<Vector3Int>> biomeTiles = new();
        foreach (var kvp in tileToRegionMap)
        {
            int regionId = kvp.Value;
            if (!regionBiomes.TryGetValue(regionId, out var biome)) continue;

            if (!biomeTiles.ContainsKey(biome)) biomeTiles[biome] = new();
            biomeTiles[biome].Add(kvp.Key);
        }

        // 2. 바이옴마다 특수 구조물 배치
        foreach (var structureData in specialStructures)
        {
            if (!biomeTiles.TryGetValue(structureData.biome, out var candidates)) continue;

            int targetCount = prng.Next(structureData.minCount, structureData.maxCount + 1);
            List<Vector3> placed = new();

            for (int i = 0; i < targetCount; i++)
            {
                int tries = 0;
                bool placedSuccessfully = false;

                while (tries++ < 50)
                {
                    Vector3Int cellPos = candidates[prng.Next(candidates.Count)];
                    if (!tileToRegionMap.ContainsKey(cellPos)) continue;

                    int regionId = tileToRegionMap[cellPos];
                    if (!regionBiomes.TryGetValue(regionId, out var biome)) continue;
                    if (biome != structureData.biome) continue;

                    // 길 피하기
                    if (roadTilemap.HasTile(cellPos)) continue;

                    // 월드 좌표
                    Vector3 worldPos = groundTilemap.CellToWorld(cellPos) + new Vector3(0.5f, 0.5f, 0);

                    // 거리 검사
                    if (placed.Any(p => Vector3.Distance(p, worldPos) < structureData.minSpacing)) continue;

                    // 충돌 검사
                    if (Physics2D.OverlapCircle(worldPos, 1f) != null) continue;

                    // 구조물 배치
                    GameObject prefab = structureData.prefabs[prng.Next(structureData.prefabs.Count)];
                    Instantiate(prefab, worldPos, Quaternion.identity);
                    placed.Add(worldPos);
                    placedSuccessfully = true;
                    break;
                }

                if (!placedSuccessfully)
                {
                    Debug.LogWarning($"[{structureData.biome}] 특수 구조물 배치 실패 (충돌 또는 거리 조건 불만족)");
                }
            }
        }
    }
}
