using System.Collections.Generic;
using UnityEngine;

public class RecipeDatabase : MonoBehaviour
{
    [SerializeField] private List<ItemSO> items;
    public List<ItemSO> Items => items;

    private void Awake()
    {
        // 우선 순위 오름차순 정렬.
        items.Sort((a, b) => a.cookedData.priority.CompareTo(b.cookedData.priority));
    }
}
