using System.Collections.Generic;
using UnityEngine;

public class CraftingSlotManager : MonoBehaviour
{
    [Header("스크립트 참조")]
    [SerializeField] private CraftingStationDatabase craftingStationDatabase;

    private Dictionary<CraftingStation, HashSet<CraftingSlot>> craftingSlots;

    public void AddCraftingSlot(int _idx, CraftingSlot _craftingSlot)
    {
        CraftingStation craftingStation = craftingStationDatabase.GetCraftingStation(_idx);
        if (craftingStation != CraftingStation.Normal)
        {
            craftingSlots[craftingStation].Add(_craftingSlot);
            _craftingSlot.SetLocked(true);
        }
    }

    public void RemoveCraftingSlot(int _idx, CraftingSlot _craftingSlot)
    {
        CraftingStation craftingStation = craftingStationDatabase.GetCraftingStation(_idx);
        if (craftingStation != CraftingStation.Normal &&
            craftingSlots[craftingStation].Contains(_craftingSlot))
        {
            // 관리대상 해제.
            craftingSlots[craftingStation].Remove(_craftingSlot);
            craftingStationDatabase.UpdateCraftingStation(_idx);
        }
    }

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

    private void asdf(CraftingStation _craftingStation, bool _isLocked)
    {
        foreach (CraftingSlot _craftingSlot in craftingSlots[_craftingStation])
            _craftingSlot.SetLocked(_isLocked);
    }
}
