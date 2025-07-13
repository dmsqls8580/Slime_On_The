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
        outLineImage.color = _isSelected ? Color.yellow : Color.white;

        var handIcon = FindObjectOfType<ItemHandler>();
        if (!handIcon.IsUnityNull())
        {
            if (_isSelected)
            {
                var data = GetData();
                handIcon.ShowItemIcon(data != null ? data.ItemData : null);
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