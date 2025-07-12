using UnityEngine;
using UnityEngine.UI;

public class Craft : MonoBehaviour
{
    private CraftingSlot craftingSlot;
    public CraftingSlot CraftingSlot => craftingSlot;
    private ItemSO itemSO;
    private bool canCraft = false;

    private Button button;
    
    private InventoryManager inventoryManager;
    private CraftingSlotManager craftingSlotManager;

    private void Awake()
    {
        button = GetComponent<Button>();
        inventoryManager = InventoryManager.Instance;
    }

    private void Start()
    {
        button.onClick.AddListener(() => OnClickCraftButton());
    }

    public void Initialize(CraftingSlot _craftingSlot, CraftingSlotManager _craftingSlotManager)
    {
        craftingSlot = _craftingSlot;
        itemSO = _craftingSlot.ItemSO;
        CanCraft();
        craftingSlotManager = _craftingSlotManager;
    }

    // 제작 가능한지 판별.
    public void CanCraft()
    {
        if (craftingSlot.IsLocked == true)
        {
            // TODO: 버튼 비활성화.
            Debug.Log("슬롯이 잠김.");
            canCraft = false;
            return;
        }
        foreach (RecipeIngredient _recipe in itemSO.recipe)
        {
            if (!inventoryManager.CanRemoveItem(_recipe.item, _recipe.amount))
            {
                // TODO: 버튼 비활성화.
                Debug.Log("재료가 부족합니다. 버튼 비활성화");
                canCraft = false;
                return;
            }
        }
        // TODO: 버튼 활성화.
        Debug.Log("재료가 충분합니다. 버튼 활성화");
        canCraft = true;
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
        // 잠금관리 등록 해제.
        craftingSlotManager.RemoveCraftingSlot(itemSO.idx, craftingSlot);
    }
}
