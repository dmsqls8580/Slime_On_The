using UnityEngine;

[CreateAssetMenu(menuName = "Placeable/Info")]
public class PlaceableInfo : ScriptableObject
{
    public GameObject normalPrefab;
    public GameObject previewPrefab;
    // 오브젝트의 가로가 짝수인지.
    public bool isEvenWidth;
}
