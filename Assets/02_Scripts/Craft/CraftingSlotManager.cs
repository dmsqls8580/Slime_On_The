using System.Collections.Generic;
using UnityEngine;

public class CraftingSlotManager : MonoBehaviour
{
    [Header("스크립트 참조")]
    [SerializeField] private CraftingStationDatabase craftingStationDatabase;

    private Dictionary<CraftingStation, HashSet<CraftingSlot>> craftingSlots = new Dictionary<CraftingStation, HashSet<CraftingSlot>>();

    public void AddCraftingSlot(int _idx, CraftingSlot _craftingSlot)
    {
        CraftingStation craftingStation = craftingStationDatabase.GetCraftingStation(_idx);
        if (craftingStation != CraftingStation.Normal)
        {
            if (!craftingSlots.TryGetValue(craftingStation, out HashSet<CraftingSlot> slotSet))
            {
                slotSet = new HashSet<CraftingSlot>();
                craftingSlots[craftingStation] = slotSet;
            }
            // 관리대상 등록.
            slotSet.Add(_craftingSlot);
            _craftingSlot.SetLocked(true);
        }
    }

    public void RemoveCraftingSlot(int _idx, CraftingSlot _craftingSlot)
    {
        CraftingStation craftingStation = craftingStationDatabase.GetCraftingStation(_idx);
        if (craftingStation != CraftingStation.Normal)
        {
            if (craftingSlots.TryGetValue(craftingStation, out HashSet<CraftingSlot> slotSet) &&
                slotSet.Contains(_craftingSlot))
            {
                // 관리대상 해제.
                slotSet.Remove(_craftingSlot);
                craftingStationDatabase.UpdateCraftingStation(_idx);
            }
        }
    }

    public void UpdateCraftingSlot(CraftingStation _currentCraftingStation, CraftingStation _previousCraftingStation)
    {
        if (_currentCraftingStation == CraftingStation.Normal)
            SetLocked(_previousCraftingStation, false);
        else
        {
            SetLocked(_currentCraftingStation, true);
            if (_previousCraftingStation != CraftingStation.Normal)
                SetLocked(_previousCraftingStation, false);
        }
    }

    private void SetLocked(CraftingStation _craftingStation, bool _isLocked)
    {
        foreach (CraftingSlot _craftingSlot in craftingSlots[_craftingStation])
            _craftingSlot.SetLocked(_isLocked);
    }
}
