using UnityEngine;
using UnityEngine.Tilemaps;

public class HexMapManager : MonoBehaviour
{
    public Tilemap tilemap;
    public TileBase plainsTile, forestTile, quarryTile;

    public int cellWidth = 30;
    public int cellHeight = 30;
    public float fillPercent = 0.45f;
    public int smoothIterations = 5;

    private Vector2Int[] hexOffsets = new Vector2Int[]
    {
        new Vector2Int(0, 0),
        new Vector2Int(1, 0),
        new Vector2Int(-1, 0),
        new Vector2Int(0, 1),
        new Vector2Int(0, -1),
        new Vector2Int(1, -1),
        new Vector2Int(-1, 1)
    };

    void Start()
    {
        HexMapGenerator generator = new(tilemap);
        generator.SetTileAssets(plainsTile, forestTile, quarryTile);

        foreach (Vector2Int coord in hexOffsets)
        {
            Vector3Int offset = HexToWorldOffset(coord.x, coord.y);
            HexMapData data = new();
            data.GenerateCellularTerrain(cellWidth, cellHeight, smoothIterations, fillPercent);
            generator.GenerateHexCell(offset, data);
        }
    }

    // Flat-top 육각형 좌표계 기준 오프셋 계산
    Vector3Int HexToWorldOffset(int q, int r)
    {
        float x = q * (cellWidth * 0.75f);
        float y = r * cellHeight + (q % 2 != 0 ? cellHeight * 0.5f : 0);
        return new Vector3Int(Mathf.RoundToInt(x), Mathf.RoundToInt(y), 0);
    }
}