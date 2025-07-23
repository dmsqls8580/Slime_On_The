using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Linq;

public enum BiomeType { Grass, Forest, Desert, Rocky, Marsh }


public class FlatTopHexMapGenerator : MonoBehaviour
{
    [System.Serializable]
    public class ResourcePool
    {
        public string name;
        public List<GameObject> prefabs;
        [Range(0f, 1f)] public float weight = 1f;
        public float minSpacing = 5f;
    }
    
    [System.Serializable]
    public class SetPiece
    {
        public string name;
        public List<GameObject> prefabs;
        public float radius = 5f;
        public int count = 10;
        public float jitter = 1.5f;
    }

    [Header("맵 설정")]
    public int minWidth = 100;
    public int maxWidth = 200;
    public int height = 170;
    public int regionCount = 20;
    public int seed = 12345;
    public float waterRingRadius = 280f;

    [Header("타일 및 타일맵")]
    public Tilemap groundTilemap;
    public Tilemap roadTilemap;
    public TileBase grassTile;
    public TileBase forestTile;
    public TileBase desertTile;
    public TileBase rockyTile;
    public TileBase marshTile;
    public TileBase waterTile;
    public TileBase roadTile;

    [Header("Set Piece (바이옴별)")]
    public List<SetPiece> grassSetPieces;
    public List<SetPiece> forestSetPieces;
    public List<SetPiece> desertSetPieces;
    public List<SetPiece> rockySetPieces;
    public List<SetPiece> marshSetPieces;

    private System.Random prng;
    private List<Vector3Int> validGroundTiles = new();
    private List<Vector3Int> regionCenters = new();
    private Dictionary<Vector3Int, int> tileToRegion = new();
    private Dictionary<int, BiomeType> regionBiomes = new();

    void Start()
    {
        prng = new System.Random(seed);
        UnityEngine.Random.InitState(seed);

        GenerateHexBaseMap();
        PickRegionCenters();
        AssignTilesToRegions();
        AssignBiomesToRegions();
        PaintBiomeTiles();
        ConnectRegionsWithRoads();
        PlaceSetPieces();
        ApplyWaterRing();
    }

    void GenerateHexBaseMap()
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

        foreach (var offset in offsets)
        {
            GenerateHexShape(offset);
        }
    }

    void GenerateHexShape(Vector3Int centerOffset)
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
                validGroundTiles.Add(pos);
            }
        }
    }

    void PickRegionCenters()
    {
        HashSet<Vector3Int> picked = new();
        while (regionCenters.Count < regionCount)
        {
            int index = prng.Next(validGroundTiles.Count);
            Vector3Int candidate = validGroundTiles[index];
            if (!picked.Contains(candidate))
            {
                picked.Add(candidate);
                regionCenters.Add(candidate);
            }
        }
    }

    void AssignTilesToRegions()
    {
        foreach (var tilePos in validGroundTiles)
        {
            float minDist = float.MaxValue;
            int closestRegion = 0;

            for (int i = 0; i < regionCenters.Count; i++)
            {
                float dist = Vector3Int.Distance(tilePos, regionCenters[i]);
                if (dist < minDist)
                {
                    minDist = dist;
                    closestRegion = i;
                }
            }
            tileToRegion[tilePos] = closestRegion;
        }
    }

    void AssignBiomesToRegions()
    {
        float maxDistance = 0f;
        foreach (var center in regionCenters)
        {
            float d = Vector2.Distance(new Vector2(center.x, center.y), Vector2.zero);
            if (d > maxDistance) maxDistance = d;
        }

        for (int i = 0; i < regionCenters.Count; i++)
        {
            var center = regionCenters[i];
            float distNorm = Vector2.Distance(new Vector2(center.x, center.y), Vector2.zero) / maxDistance;
            float perlin = Mathf.PerlinNoise(center.x * 0.01f + seed, center.y * 0.01f + seed);
            float noiseAdjusted = Mathf.Clamp01((distNorm + perlin * 0.5f) / 1.5f);

            BiomeType biome = noiseAdjusted switch
            {
                <= 0.25f => BiomeType.Grass,
                <= 0.45f => BiomeType.Forest,
                <= 0.65f => BiomeType.Desert,
                <= 0.85f => BiomeType.Rocky,
                _ => BiomeType.Marsh,
            };
            regionBiomes[i] = biome;
        }
    }

    void PaintBiomeTiles()
    {
        foreach (var kvp in tileToRegion)
        {
            var tilePos = kvp.Key;
            int regionId = kvp.Value;
            BiomeType biome = regionBiomes[regionId];

            TileBase tile = biome switch
            {
                BiomeType.Grass => grassTile,
                BiomeType.Forest => forestTile,
                BiomeType.Desert => desertTile,
                BiomeType.Rocky => rockyTile,
                BiomeType.Marsh => marshTile,
                _ => grassTile
            };
            groundTilemap.SetTile(tilePos, tile);
        }
    }

    void ConnectRegionsWithRoads()
    {
        var edges = new List<(int from, int to, float dist)>();
        for (int i = 0; i < regionCenters.Count; i++)
        {
            for (int j = i + 1; j < regionCenters.Count; j++)
            {
                float dist = Vector3Int.Distance(regionCenters[i], regionCenters[j]);
                edges.Add((i, j, dist));
            }
        }
        edges.Sort((a, b) => a.dist.CompareTo(b.dist));

        var parent = new int[regionCenters.Count];
        for (int i = 0; i < parent.Length; i++) parent[i] = i;
        int Find(int x) => parent[x] == x ? x : (parent[x] = Find(parent[x]));
        void Union(int x, int y) => parent[Find(x)] = Find(y);

        foreach (var (fromIdx, toIdx, _) in edges)
        {
            if (Find(fromIdx) != Find(toIdx))
            {
                Union(fromIdx, toIdx);
                Vector3Int from = regionCenters[fromIdx];
                Vector3Int to = regionCenters[toIdx];

                foreach (var pos in GetLine(from, to))
                {
                    for (int dx = -1; dx <= 1; dx++)
                    {
                        for (int dy = -1; dy <= 1; dy++)
                        {
                            Vector3Int offset = new(pos.x + dx, pos.y + dy, 0);
                            if (validGroundTiles.Contains(offset))
                                roadTilemap.SetTile(offset, roadTile);
                        }
                    }
                }
            }
        }
    }

    IEnumerable<Vector3Int> GetLine(Vector3Int start, Vector3Int end)
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

    void PlaceSetPieces()
    {
        Dictionary<BiomeType, List<SetPiece>> biomeSetPieces = new()
    {
        { BiomeType.Grass, grassSetPieces },
        { BiomeType.Forest, forestSetPieces },
        { BiomeType.Desert, desertSetPieces },
        { BiomeType.Rocky, rockySetPieces },
        { BiomeType.Marsh, marshSetPieces },
    };

        List<Vector3Int> shuffled = validGroundTiles.OrderBy(_ => prng.Next()).ToList();
        HashSet<Vector3> placedPositions = new();

        int setPieceAttempts = 300; // 전체 군집 수

        for (int i = 0; i < setPieceAttempts; i++)
        {
            Vector3Int center = shuffled[prng.Next(shuffled.Count)];
            if (!tileToRegion.ContainsKey(center)) continue;
            if (roadTilemap.HasTile(center)) continue;

            int regionId = tileToRegion[center];
            BiomeType biome = regionBiomes[regionId];
            if (!biomeSetPieces.ContainsKey(biome)) continue;

            var candidates = biomeSetPieces[biome];
            if (candidates.Count == 0) continue;

            SetPiece selected = candidates[prng.Next(candidates.Count)];

            for (int j = 0; j < selected.count; j++)
            {
                Vector2 offset = Random.insideUnitCircle * selected.radius;
                offset += new Vector2(
                    Random.Range(-selected.jitter, selected.jitter),
                    Random.Range(-selected.jitter, selected.jitter)
                );

                Vector3 spawnPos = groundTilemap.CellToWorld(center) + (Vector3)offset;

                if (placedPositions.Any(p => Vector3.Distance(p, spawnPos) < 1f)) continue;
                if (Physics2D.OverlapCircleAll(spawnPos, 0.5f).Length > 0) continue;

                GameObject prefab = selected.prefabs[prng.Next(selected.prefabs.Count)];
                Instantiate(prefab, spawnPos, Quaternion.identity);
                placedPositions.Add(spawnPos);
            }
        }
    }


    void ApplyWaterRing()
    {
        foreach (var pos in validGroundTiles)
        {
            float dist = Vector3.Distance((Vector3)pos, Vector3.zero);
            if (!tileToRegion.ContainsKey(pos) || dist > waterRingRadius)
            {
                groundTilemap.SetTile(pos, waterTile);
            }
        }
    }
}
