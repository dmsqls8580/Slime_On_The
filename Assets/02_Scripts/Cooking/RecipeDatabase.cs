using System.Collections.Generic;
using UnityEngine;

public class RecipeDatabase : MonoBehaviour
{
    [SerializeField] private List<ItemSO> recipes;
    public List<ItemSO> Recipes => recipes;
    private void Awake()
    {
        recipes.Sort((a, b) => a.idx.CompareTo(b.idx));
    }
}
