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
    
    public void SetItem(ItemInstanceData data)
    {
        InventoryManager.Instance.SetEquipItem(SlotIndex, data);
        Refresh();
    }

    public override void Clear(int amount)
    {
        InventoryManager.Instance.ClearEquipItem(SlotIndex);
        Refresh();
    }

    public override void OnPointerClick(PointerEventData eventData)
    {
        if (eventData == null) return;

        bool isLeft = eventData.button == PointerEventData.InputButton.Left;
        bool isRight = eventData.button == PointerEventData.InputButton.Right;

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