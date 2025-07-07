using UnityEngine;
using UnityEngine.EventSystems;

public class InventorySlot : SlotBase
{
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

    public override void Clear(int amount)
    {
        InventoryManager.Instance.RemoveItem(SlotIndex, amount);
        Refresh();
    }
}