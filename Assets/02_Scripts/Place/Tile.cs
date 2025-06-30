using UnityEngine;

public class Tile
{
    // 그리드 상 좌표.
    public Vector3Int gridPosition;
    // 설치 가능 여부.
    public bool isPlaceable;
    // 어떤 종류의 오브젝트가 설치 가능한가.
    public PlaceType placeType;
    // 설치된 오브젝트.
    public GameObject placedObject;
    // 특정 타입이 설치 가능한지 체크하는 함수.

    public bool CanPlace(PlaceType type)
    {
        return isPlaceable && placeType.HasFlag(type) && placedObject == null;
    }
}
