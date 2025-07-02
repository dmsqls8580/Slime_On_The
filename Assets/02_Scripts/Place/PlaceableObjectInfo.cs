using UnityEngine;

// 테스트용.
[CreateAssetMenu(fileName = "BuildData", menuName = "Build Data")]
public class PlaceableObjectInfo : ScriptableObject
{
    public int idx;
    public GameObject installablePrefab;
    public GameObject previewPrefab;
    public Vector2Int size;
    public PlaceType placeType;
}
