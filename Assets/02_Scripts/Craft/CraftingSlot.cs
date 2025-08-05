using UnityEngine;
using UnityEngine.UI;

public class CraftingSlot : MonoBehaviour
{
    [SerializeField] private GameObject Lock;

    private Image image;
    private Button button;
    private CraftingItemInfoPanel craftingItemInfoPanel;

    private InventoryManager inventoryManager;
    private CraftingSlotManager craftingSlotManager;

    private ItemSO itemSO;
    public ItemSO ItemSO => itemSO;
    private bool isLocked = false;
    public bool IsLocked => isLocked;

    private void Awake()
    {
        inventoryManager = InventoryManager.Instance;
        image = GetComponent<Image>();
        button = GetComponent<Button>();
        button.onClick.AddListener(() => OnClickSlot());
        gameObject.SetActive(false);
    }

    public void Initialize(ItemSO _itemSO, CraftingItemInfoPanel _craftingItemInfoPanel, CraftingSlotManager _craftingSlotManager)
    {
        itemSO = _itemSO;
        image.sprite = itemSO.icon;
        craftingItemInfoPanel = _craftingItemInfoPanel;
        craftingSlotManager = _craftingSlotManager;
    }

    private void OnClickSlot()
    {
        SoundManager.Instance.PlaySFX(SFX.Click);
        craftingItemInfoPanel.image.sprite = image.sprite;
        craftingItemInfoPanel.name.text = itemSO.itemName.ToString();
        UpdateRequiredIngredientPanel();
        craftingItemInfoPanel.craft.Initialize(this, craftingSlotManager);
    }

    public void UpdateRequiredIngredientPanel()
    {
        foreach (Transform _slot in craftingItemInfoPanel.requiredIngredient)
            Destroy(_slot.gameObject);

        foreach (RecipeIngredient _recipe in itemSO.recipe)
        {
            GameObject slot = Instantiate(craftingItemInfoPanel.requiredIngredientItemSlotPrefab, craftingItemInfoPanel.requiredIngredient);
            if (slot.TryGetComponent(out IngredientSlot _ingredientSlot))
            {
                Sprite icon = _recipe.item.icon;
                int havingItemCount = inventoryManager.CountItem(_recipe.item);
                int requiredItemCount = _recipe.amount;
                _ingredientSlot.UpdateIngredientSlot(icon, havingItemCount, requiredItemCount);
            }
        }
    }

    public void SetLocked(bool _isLocked)
    {
        isLocked = _isLocked;
        Lock.SetActive(isLocked);
    }
}
