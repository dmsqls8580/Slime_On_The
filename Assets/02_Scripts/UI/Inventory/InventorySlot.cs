using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class InventorySlot : SlotBase
{
    public override void OnPointerClick(PointerEventData _eventData)
    {
        if (_eventData == null) return;

        bool isLeft = _eventData.button == PointerEventData.InputButton.Left;
        bool isRight = _eventData.button == PointerEventData.InputButton.Right;

        bool isShift = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        bool isCtrl = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);

        if (isLeft)
        {
            var handIcon = FindObjectOfType<ItemHandler>();
            if (!handIcon.IsUnityNull())
            {
                var data = GetData();
                handIcon.ShowItemIcon(data?.ItemData);
            }

            InventoryInteractionHandler.Instance.HandleLeftClick(this, isShift, isCtrl);
        }
        else if (isRight)
        {
            InventoryInteractionHandler.Instance.HandleRightClick(this, isShift, isCtrl);
        }
    }

    public override void Clear(int _amount)
    {
        InventoryManager.Instance.RemoveItem(SlotIndex, _amount);
        Refresh();
    }
}