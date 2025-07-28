using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class BiomeTilePainter : MonoBehaviour
{
    private readonly Tilemap _tilemap;
    private readonly Dictionary<BiomeType, TileBase> _biomeTiles;

    public BiomeTilePainter(Tilemap tilemap, Dictionary<BiomeType, TileBase> biomeTiles)
    {
        _tilemap = tilemap;
        _biomeTiles = biomeTiles;
    }

    public void PaintTiles(Dictionary<Vector3Int, int> tileToRegionMap, Dictionary<int, BiomeType> regionBiomes)
    {
        _tilemap.ClearAllTiles();

        foreach (var kvp in tileToRegionMap)
        {
            if (!regionBiomes.TryGetValue(kvp.Value, out var biomeType)) continue;
            if (!_biomeTiles.TryGetValue(biomeType, out var tile)) continue;

            _tilemap.SetTile(kvp.Key, tile);
        }
    }
}
