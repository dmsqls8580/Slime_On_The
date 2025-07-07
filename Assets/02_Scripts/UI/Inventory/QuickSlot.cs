using UnityEngine;
using UnityEngine.EventSystems;

public class QuickSlot : SlotBase
{
    private UIQuickSlot owner;
    private int quickSlotIndex;

    public void Initialize(int index, UIQuickSlot ownerUI)
    {
        quickSlotIndex = index;
        owner = ownerUI;
        base.Initialize(index);
    }
    
    public override void OnSlotSelectedChanged(bool isSelected)
    {
        backgroundImage.color = isSelected ? Color.yellow : Color.white;
    }

    public override void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            owner?.SelectSlot(quickSlotIndex);
        }
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            var data = GetData();
            if (data == null || !data.IsValid) return;

            InventoryInteractionHandler.Instance.TryUse(data, this);
        }
    }
}