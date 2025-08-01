using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class BiomeDecorationPlacer : MonoBehaviour
{
    [System.Serializable]
    public class DecorationTileSet
    {
        public BiomeType biome;
        public List<TileBase> decorationTiles;
    }

    [Header("타일맵")]
    public Tilemap decorationTilemap;
    public Tilemap roadTilemap;

    [Header("장식 타일 설정")]
    public List<DecorationTileSet> biomeDecorations;

    [Header("피해야 할 오브젝트 부모")]
    public Transform resourceParent;
    public Transform structureParent;

    [Header("설정")]
    [Range(0f, 1f)] public float decorationChance = 0.1f;
    public int seed = 12345;

    private System.Random prng;

    public void Place(Dictionary<Vector3Int, int> tileToRegion, Dictionary<int, BiomeType> regionBiomes)
    {
        prng = new System.Random(seed);
        decorationTilemap.ClearAllTiles();

        // 자원과 특수 지형이 점유한 타일 수집
        HashSet<Vector3Int> occupiedTiles = new();

        CollectOccupiedTiles(resourceParent, occupiedTiles);
        CollectOccupiedTiles(structureParent, occupiedTiles);

        // 장식 타일 배치
        foreach (var tilePos in tileToRegion.Keys)
        {
            if (occupiedTiles.Contains(tilePos)) continue; // 자원/구조물 위 피하기
            if (roadTilemap.HasTile(tilePos)) continue;    // 길 위 피하기

            int regionId = tileToRegion[tilePos];
            if (!regionBiomes.TryGetValue(regionId, out var biome)) continue;

            var tileSet = biomeDecorations.Find(b => b.biome == biome);
            if (tileSet == null || tileSet.decorationTiles.Count == 0) continue;

            if (prng.NextDouble() > decorationChance) continue;

            TileBase tile = tileSet.decorationTiles[prng.Next(tileSet.decorationTiles.Count)];
            decorationTilemap.SetTile(tilePos, tile);
        }
    }

    private void CollectOccupiedTiles(Transform parent, HashSet<Vector3Int> tileSet)
    {
        if (parent == null) return;

        foreach (Transform child in parent)
        {
            Vector3Int tilePos = decorationTilemap.WorldToCell(child.position);
            tileSet.Add(tilePos);
        }
    }
}
