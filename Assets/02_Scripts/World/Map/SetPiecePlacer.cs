using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

[System.Serializable]
public class SetPiece
{
    public string name;
    public List<GameObject> prefabs;
    public float radius = 5f;
    public int count = 10;
    public float jitter = 1.5f;
    public float minSpacing = 1.5f; // 자원 간 최소 거리
}

[System.Serializable]
public class SetPieceData
{
    public BiomeType biome;
    public List<SetPiece> setPieces;
}

public class SetPiecePlacer : MonoBehaviour
{
    [Header("세트 피스 설정")]
    public List<SetPieceData> biomeSetPieces;

    [Header("타일맵")]
    public Tilemap groundTilemap;
    public Tilemap roadTilemap;

    [Header("기타 설정")]
    public int setPieceAttempts = 300;
    public int seed = 12345;

    [Header("부모 오브젝트 (Hierarchy 정리용)")]
    public GameObject resourcesParent;

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

            if (!tileToRegion.TryGetValue(center, out int regionId)) continue;
            if (!regionBiomes.TryGetValue(regionId, out BiomeType biome)) continue;
            if (roadTilemap.HasTile(center)) continue;

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

                Vector3 worldPos = groundTilemap.CellToWorld(center) + (Vector3)offset;

                // 자원 간 거리 검사
                if (placedPositions.Any(p => Vector3.Distance(p, worldPos) < selected.minSpacing)) continue;

                // 콜라이더 피하기
                if (Physics2D.OverlapCircleAll(worldPos, 0.4f).Length > 0) continue;

                GameObject prefab = selected.prefabs[prng.Next(selected.prefabs.Count)];
                GameObject instance = Instantiate(prefab, worldPos, Quaternion.identity);

                // 인스펙터에서 연결된 부모로 설정
                if (resourcesParent != null)
                    instance.transform.SetParent(resourcesParent.transform);

                placedPositions.Add(worldPos);
            }
        }
    }
}
