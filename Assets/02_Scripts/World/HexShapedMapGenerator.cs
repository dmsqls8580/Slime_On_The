using UnityEngine;
using UnityEngine.Tilemaps;

public class HexVisualShapedMap : MonoBehaviour
{
    public Tilemap tilemap;
    public TileBase tile;
    public int radius = 85; // 맵 반경 (너비, 높이 조절용)

    void Start()
    {
        GenerateHexVisualMap();
    }

    void GenerateHexVisualMap()
    {
        tilemap.ClearAllTiles();

        for (int q = -radius; q <= radius; q++)
        {
            int r1 = Mathf.Max(-radius, -q - radius);
            int r2 = Mathf.Min(radius, -q + radius);

            for (int r = r1; r <= r2; r++)
            {
                Vector2Int offset = HexToOffset(q, r);
                Vector3Int pos = new Vector3Int(offset.x, offset.y, 0);
                tilemap.SetTile(pos, tile);
            }
        }
    }

    // q: 열, r: 행
    Vector2Int HexToOffset(int q, int r)
    {
        int col = q;
        int row = r + (q - (q & 1)) / 2; // Even-Q 오프셋
        return new Vector2Int(col, row);
    }

}
