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
        image = GetComponent<Image>();
        button = GetComponent<Button>();
        inventoryManager = InventoryManager.Instance;
    }

    private void Start()
    {
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

    public void OnClickSlot()
    {
        craftingItemInfoPanel.image.sprite = image.sprite;
        craftingItemInfoPanel.name.text = itemSO.itemName.ToString();
        UpdateRequiredIngredientPanel();
        craftingItemInfoPanel.craft.Initialize(this, craftingSlotManager);
    }

    private void UpdateRequiredIngredientPanel()
    {
        foreach (Transform _slot in craftingItemInfoPanel.requiredIngredient)
            Destroy(_slot.gameObject);

        foreach (RecipeIngredient _recipe in itemSO.recipe)
        {
            GameObject slot = Instantiate(craftingItemInfoPanel.requiredIngredientItemSlotPrefab, craftingItemInfoPanel.requiredIngredient);
            if (slot.TryGetComponent(out IngredientSlot _ingredientSlot))
            {
                int havingItemCount = inventoryManager.CountItem(itemSO);
                int requiredItemCount = _recipe.amount;
                _ingredientSlot.UpdateIngredientSlot(havingItemCount, requiredItemCount);
            }
        }
    }

    public void SetLocked(bool _isLocked)
    {
        isLocked = _isLocked;
        Lock.SetActive(isLocked);
    }
}
