using System.Collections.Generic;
using NavMeshPlus.Components;
using UnityEngine;
using UnityEngine.AI;

public class NavMeshChunkManager : SceneOnlySingleton<NavMeshChunkManager>
{
    [Header("설정")]
    public float chunkSize = 10f;
    public Vector2 worldMin = new Vector2(-300, -300);
    public Vector2 worldMax = new Vector2(300, 300);
    
    public GameObject navMeshChunkPrefab;

    private Dictionary<Vector2Int, NavMeshChunk> chunks = new Dictionary<Vector2Int, NavMeshChunk>();
    
    private void Awake()
    {
        InitializeChunks();
    }
    
    /// <summary>
    /// worldMin부터 worldMax사이 모든 영역을 chunkSize로 나눠 Chunk 생성
    /// </summary>
    public void InitializeChunks()
    {
        int minX = Mathf.FloorToInt(worldMin.x / chunkSize);
        int maxX = Mathf.FloorToInt(worldMax.x / chunkSize);
        int minY = Mathf.FloorToInt(worldMin.y / chunkSize);
        int maxY = Mathf.FloorToInt(worldMax.y / chunkSize);

        for (int x = minX; x <= maxX; x++)
        {
            for (int y = minY; y <= maxY; y++)
            {
                Vector2Int coord = new Vector2Int(x, y);
                if (!chunks.ContainsKey(coord))
                {
                    var chunk = CreateChunk(coord);
                    chunks.Add(coord, chunk);
                }
            }
        }
        Logger.Log("Chunk 생성 완료");
    }
    
    /// <summary>
    /// 특정 위치의 Chunk의 NavMesh 갱신
    /// </summary>
    public void UpdateChunkPos(Vector3 _worldPos)
    {
        Vector2Int coord = WorldToChunk(_worldPos);
        
        if (!chunks.TryGetValue(coord, out var chunk))
        {
            chunk = CreateChunk(coord);
            chunks.Add(coord, chunk);
        }

        chunk.UpdateNavMesh();
        Logger.Log("Chunk Update");
    }
    
    /// <summary>
    /// 새로운 Chunk 생성
    /// </summary>
    private NavMeshChunk CreateChunk(Vector2Int coord)
    {
        GameObject go = Instantiate(navMeshChunkPrefab, transform);
        go.name = $"NavMeshChunk_{coord.x}_{coord.y}";

        NavMeshChunk chunk = go.AddComponent<NavMeshChunk>();
        chunk.Initialize(coord, chunkSize);

        return chunk;
    }
    
    /// <summary>
    /// 월드 위치를 Chunk 좌표로 변환
    /// </summary>
    private Vector2Int WorldToChunk(Vector3 worldPos)
    {
        int x = Mathf.FloorToInt(worldPos.x / chunkSize);
        int y = Mathf.FloorToInt(worldPos.y / chunkSize);
        return new Vector2Int(x, y);
    }
}
