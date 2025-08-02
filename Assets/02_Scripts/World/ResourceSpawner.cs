using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class ResourceSpawner : MonoBehaviour
{
    [System.Serializable]
    public class ResourceGroup
    {
        public GameObject[] prefabs;

        [Tooltip("자원의 희귀도")] // 낮을수록 희귀
        [Range(0f, 1f)] public float weight = 1f; // 0.1 = 거의 안 나옴
    }

    [Header("바이옴 타일맵")]
    public Tilemap springTilemap;
    public Tilemap summerTilemap;
    public Tilemap fallTilemap;
    public Tilemap winterTilemap;
    public Tilemap oreTilemap;

    [Header("충돌 체크용 타일맵")]
    public Tilemap colliderTilemap;

    [Header("자원 그룹 (프리팹 리스트)")]
    public ResourceGroup springTrees;
    public ResourceGroup summerTrees;
    public ResourceGroup fallTrees;
    public ResourceGroup winterTrees;
    public ResourceGroup coal;
    public ResourceGroup copper;
    public ResourceGroup gold;
    public ResourceGroup iron;
    public ResourceGroup stone;

    [Header("군집 생성 설정")]
    [Tooltip("각 바이옴에서 생성할 군집의 수")]
    public int clustersPerBiome = 10;

    [Tooltip("각 군집에 포함될 자원의 수")]
    public int resourcesPerCluster = 5;

    [Tooltip("군집이 퍼질 수 있는 반경 (셀 단위)")]
    public int clusterRadius = 3;

    [Tooltip("각 셀을 군집 중심으로 선택할 확률 (낮으면 군집이 적게 생김)")]
    [Range(0f, 1f)] public float clusterSpawnProbability = 0.2f;

    [Header("충돌 검사 설정")]
    public float colliderCheckRadius = 0.3f;
    public LayerMask obstacleLayer; // 충돌 체크 대상

    // 내부 저장용 딕셔너리
    private Dictionary<string, Tilemap> biomeTilemaps = new();
    private Dictionary<string, List<ResourceGroup>> biomeResources = new();

    // 자원 배치된 위치 추적용
    private HashSet<Vector3Int> usedPositions = new();

    private void Start()
    {
        SetupDictionaries();    // 타일맵 및 자원 매핑
        SpawnResources();       // 자원 실제 생성
        // NavMesh2DManager.Instance.UpdateThisNavMesh();
    }

    // 유효한 바이옴 타일맵만 Dictionary에 등록
    void SetupDictionaries()
    {
        if (springTilemap != null)
        {
            biomeTilemaps.Add("Spring", springTilemap);
            biomeResources.Add("Spring", new List<ResourceGroup> { springTrees });
        }

        if (summerTilemap != null)
        {
            biomeTilemaps.Add("Summer", summerTilemap);
            biomeResources.Add("Summer", new List<ResourceGroup> { summerTrees });
        }

        if (fallTilemap != null)
        {
            biomeTilemaps.Add("Fall", fallTilemap);
            biomeResources.Add("Fall", new List<ResourceGroup> { fallTrees });
        }

        if (winterTilemap != null)
        {
            biomeTilemaps.Add("Winter", winterTilemap);
            biomeResources.Add("Winter", new List<ResourceGroup> { winterTrees });
        }

        if (oreTilemap != null)
        {
            biomeTilemaps.Add("Ore", oreTilemap);
            biomeResources.Add("Ore", new List<ResourceGroup> { coal, copper, gold, iron, stone });
        }
    }

    // 각 바이옴 타일맵에서 군집 중심을 선택하고 자원을 생성
    void SpawnResources()
    {
        foreach (var kvp in biomeTilemaps)
        {
            string biome = kvp.Key;
            Tilemap tilemap = kvp.Value;

            List<Vector3Int> availablePositions = new();

            // 실제 타일이 존재하는 셀 위치만 수집
            BoundsInt bounds = tilemap.cellBounds;
            foreach (var pos in bounds.allPositionsWithin)
            {
                if (tilemap.HasTile(pos))
                    availablePositions.Add(pos);
            }

            if (availablePositions.Count == 0)
                continue;

            int clusterCount = 0;
            int tries = 0;

            // 일정 수의 군집이 생성될 때까지 시도
            while (clusterCount < clustersPerBiome && tries < availablePositions.Count)
            {
                Vector3Int center = availablePositions[Random.Range(0, availablePositions.Count)];

                if (Random.value < clusterSpawnProbability)
                {
                    PlaceCluster(center, biomeResources[biome], tilemap);
                    clusterCount++;
                }

                tries++;
            }
        }
    }

    // 하나의 군집 내 자원들을 퍼트려 배치
    void PlaceCluster(Vector3Int center, List<ResourceGroup> resourceGroups, Tilemap tilemap)
    {
        for (int i = 0; i < resourcesPerCluster; i++)
        {
            Vector3Int offset = new Vector3Int(
                Random.Range(-clusterRadius, clusterRadius + 1),
                Random.Range(-clusterRadius, clusterRadius + 1),
                0
            );

            Vector3Int spawnPos = center + offset;

            if (!tilemap.HasTile(spawnPos) || usedPositions.Contains(spawnPos))
                continue;

            // 타일맵에 충돌 타일이 있는지 검사
            if (colliderTilemap != null && colliderTilemap.HasTile(spawnPos))
                continue;

            Vector3 worldPos = tilemap.CellToWorld(spawnPos) + new Vector3(0.5f, 0.5f, 0); // 셀 중앙 정렬

            // 다른 오브젝트와 충돌 검사
            if (Physics2D.OverlapCircle(worldPos, colliderCheckRadius, obstacleLayer)) // Collider 충돌 검사
                continue;

            ResourceGroup group = GetWeightedRandomResource(resourceGroups);
            GameObject prefab = group.prefabs[Random.Range(0, group.prefabs.Length)];

            Instantiate(prefab, worldPos, Quaternion.identity, this.transform);
            usedPositions.Add(spawnPos); // 위치 기록
        }
    }

    // 자원 그룹 중 희귀도를 기반으로 랜덤 선택
    ResourceGroup GetWeightedRandomResource(List<ResourceGroup> groups)
    {
        float totalWeight = 0f;
        foreach (var group in groups)
            totalWeight += group.weight;

        float randomValue = Random.Range(0f, totalWeight);
        float cumulative = 0f;

        foreach (var group in groups)
        {
            cumulative += group.weight;
            if (randomValue <= cumulative)
                return group;
        }

        return groups[^1]; // fallback
    }
}
