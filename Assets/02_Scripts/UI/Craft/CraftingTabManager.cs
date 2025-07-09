using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CraftingTabManager : MonoBehaviour
{
    [SerializeField] private List<Button> tabButtons;
    [SerializeField] private List<TabItemList> tabList;
    [SerializeField] private List<Transform> craftingIconsPanels;

    [SerializeField] private CraftingItemInfoPanel craftingItemInfoPanel;

    private int currentTabIndex = 0;

    private void Start()
    {
        for (int i = 0; i < tabButtons.Count; i++)
        {
            int index = i;
            tabButtons[i].onClick.AddListener(() => OnClickTab(index));
            for (int j = 0; j < tabList[i].Items.Count; j++)
            {
                SetSlot(craftingIconsPanels[i].GetChild(j), tabList[i].Items[j]);
            }
            craftingIconsPanels[i].gameObject.SetActive(false);
        }
        craftingIconsPanels[currentTabIndex].gameObject.SetActive(true);
    }

    private void SetSlot(Transform _slot, ItemSO _itemSO)
    {
        _slot.gameObject.SetActive(true);
        if (_slot.gameObject.TryGetComponent(out CraftingSlot _craftingSlot))
        {
            _craftingSlot.Initialize(_itemSO, craftingItemInfoPanel);
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
