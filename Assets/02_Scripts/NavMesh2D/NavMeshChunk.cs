using NavMeshPlus.Components;
using UnityEngine;
using UnityEngine.AI;

public class NavMeshChunk : MonoBehaviour
{
    public Vector2Int ChunkCoord { get; private set; }
    public float ChunkSize { get; private set; }
    public NavMeshSurface Surface { get; private set; }
    
    private bool isHighlighted = false;
    private float highlightDuration = 0.5f;
    private float highlightTimer = 0f;
    private void Update()
    {
        if (isHighlighted)
        {
            highlightTimer -= Time.deltaTime;
            if (highlightTimer <= 0f)
            {
                isHighlighted = false;
            }
        }
    }

    private void OnDrawGizmos()
    {
        // 외곽선: 항상 빨간색
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position, new Vector3(ChunkSize, ChunkSize, 0.1f));

        // 내부 색상: 업데이트 시 반투명 파란색
        if (isHighlighted)
        {
            Gizmos.color = new Color(0f, 1f, 1f, 0.25f); // Cyan, 25% 투명도
            Gizmos.DrawCube(transform.position, new Vector3(ChunkSize, ChunkSize, 0.05f));
        }
    }

    // Chunk 초기 설정
    public void Initialize(Vector2Int _chunkCoord, float _chunkSize)
    {
        ChunkCoord = _chunkCoord;
        ChunkSize = _chunkSize;
        
        // 위치는 ChunkCoord에 따라 변경
        transform.position = new Vector3(
            ChunkCoord.x * ChunkSize + (ChunkSize / 2),
            ChunkCoord.y * ChunkSize + (ChunkSize / 2),
            0f);
        
        Surface = gameObject.GetComponent<NavMeshSurface>();

        Surface.collectObjects = CollectObjects.All;
        Surface.useGeometry = NavMeshCollectGeometry.PhysicsColliders;
        Surface.layerMask = LayerMask.GetMask("Default", "Res");
        
        
        // 영역 설정
        Surface.size = new Vector3(ChunkSize, ChunkSize, 0.1f);
        Surface.center = Vector3.zero;
        
        Surface.BuildNavMesh();
    }

    /// <summary>
    /// 해당 청크의 NavMesh 갱신
    /// </summary>
    public void UpdateNavMesh()
    {
        Surface.UpdateNavMesh(Surface.navMeshData);

        // 외곽선 색상 하이라이트 시작
        isHighlighted = true;
        highlightTimer = highlightDuration;
    }
}
