using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SlotBase : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler, IPointerClickHandler
{
    [SerializeField] protected Image iconImage;
    [SerializeField] protected TextMeshProUGUI itemNameTxt;
    [SerializeField] protected TextMeshProUGUI quantityTxt;

    public int SlotIndex { get; private set; }
    
    public UnityAction<int> onSlotClicked;
    
    public void Initialize(int index)
    {
        SlotIndex = index;
        InventoryManager.Instance.OnSlotChanged += OnSlotChanged;
        OnSlotChanged(index); 
    }
    
    public ItemInstanceData GetData()
    {
        return InventoryManager.Instance.GetItem(SlotIndex);
    }

    public bool HasItem()
    {
        var data = GetData();
        return data != null && data.IsValid;
    }

    public virtual void OnSlotSelectedChanged(bool isSelected)
    {
        
    }
    
    private void OnDestroy()
    {
        if (InventoryManager.HasInstance)
        {
            InventoryManager.Instance.OnSlotChanged -= OnSlotChanged;
        }
    }
    
    private void OnSlotChanged(int changedIdx)
    {
        if (changedIdx != SlotIndex) return;

        var data = InventoryManager.Instance.GetItem(SlotIndex);
        if (data == null || !data.IsValid)
        {
            iconImage.enabled = false;
            iconImage.sprite = null;
            quantityTxt.text = "";
            if (itemNameTxt != null) itemNameTxt.text = "";
        }
        else
        {
            iconImage.enabled = true;
            iconImage.sprite = data.Icon;
            quantityTxt.text = data.Quantity > 1 ? $"x{data.Quantity}" : "";
            if (itemNameTxt != null) itemNameTxt.text = data.Name;
        }
    }
    
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (InventoryManager.Instance.GetItem(SlotIndex)?.IsValid == true)
            DragManager.Instance.StartDrag(this);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (DragManager.Instance.IsDragging)
            DragManager.Instance.UpdateDrag(eventData.position);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        DragManager.Instance.EndDrag();
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (!DragManager.Instance.IsDragging || DragManager.Instance.DraggedSlot == this)
            return;

        InventoryManager.Instance.TrySwapOrMerge(DragManager.Instance.DraggedSlot.SlotIndex, this.SlotIndex);
    }
    
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            onSlotClicked?.Invoke(SlotIndex);
        }
    }
}
