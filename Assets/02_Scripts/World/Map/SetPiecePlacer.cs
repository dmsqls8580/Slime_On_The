using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

[System.Serializable]
public class SetPieceData
{
    public BiomeType biome;
    public List<SetPiece> setPieces;
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

public class SetPiecePlacer : MonoBehaviour
{
    [Header("설정")]
    public List<SetPieceData> biomeSetPieces;
    public Tilemap groundTilemap;
    public Tilemap roadTilemap;
    public int setPieceAttempts = 300;
    public int seed = 12345;

    private System.Random prng;

    public void Place(Dictionary<Vector3Int, int> tileToRegion, Dictionary<int, BiomeType> regionBiomes)
    {
        prng = new System.Random(seed);
        UnityEngine.Random.InitState(seed);

        List<Vector3Int> validTiles = tileToRegion.Keys.OrderBy(_ => prng.Next()).ToList();
        HashSet<Vector3> placedPositions = new();

        for (int i = 0; i < setPieceAttempts; i++)
        {
            Vector3Int center = validTiles[prng.Next(validTiles.Count)];
            if (!tileToRegion.ContainsKey(center)) continue;
            if (roadTilemap.HasTile(center)) continue;

            int regionId = tileToRegion[center];
            BiomeType biome = regionBiomes[regionId];

            var biomeData = biomeSetPieces.FirstOrDefault(b => b.biome == biome);
            if (biomeData == null || biomeData.setPieces.Count == 0) continue;

            SetPiece selected = biomeData.setPieces[prng.Next(biomeData.setPieces.Count)];

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
}
