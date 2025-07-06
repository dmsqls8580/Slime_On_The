using UnityEngine;
using UnityEngine.Tilemaps;

public class FlatTopHexVisualShapedMap : MonoBehaviour
{
    public Tilemap tilemap;
    public TileBase tile;

    public int minWidth = 100;
    public int maxWidth = 200;
    public int height = 170;

    void Start()
    {
        // 중심 육각형
        GenerateFlatTopHexMap(Vector3Int.zero);

        // 방향 6개로 복제 (수동 오프셋 예시, 추후 자동화 가능)
        Vector3Int[] offsets = new Vector3Int[]
        {
            new Vector3Int( 150,  85, 0), // 오른쪽 위
            new Vector3Int( 150, -85, 0), // 오른쪽 아래
            new Vector3Int(   0, -170, 0), // 아래
            new Vector3Int(-150, -85, 0), // 왼쪽 아래
            new Vector3Int(-150,  85, 0), // 왼쪽 위
            new Vector3Int(   0,  170, 0), // 위
        };

        foreach (var offset in offsets)
        {
            GenerateFlatTopHexMap(offset);
        }
    }

    void GenerateFlatTopHexMap(Vector3Int centerOffset)
    {
        int halfHeight = height / 2;

        for (int y = -halfHeight; y <= halfHeight; y++)
        {
            int dy = Mathf.Abs(y);
            int width = Mathf.RoundToInt(Mathf.Lerp(maxWidth, minWidth, (float)dy / halfHeight));
            int startX = -width / 2;

            for (int x = 0; x < width; x++)
            {
                Vector3Int pos = new Vector3Int(startX + x + centerOffset.x, y + centerOffset.y, 0);
                tilemap.SetTile(pos, tile);
            }
        }
    }
}
