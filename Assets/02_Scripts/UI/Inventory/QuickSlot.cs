using UnityEngine;
using UnityEngine.EventSystems;

public class QuickSlot : SlotBase
{
    private UIQuickSlot owner;
    private int quickSlotIndex;

    public void Initialize(int _index, UIQuickSlot _ownerUI)
    {
        quickSlotIndex = _index;
        owner = _ownerUI;
        base.Initialize(_index);
    }
    
    public override void OnSlotSelectedChanged(bool _isSelected)
    {
        backgroundImage.color = _isSelected ? Color.yellow : Color.white;
    }

    public override void OnPointerClick(PointerEventData _eventData)
    {
        if (_eventData.button == PointerEventData.InputButton.Left)
        {
            owner?.SelectSlot(quickSlotIndex);
        }
        else if (_eventData.button == PointerEventData.InputButton.Right)
        {
            var data = GetData();
            if (data == null || !data.IsValid) return;

            InventoryInteractionHandler.Instance.TryUse(data, this);
        }
    }
}