using UnityEngine;
using UnityEngine.EventSystems;

public class EquipSlot : SlotBase
{
    [SerializeField] private EquipType equipType;

    public EquipType EquipType => equipType;
    
    public bool IsCorrectEquipType(EquipType type)
    {
        return equipType == type;
    }
    
    public override ItemInstanceData GetData()
    {
        return InventoryManager.Instance.GetEquipItem(SlotIndex);
    }
    
    public void SetItem(ItemInstanceData _data)
    {
        InventoryManager.Instance.SetEquipItem(SlotIndex, _data);
        Refresh();
    }

    public override void Clear(int _amount)
    {
        InventoryManager.Instance.ClearEquipItem(SlotIndex);
        Refresh();
    }

    public override void OnPointerClick(PointerEventData _eventData)
    {
        if (_eventData == null) return;

        bool isLeft = _eventData.button == PointerEventData.InputButton.Left;
        bool isRight = _eventData.button == PointerEventData.InputButton.Right;

        bool isShift = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        bool isCtrl = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);

        if (isLeft)
        {
            InventoryInteractionHandler.Instance.HandleLeftClick(this, isShift, isCtrl);
        }
        else if (isRight)
        {
            InventoryInteractionHandler.Instance.HandleRightClick(this, isShift, isCtrl);
        }
    }
}