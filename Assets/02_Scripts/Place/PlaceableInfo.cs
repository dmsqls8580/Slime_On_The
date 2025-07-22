using UnityEngine;

[CreateAssetMenu(menuName = "Placeable/Info")]
public class PlaceableInfo : ScriptableObject
{
    public GameObject objectPrefab;
    public Vector2Int size;
}
