using System.Collections.Generic;
using UnityEngine;

public class CraftingSlotManager : MonoBehaviour
{
    [Header("스크립트 참조")]
    [SerializeField] private CraftingStationDatabase craftingStationDatabase;

    private Dictionary<CraftingStation, List<CraftingSlot>> craftingSlots;

    public void AddCraftingSlot(int _idx, CraftingSlot _craftingSlot)
    {
        CraftingStation craftingStation = craftingStationDatabase.GetCraftingStation(_idx);
        if (craftingStation != CraftingStation.Normal)
        {
            craftingSlots[craftingStation].Add(_craftingSlot);
            // 무조건 _craftingSlot에 대해 Lcoked
        }
    }

    // RemoveCraftingSlot(); 왜냐 제작됐을떄 삭제해야하기때문 그리고 삭제할땐 잠금해제도

    public void UpdateCraftingSlot(CraftingStation _currentCraftingStation, CraftingStation _previousCraftingStation)
    {
        if (_currentCraftingStation == CraftingStation.Normal)
            asdf(_previousCraftingStation, false);
        else
        {
            asdf(_currentCraftingStation, true);
            if (_previousCraftingStation != CraftingStation.Normal)
                asdf(_previousCraftingStation, false);
        }
    }

    private void asdf(CraftingStation _craftingStation, bool _locked)
    {
        foreach (CraftingSlot _craftingSlot in craftingSlots[_craftingStation])
        {
            //_craftingSlot.SetLocked(_locked);
        }
    }
}
