using UnityEngine;

public class Tile
{
    // �׸��� �� ��ǥ.
    public Vector3Int gridPosition;
    // ��ġ ���� ����.
    public bool isPlaceable;
    // � ������ ������Ʈ�� ��ġ �����Ѱ�.
    public PlaceType placeType;
    // ��ġ�� ������Ʈ.
    public GameObject placedObject;
    // Ư�� Ÿ���� ��ġ �������� üũ�ϴ� �Լ�.

    public bool CanPlace(PlaceType type)
    {
        return isPlaceable && placeType.HasFlag(type) && placedObject == null;
    }
}
