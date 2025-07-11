using UnityEngine;

public enum CraftingStation
{
    Normal,
    Workbench,
    Anvil
}

public class CraftingStationManager : MonoBehaviour
{
    [Header("스크립트 참조")]
    [SerializeField] private CraftingSlotManager craftingSlotManager;

    private CraftingStation currentCraftingStation = CraftingStation.Normal;
    //public CraftingStation CurrentCraftingStation => currentCraftingStation;

    public void UpdateCurrentCraftingStation(CraftingStation _craftingStation)
    {
        craftingSlotManager.UpdateCraftingSlot(currentCraftingStation, _craftingStation);
        currentCraftingStation = _craftingStation;
    }
}
