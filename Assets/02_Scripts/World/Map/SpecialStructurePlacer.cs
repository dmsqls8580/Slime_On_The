using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

[System.Serializable]
public class SavedSpecialStructure
{
    public string prefabName;
    public Vector3 position;
}

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

    [Header("부모 오브젝트")]
    public GameObject structureParent;

    [Header("기본 설정")]
    public int seed = 12345;

    public List<SavedSpecialStructure> PlacedStructures { get; private set; } = new();

    private System.Random prng;

    public void Place(Dictionary<Vector3Int, int> tileToRegionMap, Dictionary<int, BiomeType> regionBiomes)
    {
        prng = new System.Random(seed);
        UnityEngine.Random.InitState(seed);

        PlacedStructures.Clear();

        // 1. 바이옴별 타일 수집
        Dictionary<BiomeType, List<Vector3Int>> biomeTiles = new();
        foreach (var kvp in tileToRegionMap)
        {
            int regionId = kvp.Value;
            if (!regionBiomes.TryGetValue(regionId, out var biome)) continue;

            if (!biomeTiles.ContainsKey(biome))
                biomeTiles[biome] = new();

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

                GameObject selectedPrefab = null;
                Vector3 selectedWorldPos = Vector3.zero;

                while (tries++ < 100)
                {
                    Vector3Int cellPos = candidateTiles[prng.Next(candidateTiles.Count)];
                    if (!tileToRegionMap.TryGetValue(cellPos, out int regionId)) continue;
                    if (!regionBiomes.TryGetValue(regionId, out var biome) || biome != structureData.biome) continue;

                    // 위치 보정: 중심 정렬
                    Vector3 worldPos = groundTilemap.CellToWorld(cellPos);
                    worldPos.x = Mathf.Round(worldPos.x);
                    worldPos.y = Mathf.Round(worldPos.y);

                    if (placedPositions.Any(p => Vector3.Distance(p, worldPos) < structureData.minSpacing)) continue;

                    GameObject prefab = structureData.prefabs[prng.Next(structureData.prefabs.Count)];

                    // 충돌 검사
                    if (!CanPlaceWithoutOverlap(prefab, worldPos)) continue;

                    GameObject instance = Instantiate(prefab, worldPos, Quaternion.identity);
                    if (structureParent != null)
                        instance.transform.SetParent(structureParent.transform);

                    selectedPrefab = prefab;
                    selectedWorldPos = worldPos;

                    placedPositions.Add(worldPos);
                    success = true;
                    break;
                }

                if (!success)
                {
                    Logger.Log($"[{structureData.biome}] 구조물 배치 실패");
                }
                else
                {
                    PlacedStructures.Add(new SavedSpecialStructure
                    {
                        prefabName = selectedPrefab.name,
                        position = selectedWorldPos
                    });
                }
            }
        }
    }

    // 구조물 프리팹이 길 타일과 겹치는지 확인
    private bool CanPlaceWithoutOverlap(GameObject prefab, Vector3 worldPos)
    {
        // 임시 인스턴스 생성 (HideFlags로 비표시)
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

                Vector3Int worldCell = roadTilemap.WorldToCell(tilemap.CellToWorld(pos) + (worldPos - tilemap.transform.position));
                if (roadTilemap.HasTile(worldCell))
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
