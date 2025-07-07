using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public abstract class SlotBase : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] protected Image iconImage;
    [SerializeField] protected Image backgroundImage;
    [SerializeField] protected TextMeshProUGUI quantityText;

    public int SlotIndex { get; protected set; }

    // 슬롯 초기화
    public virtual void Initialize(int _index)
    {
        SlotIndex = _index;
        InventoryManager.Instance.OnSlotChanged += OnSlotChanged;
        OnSlotChanged(_index);
    }
    
    private void OnDestroy()
    {
        if (InventoryManager.HasInstance)
            InventoryManager.Instance.OnSlotChanged -= OnSlotChanged;
    }
    
    // 아이템 데이터 반환
    public virtual ItemInstanceData GetData()
    {
        return InventoryManager.Instance.GetItem(SlotIndex);
    }
    
    // 아이템 존재여부 확인
    public bool HasItem()
    {
        var data = GetData();
        return data != null && data.IsValid;
    }
    
    public virtual void Refresh()
    {
        OnSlotChanged(SlotIndex);
    }
    
    public virtual void Clear(int _amount)
    {
        InventoryManager.Instance.RemoveItem(SlotIndex, _amount);
        Refresh();
    }
    
    public virtual void OnSlotSelectedChanged(bool _isSelected)
    {
        
    }
    
    public virtual void OnSlotChanged(int _index)
    {
        if (_index != SlotIndex) return;

        var data = GetData();
        if (data != null && data.IsValid)
        {
            iconImage.sprite = data.ItemData.icon;
            iconImage.enabled = true;
            quantityText.text = data.Quantity > 1 ? data.Quantity.ToString() : "";
        }
        else
        {
            iconImage.sprite = null;
            iconImage.enabled = false;
            quantityText.text = "";
        }
    }

    public abstract void OnPointerClick(PointerEventData _eventData);
}