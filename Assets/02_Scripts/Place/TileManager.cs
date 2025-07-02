using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TileManager : MonoBehaviour
{
    [SerializeField] private List<Tilemap> tilemaps = new List<Tilemap>();

    private Dictionary<Vector3Int, Tile> tiles = new Dictionary<Vector3Int, Tile>();

    private void Start()
    {
        foreach(Tilemap _tilemap in tilemaps)
        {
            RegisterTilesFromTilemap(_tilemap);
        }
    }

    public void AddTilemap(Tilemap _tilemap)
    {
        if (!tilemaps.Contains(_tilemap))
        {
            tilemaps.Add(_tilemap);
            RegisterTilesFromTilemap(_tilemap);
        }
    }

    private void RegisterTilesFromTilemap(Tilemap _tilemap)
    {
        BoundsInt bounds = _tilemap.cellBounds;

        int count = 0;

        foreach (Vector3Int pos in bounds.allPositionsWithin)
        {
            if (!_tilemap.HasTile(pos)) continue;
            if (tiles.ContainsKey(pos)) continue;

            Tile tile = new Tile()
            {
                gridPosition = pos,
                isPlaceable = true,
                placeType = PlaceType.Building | PlaceType.Seed,
                placedObject = null
            };

            tiles[pos] = tile;
            count++;
        }

        Debug.Log($"[TileManager] {count}개의 타일이 등록되었습니다.");
    }

    public Tile GetTileAt(Vector3Int _gridPos)
    {
        tiles.TryGetValue(_gridPos, out Tile tile);
        return tile;
    }

    public bool CanPlaceAt(Vector3Int _gridPos, PlaceType _type)
    {
        Tile tile = GetTileAt(_gridPos);
        return tile != null && tile.CanPlace(_type);
    }

    public void SetPlacedObject(Vector3Int _gridPos, GameObject _installedPrefab)
    {
        if (tiles.TryGetValue(_gridPos, out Tile tile))
        {
            tile.isPlaceable = false;
            tile.placedObject = _installedPrefab;
        }
    }

    public void ClearPlacedObject(Vector3Int _gridPos)
    {
        if (tiles.TryGetValue(_gridPos, out Tile tile))
        {
            tile.isPlaceable = true;
            tile.placedObject = null;
        }
    }

    public Vector3Int GetGridPosition(Vector3 _worldPos)
    {
        foreach (Tilemap _tilemap in tilemaps)
        {
            Vector3Int gridPos = _tilemap.WorldToCell(_worldPos);
            if (_tilemap.HasTile(gridPos))
            {
                return gridPos;
            }
        }

        Debug.LogError("타일이 존재하지 않습니다.");
        return Vector3Int.zero;
    }

    public Vector3 GetWorldPosition(Vector3Int _gridPos)
    {
        foreach (Tilemap _tilemap in tilemaps)
        {
            if (_tilemap.HasTile(_gridPos))
            {
                return _tilemap.GetCellCenterWorld(_gridPos);
            }
        }

        Debug.LogError("타일이 존재하지 않습니다.");
        return Vector3Int.zero;
    }
}
