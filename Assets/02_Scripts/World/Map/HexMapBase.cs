using System.Collections.Generic;
using UnityEngine;

public class HexMapBase : MonoBehaviour
{
    public int minWidth = 100;
    public int maxWidth = 200;
    public int height = 170;

    public List<Vector3Int> GeneratedTiles { get; private set; } = new();

    public void GenerateHexMap()
    {
        Vector3Int[] offsets = new Vector3Int[]
        {
            Vector3Int.zero,
            new Vector3Int( 150,  85, 0),
            new Vector3Int( 150, -85, 0),
            new Vector3Int(   0, -170, 0),
            new Vector3Int(-150, -85, 0),
            new Vector3Int(-150,  85, 0),
            new Vector3Int(   0,  170, 0),
        };

        GeneratedTiles.Clear();
        foreach (var offset in offsets)
        {
            GenerateHexShape(offset);
        }
    }

    private void GenerateHexShape(Vector3Int centerOffset)
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
                GeneratedTiles.Add(pos);
            }
        }
    }
}
