using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TileManager : MonoBehaviour
{
    [SerializeField] private Tilemap groundTilemap;

    private Dictionary<Vector3Int, Tile> tiles = new Dictionary<Vector3Int, Tile>();

    private void Start()
    {
        InitializeTiles();
    }

    private void InitializeTiles()
    {
        BoundsInt bounds = groundTilemap.cellBounds;

        foreach (Vector3Int pos in bounds.allPositionsWithin)
        {
            if (!groundTilemap.HasTile(pos)) continue;

            Tile tile = new Tile()
            {
                gridPosition = pos,
                isPlaceable = true,
                placeType = PlaceType.Building | PlaceType.Seed,
                placedObject = null
            };

            tiles[pos] = tile;
        }
    }

    public Tile GetTileAt(Vector3Int gridPos)
    {
        tiles.TryGetValue(gridPos, out Tile tile);
        return tile;
    }

    public bool CanPlaceAt(Vector3Int gridPos, PlaceType type)
    {
        Tile tile = GetTileAt(gridPos);
        return tile != null && tile.CanPlace(type);
    }

    public void SetPlacedObject(Vector3Int gridPos, GameObject obj)
    {
        if (tiles.TryGetValue(gridPos, out Tile tile))
        {
            tile.isPlaceable = false;
            tile.placedObject = obj;
        }
    }

    public void ClearPlacedObject(Vector3Int gridPos)
    {
        if (tiles.TryGetValue(gridPos, out Tile tile))
        {
            tile.isPlaceable = true;
            tile.placedObject = null;
        }
    }

    public Vector3Int GetGridPosition(Vector3 worldPos)
    {
        return groundTilemap.WorldToCell(worldPos);
    }

    public Vector3 GetWorldPosition(Vector3Int gridPos)
    {
        return groundTilemap.GetCellCenterWorld(gridPos);
    }
}
