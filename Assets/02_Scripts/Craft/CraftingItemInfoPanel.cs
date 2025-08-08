using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CraftingItemInfoPanel : MonoBehaviour
{
    public Image image;
    public new TextMeshProUGUI name;
    public Transform requiredIngredient;
    public GameObject requiredIngredientItemSlotPrefab;
    public Craft craft;

    private InventoryManager inventoryManager;

    private void Awake()
    {
        inventoryManager = InventoryManager.Instance;
    }

    public void UpdateRequiredIngredientPanel(ItemSO _itemSO)
    {
        foreach (Transform _slot in requiredIngredient)
        {
            Destroy(_slot.gameObject);
            //_slot.gameObject.SetActive(false);
        }

        foreach (RecipeIngredient _recipe in _itemSO.recipe)
        {
            GameObject slot = Instantiate(requiredIngredientItemSlotPrefab, requiredIngredient);
            if (slot.TryGetComponent(out IngredientSlot _ingredientSlot))
            {
                Sprite icon = _recipe.item.icon;
                int havingItemCount = inventoryManager.CountItem(_recipe.item);
                int requiredItemCount = _recipe.amount;
                _ingredientSlot.UpdateIngredientSlot(icon, havingItemCount, requiredItemCount);
            }
        }
    }
}
