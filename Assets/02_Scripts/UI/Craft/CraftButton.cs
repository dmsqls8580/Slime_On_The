using UnityEngine;
using UnityEngine.UI;

public class CraftButton : MonoBehaviour
{
    private ItemSO itemSO;
    private bool canCraft = false;

    private Button button;
    private InventoryManager inventoryManager;

    private int amount;

    private void Awake()
    {
        button = GetComponent<Button>();
        inventoryManager = InventoryManager.Instance;
    }

    private void Start()
    {
        button.onClick.AddListener(() => OnClickCraftButton());
    }

    public void Initialize(ItemSO _itemSO)
    {
        itemSO = _itemSO;
        canCraft = CanCraft();
    }

    // 제작 가능한지 판별.
    private bool CanCraft()
    {
        foreach (RecipeIngredient _recipe in itemSO.recipe)
        {
            if (!inventoryManager.CanRemoveItem(_recipe.item, _recipe.amount))
            {
                // TODO: 버튼 비활성화.
                Debug.Log("재료가 부족합니다. 버튼 비활성화");
                return false;
            }
        }
        // TODO: 버튼 활성화.
        Debug.Log("재료가 충분합니다. 버튼 활성화");
        return true;
    }

    // 버튼 클릭 했을 때 제작하기.
    private void OnClickCraftButton()
    {
        if (!canCraft) return;
        // 재료 소모.
        foreach (RecipeIngredient _recipe in itemSO.recipe)
            inventoryManager.TryRemoveItemGlobally(_recipe.item, _recipe.amount);
        // 아이템 제작.
        inventoryManager.TryAddItemGlobally(itemSO, 1);
        // 다시 판별.
        canCraft = CanCraft();
    }
}
