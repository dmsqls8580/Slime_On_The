using UnityEngine;
using UnityEngine.Tilemaps;

public class HexShapedMapGenerator : MonoBehaviour
{
    public Tilemap tilemap;
    public TileBase tile;

    public int minWidth = 100;
    public int maxWidth = 200;
    public int height = 170; // 전체 줄 수

    void Start()
    {
        GenerateFlatTopHexMap();
    }

    void GenerateFlatTopHexMap()
    {
        tilemap.ClearAllTiles();

        int halfHeight = height / 2;

        for (int y = -halfHeight; y <= halfHeight; y++)
        {
            int dy = Mathf.Abs(y); // 중앙으로부터 떨어진 정도
            int width = Mathf.RoundToInt(Mathf.Lerp(maxWidth, minWidth, (float)dy / halfHeight));

            int startX = -width / 2;

            for (int x = 0; x < width; x++)
            {
                Vector3Int pos = new Vector3Int(startX + x, y, 0);

                tilemap.SetTile(pos, tile);
            }
        }
    }
}