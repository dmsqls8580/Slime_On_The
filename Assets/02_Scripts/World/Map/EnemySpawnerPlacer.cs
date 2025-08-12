using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public enum SpawnDensity
{
    None,     // 생성 안 함
    Low,      // 적게
    Medium,   // 보통
    High      // 많이
}

[System.Serializable]
public class SpawnerPrefabWithDensity
{
    public GameObject prefab;
    public SpawnDensity density;
    [Tooltip("이 스포너 간 최소 거리 (지정하지 않으면 기본값 사용)")]
    public float minDistance = -1f;
}

[System.Serializable]
public class BiomeSpawnerSet
{
    public BiomeType biome;
    public List<SpawnerPrefabWithDensity> spawnerEntries;
}


public class EnemySpawnerPlacer : MonoBehaviour
{
    public List<BiomeSpawnerSet> biomeSpawners;
    public Tilemap groundTilemap;
    public Transform spawnerParent;

    public float minSpawnerDistance = 30;
    public float safeDistanceFromPlayer = 80;

    public int seed = 12345;

    private List<Vector3> placedSpawnerPositions = new();

    public void Place(Dictionary<Vector3Int, int> tileToRegion, Dictionary<int, BiomeType> regionBiomes)
    {
        var prng = new System.Random(seed);
        UnityEngine.Random.InitState(seed);
        placedSpawnerPositions.Clear();

        foreach (var kvp in tileToRegion)
        {
            Vector3Int tilePos = kvp.Key;
            int regionId = kvp.Value;

            if (!regionBiomes.TryGetValue(regionId, out BiomeType biome)) continue;

            var biomeData = biomeSpawners.FirstOrDefault(b => b.biome == biome);
            if (biomeData == null || biomeData.spawnerEntries.Count == 0) continue;

            foreach (var entry in biomeData.spawnerEntries)
            {
                float spawnChance = GetSpawnChanceFromDensity(entry.density);
                if (prng.NextDouble() > spawnChance) continue;

                GameObject prefab = entry.prefab;
                Vector3 centerWorldPos = groundTilemap.GetCellCenterWorld(tilePos);

                // (0,0,0) 위치 기준으로 플레이어와 거리 확인
                if (Vector3.Distance(centerWorldPos, Vector3.zero) < safeDistanceFromPlayer)
                    continue;

                // 개별 스포너의 minDistance 사용 (없으면 기본값)
                float thisMinDistance = entry.minDistance > 0 ? entry.minDistance : minSpawnerDistance;

                // 기존 스포너들과 거리 확인
                if (placedSpawnerPositions.Any(p => Vector3.Distance(p, centerWorldPos) < thisMinDistance))
                    continue;

                GameObject instance = Instantiate(prefab, centerWorldPos, Quaternion.identity);

                var collider = instance.GetComponent<CircleCollider2D>();
                if (collider == null)
                {
                    Destroy(instance);
                    continue;
                }

                float radius = collider.radius * instance.transform.lossyScale.x;
                if (!IsColliderInsideRegion(centerWorldPos, radius, regionId, tileToRegion))
                {
                    Destroy(instance);
                    continue;
                }

                instance.transform.SetParent(spawnerParent);
                placedSpawnerPositions.Add(centerWorldPos);
            }
        }
    }

    private float GetSpawnChanceFromDensity(SpawnDensity density)
    {
        return density switch
        {
            SpawnDensity.None => 0f,
            SpawnDensity.Low => 0.02f,
            SpawnDensity.Medium => 0.05f,
            SpawnDensity.High => 0.15f,
            _ => 0.05f
        };
    }

    private bool IsColliderInsideRegion(Vector3 center, float radius, int regionId, Dictionary<Vector3Int, int> tileToRegion)
    {
        const int steps = 16;
        for (int i = 0; i < steps; i++)
        {
            float angle = Mathf.Deg2Rad * (i * 360f / steps);
            Vector3 offset = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * radius;
            Vector3Int checkCell = groundTilemap.WorldToCell(center + offset);

            if (!tileToRegion.TryGetValue(checkCell, out int checkRegion) || checkRegion != regionId)
                return false;
        }
        return true;
    }
}