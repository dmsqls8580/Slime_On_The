using UnityEngine;
using UnityEngine.UI;

public class CraftingSlot : MonoBehaviour
{
    private ItemSO itemSO;

    private Image image;
    private Button button;
    private CraftingItemInfoPanel craftingItemInfoPanel;

    private void Awake()
    {
        image = GetComponent<Image>();
        button = GetComponent<Button>();
    }

    private void Start()
    {
        button.onClick.AddListener(() => OnClickSlot());
        gameObject.SetActive(false);
    }

    public void Initialize(ItemSO _itemSO, CraftingItemInfoPanel _craftingItemInfoPanel)
    {
        itemSO = _itemSO;
        image.sprite = itemSO.icon;
        craftingItemInfoPanel = _craftingItemInfoPanel;
    }

    public void OnClickSlot()
    {
        craftingItemInfoPanel.image.sprite = image.sprite;
        craftingItemInfoPanel.name.text = itemSO.itemName.ToString();
        craftingItemInfoPanel.description.text = itemSO.description.ToString();
        craftingItemInfoPanel.craftButton.Initialize(itemSO);
    }
}
