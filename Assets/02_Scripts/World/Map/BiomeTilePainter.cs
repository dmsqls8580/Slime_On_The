using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class BiomeTilePainter : MonoBehaviour
{
    private Tilemap _tilemap;
    private Dictionary<BiomeType, TileBase> _biomeTiles;

    public BiomeTilePainter(Tilemap tilemap, Dictionary<BiomeType, TileBase> biomeTiles)
    {
        _tilemap = tilemap;
        _biomeTiles = biomeTiles;
    }

    public void PaintTiles(Dictionary<Vector3Int, int> tileToRegionMap, Dictionary<int, BiomeType> regionBiomes)
    {
        foreach (var kvp in tileToRegionMap)
        {
            Vector3Int tilePos = kvp.Key;
            int regionId = kvp.Value;

            if (!regionBiomes.TryGetValue(regionId, out var biomeType)) continue;
            if (!_biomeTiles.TryGetValue(biomeType, out var tile)) continue;

            _tilemap.SetTile(tilePos, tile);
        }
    }
}
