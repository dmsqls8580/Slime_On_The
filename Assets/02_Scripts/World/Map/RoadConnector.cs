using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class RoadConnector : MonoBehaviour
{
    [Header("타일맵 설정")]
    public Tilemap roadTilemap;
    public TileBase roadTile;

    [Header("도로 폭 설정")]
    public int roadWidth = 1;

    public void Connect(List<Vector3Int> regionCenters, List<Vector3Int> validGroundTiles)
    {
        var edges = BuildMST(regionCenters);
        HashSet<Vector3Int> validTiles = new(validGroundTiles);

        foreach (var edge in edges)
        {
            foreach (var pos in GetLine(edge.from, edge.to))
            {
                for (int dx = -roadWidth; dx <= roadWidth; dx++)
                {
                    for (int dy = -roadWidth; dy <= roadWidth; dy++)
                    {
                        Vector3Int offset = new(pos.x + dx, pos.y + dy, 0);
                        if (validTiles.Contains(offset))
                        {
                            roadTilemap.SetTile(offset, roadTile);
                        }
                    }
                }
            }
        }
    }

    private List<(Vector3Int from, Vector3Int to)> BuildMST(List<Vector3Int> centers)
    {
        var edges = new List<(int from, int to, float dist)>();

        for (int i = 0; i < centers.Count; i++)
        {
            for (int j = i + 1; j < centers.Count; j++)
            {
                float dist = Vector3Int.Distance(centers[i], centers[j]);
                edges.Add((i, j, dist));
            }
        }

        edges.Sort((a, b) => a.dist.CompareTo(b.dist));
        var mst = new List<(Vector3Int from, Vector3Int to)>();

        int[] parent = new int[centers.Count];
        for (int i = 0; i < parent.Length; i++) parent[i] = i;

        int Find(int x) => parent[x] == x ? x : (parent[x] = Find(parent[x]));
        void Union(int x, int y) => parent[Find(x)] = Find(y);

        foreach (var (fromIdx, toIdx, _) in edges)
        {
            if (Find(fromIdx) != Find(toIdx))
            {
                Union(fromIdx, toIdx);
                mst.Add((centers[fromIdx], centers[toIdx]));
            }
        }

        return mst;
    }

    private IEnumerable<Vector3Int> GetLine(Vector3Int start, Vector3Int end)
    {
        int x0 = start.x, y0 = start.y;
        int x1 = end.x, y1 = end.y;
        int dx = Mathf.Abs(x1 - x0), dy = Mathf.Abs(y1 - y0);
        int sx = x0 < x1 ? 1 : -1;
        int sy = y0 < y1 ? 1 : -1;
        int err = dx - dy;

        while (true)
        {
            yield return new Vector3Int(x0, y0, 0);
            if (x0 == x1 && y0 == y1) break;
            int e2 = 2 * err;
            if (e2 > -dy) { err -= dy; x0 += sx; }
            if (e2 < dx) { err += dx; y0 += sy; }
        }
    }
}
