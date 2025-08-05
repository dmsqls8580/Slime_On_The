using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

[System.Serializable]
public class WorldObjectData
{
    public string prefabName;
    public Vector3 position;
    // public string biomeType;
}

[System.Serializable]
public class SetPiece
{
    public string name;
    public List<GameObject> prefabs;
    public float radius = 5f;
    public int count = 10;
    public float jitter = 1.5f;
}

[System.Serializable]
public class SetPieceData
{
    public BiomeType biome;
    public List<SetPiece> setPieces;
}

public class SetPiecePlacer : MonoBehaviour
{
    public List<WorldObjectData> PlacedResources { get; private set; } = new();

    [Header("세트 피스 설정")]
    public List<SetPieceData> biomeSetPieces;

    [Header("타일맵")]
    public Tilemap groundTilemap;
    public Tilemap roadTilemap;

    [Header("부모 오브젝트")]
    public Transform resourceParent;

    [Header("기타 설정")]
    public int setPieceAttempts = 300;
    public int seed = 12345;

    private System.Random prng;

    public void Place(Dictionary<Vector3Int, int> tileToRegion, Dictionary<int, BiomeType> regionBiomes)
    {
        prng = new System.Random(seed);
        UnityEngine.Random.InitState(seed);

        List<Vector3Int> validTiles = tileToRegion.Keys.OrderBy(_ => prng.Next()).ToList();
        HashSet<Vector3> placedPositions = new();

        for (int i = 0; i < setPieceAttempts; i++)
        {
            Vector3Int center = validTiles[prng.Next(validTiles.Count)];
            if (!tileToRegion.ContainsKey(center)) continue;
            if (roadTilemap.HasTile(center)) continue;

            int regionId = tileToRegion[center];
            BiomeType biome = regionBiomes[regionId];

            var biomeData = biomeSetPieces.FirstOrDefault(b => b.biome == biome);
            if (biomeData == null || biomeData.setPieces.Count == 0) continue;

            SetPiece selected = biomeData.setPieces[prng.Next(biomeData.setPieces.Count)];

            for (int j = 0; j < selected.count; j++)
            {
                Vector2 offset = Random.insideUnitCircle * selected.radius;
                offset += new Vector2(
                    Random.Range(-selected.jitter, selected.jitter),
                    Random.Range(-selected.jitter, selected.jitter)
                );

                Vector3 spawnPos = groundTilemap.CellToWorld(center) + (Vector3)offset;
                Vector3Int spawnCell = groundTilemap.WorldToCell(spawnPos);

                // 1. 바이옴 범위 외 타일인지 확인
                if (!tileToRegion.TryGetValue(spawnCell, out int spawnRegionId)) continue;
                if (!regionBiomes.TryGetValue(spawnRegionId, out BiomeType spawnBiome)) continue;
                if (spawnBiome != biome) continue;

                // 길 타일 위인지 확인
                if (roadTilemap.HasTile(spawnCell)) continue;

                // 2. 거리 및 충돌 검사
                if (placedPositions.Any(p => Vector3.Distance(p, spawnPos) < 1f)) continue;
                var colliders = Physics2D.OverlapCircleAll(spawnPos, 0.5f);

                // 플레이어를 제외한 충돌체가 있는지 검사
                bool hasValidCollision = colliders.Any(c =>
                    !(c.CompareTag("Player"))
                );
                if (hasValidCollision) continue;

                // 3. 프리팹 생성
                GameObject prefab = selected.prefabs[prng.Next(selected.prefabs.Count)];
                GameObject instance = Instantiate(prefab, spawnPos, Quaternion.identity, resourceParent);
                placedPositions.Add(spawnPos);

                // 데이터 저장
                PlacedResources.Add(new WorldObjectData
                {
                    prefabName = prefab.name,
                    position = spawnPos,
                    // biomeType = biome.ToString()
                });
            }
        }
    }
}

