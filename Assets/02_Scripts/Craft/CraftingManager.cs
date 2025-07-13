using _02_Scripts.Craft;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CraftingManager : MonoBehaviour
{
    [SerializeField] private List<Button> tabButtons;
    [SerializeField] private List<TabItemList> tabList;
    [SerializeField] private List<Transform> craftingIconsPanels;

    [SerializeField] private CraftingItemInfoPanel craftingItemInfoPanel;

    [Header("스크립트 참조")]
    [SerializeField] private CraftingSlotManager craftingSlotManager;

    private int currentTabIndex = 0;

    private void Start()
    {
        for (int i = 0; i < tabButtons.Count; i++)
        {
            int index = i;
            tabButtons[i].onClick.AddListener(() => OnClickTab(index));
            for (int j = 0; j < tabList[i].Items.Count; j++)
            {
                SetCraftingSlot(craftingIconsPanels[i].GetChild(j), tabList[i].Items[j]);
            }
            craftingIconsPanels[i].gameObject.SetActive(false);
        }
        craftingIconsPanels[currentTabIndex].gameObject.SetActive(true);
    }

    private void SetCraftingSlot(Transform _slot, ItemSO _itemSO)
    {
        _slot.gameObject.SetActive(true);
        if (_slot.gameObject.TryGetComponent(out CraftingSlot _craftingSlot))
        {
            _craftingSlot.Initialize(_itemSO, craftingItemInfoPanel, craftingSlotManager);
            craftingSlotManager.AddCraftingSlot(_itemSO.idx, _craftingSlot);
        }
    }

    public void OnClickTab(int _index)
    {
        if (currentTabIndex == _index) return;
        craftingIconsPanels[currentTabIndex].gameObject.SetActive(false);
        craftingIconsPanels[_index].gameObject.SetActive(true);
        currentTabIndex = _index;
    }
}
