using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public enum BiomeType { Grass, Forest, Swamp, Desert }

public class ProceduralMapGenerator : MonoBehaviour
{
    [Header("맵 크기 설정")]
    public int mapWidth = 100;
    public int mapHeight = 100;
    public int seed = 12345;

    [Header("바이옴 및 타일")]
    public TileBase grassTile;
    public TileBase forestTile;
    public TileBase swampTile;
    public TileBase desertTile;
    public TileBase waterTile;
    public TileBase roadTile;

    [Header("타일맵")]
    public Tilemap groundTilemap;
    public Tilemap roadTilemap;

    [Header("리전/자원 설정")]
    public int regionCount = 20;
    public int resourceCount = 100;
    public GameObject resourcePrefab;
    public float resourceMinSpacing = 5f;

    private System.Random prng;
    private List<Vector2Int> regionCenters = new();
    private int[,] regionMap;
    private BiomeType[,] biomeMap;

    void Start()
    {
        prng = new System.Random(seed);
        UnityEngine.Random.InitState(seed);

        GenerateRegionCenters();
        AssignRegionsToTiles();
        GenerateBiomes();
        RenderTiles();
        ConnectRegionsWithRoads();
        PlaceResources();
        ApplyEdgeFalloff();
    }

    void GenerateRegionCenters()
    {
        for (int i = 0; i < regionCount; i++)
        {
            int x = prng.Next(0, mapWidth);
            int y = prng.Next(0, mapHeight);
            regionCenters.Add(new Vector2Int(x, y));
        }
    }

    void AssignRegionsToTiles()
    {
        regionMap = new int[mapWidth, mapHeight];

        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                int closest = 0;
                float minDist = float.MaxValue;
                Vector2Int pos = new(x, y);

                for (int i = 0; i < regionCenters.Count; i++)
                {
                    float dist = Vector2Int.Distance(regionCenters[i], pos);
                    if (dist < minDist)
                    {
                        minDist = dist;
                        closest = i;
                    }
                }
                regionMap[x, y] = closest;
            }
        }
    }

    void GenerateBiomes()
    {
        biomeMap = new BiomeType[mapWidth, mapHeight];
        Vector2 center = new(mapWidth / 2f, mapHeight / 2f);

        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                float nx = x / (float)mapWidth;
                float ny = y / (float)mapHeight;
                float noise = Mathf.PerlinNoise(nx * 4f + seed, ny * 4f + seed);
                float dist = Vector2.Distance(new Vector2(x, y), center) / (mapWidth * 0.5f);
                float value = noise + dist * 0.5f;

                if (value < 0.4f) biomeMap[x, y] = BiomeType.Grass;
                else if (value < 0.6f) biomeMap[x, y] = BiomeType.Forest;
                else if (value < 0.8f) biomeMap[x, y] = BiomeType.Swamp;
                else biomeMap[x, y] = BiomeType.Desert;
            }
        }
    }

    void RenderTiles()
    {
        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                TileBase tile = biomeMap[x, y] switch
                {
                    BiomeType.Grass => grassTile,
                    BiomeType.Forest => forestTile,
                    BiomeType.Swamp => swampTile,
                    BiomeType.Desert => desertTile,
                    _ => null
                };

                groundTilemap.SetTile(new Vector3Int(x, y, 0), tile);
            }
        }
    }

    void ConnectRegionsWithRoads()
    {
        for (int i = 0; i < regionCenters.Count - 1; i++)
        {
            Vector2Int from = regionCenters[i];
            Vector2Int to = regionCenters[i + 1];
            foreach (var pos in GetLine(from, to))
            {
                roadTilemap.SetTile(new Vector3Int(pos.x, pos.y, 0), roadTile);
            }
        }
    }

    IEnumerable<Vector2Int> GetLine(Vector2Int start, Vector2Int end)
    {
        int x0 = start.x, y0 = start.y;
        int x1 = end.x, y1 = end.y;

        int dx = Mathf.Abs(x1 - x0), dy = Mathf.Abs(y1 - y0);
        int sx = x0 < x1 ? 1 : -1;
        int sy = y0 < y1 ? 1 : -1;
        int err = dx - dy;

        while (true)
        {
            yield return new Vector2Int(x0, y0);
            if (x0 == x1 && y0 == y1) break;
            int e2 = 2 * err;
            if (e2 > -dy) { err -= dy; x0 += sx; }
            if (e2 < dx) { err += dx; y0 += sy; }
        }
    }

    void PlaceResources()
    {
        HashSet<Vector2Int> used = new();
        int attempts = 0;

        for (int i = 0; i < resourceCount && attempts < resourceCount * 10; i++)
        {
            Vector2Int pos = new(prng.Next(0, mapWidth), prng.Next(0, mapHeight));
            if (used.Any(p => Vector2Int.Distance(p, pos) < resourceMinSpacing))
            {
                i--; // retry
                attempts++;
                continue;
            }
            used.Add(pos);
            Instantiate(resourcePrefab, groundTilemap.CellToWorld((Vector3Int)pos), Quaternion.identity);
        }
    }

    void ApplyEdgeFalloff()
    {
        Vector2 center = new(mapWidth / 2f, mapHeight / 2f);
        float edgeThreshold = 0.85f;

        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), center) / (mapWidth * 0.5f);
                if (dist > edgeThreshold)
                {
                    groundTilemap.SetTile(new Vector3Int(x, y, 0), waterTile);
                }
            }
        }
    }
}
