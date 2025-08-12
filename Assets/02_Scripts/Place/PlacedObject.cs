using UnityEngine;

// 모든 건물 프리팹이 갖고있어야 함.
public class PlacedObject : MonoBehaviour
{
    [SerializeField] private ItemSO itemSO;
    public ItemSO ItemSO => itemSO;
}
