using System.Collections.Generic;
using UnityEngine;

public class RecipeDatabase : MonoBehaviour
{
    [SerializeField] private List<ItemSO> items;
    public List<ItemSO> Items => items;

    private void Awake()
    {
        items.Sort((a, b) => a.idx.CompareTo(b.idx));
    }
}
