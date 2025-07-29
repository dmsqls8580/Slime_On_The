using UnityEngine;
using UnityEngine.Tilemaps;

public class HexMapGenerator
{
    private Tilemap tilemap;
    private TileBase plainsTile, forestTile, quarryTile;

    public HexMapGenerator(Tilemap _tilemap)
    {
        tilemap = _tilemap;
    }

    public void SetTileAssets(TileBase plains, TileBase forest, TileBase quarry)
    {
        plainsTile = plains;
        forestTile = forest;
        quarryTile = quarry;
    }

    public void GenerateHexCell(Vector3Int offset, HexMapData mapData)
    {
        for (int x = 0; x < mapData.width; x++)
        {
            for (int y = 0; y < mapData.height; y++)
            {
                Vector3Int pos = new Vector3Int(offset.x + x, offset.y + y, 0);
                TerrainType type = mapData.terrainMap[x, y];
                TileBase tile = type switch
                {
                    TerrainType.Forest => forestTile,
                    TerrainType.Quarry => quarryTile,
                    _ => plainsTile,
                };
                tilemap.SetTile(pos, tile);
            }
        }
    }
}