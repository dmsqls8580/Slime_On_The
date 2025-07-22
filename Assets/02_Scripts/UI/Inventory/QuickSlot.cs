using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class QuickSlot : SlotBase
{
    [SerializeField] private Image outLineImage;
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
        outLineImage.gameObject.SetActive(_isSelected);
        outLineImage.color = _isSelected ? new Color32(242, 109, 91, 255) : Color.white;

        if (_isSelected)
        {
            var data = GetData();
            if (!data.IsUnityNull())
            {
                if (data.ItemData.itemTypes == ItemType.Placeable)
                    InventoryManager.Instance.placeMode.SetActiveTruePlaceMode(data.ItemData.placeableData.placeableInfo, data.ItemData);
                else
                    InventoryManager.Instance.placeMode.SetActiveFalsePlaceMode();
            }
        }
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