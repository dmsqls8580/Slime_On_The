using UnityEngine;

[CreateAssetMenu(menuName = "Placeable/Info")]
public class PlaceableInfo : ScriptableObject
{
    public GameObject normalPrefab;
    public GameObject previewPrefab;
    public Vector2Int size;
}
