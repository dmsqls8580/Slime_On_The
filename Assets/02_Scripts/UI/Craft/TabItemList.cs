using System.Collections.Generic;
using UnityEngine;

public class TabItemList : MonoBehaviour
{
    [SerializeField] private List<ItemSO> items;
    public List<ItemSO> Items => items;
}
