using UnityEngine;

public static class HexGridHelper
{
    public static readonly Vector2Int[] Directions = new Vector2Int[]
    {
        new Vector2Int(1, 0),
        new Vector2Int(-1, 0),
        new Vector2Int(0, 1),
        new Vector2Int(0, -1),
        new Vector2Int(1, -1),
        new Vector2Int(-1, 1)
    };

    public static Vector3Int HexToWorldOffset(int q, int r, int maxWidth, int height)
    {
        float x = q * (maxWidth * 0.75f);
        float y = r * height;

        if (q > 0) y += height * 0.5f;
        else if (q < 0) y -= height * 0.5f;

        return new Vector3Int(Mathf.RoundToInt(x), Mathf.RoundToInt(y), 0);
    }
}